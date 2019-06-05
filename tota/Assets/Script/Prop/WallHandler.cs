using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHandler : PropHandler
{
    //Defining attribute
    [Header("Wall Attribute")]
    public int maxHitPoint = 200;
    public Item needItemToRepair = null;

    //Private attribute
    protected int _currentHitPoint;

    //Command enu
    public enum WallCommand
    {
        ModifyHP,
    }

    //Start
    private void Start()
    {
        //Call the Init for OrganicOpacity
        BeginOpacity();


        //Init the Private attributes
        _currentHitPoint = maxHitPoint;
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: Break(chara, actionIndex); break; //Break
            case 1: Repair(chara); break; //Repair
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return CanBreak(chara, 0); //Break
            case 1: return CanRepair(chara, 1); //Repair
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return GetBreakTime(chara); //Break
            case 1: return GetRepairTime(chara); //Repair
        }
        return 1f;
    }
    
    //====================Action Method====================
    //Break
    protected void Break(CharaHead chara, int actionIndex)
    {
        int damage = Mathf.FloorToInt(chara.GetComponent<CharaRpg>().GetStrengthModifier() * chara.GetComponent<CharaInventory>().GetMaxBreakDamage());
        
        //Simule la perte de pv pour savoir si on relance le Break
        if (_currentHitPoint - damage > 0)
        {
            //Relance le Break
            chara.SetFocus(this, actionIndex);
        }

        Debug.Log("Break: dealing " + damage);
        SendModifyHP(-damage);
    }

    protected bool CanBreak(CharaHead chara, int index)
    {
        _possibleActionNames[index] = "Break";
        Debug.Log("CanBreak: checking break with " + _currentHitPoint + "/" + maxHitPoint);
        if (_currentHitPoint < maxHitPoint)
        {
            Debug.Log("CanBreak: changing name");
            _possibleActionNames[index] += " (" + ((_currentHitPoint * 100f) / maxHitPoint) + "%)";
        }
        return true;
    }

    protected float GetBreakTime(CharaHead chara)
    {
        return 1f;
    }

    //Repair
    protected void Repair(CharaHead chara)
    {
        chara.GetComponent<CharaInventory>().Remove(needItemToRepair);

        SendModifyHP(maxHitPoint);
    }

    protected bool CanRepair(CharaHead chara, int index)
    {
        if (_currentHitPoint < maxHitPoint)
        {
            _makesActionNotAppearWhenUnavailable[index] = false;

            if(needItemToRepair == null)
            {
                Debug.LogWarning("CanRepair: wall do not require any items");
                return true;
            }

            return chara.GetComponent<CharaInventory>().Contains(needItemToRepair);
        }
        _makesActionNotAppearWhenUnavailable[index] = true;

        return false;
    }

    protected float GetRepairTime(CharaHead chara)
    {
        return 1f;
    }

    //ModifyHP (Used in Break and Repair)
    protected virtual void SendModifyHP(int modifier)
    {
        Debug.Log("SendModifyHP: sending modifier (" + modifier + ")");

        CommandSend(new int[2] { (int)WallCommand.ModifyHP, modifier });
    }

    protected void ModifyHP(int modifier)
    {
        Debug.Log("ModifyHP: modify hp by " + modifier);

        _currentHitPoint += modifier;
        
        if(_currentHitPoint <= 0)
        {
            DestroySelf();
        }
        else if(_currentHitPoint > maxHitPoint)
        {
            _currentHitPoint = maxHitPoint;
        }
    }

    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        //Debug.Log("CommandReceive: WallHandler");

        switch((WallCommand) command[0])
        {
            case WallCommand.ModifyHP: ModifyHP(command[1]); break;
        }
    }

}
