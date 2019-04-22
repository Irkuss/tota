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
    public override bool Use(GameObject refInventChara)
    {
        //Equip the wearable, process as follows:

        //if this wearable type can be equipped with bodyPart:
        //    equip this wearable to chara
        //    return true
        //return false;

        CharaInventory inv = refInventChara.GetComponent<CharaInventory>();
        switch (inventorySpotTaken)
        {
            case BodyInvSpace.Head:
                if (inv.wearables[0] != null)
                {
                    inv.Add(inv.wearables[0]);
                }
                inv.wearables[0] = this;
                break;
            case BodyInvSpace.Torso:
                if (inv.wearables[1] != null && inv.wearables[2] == null)
                {
                    inv.wearables[2] = this;
                }
                else
                {
                    if (inv.wearables[2] != null)
                    {
                        inv.Add(inv.wearables[2]);
                    }
                    inv.wearables[2] = inv.wearables[1];
                    inv.wearables[1] = this;
                }
                break;
            case BodyInvSpace.Leg:
                if (inv.wearables[3] != null)
                {
                    inv.Add(inv.wearables[3]);
                }
                inv.wearables[3] = this;
                break;
        }

        GameObject _interface = inv.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(inv);

        return true;
    }
    public override bool Unequip(GameObject refInventChara)
    {
        //Unequip the weapon, process as follows:

        //if inventory has enough space (1)
        //    return true
        //return false

        CharaInventory inv = refInventChara.GetComponent<CharaInventory>();
        for(int i = 0; i < inv.wearables.Length; i++)
        {
            if (inv.wearables[i] == this)
            {
                inv.wearables[i] = null;
            }
        }

        GameObject _interface = inv.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(inv);

        return inv.Add(this);
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
