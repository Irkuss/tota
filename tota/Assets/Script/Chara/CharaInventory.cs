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
        private Button _info;
        private GameObject _counter;
        
        private CharaInventory _linkedCharaInventory;

        public Slot(CharaInventory charaInventory,Dictionary<Item, int> inventory,Image icon, Button removeButton, Button itemButton, Button info, GameObject counter, bool pos)
        {
            //Reference au CharaInventory pour appeler UpdateUI (SPAGHETTI BOIII)
            _linkedCharaInventory = charaInventory;

            isEmpty = pos;
            //Initialisation de  l'UI
            _icon = icon;
            _removeButton = removeButton;
            _itemButton = itemButton;
            _info = info;
            _counter = counter;
            //Initialisation du bouton (appelle OnRemoveButton quand il est cliqué)
            _removeButton.onClick.AddListener(OnRemoveButton);
            _itemButton.onClick.AddListener(OnClickButton);
            _info.onClick.AddListener(Pop);
            //L'emplacement est initialisé vide
            ClearSlot();
        }

        public void AddItem(Item item, int itemCount) //(Item newItem)
        {
            //if (_linkedCharaInventory._interface == null) return;

            _item = item;
            _itemCount = itemCount;

            //Change Icon
            if (_icon != null)
            {
                _icon.enabled = true;
                _icon.sprite = _item.icon;
            }
            else
            {
                Debug.LogWarning("AddItem: icon is null"); //THIS HAPPEN WHEN THE INVENTORY IS NOT OPEN WHEN ADDING THE ITEM
            }
            
            //Change counter
            if(_counter != null)
            {
                _counter.SetActive(true);
                _counter.GetComponent<Text>().text = _itemCount.ToString();
            }            
            //RemoveButton is now active
            if (_removeButton != null) _removeButton.interactable = true;
            isEmpty = false;

            //_itemButton.transform.parent.GetChild(5).GetChild(0).GetComponent<Text>().text = item.description;

            if(_itemButton != null) _itemButton.transform.parent.GetComponent<SlotDescription>().description = item.description;
        }

        public void ClearSlot()
        {
            //if (_linkedCharaInventory._interface == null) return;

            _item = null;

            //Clear Icon
            if(_icon != null)
            {
                _icon.sprite = null;
                _icon.enabled = false;
            }
            
            //Clear counter
            if(_counter != null) _counter.SetActive(false);
            //deactivate RemoveButton=
            if(_removeButton != null) _removeButton.interactable = false;
            isEmpty = true;

            if (_itemButton != null) _itemButton.transform.parent.GetComponent<SlotDescription>().description = "";
        }

        public void OnRemoveButton()
        {
            //Appelé par le bouton pour detruire l'item
            _linkedCharaInventory.Remove(_item);
        }

        public void OnClickButton()
        {
            // Si l'item est utilisable
            //if (item.usable) -> tanguy: on verifie dans Use (pour pouvoir le deplacer depuis un inventaire de chara)
            //{
                bool itemUsed = item.Use(_linkedCharaInventory.gameObject); // On appelle la fonction virtual de l'item en prenant la référence de son parent //_linkedCharaInventory._slotParent
                if (itemUsed) OnRemoveButton();                            // On enleve l'item utilisé
            //}
        }

        private void Pop()
        {
            _itemButton.transform.parent.GetChild(5).gameObject.SetActive(!_itemButton.transform.parent.GetChild(4).gameObject.activeSelf);            
        }
    }

    //Database des items (pour avoir leur id, pour les update en rpc)
    [SerializeField] private ItemTable itemTable = null;
    public ItemTable ItemTable { get { return itemTable; } }

    //Dictionnaire représentant l'inventaire
    [HideInInspector]
    public Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    [HideInInspector] public Wearable[] wearables = new Wearable[4];
    [HideInInspector] public Equipable[] equipments = new Equipable[2];

    //Nombre d'emplacement (Tweakable)
    private int _inventorySpace = 12;

    //List and stuff (slots)
    private GameObject _slotParent;
    [SerializeField] private GameObject _inventoryPrefab = null;
    [SerializeField] private GameObject _interfacePrefab = null;
    private GameObject _inventory = null;
    private GameObject _interface = null;
    private Slot[] _slots = null;

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
                child.transform.GetChild((int)InterfaceManager.SlotIndex.Item).GetComponent<Image>(),
                child.transform.GetChild((int)InterfaceManager.SlotIndex.RemoveButton).GetComponent<Button>(),
                child.transform.GetChild((int)InterfaceManager.SlotIndex.Item).GetComponent<Button>(),
                child.transform.GetChild((int)InterfaceManager.SlotIndex.PopDescription).GetComponent<Button>(),
                child.transform.GetChild((int)InterfaceManager.SlotIndex.Stack).gameObject,
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
        if (_slots == null) return;
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

        if(_interface != null)
        {
            UpdateCraft();
            UpdateWeight();
        }

    }
    public void UpdateCraft()
    {
        if (_interface == null)
        {
            Debug.LogWarning("UpdateCraft: interface was null");
            return;
        }
        _interface.GetComponent<InterfaceManager>().UpdateCraft(this);
    }

    //Openning and closing Inventory (canvas)
    public void ToggleInterface(GameObject parent, string[] stats)
    {
        if (_interface != null) return;

        _interface = Instantiate(_interfacePrefab);
        _interface.transform.SetParent(parent.transform, false);
        _interface.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = gameObject.GetComponent<CharaRpg>().NameFull;
        _interface.GetComponent<InterfaceManager>().InstantiateCraft(this);
        _interface.GetComponent<InterfaceManager>().InstantiateEquipment();
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(this);
        _interface.GetComponent<InterfaceManager>().UpdateInjuries(GetComponent<CharaRpg>().GetWoundsInfo());
        _interface.GetComponent<InterfaceManager>().UpdateStats(GetComponent<CharaRpg>().GetHealthStats());

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
        _inventory.GetComponent<Image>().color = Color.gray;
        _slotParent = _inventory.transform.GetChild(0).GetChild(0).gameObject;
        InitSlots();
    }

    public GameObject GetInterface()
    {
        return _interface;
    }

    public int UpdateWeight()
    {
        if (_inventory == null) return -1;
        int sum = 0;

        foreach(var item in inventory)
        {
            sum += item.Key.weight * item.Value;
        }

        _inventory.transform.GetChild(1).GetComponent<Text>().text = "Poids actuel : " + sum + " || Poids max : " + gameObject.GetComponent<CharaRpg>().GetCurrentStat(CharaRpg.Stat.ms_strength)/5;

        return sum;
    }
    public void UpdateStats(string[] stats)
    {
        //GameObject stat = _interface.gameObject.GetComponent<InterfaceManager>().tooltip;
        //stat.GetComponent<ToolTip>().UpdateTool(stats);
    }

    public void CloseInterface()
    {
        //Appelé CloseInventoryOnDeselected() pour fermer l'inventaire quand un Chara est deselectionné
        
        if (_interface != null)
        {
            Destroy(_interface);
            GameObject slot = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Description;
            slot.SetActive(false);

        }
        //Debug.Log("CharaInventory: closed Inventory after being deselected");
    }
    public void CloseInventory()
    {
        if(_inventory != null)
        {
            Destroy(_inventory);
            GameObject slot = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Description;
            slot.SetActive(false);
        }
    }

    //Resistance getters

    public int GetBodyPartSharpResistance(CharaRpg.BodyType type)
    {
        int bodyPartSharpResistance = 0;
        foreach (Wearable equipment in wearables)
        {
            if (equipment != null)
            {
                if (equipment.ContainsBodyType(type))
                {
                    bodyPartSharpResistance += equipment.sharpResistance;
                }
            }
        }
        return bodyPartSharpResistance;
    }
    public int GetTotalSharpResistance()
    {
        int totalSharpResistance = 0;
        foreach(Wearable equipment in wearables)
        {
            if (equipment != null)
            {
                totalSharpResistance += equipment.sharpResistance;
            }
        }
        return totalSharpResistance;
    }
    public int GetBodyPartMaceResistance(CharaRpg.BodyType type)
    {
        int bodyPartMaceResistance = 0;
        foreach (Wearable equipment in wearables)
        {
            if(equipment != null)
            {
                if (equipment.ContainsBodyType(type))
                {
                    bodyPartMaceResistance += equipment.maceResistance;
                }
            }
        }
        return bodyPartMaceResistance;
    }
    public int GetTotalMaceResistance()
    {
        int totalMaceResistance = 0;
        foreach (Wearable equipment in wearables)
        {
            if (equipment != null)
            {
                totalMaceResistance += equipment.maceResistance;
            }
        }
        return totalMaceResistance;
    }

    public int GetMinTemperatureModifier()
    {
        int minTempModifier = 0;
        foreach(Wearable wearable in wearables)
        {
            if(wearable != null)
            {
                minTempModifier += wearable.minTempModifier;
            }
        }
        return minTempModifier;
    }
    //Break Getters
    public float GetMaxBreakDamage()
    {
        float damageLeft = equipments[0] == null ? 5 : equipments[0].equipType == Equipable.EquipType.Remote ? 5 : equipments[0].BreakDamage;
        float damageRight = equipments[1] == null ? 5 : equipments[1].equipType == Equipable.EquipType.Remote ? 5 : equipments[1].BreakDamage;
        Debug.Log("GetMaxBreakDamage: returning " + Mathf.Max(damageLeft, damageRight));
        return Mathf.Max(damageLeft, damageRight);
    }

    //Item Getters
    public bool Contains(Item item, int quantity = 1)
    {
        if (item == null) return true;

        if(inventory.ContainsKey(item))
        {
            return inventory[item] >= quantity;
        }
        return false;
    }

    //Adding and removing item
    public bool Add(Item item)
    {
        if(item == null)
        {
            Debug.Log("CharaInventory: item was null (happens when generating Loots in furniture)");
            return false;
        }
        //Le booléen retourné représente la réussite de l'ajout de l'item
        //Debug.Log("CharaInventory: Checking space");

        bool noMoreSpace = inventory.Count >= _inventorySpace;
        bool noMoreStrength = GetComponent<CharaRpg>() == null ? false : (UpdateWeight() + item.weight > (GetComponent<CharaRpg>().GetCurrentStat(CharaRpg.Stat.ms_strength) / 5));
        bool lastSpotIsNotEmpty = _slots == null ? false : !_slots[_inventorySpace - 1].isEmpty;
        bool lastSpotIsFull = _slots == null ? false : _slots[_inventorySpace - 1].itemCount == item.stack;
        bool lastSpotContainsADifferentItem = _slots == null ? false : _slots[_inventorySpace - 1].item != item;

        if (noMoreSpace || noMoreStrength ||  lastSpotIsNotEmpty && (lastSpotIsFull || lastSpotContainsADifferentItem)) // Du spaghetti comme on aime
        {
            //Si l'inventaire est rempli, ne fait rien
            return false;
        }
        Debug.Log("CharaInventory: Adding item " + item.nickName);
        ModifyCount(item, 1);
        return true;
    }

    public void Remove(Item item)
    {
        if(item == null)
        {
            Debug.LogWarning("CharaInventory: Unexpected null item");
            return;
        }
        if(!Contains(item))
        {
            Debug.LogWarning("CharaInventory: Unexpected item not present");
            return;
        }

        Debug.Log("CharaInventory: Removing item " + item.nickName);
        ModifyCount(item, -1);
    }

    public void ModifyCount(Item item, int countModifier)
    {
        CharaConnect conn = GetComponent<CharaConnect>();
        if (conn != null)
        {
            conn.SendMsg(CharaConnect.CharaCommand.ModifyCountWithId, new int[2] { itemTable.GetIdWithItem(item), countModifier }, null, null);
        }
        else
        {
            GetComponent<PropHandler>().CommandSend(new int[3] { (int)PropFurniture.FurnitureCommand.Modify, itemTable.GetIdWithItem(item), countModifier });
        }
    }

    //RPC functions
    public void ModifyCountWithId(int id, int countModifier)
    {
        if (countModifier == 0) return;

        Item item = itemTable.GetItemWithId(id);

        if (item == null) Debug.LogWarning("ModifyCountWithId: Unexpected null item in inventory");

        if(countModifier > 0)
        {
            if (inventory.ContainsKey(item))
            {
                //Si on veut ajouter un item qui est présent dans l'inventaire
                inventory[item] += countModifier;
            }
            else
            {
                //Si on veut ajouter un item qui n'est pas présent dans l'inventaire
                inventory.Add(item, countModifier);
            }
        }
        else
        {
            //Dans le cas ou on diminue le nombre d'item
            inventory[item] += countModifier;

            if (inventory[item] <= 0)
            {
                //Si on supprimé tous les items d'un meme type
                inventory.Remove(item);
            }
        }
        //Met a jour l'inventaire
        UpdateUI();
    }



    public void SetEquipments(Item item, int index)
    {
        SendSetEquip(item, index, true);
    }
    public void SetWearable(Item item, int index)
    {
        SendSetEquip(item, index, false);
    }

    private void SendSetEquip(Item item, int index, bool toEquip)
    {
        int id = item == null ? -1: itemTable.GetIdWithItem(item);

        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetEquip, new int[3] { id, index, toEquip ? 1 : 0});
    }

    public void RPC_SetEquip(int itemId, int index, bool toEquip)
    {
        if(itemId < 0)
        {
            if(toEquip)
            {
                equipments[index] = null;
            }
            else
            {
                wearables[index] = null;
            }
        }
        else
        {
            Item item = itemTable.GetItemWithId(itemId);

            if (toEquip)
            {
                equipments[index] = item as Equipable;
            }
            else
            {
                wearables[index] = item as Wearable;
            }
        }
        
        if (_interface == null) return;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(this);
    }
}
