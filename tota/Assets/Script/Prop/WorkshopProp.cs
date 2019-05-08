using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopProp : PropHandler
{
    //Reference
    private Outline _outline;

    //Enum
    public enum WorkShopCommand
    {
        Usage
    }
    public enum WorkshopType
    {
        Undecided,
        ElecWork,
        ForgeWork,
        MecaWork,
        WoodWork
    }
    public static Dictionary<WorkshopType, int> workTypeToTabIndex = new Dictionary<WorkshopType, int>
    {
        { WorkshopType.Undecided, 0},
        { WorkshopType.WoodWork, 1},
        { WorkshopType.MecaWork, 2},
        { WorkshopType.ElecWork, 3},

        { WorkshopType.ForgeWork, 0}, //will probably not be used
    };
    [Header("Workshop attribute")]
    public WorkshopType workType = WorkshopType.Undecided;

    //Usage status
    private bool _isBeingUsed = false;
    private CharaHead _charaUsing = null;
    private IEnumerator _currentCloseUpdate = null;

    //Init
    private void Start()
    {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    //Interract override
    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch(actionIndex)
        {
            case 0: StartUsage(chara); break; //Use Workshop
            case 1: break;//Salvage
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return !_isBeingUsed; //Use Workshop
            case 1: return true; //Salvage
        }
        return false;
    }
    //Interract Implementation
    private void StartUsage(CharaHead chara)
    {
        if (!CheckAvailability(chara, 0)) return; //Si au moment d'arriver, l'établi est finalement utilisé annule tout

        SendToggleUsage(true);
        _charaUsing = chara;

        //ICI -> OUVRE LONGLET, UTILISER LE DICTIONNAIRE 'workTypeToTabIndex' EN PARAMETRE

        _currentCloseUpdate = Cor_UpdateClose();
        StartCoroutine(_currentCloseUpdate);
    }
    private IEnumerator Cor_UpdateClose()
    {
        _outline.enabled = true;

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

        _outline.enabled = false;
    }
    private void SendToggleUsage(bool isUsed)
    {
        CommandSend(new int[2] { (int)WorkShopCommand.Usage, isUsed ? 1 : 0 });
    }
    private void ToggleUsage(bool isUsed)
    {
        _isBeingUsed = isUsed;
    }

    //command
    public override void CommandReceive(int[] command, float[] commandFloat)
    {
        switch((WorkShopCommand)command[0])
        {
            case WorkShopCommand.Usage: ToggleUsage(command[1] == 1); break;
        }
    }
}
