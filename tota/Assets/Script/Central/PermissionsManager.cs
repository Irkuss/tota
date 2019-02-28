using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissionsManager : MonoBehaviour
{
    //Classes

    public class Player
    {
        private string _name;
        public string Name
        {
            get => _name;
        }
        private string _myTeamName;
        public string MyTeamName
        {
            get => _myTeamName;
        }
        private int[] _linkedColor;
        public int[] LinkedColor
        {
            get => _linkedColor;
        }

        public Player(string name,string myTeamName)
        {
            _name = name;
            _myTeamName = myTeamName;
            _linkedColor = GetRandomColor();
        }

        private int[] GetRandomColor()
        {
            return new int[4] 
            {
                Random.Range(0, 255),
                Random.Range(0, 255),
                Random.Range(0, 255),
                255
            };
        }

        public bool IsEqual(Player player)
        {
            return _name == player._name;
        }

    }

    public class Team
    {
        private string _name;
        public string Name
        {
            get => _name;
        }
        private List<Player> _playerList;
        private int[] _linkedColor;
        public int[] LinkedColor
        {
            get => _linkedColor;
        }

        public Team(string name)
        {
            _name = name;

            _playerList = new List<Player>();

            _linkedColor = GetRandomColor();
        }

        private int[] GetRandomColor()
        {
            return new int[4]
            {
                Random.Range(0, 255),
                Random.Range(0, 255),
                Random.Range(0, 255),
                255
            };
        }

        //Messing with list

        public void AddPlayer(Player player)
        {
            _playerList.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            _playerList.Remove(player);
        }

        //Getter

        public Player GetPlayerWithName(string name)
        {
            foreach(Player player in _playerList)
            {
                if (player.Name == name)
                {
                    return player;
                }
            }
            return null;
        }

        // Question

        public bool ContainsPlayer(Player playerToFind)
        {
            foreach(Player player in _playerList)
            {
                if (playerToFind.IsEqual(player))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsEqual(Team team)
        {
            return _name == team._name;
        }
    }

    //Attributes

    private List<Team> _teamList;

    //Unity Callbacks

    private void Start()
    {
        _teamList = new List<Team>();
    }

    //Team Getters

    public int GetNumberOfTeams()
    {
        return _teamList.Count;
    }

    public Team GetTeamWithName(string name)
    {
        foreach (Team team in _teamList)
        {
            if (team.Name == name)
            {
                Debug.Log("GetTeamWithName: found team with name " + name);
                return team;
            }
        }
        Debug.Log("GetTeamWithName: did not find team with name " + name);
        return null;
    }

    public Team GetTeamWithPlayer(Player player)
    {
        foreach(Team team in _teamList)
        {
            if (team.ContainsPlayer(player))
            {
                return team;
            }
        }
        return null;
    }
    
    public Player GetPlayerWithName(string name)
    {
        Player player = null;
        foreach(Team team in _teamList)
        {
            player = team.GetPlayerWithName(name);
            if (player != null)
            {
                return player;
            }
        }
        return null;
    }
    

    //Setters RPC
    
    [PunRPC]
    public void CreateTeam(string teamName)
    {
        _teamList.Add(new Team(teamName));
    }
    [PunRPC]
    public void AddNewPlayerToTeam(string teamName, string playerName)
    {
        GetTeamWithName(teamName).AddPlayer(new Player(playerName,teamName));
    }
}
