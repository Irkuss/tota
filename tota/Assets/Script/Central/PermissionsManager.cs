using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissionsManager : MonoBehaviour
{
    private List<List<string>> _allTeams;

    private void Start()
    {
        _allTeams = new List<List<string>>();
    }

    //Public methods

    public int GetNumberOfTeams()
    {
        return _allTeams.Count;
    }

    public int GetGroupIndex(string playerToFind)
    {
        for (int index = 0; index < _allTeams.Count; index++)
        {
            foreach (string player in _allTeams[index])
            {
                if (player == playerToFind)
                {
                    return index;
                }
            }
        }
        return -1;
    }

    public bool IsPlayerInTeam(int index, string playerName)
    {
        foreach (string player in _allTeams[index])
        {
            if (player == playerName)
            {
                return true;
            }
        }
        return false;
    }

    //Setters RPC
    [PunRPC]
    public void AddTeam()
    {
        _allTeams.Add(new List<string>());
    }
    [PunRPC]
    public void AddTeamWithPlayer(string playerName)
    {
        _allTeams.Add(new List<string>() { playerName });
    }
    [PunRPC]
    public void AddPlayerToTeam(int index, string playerName)
    {
        _allTeams[index].Add(playerName);
    }
}
