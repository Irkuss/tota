using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WoundInfo", menuName = "Health/WoundInfo")]
public class WoundInfo : ScriptableObject
{
    
    public enum WoundType
    {
        Fracture,  //treated by a split
        Bruise, //treated by cream1
        Burn,   //treated by cream2
        FrostBite,//treated by FIRE
        Bleeding, //treated by bandage
        DeathBite, //treated by amputation
    }
    [Header("Main Wound Info")]
    public string nickName = "wound name";
    public WoundType type = WoundType.Fracture;
    [Header("Bleeding")]
    public bool makesBleed = false;
    [Header("Pain")]
    public float painFactor = 1f;

    [Header("Treatment")]
    public Item treatment = null;
    public bool hasToBeOperated = false;
}
