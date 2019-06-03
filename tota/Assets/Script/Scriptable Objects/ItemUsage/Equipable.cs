using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Equipable")]
public class Equipable : Item
{
    public enum EquipType
    {
        Melee,
        Remote,
    }
    public enum DamageType
    {
        Sharp,
        Mace
    }
    public enum EquipSpace
    {
        OneHanded,
        TwoHanded
    }
    //Type d'arme
    [Header("Weapon attribute")]
    public EquipType equipType = EquipType.Melee;
    public DamageType dmgType = DamageType.Sharp;
    public EquipSpace equipSpace = EquipSpace.OneHanded;
    [Header("Common melee/remote attribute")]
    public int damage = 0;
    public int accuracyModifier = 0;
    public float attackSpeedModifier = 1f;
    //[Header("Melee attribute")]
    //Attribut au moment d'équipper l'arme
    //public int meleeRequiredStrength = 0;
    [Header("Remote attribute")]
    public float remoteFalloffRange = 0;
    public float remoteMaxRange = 0;
    [Header("Break attribute")]
    public float breakModifier = 1f;
    public float BreakDamage => breakModifier * damage;

    //Use (Equip)
    public override bool UseAsChara(CharaInventory charaInventory)
    {
        //Equip the weapon, process as follows:

        //if this weapon type can be equipped with equipSpace (one handed needs 1 invSpace, two handed needs 2):
        //    if this weapon is melee and chara does not have enough strenght ( check with meleeRequiredStrength)
        //        return false;
        //    equip this weapon to chara
        //    return true
        //return false;

        if(equipSpace == EquipSpace.OneHanded)
        {
            //Si on veut équiper une arme à une main
            if (charaInventory.equipments[0] != null)
            {
                //Si l'emplacement 0 est occupée
                if(charaInventory.equipments[0].equipSpace == EquipSpace.TwoHanded)
                {
                    //Si l'emplacement 0 est occupée par une arme à deux mains -> on tente de ranger l'item de l'emplacement 0
                    if (!charaInventory.Add(charaInventory.equipments[0])) return false;

                    charaInventory.equipments[1] = null;
                }
                else
                {
                    //Si l'emplacement 0 est occupée par une arme à une main
                    if (charaInventory.equipments[1] != null)
                    {
                        //Si l'emplacement 1 est occupée -> on tente de ranger l'item de l'emplacement 1
                        if (!charaInventory.Add(charaInventory.equipments[1])) return false;
                    }
                    //On décale l'item de l'emplacement 0 dans l'emplacement 1
                    charaInventory.equipments[1] = charaInventory.equipments[0];
                }
            }
            //On s'équipe dans l'emplacement 0
            charaInventory.equipments[0] = this;         
        }
        else
        {
            //Si on veut équiper une arme à deux mains
            if (charaInventory.equipments[0] != null)
            {
                //Range l'item de l'emplacement 0
                if (!charaInventory.Add(charaInventory.equipments[0])) return false;

                //Range l'item de l'emplacement 1 si c'est necessaire
                if (charaInventory.equipments[0].equipSpace == EquipSpace.OneHanded && charaInventory.equipments[1] != null)
                {
                    if (!charaInventory.Add(charaInventory.equipments[1])) return false;
                }
            }
            charaInventory.equipments[0] = this;
            charaInventory.equipments[1] = this;
        }

        GameObject _interface = charaInventory.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(charaInventory);

        return true;
    }
    public override bool Unequip(CharaInventory charaInventory)
    {
        //Unequip the weapon
        Debug.Log("Unequip: unequiping " + nickName + "(weapon), ");
        if (!charaInventory.Add(this)) return false;

        if(charaInventory.equipments[0] == this)
        {
            Debug.Log("Unequip: unequiping found in slot 0");
            if (equipSpace == EquipSpace.TwoHanded)
            {
                charaInventory.equipments[0] = null;
            }
            else
            {
                charaInventory.equipments[0] = charaInventory.equipments[1];
                
            }
            charaInventory.equipments[1] = null;
        }
        else
        {
            if (charaInventory.equipments[1] == this)
            {
                Debug.Log("Unequip: unequiping found in slot 1, unequiping");
                charaInventory.equipments[1] = null;
            }
            else
            {
                Debug.LogWarning("Unequip: didnt found weapon to unequip");
            }
        }

        /*
        for (int i = 0; i < charaInventory.equipments.Length; i++)
        {
            if (charaInventory.equipments[i] == this)
            {
                charaInventory.equipments[i] = null;
                if (equipSpace == EquipSpace.OneHanded) break;
            }
        }*/
        Debug.LogWarning("Unequip: ending unequip " + nickName);
        GameObject _interface = charaInventory.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(charaInventory);

        return true;
    }
}
