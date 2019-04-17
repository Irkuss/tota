using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Wearable")]
public class Wearable : Item
{
    public enum BodyPart
    {
        Torso,
        Legs,
        Head
    }

    //Type de wearable
    public BodyPart bodyPart = BodyPart.Torso;
    //Attribut du wearable



    public override bool Use(GameObject refInventChara)
    {
        //Equip the wearable, process as follows:

        //if this wearable type can be equipped with bodyPart:
        //    equip this wearable to chara
        //    return true
        //return false;

        return false;
    }
}
