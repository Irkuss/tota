using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CharaHead : MonoBehaviour
{
    //Chara Family (Component attaché à Chara)
    [SerializeField]
    private CharaMovement movement;
    [SerializeField]
    private CharaPermissions permissions;
    [SerializeField]
    private CharaOutline outline;

    //Searched in Start
    private GameObject eManager;
    public PermissionsManager permManager;
    

    //Unity Callbacks

    private void Start()
    {
        eManager = GameObject.Find("eCentralManager"); //pas ouf comm methode, mieux vaux avec un tag
        permManager = eManager.GetComponent<PermissionsManager>();
    }

    //Clic Gauche
    public bool LeftClickedOn(PermissionsManager.Player playerWhoClickedUs)
    {
        //LeftClickedOn renvoie true, si le Spirit a réussi a slectionné Chara, false sinon
        //NB: Il renvoie aussi false lors de la deselection
        Debug.Log("Chara: I have been clicked by "+ playerWhoClickedUs.Name);
        
        if (permissions.GetTeam().ContainsPlayer(playerWhoClickedUs))
        {
            //Si le joueur qui a cliqué sur Chara appartient à notre équipe
            if (!permissions.HasOwner())
            {
                //Si personne controle Chara, le joueur prend controle de Chara
                Debug.Log("Chara: now controlled by "+ playerWhoClickedUs.Name + " (was empty)");

                SelectAsPlayer(playerWhoClickedUs);
                return true;
            }
            else if (permissions.IsOwner(playerWhoClickedUs.Name))
            {
                //Si le joueur qui nous controle a cliqué sur Chara -> deselect
                Debug.Log("Chara:deselected by " + playerWhoClickedUs.Name);
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
            Debug.Log("Chara: Access Denied: " + playerWhoClickedUs.Name + " is not in team " + permissions.GetTeam().Name);
            return false;
        }
    }

    private void SelectAsPlayer(PermissionsManager.Player player)
    {
        permissions.GetComponent<PhotonView>().RPC("SetOwner", PhotonTargets.AllBuffered, player.Name);

        outline.GetComponent<PhotonView>().RPC("SetOutlineToSelected", PhotonTargets.AllBuffered);
        outline.GetComponent<PhotonView>().RPC("ChangeColorTo", PhotonTargets.AllBuffered, player.LinkedColor);
    }

    public void Deselect()
    {
        permissions.GetComponent<PhotonView>().RPC("SetOwnerNull", PhotonTargets.AllBuffered);

        outline.GetComponent<PhotonView>().RPC("SetOutlineToNotSelected", PhotonTargets.AllBuffered);
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            RemoveInventoryOnDeselected(this.gameObject);
        }
        
    }

    //Clic Droit

    public void RightClickedOn(string spiritName)
    {

    }

    public void SetDestination(Vector3 destination)
    {
        //if (EventSystem.current.IsPointerOverGameObject()) return;

        movement.MoveTo(destination);
    }

    private void RemoveInventoryOnDeselected(GameObject chara)
    {
        chara.GetComponentInChildren<Inventory>().RemoveInventory();
        Debug.Log("removing");
    }
}
