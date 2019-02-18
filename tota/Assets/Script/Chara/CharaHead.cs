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
    [SerializeField]
    private GameObject eManager;
    private PermissionsManager permManager;

    //Unity Callbacks

    private void Start()
    {
        eManager = GameObject.Find("eCentralManager"); //pas ouf comm methode, mieux vaux avec un tag
        permManager = eManager.GetComponent<PermissionsManager>();
    }

    //Public methods
    public bool LeftClickedOn(string spiritName)
    {
        //LeftClickedOn renvoie true, si le Spirit a réussi a slectionné Chara, false sinon

        Debug.Log("Chara: I have been clicked by "+ spiritName);


        //Si le joueur qui a cliqué sur Chara appartient à notre équipe
        if (permManager.IsPlayerInTeam(permissions.GetGroupMasterIndex(),spiritName))
        {
            string ownerName = permissions.GetSpiritMasterName();

            //Si personne controle Chara, le joueur prend controle de Chara
            if (ownerName == null)
            {
                Debug.Log("Chara: now controlled by "+ spiritName +" (was empty)");
                Select(spiritName);
                return true;
            }
            //Si le joueur qui nous controle n'a pas cliqué sur Chara, un autre joueur l'a fait
            else if (ownerName != spiritName)
            {
                Debug.Log("Chara: now controlled by " + spiritName + " (was full)");
                Select(spiritName);
                return true;
            }
            //Si le joueur qui nous controle a cliqué sur Chara -> deselect
            else
            {
                Debug.Log("Chara:deselected by " + spiritName);
                Deselect();
                return false;
            }
            
        }
        else
        {
            Debug.Log("Chara: Access Denied: " + spiritName + " is not in team " + permissions.GetGroupMasterIndex());
            return false;
        }
    }

    public void Select(string spiritName)
    {
        //permissions.SetSpiritMaster(spiritName);
        permissions.GetComponent<PhotonView>().RPC("SetSpiritMaster", PhotonTargets.All, spiritName);
        outline.SetOutlineToSelected();
    }

    public void Deselect()
    {
        //permissions.SetSpiritMasterNull();
        permissions.GetComponent<PhotonView>().RPC("SetSpiritMasterNull", PhotonTargets.All);
        outline.SetOutlineToNotSelected();
    }

    public void RightClickedOn(string spiritName)
    {

    }

    public void SetDestination(Vector3 destination)
    {
        movement.MoveTo(destination);
    }
}
