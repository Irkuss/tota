using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    private Item _item;
    private int _itemCount;

    //Element lié au slot
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private Button _removeButton;
    [SerializeField]
    private GameObject _counter;

    //private InventoryManager _instance; //Changed _instance to _inventory
    private CharaInventory _inventory;

    private void Start()
    {
        //_instance = GetComponentInParent<InventoryManager>();
        _inventory = GetComponentInParent<CharaInventory>();
    }


    public void AddItem (Item item, int itemCount) //(Item newItem)
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
    }

    public void OnRemoveButton()
    { 
        //Appelé par le bouton pour detruire l'item
        if (_inventory.inventory[_item] <= _item.stack) 
        {
            _inventory.Remove(_item);
        }
        else
        {
            _inventory.inventory[_item] -= _itemCount;
            ClearSlot();
            _inventory.onItemChangedCallback.Invoke();
        }
        // Remettre le gameObject dans la scène si possible 
    }

    /*
    public void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
    }*/
}
