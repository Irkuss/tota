using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaInteract : Interactable
{
    
    public override void Interact(CharaHead chara, int actionIndex = 0)
    {
        //TO CHANGE WHEN MULTIPLE INTERACT ARE POSSIBLE
        GetComponent<CharaRpg>().DebugGetRandomDamage((int)CharaRpg.WoundType.Bruise);
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        return true; //TODO
    }

    //Actions

    public void Attack1(CharaHead chara)//Get attacked
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        Equipable weapon = inv.equipments[0];
        if (weapon != null)
        {
            GetComponent<CharaRpg>().GetAttackedWith(weapon, weapon.damage);
        }
    }

    public void Attack2(CharaHead chara)//Get attacked
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        Equipable weapon = inv.equipments[1];
        if(weapon != null)
        {
            GetComponent<CharaRpg>().GetAttackedWith(weapon, weapon.damage);
        }
    }
}
