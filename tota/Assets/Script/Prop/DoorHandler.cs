using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorHandler : WallHandler
{
    //Defining attribute
    [Header("Door attribute")]
    public Transform transRotate = null;

    //Reference
    private BoxCollider boxColl = null;
    private NavMeshObstacle navObstacle = null;
    private float _doorBaseColliderHeight;
    
    //Private attribute
    private float _baseYRotation;

    private bool _isOpen = false;
    private IEnumerator _rotationCor = null;
    
    private bool IsLocked => _belongingTeam != null;
    private PermissionsManager.Team _belongingTeam = null;

    //Command enum
    public enum DoorCommand
    {
        ToggleOpen,
        SetTeam,
        ModifyHP,
    }

    //Start
    private void Start()
    {
        //Called the Init for OrganicOpacity
        BeginOpacity();

        //Set the references
        boxColl = GetComponent<BoxCollider>();
        if (boxColl == null) Debug.LogWarning("DoorHandler: unexpected null Box collider");
        _doorBaseColliderHeight = boxColl.size.y;

        navObstacle = GetComponent<NavMeshObstacle>();
        if (navObstacle == null) Debug.LogWarning("DoorHandler: unexpected null Navmesh Obstacle");

        //Init the Private attributes
        _baseYRotation = transRotate.eulerAngles.y;
        navObstacle.enabled = _isOpen;
        
        _currentHitPoint = maxHitPoint; //(override from WallHandler)
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: SendToggleOpen(chara); break;//Open/Close (change name)
            case 1: Break(chara, actionIndex); break;//Break
            case 2: Repair(chara); break;//Repair
            case 3: Lock(chara); break;//Lock (for team)
            case 4: ; break;//Barricade
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        //if(actionIndex == 0 && !_isOpen)Debug.Log("CheckAvailability: door interacted by chara " + transform.InverseTransformPoint(chara.transform.position));


        switch (actionIndex)
        {
            case 0: return CanBeToggled(chara);//Open/Close (change name)
            case 1: return CanBreak(chara, 1);//Break
            case 2: return CanRepair(chara, 2);//Repair
            case 3: return CheckLock(chara);//Lock (for team)
            case 4: return false;//Barricade
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return 0;//Open/Close (change name)
            case 1: return GetBreakTime(chara);//Break
            case 2: return GetRepairTime(chara);//Repair
            case 3: return 1;//Lock (for team)
            case 4: return 1;//Barricade
        }
        return 0f;
    }
    
    //====================Action Method====================

    //Open
    private void SendToggleOpen(CharaHead chara)
    {
        Vector3 relativeCharPosition = transRotate.InverseTransformPoint(chara.transform.position);
        int openInFront = relativeCharPosition.z < 0 ? 1 : 0;

        CommandSend(new int[2] { (int)DoorCommand.ToggleOpen, openInFront });
    }
    private void ToggleOpen(bool openInFront)
    {
        if(_rotationCor != null)
        {
            Debug.LogWarning("ToggleOpen: Unexpected Door was already moving, canceling movement (collider should not be active)");
            StopCoroutine(_rotationCor);
            _rotationCor = null;
        }

        _rotationCor = RotationCor(openInFront, !_isOpen);
        StartCoroutine(_rotationCor);
    }


    private bool CanBeToggled(CharaHead chara)
    {
        if (_isOpen)
        {
            _possibleActionNames[0] = "Close";
            return true;
        }
        _possibleActionNames[0] = "Open";

        if (IsLocked)
        {
            //Debug.Log("CanBeToggled: Door is locked");
            return false;
        }
        //Debug.Log("CanBeToggled: Door can be opened freely");
        return true;
    }

    private IEnumerator RotationCor(bool openInFront, bool isOpening)
    {
        //Desactive le collider le temps de la rotation
        boxColl.enabled = false;
        navObstacle.enabled = false;

        float desiredY = _baseYRotation;
        //Decider la rotation de la porte
        if(isOpening)
        {
            int rotatModifier = openInFront ? -100 : 100;
            desiredY += rotatModifier;
        }
        //Rotation
        while (!FloatEqual(desiredY, transRotate.eulerAngles.y))
        {
            float newY = Mathf.LerpAngle(transRotate.eulerAngles.y, desiredY, 0.3f);

            transRotate.eulerAngles = new Vector3(transRotate.eulerAngles.x, newY, transRotate.eulerAngles.z);

            yield return null;
        }
        //Fin de rotation (hardset à la rotation voulue)
        transRotate.eulerAngles = new Vector3(transRotate.eulerAngles.x, desiredY, transRotate.eulerAngles.z);

        //Changement de status
        _isOpen = isOpening;
        //Changement du Collider (reactive le collider a la fin de la rotation
        boxColl.enabled = true;
        
        //Vector3 currCenter = boxColl.center;
        Vector3 currSize = boxColl.size;
        boxColl.size = new Vector3(currSize.x, currSize.y, _isOpen ? 0.4f : 0.8f);

        navObstacle.enabled = _isOpen;//Active le navObstacle que quand ouvert

        //Fin de la coroutine
        _rotationCor = null;
    }
    public static bool FloatEqual(float f1, float f2)
    {
        //Also used when facing a focus at the start of an interaction
        return (Mathf.Abs(f1 - f2) % 360) < 0.2f;
    }

    //Lock
    private void Lock(CharaHead chara)
    {
        if(IsLocked)
        {
            //Unlock (permissions has already been checked
            SendSetTeam(null);
        }
        else
        {
            //Locking
            SendSetTeam(chara.GetComponent<CharaPermissions>().GetTeam());
        }
    }

    private void SendSetTeam(PermissionsManager.Team newTeam)
    {
        string teamName = newTeam == null ? "" : newTeam.Name;

        CommandSend(new int[1] { (int)DoorCommand.SetTeam }, null, new string[1] { teamName });
    }
    private void SetTeam(string teamName)
    {
        if (teamName == "")
        {
            _belongingTeam = null;
        }
        else
        {
            _belongingTeam = PermissionsManager.Instance.GetTeamWithName(teamName);
        }
    }
    
    private bool CheckLock(CharaHead chara)
    {
        if(IsLocked)
        {
            _possibleActionNames[3] = "Unlock";
            if(chara.GetComponent<CharaPermissions>().GetTeam().IsEqual(_belongingTeam))
            {
                return true;
            }
            return false;
        }
        _possibleActionNames[3] = "Lock";
        return true;
    }
    
    //ModifyHP (override from WallHandler)
    protected override void SendModifyHP(int modifier)
    {
        CommandSend(new int[2] { (int)DoorCommand.ModifyHP, modifier });
    }

    //====================Open Action Method====================

    //ForceOpen
    public bool CanForceOpen(CharaHead chara)
    {
        if (_isOpen) return false;
        if (_rotationCor != null) return false;
        if (IsLocked) return false;

        return true;
    }

    public void ForceOpen(CharaHead chara)
    {
        //Called by charas when fording the door
        SendToggleOpen(chara);
    }

    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch((DoorCommand)command[0])
        {
            case DoorCommand.ToggleOpen: ToggleOpen(command[1] == 1); break;
            case DoorCommand.SetTeam: SetTeam(commandString[0]); break;
            case DoorCommand.ModifyHP: ModifyHP(command[1]); break;
        }
    }
}
