using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item/Equipable")]
public class Equipable : Item
{
    public enum EquipType
    {
        MeleeSharp,
        MeleeMace,
        RemoteGun
    }
    public enum EquipSpace
    {
        OneHanded,
        TwoHanded
    }
    public enum SpecificType //Used with the trait system
    {
        Unspecific,
        Bow,
        Blade,
    }
    //Type d'arme
    public EquipType equipType = EquipType.MeleeSharp;
    public EquipSpace equipSpace = EquipSpace.OneHanded;
    public SpecificType specificType = SpecificType.Unspecific;
    //Attribut au moment d'équipper l'arme
    public int meleeRequiredStrength = 0;
    //Atrribut de l'arme
    public float meleeRange = 0;
    public int meleeDamage = 0;
    public int meleeAccuracyModifier = 0;
    public float remoteRange = 0;
    public int remoteDamage = 0;
    public int remoteAccuracyModifier = 0;

    public override bool Use(GameObject refInventChara)
    {
        //Equip the weapon, process as follows:

        //if this weapon type can be equipped with equipSpace:
        //    if this weapon is melee and chara does not have enough strenght ( check with meleeRequiredStrength)
        //        return false;
        //    equip this weapon to chara
        //    return true
        //return false;
        return false;
    }
}
