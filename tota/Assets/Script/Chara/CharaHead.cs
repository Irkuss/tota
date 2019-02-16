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
        eManager = GameObject.Find("eManager"); //pas ouf comm methode, mieux vaux avec un tag
        permManager = eManager.GetComponent<PermissionsManager>();
    }

    //Public methods
    public void LeftClickedOn(string spiritName)
    {
        Debug.Log("Clique moi dessus!");
        //Si le joueur qui a cliqué sur Chara appartient à notre équipe
        if (permManager.IsPlayerInTeam(permissions.GetGroupMasterIndex(),spiritName))
        {
            string ownerName = permissions.GetSpiritMasterName();
            
            if (ownerName == null)//Si personne controle Chara, le joueur prend controle de Chara
            {
                Debug.Log("Il était vide, plus maintenant!");

                permissions.SetSpiritMaster(spiritName);
                outline.SetOutlineToSelected(); //dans ce cas, Chara est selectionné
            }
            else if (ownerName != spiritName) //Si le joueur qui nous controle n'a pas cliqué sur Chara, un autre joueur l'a fait
            {
                Debug.Log("Ouste!");
                permissions.SetSpiritMaster(spiritName);
            }
            else //Si le joueur qui nous controle a cliqué sur Chara -> deselect
            {
                Debug.Log("On le possede deja!");
                Deselect();
            }
            
        }
    }

    public void Deselect()
    {
        permissions.SetSpiritMasterNull();
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
