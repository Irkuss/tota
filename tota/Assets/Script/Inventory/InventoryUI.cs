using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;

    private InventoryManager inventory;
    private InventorySlot[] slots;

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
                int count = item.Value;
                int maxCount = item.Key.stack;

                if (count >= maxCount)
                {
                    while (count > maxCount)
                    {
                        slots[i].AddItem(item.Key, maxCount);
                        i++;
                        count -= maxCount;
                    }
                    slots[i].AddItem(item.Key, count);
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
        //Clear tout les spots suivants
        for (; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
        
    }
}
