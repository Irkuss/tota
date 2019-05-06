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
            case 0: AttackWithSlot(chara, 0); break;//melee 0
            case 1: AttackWithSlot(chara, 1); break;//melee 1
            case 2: AttackWithSlot(chara, 0); break;//remote 0
            case 3: AttackWithSlot(chara, 1); break;//remote 1
            case 4: break; //Follow target
            case 5: GetComponent<CharaRpg>().AmputateEveryInfectedPart(); break; //Amputate infected parts
            case 6: GetComponent<CharaRpg>().TreatAllWoundsOfType(WoundInfo.WoundType.Bleeding); break; //Treat Bleeding
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
            case 4: return (chara != GetComponent<CharaHead>()); //Follow
            case 5: return
                    (GetComponent<CharaRpg>().IsInfected()
                    && (chara.GetComponent<CharaRpg>().GetCurrentStat(CharaRpg.Stat.sk_doctor) >= 2));//Amputate infected parts
            case 6: return GetComponent<CharaRpg>().HasWoundOfType(WoundInfo.WoundType.Bleeding); break; //Treat Bleeding
        }
        return false;
    }
    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        CharaRpg rpg = chara.GetComponent<CharaRpg>();

        switch (actionIndex)
        {
            case 0: return inv.equipments[0].attackSpeedModifier * rpg.GetTimeModifier(CharaRpg.Stat.ms_strength);//melee 0
            case 1: return inv.equipments[1].attackSpeedModifier * rpg.GetTimeModifier(CharaRpg.Stat.ms_strength);//melee 1
            case 2: return inv.equipments[0].attackSpeedModifier * rpg.GetTimeModifier(CharaRpg.Stat.sk_marksman);//remote 0
            case 3: return inv.equipments[1].attackSpeedModifier * rpg.GetTimeModifier(CharaRpg.Stat.sk_marksman);//remote 1
            case 4: return 0f; //Follow
            case 5: return 10 * rpg.GetTimeModifier(CharaRpg.Stat.sk_doctor); //Amputate infected parts
            case 6: return GetComponent<CharaRpg>().GetCountWoundsOfType(WoundInfo.WoundType.Bleeding) * 2 * rpg.GetTimeModifier(CharaRpg.Stat.sk_doctor); //Treat Bleeding
        }
        return 0f;
    }

    //Actions

    public void AttackWithSlot(CharaHead chara, int slot)//Get attacked
    {
        Equipable weapon = chara.GetComponent<CharaInventory>().equipments[slot]; 

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
        else
        {
            Debug.Log("AttackWithSlot: Unexpected null weapon, CheckAttackWithSlot failed!");
        }
    }
    public bool CheckAttackWithSlot(CharaHead chara, int slot, bool isMelee)
    {
        if (chara == GetComponent<CharaHead>()) return false;

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

                if (isMelee) return false;
                //Attack remote
                float maxRange = weapon.remoteMaxRange;

                //testing if in range of remote weapon
                if (Vector3.Distance(transform.position, chara.transform.position) < maxRange)
                {
                    RaycastHit hit;
                    //testing if visible from player
                    Vector3 higher = new Vector3(0, 1.5f, 0);
                    Debug.Log("CheckAttackWithSlot: checking victim sight with raycast");
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
