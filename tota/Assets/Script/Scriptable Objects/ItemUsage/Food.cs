using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Food")]
public class Food : Item
{
    public int foodValue = 0;
    
    protected override bool UseAsChara(GameObject refInventChara)
    {
        Debug.Log("Using food:" + this.nickName);
        return refInventChara.GetComponentInParent<CharaRpg>().Eat(foodValue);
    }
}
