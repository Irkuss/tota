using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Food")]
public class Food : Item
{
    public int foodValue = 0;

    public override void Use()
    {
        Debug.Log("Using food:" + this.nickName);
    }
}
