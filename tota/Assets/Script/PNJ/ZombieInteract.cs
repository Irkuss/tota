using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieInteract : Interactable
{
    //Override
    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch (actionIndex)
        {
            case 0: AttackWithSlot(chara, 0); break;//melee 0
            case 1: AttackWithSlot(chara, 1); break;//melee 1
            case 2: AttackWithSlot(chara, 0); break;//remote 0
            case 3: AttackWithSlot(chara, 1); break;//remote 1
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
        }
        return 0f;
    }

    public void AttackWithSlot(CharaHead chara, int slot)//Get attacked
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();
        Equipable weapon = inv.equipments[slot];

        Debug.Log("AttackWithSlot: attacking Zombie with slot " + slot + " as weapon: " + weapon.nickName);

        if (weapon != null)
        {
            if (weapon.equipType == Equipable.EquipType.Melee)
            {
                //Attack melee
                CharaRpg rpg = chara.GetComponent<CharaRpg>();

                if (rpg.GetCheck(CharaRpg.Stat.ms_strength))
                {
                    GetComponent<Zombie>().GetAttackedWith(weapon.damage);
                }
            }
            else
            {
                float distance = Vector3.Distance(transform.position, chara.transform.position);

                int damage =
                    distance <= weapon.remoteFalloffRange
                    ? weapon.damage
                    : Mathf.FloorToInt((weapon.damage * (distance - weapon.remoteMaxRange)) / (weapon.remoteFalloffRange - weapon.remoteMaxRange));

                //Attack remote
                GetComponent<Zombie>().GetAttackedWith(damage);
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
                if (isMelee) return false;
                //Attack remote
                float maxRange = weapon.remoteMaxRange;

                //testing if in range of remote weapon
                if (Vector3.Distance(transform.position, chara.transform.position) < maxRange)
                {
                    RaycastHit hit;
                    //testing if visible from player
                    Vector3 higher = new Vector3(0, 1.5f, 0);
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
