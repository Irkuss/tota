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
        refInventChara.GetComponent<CharaHead>().CallCoroutine(useTime);
        return refInventChara.GetComponentInParent<CharaRpg>().Eat(foodValue);
    }
}
