using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : PropHandler
{
    //Defining attribut
    [Header("Door attribute")]
    public int maxBaseHitPoint = 100;
    public Item neededItemToRepair = null;
    public Transform transRotate = null;

    //Reference
    private BoxCollider boxColl = null;
    private float _doorBaseColliderHeight;

    public enum DoorCommand
    {
        ToggleOpen,
        SetTeam,
    }

    //Status attribute
    private float _baseYRotation;
    //Open attribute
    private bool _isOpen = false;
    private IEnumerator _rotationCor = null;

    //Lock Attribute
    private bool IsLocked => _belongingTeam != null;
    private PermissionsManager.Team _belongingTeam = null;

    //Barricade Attribute

    //Break Attribut
    private int _currentHitPoint;

    //Init
    private void Start()
    {
        boxColl = GetComponent<BoxCollider>();
        if (boxColl == null) Debug.LogWarning("DoorHandler: unexpected null Box collider");
        _doorBaseColliderHeight = boxColl.size.y;

        _baseYRotation = transRotate.eulerAngles.y;

        _currentHitPoint = maxBaseHitPoint;
    }

    //Override Interact
    public override void Interact(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: SendToggleOpen(chara); break;//Open/Close (change name)
            case 1: ; break;//Consolidate
            case 2: ; break;//Repair
            case 3: Lock(chara); break;//Lock (for team)
            case 4: ; break;//Break
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        if(actionIndex == 0 && !_isOpen)Debug.Log("CheckAvailability: door interacted by chara " + transform.InverseTransformPoint(chara.transform.position));


        switch (actionIndex)
        {
            case 0: return CanBeToggled(chara);//Open/Close (change name)
            case 1: return false;//Consolidate
            case 2: return false;//Repair
            case 3: return CheckLock(chara);//Lock (for team)
            case 4: return false;//Break
        }
        return false;
    }
    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return 0;//Open/Close (change name)
            case 1: return 1;//Consolidate
            case 2: return 1;//Repair
            case 3: return 1;//Lock (for team)
            case 4: return 1;//Break
        }
        return 0f;
    }

    //ForceOpen
    public bool CanForceOpen(CharaHead chara)
    {
        if (_isOpen) return false;
        if (_rotationCor != null) return false;

        if(IsLocked)
        {
            return false;
        }
        return true;
    }
    public void ForceOpen(CharaHead chara)
    {
        //Called by charas when fording the door
        SendToggleOpen(chara);
    }

    //===============Interact Method===============
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

        if(_isOpen)
        {
            //Close
            Debug.Log("ToggleOpen: Closing door");
            _rotationCor = RotationCor(openInFront, false);
            StartCoroutine(_rotationCor);
        }
        else
        {
            //Open
            Debug.Log("ToggleOpen: Opening door");
            _rotationCor = RotationCor(openInFront, true);
            StartCoroutine(_rotationCor);
        }
    }
    
    private IEnumerator RotationCor(bool openInFront, bool isOpening)
    {
        //Desactive le collider le temps de la rotation
        boxColl.enabled = false;
        
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

        //Fin de la coroutine
        _rotationCor = null;
    }
    private static bool FloatEqual(float f1, float f2)
    {
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


    //===============Available===============
    private bool CanBeToggled(CharaHead chara)
    {
        if(_isOpen)
        {
            _possibleActionNames[0] = "Close";
            return true;
        }
        _possibleActionNames[0] = "Open";
        
        if(IsLocked)
        {
            Debug.Log("CanBeToggled: Door is locked");
            return false;
        }
        Debug.Log("CanBeToggled: Door can be opened freely");
        return true;
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




    //===============Command PropHandler===============
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch((DoorCommand)command[0])
        {
            case DoorCommand.ToggleOpen: ToggleOpen(command[1] == 1); break;
            case DoorCommand.SetTeam: SetTeam(commandString[0]); break;
        }
    }
}
