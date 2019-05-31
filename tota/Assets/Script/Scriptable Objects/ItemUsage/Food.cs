using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Food")]
public class Food : Item
{
    [Header("Food Attribute")]
    public int foodValue = 0;
    
    public override bool UseAsChara(CharaInventory charaInventory)
    {
        Debug.Log("Using food: " + this.nickName + " with foodValue " + foodValue);
        return charaInventory.GetComponent<CharaRpg>().Eat(foodValue);
    }
}
