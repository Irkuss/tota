using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    // METTRE [SERIALIZEFIELD]
    Item item;
    public Image icon;
    public Button removeButton;
    public GameObject count;

<<<<<<< HEAD
    /*private InventoryManager instance;

    private void Start()
    {
        instance = GetComponentInParent<InventoryManager>().instance;
    }*/
=======
    private InventoryManager instance;

    private void Start()
    {
        instance = GetComponentInParent<InventoryManager>();
    }

>>>>>>> launcher

    public void AddItem (Item newItem, int ccount) //(Item newItem)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true;
        count.SetActive(true);
        count.GetComponent<Text>().text = ccount.ToString();
        removeButton.interactable = true;
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
        count.SetActive(false);
        removeButton.interactable = false;
    }

    public void OnRemoveButton()
    {
<<<<<<< HEAD
        if (InventoryManager.instance.itemss[item] <= item.stack) 
        {
            InventoryManager.instance.Remove(item); 
        }
        else
        {
            InventoryManager.instance.itemss[item] -= int.Parse(count.GetComponent<Text>().text);
            ClearSlot();
            InventoryManager.instance.onItemChangedCallback.Invoke();  
=======
        if (instance.itemss[item] <= item.stack) 
        {
            instance.Remove(item); 
        }
        else
        {
            instance.itemss[item] -= int.Parse(count.GetComponent<Text>().text);
            ClearSlot();
            instance.onItemChangedCallback.Invoke();  
>>>>>>> launcher
        }
        // Remettre le gameObject dans la scène si possible 
    }

    /*public void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
    }*/
}
