using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Pour la classe Slot
using UnityEngine.UI;

public class CharaInventory : MonoBehaviour
{
    public class Slot
    {
        private Item _item;
        public Item item { get { return _item; } }
        private int _itemCount;
        public int itemCount { get { return _itemCount; } }
        public bool isEmpty;
        private Image _icon;
        private Button _removeButton;
        private Button _itemButton;
        private GameObject _counter;

        private Dictionary<Item, int> _linkedInventory;
        private CharaInventory _linkedCharaInventory;

        public Slot(CharaInventory charaInventory,Dictionary<Item, int> inventory,Image icon, Button removeButton, Button itemButton, GameObject counter, bool pos)
        {
            //Reference au CharaInventory pour appeler UpdateUI (SPAGHETTI BOIII)
            _linkedCharaInventory = charaInventory;
            //Reference à l'inventaire
            _linkedInventory = inventory;
            isEmpty = pos;
            //Initialisation de  l'UI
            _icon = icon;
            _removeButton = removeButton;
            _itemButton = itemButton;
            _counter = counter;
            //Initialisation du bouton (appelle OnRemoveButton quand il est cliqué)
            _removeButton.onClick.AddListener(OnRemoveButton);
            _itemButton.onClick.AddListener(OnClickButton);
            //L'emplacement est initialisé vide
            ClearSlot();
        }

        public void AddItem(Item item, int itemCount) //(Item newItem)
        {
            _item = item;
            _itemCount = itemCount;

            //Change Icon
            _icon.sprite = _item.icon;
            _icon.enabled = true;
            //Change counter
            _counter.SetActive(true);
            _counter.GetComponent<Text>().text = _itemCount.ToString();
            //RemoveButton is now active
            _removeButton.interactable = true;
            isEmpty = false;
        }

        public void ClearSlot()
        {
            _item = null;

            //Clear Icon
            _icon.sprite = null;
            _icon.enabled = false;
            //Clear counter
            _counter.SetActive(false);
            //deactivate RemoveButton=
            _removeButton.interactable = false;
            isEmpty = true;
        }

        public void OnRemoveButton()
        {
            //Appelé par le bouton pour detruire l'item
            if (_linkedInventory[_item] <= 1)
            {
               _linkedCharaInventory.Remove(_item);
            }
            else
            {
                _linkedCharaInventory.ModifyCount(_item, -1);
                //ClearSlot(); lets try this
            }
            _linkedCharaInventory.UpdateUI();
        }

        public void OnClickButton()
        {
            // Si l'item est utilisable
            if (item.usable)
            {
                item.Use(_linkedCharaInventory.gameObject); // On appelle la fonction virtual de l'item en prenant la référence de son parent //_linkedCharaInventory._slotParent
                OnRemoveButton();                            // On enleve l'item utilisé
            }
        }
    }

    //Database des items (pour avoir leur id, pour les update en rpc)
    [SerializeField] private ItemTable itemTable = null;

    //Dictionnaire représentant l'inventaire
    [HideInInspector]
    public Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    //Nombre d'emplacement (Tweakable)
    private int _inventorySpace = 12;

    //Callback pour Update L'UI
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    //List and stuff (slots)
    private GameObject _slotParent;
    [SerializeField] private GameObject _inventoryPrefab = null;
    [SerializeField] private GameObject _interfacePrefab = null;
    private GameObject _inventory = null;
    private GameObject _interface = null;
    private Slot[] _slots;

    //Init

