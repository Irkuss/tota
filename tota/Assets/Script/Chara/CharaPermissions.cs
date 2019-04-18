using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaPermissions : MonoBehaviour
{
    //Recuperer le permissions manager component
    private PermissionsManager permManager = null;
    private PermissionsManager.Player player = null;
    private PermissionsManager.Team team = null;

    private void Start()
    {
        permManager = PermissionsManager.Instance;

        //teamNameRPC est initialisé par RPC des que Chara est instancié;
        //myTeam = permManager.GetTeamWithName(teamNameRPC);

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
        //return myTeam != null;
        return team != null;
    }
    public PermissionsManager.Team GetTeam()
    {
        if (team != null)
        {
            Debug.Log("CharaPermissions: Getting team: " + team.Name);
        }
        else
        {
            if (teamNameRPC != null)
            {
                team = PermissionsManager.Instance.GetTeamWithName(teamNameRPC);
            }            
        }
        
        return team;
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
    public void SetTeam(string teamName)
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetTeam, null, new string[1] { teamName }, null);
        //GetComponent<PhotonView>().RPC("RPC_SetTeam", PhotonTargets.AllBuffered, teamName);
    }
    public void SetTeamNull()
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetTeamNull, null, null, null);
        //GetComponent<PhotonView>().RPC("RPC_SetTeamNull", PhotonTargets.AllBuffered);
    }
    /*[PunRPC] private*/ public void RPC_SetTeam(string teamName)
    {
        Debug.Log("CharaPermissions: Setting Team to: " + teamName);

        if (permManager == null) //Cas ou perManager n'a pas encore été trouvé (Start ne s'est pas lancé)
        {
            teamNameRPC = teamName;
        }
        else
        {
            //myTeam = permManager.GetTeamWithName(teamName);
            team = permManager.GetTeamWithName(teamName);
        }
        
    }
    /*[PunRPC] private*/
    public void RPC_SetTeamNull() //Probably useless now
    {
        //myTeam = null;
        team = null;
    }

    /*[PunRPC]*/
    public void SetOwner(string newOwnerName)
    {
        //Change la personne qui a le controle de Chara
        ownerName = newOwnerName;
    }
    /*[PunRPC]*/
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
