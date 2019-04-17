using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Food")]
public class Food : Item
{
    public int foodValue = 0;
    
    public override bool Use(GameObject refInventChara)
    {

        //Temporaire
        if (refInventChara.GetComponent<CharaRpg>() != null)
        {
            //Dans le chara o
            Debug.Log("Using food:" + this.nickName);
            refInventChara.GetComponentInParent<CharaRpg>().Eat(foodValue); // On utilise l'item sur le bon chara par référence de son inventaire
            return true;
        }
        //dafuq is this
        else
        {
            if (SpiritHead.SelectedList.Count != 0)
            {
                SpiritHead.SelectedList[0].GetComponent<CharaInventory>().Add(this);
                return true;
            }
        }
        return false;
    }
}
