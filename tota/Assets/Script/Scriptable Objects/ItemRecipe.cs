using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Recipe", menuName = "Recipe")]
public class ItemRecipe : ScriptableObject
{
    [Header("Visu attributes")]
    public string visuPath = "Visu/";
    public Sprite visuSprite = null;

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
    [Header("Recipe needed skill")]
    public bool useOnlyManipulation = true;
    public CharaRpg.Stat statUsed = CharaRpg.Stat.sk_carpenter;
    public int neededStatLevel = 0;
    public int maxStatLevelBeforeNotGivingXp = 10;
    [Header("Recipe needed workshop")]
    public bool recipeNeedWorkshop = false;
    public WorkshopProp.WorkshopType neededWorkshop = WorkshopProp.WorkshopType.Undecided;
    [Header("Base recipe time")]
    public float baseRecipeTime = 0;
    
    //Condition
    public bool CanBeCraftedBy(CharaInventory chara)
    {
        return CanBeCraftedBySkill(chara) && CanBeCraftedByWorkshop(chara) && CanBeCraftedWith(chara.inventory);
    }

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
    public bool CanBeCraftedBySkill(CharaInventory chara)
    {
        if (useOnlyManipulation) return true; //Si l'item n'utilise aucun stat en particulier

        CharaRpg rpg = chara.GetComponent<CharaRpg>();

        return rpg.GetCurrentStat(statUsed) >= neededStatLevel; //retourne si la stat requise est suffisante au niveau requis
    }
    public bool CanBeCraftedByWorkshop(CharaInventory chara)
    {
        if (!recipeNeedWorkshop) return true; //Si on a pas besoin de workshop

        Interactable focus = chara.GetComponent<CharaHead>().LastInteractedFocus;
        if (focus == null) return false;

        WorkshopProp workShop = focus.GetComponent<WorkshopProp>();

        if(workShop != null) //Si le chara interragis avec un workshop
        {
            if(workShop.workType == neededWorkshop) //Si le Workshop correspond au workshop requis
            {
                return true;
            }
        }
        return false;
    }
    //Wait time
    public float GetCraftTime(CharaInventory chara)
    {
        CharaRpg rpg = chara.GetComponent<CharaRpg>();

        float charaModifier = 1f;
        //Modification par le skill requis (NB: toujours >= 1)
        if(!useOnlyManipulation)
        {
            charaModifier = charaModifier * rpg.GetTimeModifier(statUsed);
        }
        //Modification par la manipulation (NB: toujours <= 1)
        charaModifier = charaModifier * rpg.Manipulation;
        //Modification par le temps de base (NB: toujours >= 0)
        return baseRecipeTime * charaModifier;
    }

    //Crafing
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

    //Refunding (used in blueprint cancelling)
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
