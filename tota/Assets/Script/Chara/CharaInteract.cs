using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaInteract : Interactable
{
    
    public override void Interact(CharaHead chara, int actionIndex = 0)
    {
        //TO CHANGE WHEN MULTIPLE INTERACT ARE POSSIBLE
        
        switch (actionIndex)
        {
            case 0: AttackWithSlot1(chara); break;
            case 1: AttackWithSlot2(chara); break;
            default: GetComponent<CharaRpg>().DebugGetRandomDamage((int)CharaRpg.WoundType.Bruise); break;
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return CheckAttackWithSlot1(chara);
            case 1: return CheckAttackWithSlot2(chara);
        }
        return false;
    }

    //Actions

    //Attack with slot 1
    public void AttackWithSlot1(CharaHead chara)//Get attacked
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        Equipable weapon = inv.equipments[0];
        if (weapon != null)
        {
            GetComponent<CharaRpg>().GetAttackedWith(weapon, weapon.damage);
        }
    }
    public bool CheckAttackWithSlot1(CharaHead chara)
    {
        return chara.GetComponent<CharaInventory>().equipments[0] != null;
    }

    //Attack with slot 2
    public void AttackWithSlot2(CharaHead chara)//Get attacked
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        Equipable weapon = inv.equipments[1];
        if(weapon != null)
        {
            GetComponent<CharaRpg>().GetAttackedWith(weapon, weapon.damage);
        }
    }
    public bool CheckAttackWithSlot2(CharaHead chara)
    {
        return chara.GetComponent<CharaInventory>().equipments[1] != null;
    }
}
