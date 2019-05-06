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
    public int damage = 0;
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
    protected override bool UseAsChara(GameObject refInventChara)
    {
        //Equip the weapon, process as follows:

        //if this weapon type can be equipped with equipSpace (one handed needs 1 invSpace, two handed needs 2):
        //    if this weapon is melee and chara does not have enough strenght ( check with meleeRequiredStrength)
        //        return false;
        //    equip this weapon to chara
        //    return true
        //return false;

        refInventChara.GetComponent<CharaHead>().CallCoroutine(useTime);

        CharaInventory inv = refInventChara.GetComponent<CharaInventory>();

        if(equipSpace == EquipSpace.OneHanded)
        {
            if(inv.equipments[0] != null && inv.equipments[0].equipSpace == EquipSpace.TwoHanded)
            {
                inv.Add(inv.equipments[0]);

                inv.equipments[0] = this;
                inv.equipments[1] = null;
            }
            else
            {
                if (inv.equipments[0] != null && inv.equipments[1] == null)
                {
                    inv.equipments[1] = this;
                }
                else
                {
                    if (inv.equipments[1] != null)
                    {
                        inv.Add(inv.equipments[1]);
                    }
                    inv.equipments[1] = inv.equipments[0];
                    inv.equipments[0] = this;
                }
            }            
        }
        else
        {
            if (inv.equipments[0] != null)
            {
                if (inv.equipments[0].equipSpace == EquipSpace.TwoHanded)
                {
                    inv.Add(inv.equipments[0]);
                    inv.equipments[0] = this;
                    inv.equipments[1] = this;
                }
                else
                {
                    if (inv.equipments[1] != null)
                    {
                        inv.Add(inv.equipments[1]);
                    }
                    inv.equipments[1] = this;
                    inv.equipments[0] = this;
                }
            }
            else
            {
                inv.equipments[0] = this;
                inv.equipments[1] = this;
            }
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
        if (!inv.Add(this)) return false;

        for (int i = 0; i < inv.equipments.Length; i++)
        {
            if (inv.equipments[i] == this)
            {
                inv.equipments[i] = null;
            }
        }

        GameObject _interface = inv.GetInterface();
        if (_interface == null) return false;
        _interface.GetComponent<InterfaceManager>().UpdateEquipment(inv);

        return true;
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
