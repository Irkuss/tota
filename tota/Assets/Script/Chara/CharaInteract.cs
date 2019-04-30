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
            case 0: break;
            case 1: AttackWithSlot(chara, 0); break;//melee 0
            case 2: AttackWithSlot(chara, 1); break;//melee 1
            case 3: AttackWithSlot(chara, 0); break;//remote 0
            case 4: AttackWithSlot(chara, 1); break;//remote 1
            case 5: break; //Follow target
            default: GetComponent<CharaRpg>().DebugGetRandomDamage((int)CharaRpg.WoundType.Bruise); break;
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return CheckAttackWithSlot(chara, 0, true);//melee 0
            case 1: return CheckAttackWithSlot(chara, 1, true);//melee 1
            case 2: return CheckAttackWithSlot(chara, 0, false);//remote 0
            case 3: return CheckAttackWithSlot(chara, 1, false);//remote 1
            case 4: return true; //Follow
        }
        return false;
    }

    //Actions

    public void AttackWithSlot(CharaHead chara, int slot)//Get attacked
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        Equipable weapon = inv.equipments[slot];        

        if (weapon != null)
        {
            Debug.Log("AttackWithSlot: attacking with slot " + slot + " as weapon: " + weapon.nickName);

            if (weapon.equipType == Equipable.EquipType.Melee)
            {
                //Attack melee
                CharaRpg rpg = chara.GetComponent<CharaRpg>();

                if (true)//rpg.GetCheck(CharaRpg.Stat.ms_strength))
                {
                    GetComponent<CharaRpg>().GetAttackedWith(weapon, weapon.damage);
                }
            }
            else
            {
                float distance = Vector3.Distance(transform.position, chara.transform.position);

                int damage = 
                    distance <= weapon.remoteFalloffRange 
                    ? weapon.damage 
                    : Mathf.FloorToInt(  ( weapon.damage * (distance - weapon.remoteMaxRange) )  /  (weapon.remoteFalloffRange-weapon.remoteMaxRange)  );
                
                //Attack remote
                GetComponent<CharaRpg>().GetAttackedWith(weapon, damage);
            }
        }
    }
    public bool CheckAttackWithSlot(CharaHead chara, int slot, bool isMelee)
    {
        Equipable weapon = chara.GetComponent<CharaInventory>().equipments[slot];
        if (weapon != null)
        {
            if (weapon.equipType == Equipable.EquipType.Melee)
            {
                //Attack melee
                return isMelee;
            }
            else
            {
                //Attack remote
                float maxRange = weapon.remoteMaxRange;

                //testing if in range of remote weapon
                if (Vector3.Distance(transform.position, chara.transform.position) < maxRange)
                {
                    RaycastHit hit;
                    //testing if visible from player
                    Vector3 higher = new Vector3(0, 1.5f, 0);
                    Debug.Log("DRAWING RAYCAST");
                    Debug.DrawRay(transform.position + higher, (chara.transform.position - transform.position) + higher, Color.green, 3, false);
                    if (Physics.Raycast(transform.position + higher, (chara.transform.position - transform.position) + higher, out hit, maxRange))
                    {
                        if (hit.transform.gameObject == chara.gameObject)
                        {
                            //Visible and in range
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
