﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName ="Inventory/Item")]
public class Item : ScriptableObject
{
    new public string name = "Item Name";
    public Sprite icon = null;
    public int stack;
    public bool isDefaultItem = false;

    /*public virtual void Use()
    {
        //USE THE ITEM
    }*/

}
