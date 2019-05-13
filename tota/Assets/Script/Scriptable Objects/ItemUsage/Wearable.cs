using System.Collections;
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
    public int minTempModifier = 0;

    //Use (Equip)
    public override bool UseAsChara(CharaInventory charaInventory)
    {
        //Equip the wearable, process as follows:

        //if this wearable type can be equipped with bodyPart:
        //    equip this wearable to chara
        //    return true
        //return false;
        
        switch (inventorySpotTaken)
        {
            case BodyInvSpace.Head:
                if (charaInventory.wearables[0] != null)
                {
                    charaInventory.Add(charaInventory.wearables[0]);
                }
                charaInventory.wearables[0] = this;
                break;
            case BodyInvSpace.Torso:
                if (charaInventory.wearables[1] != null && charaInventory.wearables[2] == null)
                {
                    charaInventory.wearables[2] = this;
                }
                else
                {
                    if (charaInventory.wearables[2] != null)
                    {
                        charaInventory.Add(charaInventory.wearables[2]);
                    }
                    charaInventory.wearables[2] = charaInventory.wearables[1];
                    charaInventory.wearables[1] = this;
                }
                break;
            case BodyInvSpace.Leg:
                if (charaInventory.wearables[3] != null)
                {
                    charaInventory.Add(charaInventory.wearables[3]);
                }
                charaInventory.wearables[3] = this;
                break;
        }

        GameObject _interface = charaInventory.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(charaInventory);

        return true;
    }
    public override bool Unequip(CharaInventory charaInventory)
    {
        //Unequip the wearable
        Debug.Log("Unequip: unequiping " + nickName + "(wearable), ");
        if (!charaInventory.Add(this)) return false;

        for (int i = 0; i < charaInventory.wearables.Length; i++)
        {
            if (charaInventory.wearables[i] == this)
            {
                charaInventory.wearables[i] = null;
                break;
            }
        }
        Debug.Log("Unequip: ending unequip " + nickName);
        GameObject _interface = charaInventory.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(charaInventory);

        return true;
    }

    //Getters
    public bool ContainsBodyType(CharaRpg.BodyType type)
    {
        foreach(CharaRpg.BodyType bodyType in zoneProtected)
        {
            if(bodyType == type)
            {
                return true;
            }
        }
        return false;
    }
}
