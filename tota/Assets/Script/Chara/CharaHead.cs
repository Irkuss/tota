﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CharaHead : Photon.PunBehaviour
{
    //Chara Family (Component attaché à Chara)
    [SerializeField] private CharaMovement _movement = null;
    [SerializeField] private CharaPermissions _permissions = null;
    [SerializeField] private CharaOutline _outline = null;

    //Autre ref
    [SerializeField] private LayerMask _aiActivationLayer;
    private float _baseStoppingDistance;
    //Searched in Start
    private GameObject _eManager;
    private PermissionsManager _permManager;

    //Unity Callbacks
    private void Awake()
    {
        _baseStoppingDistance = _movement.navMeshAgent.stoppingDistance;
    }
    private void Start()
    {
        _eManager = GameObject.Find("eCentralManager"); //pas ouf comm methode, mieux vaux avec un tag
        _permManager = PermissionsManager.Instance;

        if(PhotonNetwork.isMasterClient)
        {
            StartCoroutine(CheckForAi());
        }
    }

    //Activer les IA
    public const float c_radiusToActivate = 80f;
    
    private IEnumerator CheckForAi()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);

            Collider[] allAi = Physics.OverlapSphere(transform.position, c_radiusToActivate, _aiActivationLayer);
            Debug.Log("CheckForAi: got " + allAi.Length + " zombies in detection sphere");
            foreach(Collider aiCollider in allAi)
            {
                Zombie zombieComp = aiCollider.GetComponent<Zombie>();
                if(zombieComp != null)
                {
                    Debug.Log("CheckForAi: Activating a zombie in range");
                    zombieComp.ForceActivate(this);
                }
            }

        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, c_radiusToActivate);
    }
    //Clic Gauche

    public bool LeftClickedOn(PermissionsManager.Player playerWhoClickedUs)
    {
        //LeftClickedOn renvoie true, si le Spirit a réussi a slectionné Chara, false sinon
        //NB: Il renvoie aussi false lors de la deselection

        if (playerWhoClickedUs == null) return false;

        //Debug.Log("Chara: I have been clicked by "+ playerWhoClickedUs.Name);

        PermissionsManager.Team team = _permissions.GetTeam();
        if (team == null) return false;

        if (team.ContainsPlayer(playerWhoClickedUs))
        {
            //Si le joueur qui a cliqué sur Chara appartient à notre équipe
            if (!_permissions.HasOwner())
            {
                //Si personne controle Chara, le joueur prend controle de Chara
                //Debug.Log("Chara: now controlled by "+ playerWhoClickedUs.Name + " (was empty)");

                SelectAsPlayer(playerWhoClickedUs);
                return true;
            }
            else if (_permissions.IsOwner(playerWhoClickedUs.Name))
            {
                //Si le joueur qui nous controle a cliqué sur Chara -> deselect
                //Debug.Log("Chara:deselected by " + playerWhoClickedUs.Name);
                Deselect();
                return false;
                
            }
            else
            {
                //Si un joueur de notre équipe controle deja Chara, personne ne peut l'override
                return false;
            }
        }
        else
        {
            //Si le joueur qui a cliqué sur Chara n'appartient pas à notre équipe
            //Debug.Log("Chara: Access Denied: " + playerWhoClickedUs.Name + " is not in team " + _permissions.GetTeam().Name);
            return false;
        }
    }

    private void SelectAsPlayer(PermissionsManager.Player player)
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOwner, null, new string[1]{ player.Name}, null);
        //_permissions.GetComponent<PhotonView>().RPC("SetOwner", PhotonTargets.AllBuffered, player.Name);


        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOutlineToSelected, null, null, null);
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.ChangeColorTo, player.LinkedColor, null, null);
        //_outline.GetComponent<PhotonView>().RPC("SetOutlineToSelected", PhotonTargets.AllBuffered);
        //_outline.GetComponent<PhotonView>().RPC("ChangeColorTo", PhotonTargets.AllBuffered, player.LinkedColor);
    }

    public void Deselect()
    {
        //Appelé par SpiritHead (par une des 3 fonctions Deselect: DeselectChara(), DeselectAll(), DeselectAllExcept())
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOwnerNull, null, null, null);
        //_permissions.GetComponent<PhotonView>().RPC("SetOwnerNull", PhotonTargets.AllBuffered);

        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOutlineToNotSelected, null, null, null);
        //_outline.GetComponent<PhotonView>().RPC("SetOutlineToNotSelected", PhotonTargets.AllBuffered);

        //NB: removed "if (!EventSystem.current.IsPointerOverGameObject())" and moved it to SpiritHead
        //RemoveInventoryOnDeselected(this.gameObject);
        CloseInventoryOnDeselected();
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip();
    }

    //Clic Droit

    public void RightClickedOn(string spiritName)
    {

    }

    public void SetDestination(Vector3 destination, bool isRunning)
    {
        _movement.MoveTo(destination, isRunning);
    }

    public void SetStopDistance(float newStop)
    {
        _movement.SetStoppingDistance(newStop);
    }

    private void CloseInventoryOnDeselected()
    {
        GetComponent<CharaInventory>().CloseInterface();
        //Debug.Log("CharaHead: closing inventory of a deselected Chara");
    }

    //Focus on Interactable

    private Interactable _focus;
    private Interactable _lastInteractedFocus = null;
    private IEnumerator _checkCor;

    public void SetFocus(Interactable inter, int actionIndex)
    {
        bool isRunning = false;
        _focus = inter;
        //Prise de décision sur la speed and stoping distance of agent
        if (_focus.IsDoWhileAction[actionIndex])
        {
            SetStopDistance(_focus.Radius * 2f / 3f);

            Debug.Log("SetFocus: is a do while action");
            CharaMovement movementCompOfFocus = _focus.transform.GetComponent<CharaMovement>();
            if (movementCompOfFocus != null) isRunning = movementCompOfFocus.IsRunning;
        }
        else
        {
            SetStopDistance(_baseStoppingDistance);
        }
        //Startin coroutine
        _checkCor = CheckDistanceInter(actionIndex);
        _movement.MoveToInter(_focus, isRunning);
        StartCoroutine(_checkCor);
    }

    public void RemoveFocus()
    {
        if (_focus != null)
        {
            StopCoroutine(_checkCor);
            //Reset le focus
            _focus = null;
            //Arret l'agent
            _movement.StopAgent();
        }
    }

    private IEnumerator CheckDistanceInter(int actionIndex)
    {
        while (Vector3.Distance(transform.position, _focus.InterTransform.position) > _focus.Radius * 0.8f)
        {
            if(_focus.isMoving)
            {
                bool isRunning = false;
                CharaMovement movementCompOfFocus = _focus.transform.GetComponent<CharaMovement>();
                if (movementCompOfFocus != null) isRunning = movementCompOfFocus.IsRunning;

                _movement.MoveToInter(_focus, isRunning); //Update les positions si on sait qu'il bouge
            }
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("CharaHead: reached Inter");
        //Interragis avec l'Interactable une fois proche

        /* Dois attendre inter.GetActionTime(this, actionIndex)
         * 
         * A IMPLEMENTER
         * 
         */


        //Interragis
        _focus.Interact(this, actionIndex);
        _lastInteractedFocus = _focus;
        if(_focus.IsDoWhileAction[actionIndex])
        {
            //L'action est à continuer
            yield return new WaitForSeconds(0.4f);
            SetFocus(_focus, actionIndex);
        }
        else
        {
            //Fin de l'action
            //Reset le focus
            _focus = null;
            _movement.StopAgent();
        }
    }

    public bool TryAddItemToFurniture(Item item)
    {
        if(_lastInteractedFocus != null)
        {
            CharaInventory furnitureInv = _lastInteractedFocus.GetComponent<CharaInventory>();
            if(furnitureInv != null)
            {
                return furnitureInv.Add(item);
            }
        }
        return false;
    }
}
