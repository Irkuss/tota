using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaPermissions : MonoBehaviour
{
    //Recuperer le permissions manager component
    private PermissionsManager permManager = null;
    private PermissionsManager.Player player = null;
    private PermissionsManager.Team team = null;
    public PermissionsManager.Team Team => team;

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
    public bool HasOwner => ownerName != null;


    //Getters
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
    

    //Setters RPC
    public void SetTeam(string teamName)
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetTeam, null, new string[1] { teamName }, null);
    }

    public void RPC_SetTeam(string teamName)
    {
        Debug.Log("CharaPermissions: Setting Team to: " + (teamName == "" ? "AI" : teamName));

        if(teamName == "")
        {
            team = null;
        }
        else
        {
            team = PermissionsManager.Instance.GetTeamWithName(teamName);
        }

        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.UpdateTeamColor, null, null, null);
        GetComponent<CharaHead>().Deselect();
        
    }
    
    public void SetOwner(string newOwnerName)
    {
        //Change la personne qui a le controle de Chara
        ownerName = newOwnerName;
    }
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
