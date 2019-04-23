using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    //Interactable class, has to be derived from to be useful (ex: PropHandl

    //Characteristics of Interactable
    [Header("Is this object moving or not?")]
    public bool isMoving = false; //Used to determine if chara have to get this transform.position when moving
    [Header("Interaction center")]
    [SerializeField] protected Transform _interTransform = null;
    public Transform InterTransform => _interTransform;

    [Header("Radius of interaction, from interaction center")]
    [SerializeField] protected float _radius = 5;
    public float Radius { get => _radius; }
    [Header("All possible actions and their characteristics")]
    [SerializeField] private string[] _possibleActionNames = null; // actions names (used in the dropdown menu)
    public string[] PossibleActionNames => _possibleActionNames;
    [SerializeField] private bool[] _isDistanceAction = null; //a distance Action can be interacted without moving the chara
    public bool[] IsDistanceAction => _isDistanceAction;     // (it often has a complex CheckAvailability counterpart)
    [SerializeField] private bool[] _isDoWhileAction = null; //a do while Action is done until the focus is removed (ex: hunting / following)
    public bool[] IsDoWhileAction => _isDoWhileAction;
    //There are '_possibleActionNames.Length + 1' possible actions

    //Interact, has to be overwritten
    public virtual void Interact(CharaHead chara, int actionIndex = 0)
    {
        Debug.Log("Interactable: Interacting as " + chara.GetComponent<CharaRpg>().NameFull + " with actionIndex " + actionIndex);
        switch (actionIndex) //Should only call implemented function
        {
            case 0: //Action 0
                break;
            case 1: //Action 1
                break;
            default: //Do nothing (should not be 
                break;
        }
    }

    public virtual bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        //Return if action at index is currently available (ex: is being used by another player)
        Debug.Log("Interactable: Checking availability as " + chara.name);
        switch (actionIndex) //Should only call implemented function
        {
            case 0: //Action 0 availability
                break;
            case 1: //Action 1 availability
                break;
            default: //Do nothing (should not be 
                break;
        }
        return true;
    }
}
