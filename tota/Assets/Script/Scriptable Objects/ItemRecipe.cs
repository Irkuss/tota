﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Recipe",menuName = "Recipe")]
public class ItemRecipe : ScriptableObject
{
    [Header("Recipe result (if it has one)")]
    //Item donné par le craft
    public Item result = null;
    public int resultCount;

    [Header("Recipe path (if it has one)")]
    public string resultPath = "";

    [Header("Recipe needed items")]
    //A chaque index des deux arrays se trouvent un Item requis et la quantité de cet Item requise
    //NB: les dictionnary marchent pas avec les Scriptables Objects du coup on bidouille deso
    public Item[] neededItem;
    public int[] neededItemCount;
    [Header("Base recipe time")]
    public float recipeTime = 0;

    public RecipeTable.RecipeType type = RecipeTable.RecipeType.Base;

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

    public void RemoveNeededItems(CharaInventory charaInventory)
    {
        //Used in CraftWith but also when building from a blueprint
        for (int i = 0; i < neededItem.Length; i++)
        {
            //Diminue la quantité (ne devrait pas passé en dessous
            charaInventory.ModifyCount(neededItem[i], -neededItemCount[i]);
        }
    }

    public void CraftWith(CharaInventory charaInventory)
    {
        //Warning, should only be used after using CanBeCraftedWith
        RemoveNeededItems(charaInventory);

        for (int j = 0; j < resultCount; j++)
        {
            charaInventory.Add(result);
        }
    }

    public void Refund(CharaInventory charaInventory)
    {
        for (int i = 0; i < neededItem.Length; i++)
        {
            Item item = neededItem[i];
            for (int count = 0; count < neededItemCount[i]; count++)
            {
                charaInventory.Add(item);
            }
        }
    }
}
