using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;

    InventoryManager inventory;
    InventorySlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        //inventory = InventoryManager.instance;
        inventory = GetComponentInParent<InventoryManager>();
        inventory.onItemChangedCallback += UpdateUI;

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

    }

    void UpdateUI()
    {
        int i = 0;
        foreach (KeyValuePair<Item, int> item in inventory.itemss)
        {
            if (i < slots.Length)
            {
                int value = item.Value;
                int stack = item.Key.stack;

                if (value >= stack)
                {
                    while (value > stack)
                    {
                        slots[i].AddItem(item.Key, stack);
                        i++;
                        value -= stack;
                    }
                    slots[i].AddItem(item.Key, value);
                    i++;

                }
                else
                {
                    slots[i].AddItem(item.Key, item.Value);
                    i++;
                }
            }
            else return;
        }
        for (; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
        
    }
}
