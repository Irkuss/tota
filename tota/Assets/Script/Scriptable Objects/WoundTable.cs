using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WoundTable", menuName = "Health/WoundTable")]
public class WoundTable : ScriptableObject
{
    public WoundInfo[] woundInfos = null;


    public WoundInfo GetInfo(WoundInfo.WoundType speType)
    {
        foreach(WoundInfo info in woundInfos)
        {
            if(info.type == speType)
            {
                return info;
            }
        }
        Debug.Log("WoundTable: GetInfo: Error: unexpected woundType");
        return null;
    }
}
