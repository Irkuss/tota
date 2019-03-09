using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName ="Inventory/Item")]
public class Item : ScriptableObject
{
    public string nickName = "Item Name";
    public Sprite icon = null;
    public int stack = 0;

    /*public virtual void Use()
    {
        //USE THE ITEM
    }*/

}
