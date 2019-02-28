using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaHead : MonoBehaviour
{
    [SerializeField]
    private CharaMovement movement;
    [SerializeField]
    private CharaPermissions permissions;
    [SerializeField]
    private CharaOutline outline;
    
    private GameObject eManager;
    public PermissionsManager permManager;

    //Unity Callbacks

    private void Start()
    {
        eManager = GameObject.Find("eCentralManager"); //pas ouf comm methode, mieux vaux avec un tag
        permManager = eManager.GetComponent<PermissionsManager>();
    }

    //Public methods
    public bool LeftClickedOn(PermissionsManager.Player playerWhoClickedUs)
    {
        //LeftClickedOn renvoie true, si le Spirit a réussi a slectionné Chara, false sinon
        //NB: Il renvoie aussi false lors de la deselection
        
        Debug.Log("Chara: I have been clicked by "+ playerWhoClickedUs.Name);

        //permManager.IsPlayerInTeam(permissions.GetGroupMasterIndex(),spiritName)

        
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

    public void SelectAsPlayer(PermissionsManager.Player player)
    {
        permissions.GetComponent<PhotonView>().RPC("SetOwner", PhotonTargets.AllBuffered, player.Name);

        outline.GetComponent<PhotonView>().RPC("SetOutlineToSelected", PhotonTargets.AllBuffered);
        outline.GetComponent<PhotonView>().RPC("ChangeColorTo", PhotonTargets.AllBuffered, player.LinkedColor);
        //outline.SetOutlineToSelected();
    }

    public void Deselect()
    {
        permissions.GetComponent<PhotonView>().RPC("SetOwnerNull", PhotonTargets.AllBuffered);

        outline.GetComponent<PhotonView>().RPC("SetOutlineToNotSelected", PhotonTargets.AllBuffered);
    }

    public void RightClickedOn(string spiritName)
    {

    }

    public void SetDestination(Vector3 destination)
    {
        movement.MoveTo(destination);
    }
}
