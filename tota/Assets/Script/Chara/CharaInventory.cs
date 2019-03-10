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
        private GameObject _counter;

        private Dictionary<Item, int> _linkedInventory;
        private CharaInventory _linkedCharaInventory;

        public Slot(CharaInventory charaInventory,Dictionary<Item, int> inventory,Image icon, Button removeButton, GameObject counter, bool pos)
        {
            //Reference au CharaInventory pour appeler UpdateUI (SPAGHETTI BOIII)
            _linkedCharaInventory = charaInventory;
            //Reference à l'inventaire
            _linkedInventory = inventory;
            isEmpty = pos;
            //Initialisation de  l'UI
            _icon = icon;
            _removeButton = removeButton;
            _counter = counter;
            //Initialisation du bouton (appelle OnRemoveButton quand il est cliqué)
            _removeButton.onClick.AddListener(OnRemoveButton);
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
    }

    //Le gameObject canvas
    [SerializeField]
    private GameObject _canvas;

    //Database des items (pour avoir leur id, pour les update en rpc)
    [SerializeField]
    private ItemTable itemTable;

    //Position et rotation (Tweakable)
    private Vector3 _inventPosition = new Vector3(0, 3, 2);
    private Quaternion _inventRotation = new Quaternion();

    //Dictionnaire représentant l'inventaire
    [HideInInspector]
    public Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    //Nombre d'emplacement (Tweakable)
    private int _inventorySpace = 12;

    //Callback pour Update L'UI
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    //List and stuff (slots)
    [SerializeField]
    private GameObject _slotParent;
    private Slot[] _slots;

    //Init
    private void Start()
    {
        _canvas.SetActive(false);
        //initialisation de la rotation à 90° du canvas (pour LateUpdate)
        _inventRotation = (Quaternion.Euler(90, 0, 0));

        InitSlots();
    }

    private void InitSlots()
    {
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
                child.transform.GetChild(2).gameObject,
                true);
        }
        //Desactive les slots en trop
        for (; i < childrenCount; i++)
        {
            _slotParent.transform.GetChild(i).gameObject.SetActive(false);
        }
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

    // Correction des coordonnées du canvas
    void LateUpdate()
    {
        //Maintient une position et une rotation correcte pour le canvas
        _canvas.transform.position = transform.position + _inventPosition;
        _canvas.transform.rotation = _inventRotation;
    }

    //Openning and closing Inventory (canvas)
    public void ToggleInventory()
    {
        //Appelé par SpiritHead après avoir appuyé sur E (si le Chara est selectionné)
        if (_canvas.activeSelf)
        {
            _canvas.SetActive(false);
        }
        else
        {
            _canvas.SetActive(true);
        }
    }

    public void CloseInventory()
    {
        //Appelé RemoveInventoryOnDeselected() pour fermer l'inventaire quand un Chara est deselectionné
        _canvas.SetActive(false);
        Debug.Log("CharaInventory: closed Inventory after being deselected");
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
            GetComponent<PhotonView>().RPC("AddWithId", PhotonTargets.AllBuffered, itemTable.GetIdWithItem(item));
        }
        Debug.Log("CharaInventory: Item has been added");
        //Appelle le Callback en s'assurant que quelqu'un écoute
        //if (onItemChangedCallback != null) onItemChangedCallback.Invoke();

        //UpdateUI();

        return true;
    }

    public void Remove(Item item)
    {
        GetComponent<PhotonView>().RPC("RemoveWithId", PhotonTargets.AllBuffered, itemTable.GetIdWithItem(item));

        //inventory.Remove(item);
        //UpdateUI();
    }

    public void ModifyCount(Item item, int countModifier)
    {
        GetComponent<PhotonView>().RPC("ModifyCountWithId", PhotonTargets.AllBuffered, itemTable.GetIdWithItem(item), countModifier);
    }

    //RPC functions

    [PunRPC]
    public void RemoveWithId(int id)
    {
        inventory.Remove(itemTable.GetItemWithId(id));

        UpdateUI();
    }
    [PunRPC]
    public void AddWithId(int id)
    {
        inventory.Add(itemTable.GetItemWithId(id),1);

        UpdateUI();
    }
    [PunRPC]
    public void ModifyCountWithId(int id, int countModifier)
    {
        inventory[itemTable.GetItemWithId(id)] += countModifier;

        UpdateUI();
    }

}
