using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CharaHead : MonoBehaviour
{
    //Chara Family (Component attaché à Chara)
    [SerializeField] private CharaMovement _movement = null;
    [SerializeField] private CharaPermissions _permissions = null;
    [SerializeField] private CharaOutline _outline = null;

    //Searched in Start
    private GameObject _eManager;
    private PermissionsManager _permManager;

    //Unity Callbacks

    private void Start()
    {
        _eManager = GameObject.Find("eCentralManager"); //pas ouf comm methode, mieux vaux avec un tag
        _permManager = PermissionsManager.Instance;
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
        _permissions.GetComponent<PhotonView>().RPC("SetOwner", PhotonTargets.AllBuffered, player.Name);

        _outline.GetComponent<PhotonView>().RPC("SetOutlineToSelected", PhotonTargets.AllBuffered);
        _outline.GetComponent<PhotonView>().RPC("ChangeColorTo", PhotonTargets.AllBuffered, player.LinkedColor);
    }

    public void Deselect()
    {
        //Appelé par SpiritHead (par une des 3 fonctions Deselect: DeselectChara(), DeselectAll(), DeselectAllExcept())
        _permissions.GetComponent<PhotonView>().RPC("SetOwnerNull", PhotonTargets.AllBuffered);

        _outline.GetComponent<PhotonView>().RPC("SetOutlineToNotSelected", PhotonTargets.AllBuffered);

        //NB: removed "if (!EventSystem.current.IsPointerOverGameObject())" and moved it to SpiritHead
        //RemoveInventoryOnDeselected(this.gameObject);
        CloseInventoryOnDeselected();
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip();
    }

    //Clic Droit

    public void RightClickedOn(string spiritName)
    {

    }

    public void SetDestination(Vector3 destination)
    {
        _movement.MoveTo(destination);
    }

    public void SetStopDistance(float newStop)
    {
        _movement.SetStoppingDistance(newStop);
    }

    private void CloseInventoryOnDeselected()
    {
        GetComponent<CharaInventory>().CloseInterface();
        Debug.Log("CharaHead: closing inventory of a deselected Chara");
    }

    //Focus on Interactable

    private Interactable _focus;
    private IEnumerator _checkCor;

    public void SetFocus(Interactable inter)
    {

        _focus = inter;
        _movement.MoveToInter(_focus);

        _checkCor = CheckDistanceInter();
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

    private IEnumerator CheckDistanceInter()
    {
        while (Vector3.Distance(transform.position, _focus.InterTransform.position) > _focus.Radius * 0.8f)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("CharaHead: reached Inter");
        //Interragis avec l'Interactable une fois proche
        _focus.Interact(this);

        //Reset le focus
        _focus = null;
        _movement.StopAgent();
    }
}
