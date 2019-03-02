using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    /*public static InventoryManager instance;
    void Awake()
    {
        instance = this;
    }*/
    
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    int space = 12; 

    [HideInInspector]
    public Dictionary<Item,int> itemss = new Dictionary<Item,int>();


    public bool Add(Item item)
    {
        if (!item.isDefaultItem)
        {
            if (itemss.Count >= space) return false;

            if (!itemss.ContainsKey(item)) itemss.Add(item, 1);
            else
            {
                itemss[item] += 1;                
            }

            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }
        }
        return true;
        
    }

    public void Remove(Item item)
    {
        itemss.Remove(item);
        if (onItemChangedCallback != null)
        {
            onItemChangedCallback.Invoke();
        }
    }
}
