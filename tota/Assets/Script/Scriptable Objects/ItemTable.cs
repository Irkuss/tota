using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemTable")]
public class ItemTable : ScriptableObject
{
    public Item[] allItemArray;

    public Item GetItemWithId(int id)
    {
        if (id < 0 || id >= allItemArray.Length)
        {
            Debug.Log("ItemTable: id is out of bound (" + id + ")");
            return null;
        }
        return allItemArray[id];
    }

    public int GetIdWithItem(Item item)
    {
        int index = 0;
        while(index < allItemArray.Length)
        {
            if (allItemArray[index] == item)
            {
                return index;
            }
            index++;
        }
        Debug.Log("ItemTable: Can't find item with name " + item.nickName);
        return -1;
    }

    public Item GetRandomItem()
    {
        return allItemArray[Random.Range(0, allItemArray.Length)];
    }

    public Item GetItemWithName(string name)
    {
        foreach(var item in allItemArray)
        {
            if(item.nickName == name)
            {
                return item;
            }
        }
        return null;
    }
}
