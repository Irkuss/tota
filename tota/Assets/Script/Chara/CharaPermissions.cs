using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaPermissions : MonoBehaviour
{
    //Recuperer le permissions manager component
    private PermissionsManager permManager;

    private void Start()
    {
        permManager = GameObject.Find("eCentralManager").GetComponent<PermissionsManager>(); //pas ouf comm methode, mieux vaux avec un tag
        
        //teamNameRPC est initialisé par RPC des que Chara est instancié;
        myTeam = permManager.GetTeamWithName(teamNameRPC);

        GetComponent<CharaOutline>().OnFinishedPermissions();
    }

    //Appartenance à une équipe
    private string teamNameRPC;
    private PermissionsManager.Team myTeam = null; //if null then controlled by AI

    //Nom du Spirit qui controle Chara, null si personne ne la controle
    //NB: on utilise le string et non le Player car passable par RPC
    private string ownerName = null;


    //Getters
    public bool HasTeam()
    {
        return myTeam != null;
    }
    public PermissionsManager.Team GetTeam()
    {
        Debug.Log("CharaPermissions: Getting team: " + myTeam.Name);
        return myTeam;
    }

    public bool HasOwner()
    {
        return (ownerName != null);
    }
    public string GetOwnerName()
    {
        return ownerName;
    }
    

    //Setters RPC
    [PunRPC]
    public void SetTeam(string teamName)
    {
        Debug.Log("CharaPermissions: Setting Team to: " + teamName);

        if (permManager == null) //Cas ou perManager n'a pas encore été trouvé (Start ne s'est pas lancé)
        {
            teamNameRPC = teamName;
        }
        else
        {
            myTeam = permManager.GetTeamWithName(teamName);
        }
        
    }
    [PunRPC]
    public void SetTeamNull() //Probably useless now
    {
        myTeam = null;
    }
    
    [PunRPC]
    public void SetOwner(string newOwnerName)
    {
        //Change la personne qui a le controle de Chara
        ownerName = newOwnerName;
    }
    [PunRPC]
    public void SetOwnerNull()
    {
        ownerName = null;
    }

    //Others

    public bool IsOwner(string playerName)
    {
        return ownerName == playerName;
    }
}
