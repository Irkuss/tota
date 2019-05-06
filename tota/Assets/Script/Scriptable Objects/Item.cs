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
    [Header("General attribute")]
    public string nickName = "Item Name"; //Nom de l'item
    public string description = "Item description here";
    public Sprite icon = null; //Icon de l'item
    public int stack = 1; //Nombre d'exemplaire maximum dans un slot de l'inventaire
    public int weight = 0; //Poids dans l'inventaire
    public int baseMarketValue = 0; //Valeur commerciale

    [Header("Interact attribute")]
    public bool usable = false; //Peut-il etre utilisé?
    public float useTime = 0;

    //Usage public
    
    public bool Use(GameObject refInventChara)
    {
        //Clique Handler
        //dans un inventaire de furniture
        if (refInventChara.GetComponent<CharaRpg>() == null)
        {
            if (SpiritHead.SelectedList.Count != 0)
            {
                return SpiritHead.SelectedList[0].GetComponent<CharaInventory>().Add(this);
            }
            return false;
        }
        //dans un inventaire de chara
        //si on est en train de shift clic
        if(Input.GetKey(KeyCode.LeftShift))
        {
            CharaHead head = refInventChara.GetComponent<CharaHead>();
            if(head != null)
            {
                //On essaie d'ajouter l'item à un inventaire de furniture
                if(head.TryAddItemToFurniture(this))
                {
                    //Si on réussit, on enleve cet item de l'inventaire du chara
                    return true;
                }
            }
            return false;
        }
        //clic normal
        if (!usable) return false;
        //Utilisation de l'item
        return UseAsChara(refInventChara);
    }

    protected virtual bool UseAsChara(GameObject refInventChara)
    {
        /* Use:
         * Prend en paramètre le gameObject qui l'utilise (un chara normalement)
         * -doit etre implémenté par les classes filles de Item (ex: Food, Equipable)
         * -Renvoie si l'item a pu être utilisé (certains Items sont inutilisable dans certains cas, ex: equipable)
         * */
        return false;
    }

    public virtual bool Unequip(GameObject refInventChara)
    {
        //Only overriden by Equipable and Wearable
        return false;
    }
}
