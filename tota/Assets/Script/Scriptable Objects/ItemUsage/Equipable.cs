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
    public enum SpecificType //Used with the trait system
    {
        Unspecific,
        Bow,
        Blade,
        Fist,
    }
    //Type d'arme
    [Header("Weapon attribute")]
    public EquipType equipType = EquipType.Melee;
    public DamageType dmgType = DamageType.Sharp;
    public EquipSpace equipSpace = EquipSpace.OneHanded;
    public SpecificType specificType = SpecificType.Unspecific;
    [Header("Common melee/remote attribute")]
    public float damage = 0;
    public int accuracyModifier = 0;
    public float attackSpeedModifier = 1f;
    [Header("Melee attribute")]
    //Attribut au moment d'équipper l'arme
    public int meleeRequiredStrength = 0;
    //Atrribut de l'arme
    public float meleeRange = 0;
    [Header("Remote attribute")]
    public float remoteFalloffRange = 0;
    public float remoteMaxRange = 0;
    public bool useAmmo = false;
    public Item ammo = null;

    //Use (Equip)
    public override bool Use(GameObject refInventChara)
    {
        //Equip the weapon, process as follows:

        //if this weapon type can be equipped with equipSpace (one handed needs 1 invSpace, two handed needs 2):
        //    if this weapon is melee and chara does not have enough strenght ( check with meleeRequiredStrength)
        //        return false;
        //    equip this weapon to chara
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
    //Should be check before attacking
    public bool CanAttack(CharaInventory inv)
    {
        if(useAmmo && ammo != null)
        {
            return inv.inventory.ContainsKey(ammo);
        }
        return true;
    }
}
