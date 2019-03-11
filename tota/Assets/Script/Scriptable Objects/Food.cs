using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Food")]
public class Food : Item
{
    public int foodValue = 0;
    
    public override void Use(GameObject refInventChara)
    {
        Debug.Log("Using food:" + this.nickName);
        refInventChara.GetComponentInParent<CharaRpg>().Eat(foodValue); // On utilise l'item sur le bon chara par référence de son inventaire
    }
}
