﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissionsManager : MonoBehaviour
{
    //Classes
    public static int[] GetRandomColor()
    {
        return new int[4]
        {
                Random.Range(0, 255),
                Random.Range(0, 255),
                Random.Range(0, 255),
                255
        };
    }

    public class Player
    {
        private string _name;
        public string Name => _name;
        private string _myTeamName;
        public string MyTeamName => _myTeamName;
        private int[] _linkedColor;
        public int[] LinkedColor => _linkedColor;

        public Player(string name,string myTeamName)
        {
            _name = name;
            _myTeamName = myTeamName;
            _linkedColor = GetRandomColor();
        }

        public bool IsEqual(Player player)
        {
            return _name == player._name;
        }

    }

    public class Team
    {
        private string _name;
        public string Name => _name;
        private List<Player> _playerList;
        public List<Player> PlayerList =>_playerList;

        private int[] _linkedColor;
        public int[] LinkedColor => _linkedColor;

        private Player _leaderTeam;
        public Player leaderTeam =>_leaderTeam;

        public Team(string name)
        {
            _name = name;

            _playerList = new List<Player>();

            _linkedColor = GetRandomColor();
        }

        //Messing with list

        public void AddPlayer(Player player)
        {
            if (ContainsPlayer(player)) return;
            _playerList.Add(player);
            _leaderTeam = _playerList[0];
        }

        public void RemovePlayer(Player player)
        {
            if (!ContainsPlayer(player)) return;
            _playerList.Remove(player);
            if (_playerList.Count != 0)
            {
                _leaderTeam = _playerList[0];
            }
            else
            {
                _leaderTeam = null;
            }
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
    public List<Team> TeamList => _teamList;

    //Unity Callbacks

    public static PermissionsManager Instance;

    [HideInInspector] public int numberChara = 0;
    [HideInInspector] public int heightMap = 3;
    [HideInInspector] public SpiritHead spirit;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(this);
            }
        }
        
    }

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
    public void AddNewPlayerToTeam(string teamName, string playerName, bool game)
    {
        GetTeamWithName(teamName).AddPlayer(new Player(playerName,teamName));
        if (!game)
        {
            GameObject.Find("eLaucher").GetComponent<Launcher>().TeamListing(GetTeamWithName(teamName));
        }
    }

    [PunRPC]
    public void RemovePlayerFromTeam(string teamName, string playerName)
    {
        Player player = GetPlayerWithName(playerName);
        Team team = GetTeamWithName(teamName); 

        if (player == null || team == null)
        {
            return;
        }

        team.RemovePlayer(player);
        GameObject.Find("eLaucher").GetComponent<Launcher>().PlayerLeftTeam(player);
        if (team.PlayerList.Count == 0)
        {
            GameObject.Find("eLaucher").GetComponent<Launcher>().TeamLeftRoom(team);
            TeamList.Remove(team);
        }
        else
        {
            GameObject.Find("eLaucher").GetComponent<Launcher>().TeamListing(team);
        }
        
    }
}
