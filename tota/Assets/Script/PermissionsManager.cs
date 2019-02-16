using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissionsManager : MonoBehaviour
{
    private List<List<string>> allTeams;

    private void Start()
    {
        allTeams = new List<List<string>>();
    }

    //Public methods

    public int GetNumberOfTeams()
    {
        return allTeams.Count;
    }

    public int GetGroupIndex(string playerToFind)
    {
        for (int index = 0; index < allTeams.Count; index++)
        {
            foreach (string player in allTeams[index])
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
        foreach (string player in allTeams[index])
        {
            if (player == playerName)
            {
                return true;
            }
        }
        return false;
    }

    public void AddTeam()
    {
        allTeams.Add(new List<string>());
    }

    public void AddTeamWithPlayer(string playerName)
    {
        allTeams.Add(new List<string>() { playerName });
    }

    public void AddPlayerToTeam(int index, string playerName)
    {
        allTeams[index].Add(playerName);
    }
}
