﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Wearable")]
public class Wearable : Item
{
    public enum BodyInvSpace
    {
        Head,
        Torso,
        Leg
    }
    //peut etre rajouté une reference au model 3d (voir avec marc?)
    [Header("Wearable attribute")]
    //Type de wearable
    public BodyInvSpace inventorySpotTaken = BodyInvSpace.Torso;
    public CharaRpg.BodyType[] zoneProtected = null;
    //Attribut du wearable
    public int sharpResistance = 0;
    public int maceResistance = 0;
    public int maxTempModifier = 0;

    //Use (Equip)
    public override bool Use(GameObject refInventChara)
    {
        //Equip the wearable, process as follows:

        //if this wearable type can be equipped with bodyPart:
        //    equip this wearable to chara
        //    return true
        //return false;

        return false;
    }
    public override bool Unequip(GameObject refInventChara)
    {
        //Unequip the weapon, process as follows:

        //if inventory has enough space (1)
        //    return true
        //return false
        return false;
    }
}
