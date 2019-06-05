using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedHandler : SalvageHandler
{
    //Reference
    private Outline _outline;

    //Private attribute
    private bool _isBeingUsed = false;
    private CharaHead _charaUsing = null;
    private IEnumerator _currentCloseUpdate = null;

    //Command enum
    public enum BedCommand
    {
        Usage
    }

    //Start
    private void Start()
    {
        //Call the Init for OrganicOpacity
        BeginOpacity();

        //Set the references
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch (actionIndex)
        {
            case 0: StartUsage(chara); break; //Use Bed
            case 1: Salvage(chara); break;//Salvage
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return !_isBeingUsed; //Use Bed
            case 1: return true; //Salvage
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return 0; //Use Workshop
            case 1: return GetSalvageTime(chara); //Salvage
        }
        return base.GetActionTime(chara, actionIndex);
    }

    //====================Action Method====================
    //Use Bed
    private void StartUsage(CharaHead chara)
    {
        if (!CheckAvailability(chara, 0)) return; //Si au moment d'arriver, le lit est finalement utilisé annule tout

        SendToggleUsage(true);
        _charaUsing = chara;

        _currentCloseUpdate = Cor_UpdateClose();
        StartCoroutine(_currentCloseUpdate);
    }
    private IEnumerator Cor_UpdateClose()
    {
        //_outline.enabled = true;

        while (_charaUsing != null)
        {
            if (_charaUsing.LastInteractedFocus != this)
            {
                _charaUsing = null;
                SendToggleUsage(false);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        //_outline.enabled = false;
    }
    private void SendToggleUsage(bool isUsed)
    {
        CommandSend(new int[2] { (int)BedCommand.Usage, isUsed ? 1 : 0 });
    }
    private void ToggleUsage(bool isUsed)
    {
        _outline.enabled = isUsed;

        _isBeingUsed = isUsed;
    }

    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        Debug.Log("CommandReceive: BedHandler");

        switch ((BedCommand)command[0])
        {
            case BedCommand.Usage: ToggleUsage(command[1] == 1); break;
        }
    }
}
