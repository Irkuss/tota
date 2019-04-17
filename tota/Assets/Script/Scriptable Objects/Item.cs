using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName ="Inventory/Item/OriginalItem")]
public class Item : ScriptableObject
{
    /*
    public enum ItemUsage
    {
        Unusable,
        Food,
        InfinitUsable,
        Consumable,
        Equipable,
        Wearable,
    }*/

    public string nickName = "Item Name"; //Nom de l'item
    public Sprite icon = null; //Icon de l'item
    public int stack = 1; //Nombre d'exemplaire maximum dans un slot de l'inventaire
    public int weight = 0; //Poids dans l'inventaire
    public int baseMarketValue = 0; //Valeur commerciale
    public bool usable = false; //Peut-il etre utilisé?
    //Usage public
    
    public virtual bool Use(GameObject refInventChara)
    {
        /* Use:
         * Prend en paramètre le gameObject qui l'utilise (un chara normalement)
         * -doit etre implémenté par les classes filles de Item (ex: Food, Equipable)
         * -Renvoie si l'item a pu être utilisé (certains Items sont inutilisable dans certains cas, ex: equipable)
         * */

        //Temporaire
        if (refInventChara.GetComponent<CharaRpg>() != null)
        {
            //Dans le cas ou on est cliqué dans un inventaire de Chara
            refInventChara.GetComponentInParent<CharaRpg>().UseItem(); // On utilise l'item sur le bon chara par référence de son inventaire
        }
        else
        {
            //Dans le cas ou on est cliqué dans un inventaire de meuble
            if (SpiritHead.SelectedList.Count != 0)
            {
                SpiritHead.SelectedList[0].GetComponent<CharaInventory>().Add(this);
            }            
        }
        return true;
    }

}
