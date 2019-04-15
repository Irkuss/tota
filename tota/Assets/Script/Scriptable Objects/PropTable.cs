using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prop/PropTable")]
public class PropTable : ScriptableObject
{
    public enum PropType
    {
        Undecided,
        Furniture,
        Tree,
        Rock
    }

    public Prop[] allPropArray;

    //Getters
    public Prop GetPropWithId(int id)
    {
        if (id < 0 || id >= allPropArray.Length)
        {
            Debug.Log("ItemTable: id is out of bound (" + id + ")");
            return null;
        }
        return allPropArray[id];
    }
    public int GetIdWithProp(Prop prop)
    {
        for (int i = 0; i < allPropArray.Length; i++)
        {
            if (allPropArray[i] == prop)
            {
                return i;
            }
        }
        return -1;
    }
    public int GetIdWithName(string name)
    {
        for (int i = 0; i < allPropArray.Length; i++)
        {
            if (allPropArray[i].nickName == name)
            {
                return i;
            }
        }
        return -1;
    }
    public int GetIdWithPath(string path)
    {
        for (int i = 0; i < allPropArray.Length; i++)
        {
            if (allPropArray[i].path == path)
            {
                return i;
            }
        }
        return -1;
    }
    public Prop GetRandomPropWithType(PropType type)
    {
        List<Prop> propsWithType = new List<Prop>();
        foreach(Prop prop in allPropArray)
        {
            if (prop.propType == type)
            {
                propsWithType.Add(prop);
            }
        }
        return propsWithType[Random.Range(0, propsWithType.Count)];
    }
}
