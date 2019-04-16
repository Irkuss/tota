using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Recipe",menuName = "Recipe")]
public class ItemRecipe : ScriptableObject
{
    //Item donné par le craft
    public Item result;
    public int resultCount;

    //A chaque index des deux arrays se trouvent un Item requis et la quantité de cet Item requise
    //NB: les dictionnary marchent pas avec les Scriptables Objects du coup on bidouille deso
    public Item[] neededItem;
    public int[] neededItemCount;

    public string type;

    public bool CanBeCraftedWith(Dictionary<Item, int> inventory)
    {
        for (int i = 0; i < neededItem.Length; i++)
        {
            if (inventory.ContainsKey(neededItem[i]))
            {
                if (inventory[neededItem[i]] < neededItemCount[i])
                {
                    //S'il n'y a pas assez d'exemplaire
                    return false;
                }
            }
            else
            {
                //S'il n'y a pas l'Item
                return false;
            }
        }
        return true;
    }

    public void CraftWith(CharaInventory charaInventory)
    {
        //Warning, should only be used after using CanBeCraftedWith
        for (int i = 0; i < neededItem.Length; i++)
        {
            //Diminue la quantité (ne devrait pas passé en dessous
            charaInventory.ModifyCount(neededItem[i], -neededItemCount[i]);
        }

        for (int j = 0; j < resultCount; j++)
        {
            charaInventory.Add(result);
        }
    }
}
