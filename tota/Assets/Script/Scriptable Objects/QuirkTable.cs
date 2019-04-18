using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuirkTable", menuName = "Quirk/QuirkTable")]
public class QuirkTable : ScriptableObject
{
    public Quirk[] quirks;

    //Random getters
    public Quirk GetRandomQuirk()
    {
        return quirks[Random.Range(0, quirks.Length)];
    }
    public Quirk GetRandomQuirkOfType(Quirk.QuirkType type)
    {
        //Init la list des quirks d'un certain type
        List<Quirk> quirksOfType = new List<Quirk>();
        foreach(Quirk quirk in quirks)
        {
            if(quirk.type == type)
            {
                quirksOfType.Add(quirk);
            }
        }
        if (quirksOfType.Count == 0) return null;
        return quirksOfType[Random.Range(0, quirksOfType.Count)];
    }
    public void GetRandomQuirksOfType(Quirk.QuirkType type, int numberOfQuirks, List<Quirk> listToFill)
    {
        //Init la list des quirks d'un certain type
        List<Quirk> quirksOfType = new List<Quirk>();
        foreach (Quirk quirk in quirks)
        {
            if (quirk.type == type)
            {
                quirksOfType.Add(quirk);
            }
        }
        //Ajoute numberOfQuirks à listToFill du type spécifié
        Quirk quirkToAdd;
        for (int i = 0; i < numberOfQuirks; i++)
        {
            if (quirksOfType.Count == 0) break;
            quirkToAdd = quirksOfType[Random.Range(0, quirksOfType.Count)];
            if(quirkToAdd.IsCompatibleWith(listToFill))
            {
                listToFill.Add(quirkToAdd);
            }
            else
            {
                i--;
            }
            quirksOfType.Remove(quirkToAdd);
        }
    }
    public Quirk GetRandomQuirkDifferentFrom(List<Quirk> thoseQuirks)
    {
        List<Quirk> validQuirks = new List<Quirk>();
        foreach(Quirk quirk in quirks)
        {
            if (!thoseQuirks.Contains(quirk))
            {
                validQuirks.Add(quirk);
            }
        }
        if (validQuirks.Count == 0) return null;

        return validQuirks[Random.Range(0, validQuirks.Count)];
    }
    //Id system
    public int QuirkToId(Quirk quirk)
    {
        for (int i = 0; i < quirks.Length; i++)
        {
            if (quirks[i] == quirk)
            {
                return i;
            }
        }
        return -1;
    }
    public Quirk IdToQuirk(int id)
    {
        if (id < 0 || id >= quirks.Length) return null;
        return quirks[id];
    }

    
}