    private void InitSlots()
    {
        if (_slotParent == null) return;

        int childrenCount = _slotParent.transform.childCount;

        _slots = new Slot[_inventorySpace];

        if (childrenCount < _inventorySpace) Debug.Log("CharaInventory: InitSlots: we need more deck sluts! (more slots needed)");
        int i = 0;
        //Init Slots
        for (; i < _inventorySpace; ++i)
        {
            GameObject child = _slotParent.transform.GetChild(i).gameObject;

            //pas ultra beau mais arrive que lors de l'init (pas mal spaghetti même)
            _slots[i] = new Slot(
                this,
                inventory, 
                child.transform.GetChild(0).GetComponent<Image>(), 
                child.transform.GetChild(1).GetComponent<Button>(),
                child.transform.GetChild(0).GetComponent<Button>(),
                child.transform.GetChild(2).gameObject,
                true);
        }
        //Desactive les slots en trop
        for (; i < childrenCount; i++)
        {
            _slotParent.transform.GetChild(i).gameObject.SetActive(false);
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        int i = 0;
        foreach (KeyValuePair<Item, int> item in inventory)
        {
            if (i < _slots.Length)
            {
                int count = item.Value;
                int maxCount = item.Key.stack;

                if (count >= maxCount)
                {
                    while (count > maxCount)
                    {
                        _slots[i].AddItem(item.Key, maxCount);
                        i++;
                        count -= maxCount;
                    }
                    _slots[i].AddItem(item.Key, count);
                    i++;

                }
                else
                {
                    _slots[i].AddItem(item.Key, item.Value);
                    i++;
                }
            }
            else return;
        }
        //Clear tout les spots suivants
        for (; i < _slots.Length; i++)
        {
            _slots[i].ClearSlot();
        }

    }

    //Openning and closing Inventory (canvas)
    public void ToggleInterface(GameObject parent, string[] stats)
    {
        _interface = Instantiate(_interfacePrefab);
        _interface.transform.SetParent(parent.transform, false);
        _interface.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = gameObject.GetComponent<CharaRpg>().FullName;
        _interface.GetComponent<InterfaceManager>().InstantiateCraft();

        _inventory = Instantiate(_inventoryPrefab);
        _inventory.transform.SetParent(_interface.transform.GetChild(0).GetChild(3).GetChild(0), false);
        _slotParent = _inventory.transform.GetChild(0).GetChild(0).gameObject;

        InitSlots();
        UpdateStats(stats);
    }

    public void ToggleInventory(GameObject parent)
    {
        _inventory = Instantiate(_inventoryPrefab);
        _inventory.transform.SetParent(parent.transform, false);
        _inventory.GetComponent<Image>().color = new Color(212, 210, 97, 100);
        _slotParent = _inventory.transform.GetChild(0).GetChild(0).gameObject;
        InitSlots();
    }

    public void UpdateStats(string[] stats)
    {
        GameObject stat = _interface.gameObject.GetComponent<InterfaceManager>().tooltip;
        stat.GetComponent<ToolTip>().UpdateTool(stats);
    }

    public void CloseInterface()
    {
        //Appelé CloseInventoryOnDeselected() pour fermer l'inventaire quand un Chara est deselectionné
        
        if (_interface != null)
        {
            Destroy(_interface);
        }
        Debug.Log("CharaInventory: closed Inventory after being deselected");
    }

    public void CloseInventory()
    {
        if(_inventory != null)
        {
            Destroy(_inventory);
        }
    }
    
    //Adding and removing item
    public bool Add(Item item)
    {
        //Le booléen retourné représente la réussite de l'ajout de l'item
        Debug.Log("CharaInventory: Checking space");
        if (inventory.Count >= _inventorySpace || 
            _slots[_inventorySpace-1].isEmpty == false && 
            (_slots[_inventorySpace - 1].itemCount == item.stack || _slots[_inventorySpace - 1].item != item)) // Du spaghetti comme on aime
        {
            //Si l'inventaire est rempli, ne fait rien
            return false;
        }
        Debug.Log("CharaInventory: Adding item");
        if (inventory.ContainsKey(item))
        {
            Debug.Log("CharaInventory: Item was already present");
            //Si l'item était déjà dans l'inventaire, augmente son compte de 1
            //inventory[item] += 1;
            ModifyCount(item, 1);
        }
        else
        {
            Debug.Log("CharaInventory: Item was not present");
            //Si l'item n'était pas dans l'inventaire, ajoute un exemplaire de cet item
            //inventory.Add(item, 1);
            gameObject.GetComponent<PhotonView>().RPC("AddWithId", PhotonTargets.AllBuffered, itemTable.GetIdWithItem(item));
        }
        Debug.Log("CharaInventory: Item has been added");
        //Appelle le Callback en s'assurant que quelqu'un écoute
        //if (onItemChangedCallback != null) onItemChangedCallback.Invoke();

        //UpdateUI();

        return true;
    }

    public void Remove(Item item)
    {
        gameObject.GetComponent<PhotonView>().RPC("RemoveWithId", PhotonTargets.AllBuffered, itemTable.GetIdWithItem(item));

        //inventory.Remove(item);
        //UpdateUI();
    }

    public void ModifyCount(Item item, int countModifier)
    {
        gameObject.GetComponent<PhotonView>().RPC("ModifyCountWithId", PhotonTargets.AllBuffered, itemTable.GetIdWithItem(item), countModifier);
    }

    //RPC functions

    [PunRPC]
    private void RemoveWithId(int id)
    {
        inventory.Remove(itemTable.GetItemWithId(id));

        UpdateUI();
    }
    [PunRPC]
    private void AddWithId(int id)
    {
        inventory.Add(itemTable.GetItemWithId(id),1);

        UpdateUI();
    }
    [PunRPC]
    private void ModifyCountWithId(int id, int countModifier)
    {
        inventory[itemTable.GetItemWithId(id)] += countModifier;

        UpdateUI();
    }

}
