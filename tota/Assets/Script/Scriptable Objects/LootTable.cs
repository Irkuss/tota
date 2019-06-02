using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Generation/LootTable")]
public class LootTable : ScriptableObject
{
    //Item count
    [Header("Abundance")]
    public int minItemCount = 8;
    public int maxItemCount = 12;
    //List of every possible item
    [Header("List of every possible item and their chances")]
    public Item[] possibleItem = null;

    //Liste des chances sur 100 de chaque item d'etre renvoyé
    //NB: si la somme des chances est inférieur à 100, il est possible que rien ne soit renvoyé
    public int[] randomChances = null;

    private Item GetChosenProp()
    {
        //Jet d'un D100 (dé à 100 faces)
        int rng = Random.Range(0, 100);
        //Choix du prop
        for (int i = 0; i < randomChances.Length; i++)
        {
            rng = rng - randomChances[i];

            if (rng < 0)
            {
                //Un Prop a été choisi
                return possibleItem[i];
            }
        }
        //choix: ne rien faire apparaître
        return null;
    }

    public Item[] GetChosenPropsArray()
    {
        int length = Random.Range(minItemCount, maxItemCount);

        Item[] props = new Item[length];
        for (int i = 0; i < length; i++)
        {
            props[i] = GetChosenProp();
        }
        return props;
    }

    public Item[] DebugGetAllPossibleItems()
    {
        return possibleItem;
    }
}
