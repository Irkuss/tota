using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/RecipeTable")] 
public class RecipeTable : ScriptableObject
{
    public ItemRecipe[] recipes;
}
