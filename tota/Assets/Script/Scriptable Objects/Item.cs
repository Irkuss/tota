using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName ="Inventory/Item")]
public class Item : ScriptableObject
{
    public string nickName = "Item Name";
    public Sprite icon = null;
    public int stack = 0;
    public int weight = 0;
    public bool usable = true;

    public virtual void Use(GameObject refInventChara)
    {
        if (refInventChara.GetComponent<CharaRpg>() != null)
        {
            refInventChara.GetComponentInParent<CharaRpg>().UseItem(); // On utilise l'item sur le bon chara par référence de son inventaire
        }
        else
        {
            if (SpiritHead.SelectedList.Count != 0)
            {
                SpiritHead.SelectedList[0].GetComponent<CharaInventory>().Add(this);
            }            
        }
    }

}
