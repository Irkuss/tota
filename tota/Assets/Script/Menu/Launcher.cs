﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : Photon.PunBehaviour
{
    [SerializeField] private PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    [SerializeField] private GameObject photonConnectButton = null;

    [SerializeField] private GameObject currentRoom = null;
    [SerializeField] private GameObject roomList = null;
    [SerializeField] private GameObject createRoom = null;

    [SerializeField] private GameObject nameField = null;
    [SerializeField] private Text namefield = null;

    [SerializeField] private GameObject sliderObject = null;

    [SerializeField] private GameObject backMenu = null;

    [SerializeField] private GameObject launchButton = null;
    [SerializeField] private GameObject forceLaunchButton = null;

    #region PlayerListing

    [SerializeField] private GameObject _playerLayoutGroup = null;
    private GameObject PlayerLayoutGroup
    {
        get { return _playerLayoutGroup; }
    }

    [SerializeField] private GameObject _playerListingPrefab = null;
    private GameObject PlayerListingPrefab
    {
        get { return _playerListingPrefab; }
    }

    private List<PlayerListing> _playerListings = new List<PlayerListing>();
    private List<PlayerListing> PlayerListings
    {
        get { return _playerListings; }
    }

    #endregion    

    [SerializeField] private GameObject _teamLayoutGroup = null;
    [SerializeField] private GameObject _teamListingPrefab = null;
    [SerializeField] private GameObject _teamNamePrefab = null;

    private List<PermissionsManager.Team> teams = new List<PermissionsManager.Team>();
    private List<PlayerListing> _playerListInTeam = new List<PlayerListing>();
    private List<PlayerListing> PlayerListInTeam
    {
        get { return _playerListInTeam; }
    }

    private string _gameVersion = "1";
    private bool isConnecting;
    
    private string roomName = "";    

    //Unity Callback

    private void Awake()
    {
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        PhotonNetwork.autoJoinLobby = false;
        
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // Force LogLevel
        PhotonNetwork.logLevel = Loglevel;
    }

    //Photon Callback

    public override void OnConnectedToMaster()
    {
        //Origin: ConnectPhoton()

        photonConnectButton.SetActive(false);

        roomList.SetActive(true);
        
        nameField.SetActive(false);

        backMenu.SetActive(false);

        Debug.Log("Connected to Master");

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined a lobby");
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("No longer connected to Photon");
    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        Debug.Log("Failed to create " + roomName);
        /*error.text = "FAILED TO CREATE THE ROOM";
        StartCoroutine(Waiting());*/
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        //Origin: SearchRoom()
        //error.text = "FAILED TO JOIN THE ROOM";
        Debug.Log("Failed to joined " + roomName);
        //StartCoroutine(Waiting());       
    }

    /*IEnumerator Waiting()
    {
        error.enabled = true;
        yield return new WaitForSeconds(3);
        error.enabled = false;
    }*/

    [PunRPC]
    public void ActiveMasterButton()
    {
        launchButton.SetActive(true);
        forceLaunchButton.SetActive(true);
    }

    private void RemoveMasterButton()
    {
        launchButton.SetActive(false);
        forceLaunchButton.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        roomList.SetActive(false);
        createRoom.SetActive(false);
        currentRoom.SetActive(true);

        if (PhotonNetwork.masterClient.NickName == PhotonNetwork.player.NickName)
        {
            gameObject.GetComponent<PhotonView>().RPC("ActiveMasterButton", PhotonNetwork.player);
        }

        foreach (Transform child in PlayerLayoutGroup.transform)
        {
            Debug.Log("Destroying on join" + child.name);
            Destroy(child.gameObject);
            Debug.Log("Destroyed on join");
        }

        foreach (Transform child in _teamLayoutGroup.transform)
        {
            Debug.Log("Destroying on join" + child.name);
            Destroy(child.gameObject);
            Debug.Log("Destroyed on join");
        }
                
        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
        for (int i = 0; i < photonPlayers.Length; i++)
        {
            PlayerJoinedRoom(photonPlayers[i]);
        }

        currentRoom.GetComponent<ReadyRoom>().Set();

        Debug.Log("Joined a room");
    }

    //Called by photon when a player joins the room.
    public override void OnPhotonPlayerConnected(PhotonPlayer photonPlayer)
    {
        PlayerJoinedRoom(photonPlayer);
    }

    //Called by photon when a player leaves the room.
    public override void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
    {
        PlayerLeftRoom(photonPlayer);
        gameObject.GetComponent<PhotonView>().RPC("ActiveMasterButton", PhotonNetwork.masterClient);
        Debug.Log("OnPhotonPlayerDisconnected");
    }

    private void PlayerJoinedRoom(PhotonPlayer photonPlayer)
    {
        if (photonPlayer == null)
            return;

        PlayerLeftRoom(photonPlayer);

        GameObject playerListingObj = Instantiate(PlayerListingPrefab);
        playerListingObj.transform.SetParent(PlayerLayoutGroup.transform, false);

        PlayerListing playerListing = playerListingObj.GetComponent<PlayerListing>();
        playerListing.ApplyPhotonPlayer(photonPlayer);
        PlayerListings.Add(playerListing);

        if (PhotonNetwork.player.NickName == PhotonNetwork.masterClient.NickName && PhotonNetwork.player != photonPlayer)
        {
            playerListing.kick.SetActive(true);
            playerListing.kick.GetComponent<Button>().onClick.AddListener(() => KickFromRoom(photonPlayer));
        }
        Debug.Log("PlayerJoinedRoom");
    }

    private void KickFromRoom(PhotonPlayer player)
    {
        gameObject.GetComponent<PhotonView>().RPC("SendLeave", player);
        gameObject.GetComponent<PhotonView>().RPC("PlayerLeftNoTeam", PhotonTargets.AllBuffered, player);        
    }

    [PunRPC]
    private void SendLeave()
    {
        currentRoom.GetComponent<ReadyRoom>().LeaveRoom();
    }

    private void PlayerLeftRoom(PhotonPlayer photonPlayer)
    {
        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == photonPlayer);
        if (index != -1)
        {
            Destroy(PlayerListings[index].gameObject);
            PlayerListings.RemoveAt(index);
        }
        else
        {
            int indexT = PlayerListInTeam.FindIndex(x => x.PhotonPlayer == photonPlayer);
            if (indexT != -1)
            {
                Debug.Log("Destroy" + PlayerListInTeam[indexT].PhotonPlayer.NickName);
                Destroy(PlayerListInTeam[indexT].gameObject);
                PlayerListInTeam.RemoveAt(indexT);
            }
        }
        Debug.Log("PlayerLeftRoom");
    }

    [PunRPC]
    private void PlayerLeftNoTeam(PhotonPlayer photonPlayer)
    {
        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == photonPlayer);
        if (index != -1)
        {
            Destroy(PlayerListings[index].gameObject);
            PlayerListings.RemoveAt(index);
        }
    }

    public void ReadyInRoom(PhotonPlayer player)
    {
        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == player);
        if (index != -1)
        {
            GameObject toggle = PlayerListings[index].toggle;
            if (PlayerListings[index].isReady)
            {
                PlayerListings[index].isReady = false;
                toggle.SetActive(false);
            }
            else
            {
                PlayerListings[index].isReady = true;
                toggle.SetActive(true);
            }            
        }
        else
        {
            int indexT = PlayerListInTeam.FindIndex(x => x.PhotonPlayer == player);
            if (indexT != -1)
            {
                GameObject toggle = PlayerListInTeam[indexT].toggle;
                if (PlayerListInTeam[indexT].isReady)
                {
                    PlayerListInTeam[indexT].isReady = false;
                    toggle.SetActive(false);
                }
                else
                {
                    PlayerListInTeam[indexT].isReady = true;
                    toggle.SetActive(true);
                }
            }
        }
    }

    public bool LaunchIfReady()
    {
        foreach(var player in PlayerListings)
        {
            if (!player.isReady) return false;
        }
        foreach(var player in _playerListInTeam)
        {
            if (!player.isReady) return false;
        }
        return true;
    }

    public void AddTeam(string teamName)
    {
        PermissionsManager permissions = PermissionsManager.Instance;
        if (permissions.GetTeamWithName(teamName) != null)//&& permissions.GetTeamWithPlayer(permissions.GetPlayerWithName(PhotonNetwork.player.NickName)) != null
        {
            return;
        }
        GameObject.Find("PermissionManager").GetComponent<PhotonView>().RPC("CreateTeam", PhotonTargets.AllBuffered, teamName);
        AddToTeam(teamName, PhotonNetwork.player);
    }

    public void AddToTeam(string teamName, PhotonPlayer player)
    {
        PermissionsManager permissions = PermissionsManager.Instance;
        PermissionsManager.Player playerPerm = permissions.GetPlayerWithName(player.NickName);

        if (permissions.GetTeamWithName(teamName) == null) return;

        if (playerPerm != null && permissions.GetTeamWithName(teamName) != null)
        {
            if (permissions.GetTeamWithName(teamName) == permissions.GetTeamWithPlayer(playerPerm)) return;
        }

        int playersInTeam = permissions.GetTeamWithName(teamName).PlayerList.Count;
        if (playersInTeam < (int) PhotonNetwork.room.CustomProperties["maxInTeam"])
        {
            int i = 0;
            while (i < permissions.TeamList.Count)
            {
                PermissionsManager.Team team = permissions.TeamList[i];
                if (playerPerm != null && team.ContainsPlayer(playerPerm))
                {
                    GameObject.Find("PermissionManager").GetComponent<PhotonView>().RPC("RemovePlayerFromTeam", PhotonTargets.AllBuffered, team.Name, playerPerm.Name);
                }
                i += 1;
            }
            GameObject.Find("PermissionManager").GetComponent<PhotonView>().RPC("AddNewPlayerToTeam", PhotonTargets.AllBuffered, teamName, player.NickName,false);
            gameObject.GetComponent<PhotonView>().RPC("PlayerLeftNoTeam", PhotonTargets.AllBuffered, player);
        }        
    }

    public void TeamListing(PermissionsManager.Team team)
    {
        if (team == null)
            return;

        TeamLeftRoom(team);

        GameObject teamListingObj = Instantiate(_teamListingPrefab);
        teamListingObj.transform.SetParent(_teamLayoutGroup.transform, false);

        GameObject teamNameObj = Instantiate(_teamNamePrefab);
        teamNameObj.transform.SetParent(teamListingObj.transform, false);
        teamNameObj.GetComponent<Button>().onClick.AddListener(() => AddToTeam(team.Name, PhotonNetwork.player));
        teamNameObj.GetComponentInChildren<Text>().text = team.Name;

        if ((team.leaderTeam.Name == PhotonNetwork.player.NickName
                        || PhotonNetwork.player.NickName == PhotonNetwork.masterClient.NickName))
        {
            teamNameObj.transform.GetChild(1).gameObject.SetActive(true);
            teamNameObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => RemoveTeamHard(team));
        }        

        foreach (var player in team.PlayerList)
        {
            GameObject playerObj = Instantiate(_playerListingPrefab);
            playerObj.transform.SetParent(teamListingObj.transform, false);
            
            PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
            foreach (var playerP in photonPlayers)
            {
                if (playerP.NickName == player.Name)
                {               
                    PlayerListing playerListing = playerObj.GetComponent<PlayerListing>();
                    playerListing.ApplyPhotonPlayer(playerP);
                    PlayerListInTeam.Add(playerListing);

                    if ((team.leaderTeam.Name == PhotonNetwork.player.NickName
                        || PhotonNetwork.player.NickName == PhotonNetwork.masterClient.NickName) && PhotonNetwork.player != playerP)
                    {
                        playerListing.kick.SetActive(true);
                        playerListing.kick.GetComponent<Button>().onClick.AddListener(() => KickFromTeam(playerP));
                    }
                }
            }
        }
        teams.Add(team);
    }

    private void KickFromTeam(PhotonPlayer player)
    {
        PermissionsManager permissions = PermissionsManager.Instance;
        PermissionsManager.Player playerPerm = permissions.GetPlayerWithName(player.NickName);
        PermissionsManager.Team team = permissions.GetTeamWithPlayer(playerPerm);

        if (playerPerm == null || team == null)
        {
            Debug.Log("NULL");
            return;
        }

        permissions.gameObject.GetComponent<PhotonView>().RPC("RemovePlayerFromTeam", PhotonTargets.AllBuffered, team.Name, playerPerm.Name);
        gameObject.GetComponent<PhotonView>().RPC("CallPlayerJoinedRoom", PhotonTargets.AllBuffered, player);
    }

    [PunRPC]
    private void CallPlayerJoinedRoom(PhotonPlayer player)
    {
        PlayerJoinedRoom(player);
    }

    private void RemoveTeamHard(PermissionsManager.Team team)
    {       
        PhotonPlayer[] playerlist = PhotonNetwork.playerList;
        foreach (var player in team.PlayerList)
        {
            for (int i = 0; i < playerlist.Length; i++)
            {
                if (playerlist[i].NickName == player.Name)
                {
                    gameObject.GetComponent<PhotonView>().RPC("CallPlayerJoinedRoom", PhotonTargets.AllBuffered, playerlist[i]);
                }
            }
        }
        gameObject.GetComponent<PhotonView>().RPC("CallTeamLeftRoom", PhotonTargets.AllBuffered, team.Name);
    }

    [PunRPC]
    private void CallTeamLeftRoom(string teamName)
    {        
        while (PermissionsManager.Instance.GetTeamWithName(teamName) != null)
        {
            PermissionsManager.Team team = PermissionsManager.Instance.GetTeamWithName(teamName);
            for (int i = 0; i < team.PlayerList.Count; i++)
            {
                PermissionsManager.Instance.GetComponent<PhotonView>().RPC("RemovePlayerFromTeam", PhotonTargets.AllBuffered, teamName, team.PlayerList[i].Name);
            }
        }        
    }

    public void TeamLeftRoom(PermissionsManager.Team team)
    {
        int indexPlayer;
        int index = teams.FindIndex(x => x.Name == team.Name);
        if (index != -1)
        {
            Destroy(_teamLayoutGroup.transform.GetChild(index).gameObject); 
            foreach (var player in team.PlayerList)
            {
                indexPlayer = PlayerListInTeam.FindIndex(x => x.PhotonPlayer.NickName == player.Name);
                if (indexPlayer != -1)
                {
                    PlayerListInTeam.RemoveAt(indexPlayer);
                }
            }
            teams.RemoveAt(index);
        }

    }

    public void PlayerLeftTeam(PermissionsManager.Player player)
    {
        int index = PlayerListInTeam.FindIndex(x => x.PhotonPlayer.NickName == player.Name);
        if (index != -1)
        {
            PlayerListInTeam.RemoveAt(index);
        }
    }

    //Public methods

    public void ConnectPhoton()
    {
        //Origin: photonConnectButton
        if (namefield.text != "")
        {
            //On vérifie qu'on est pas conecté
            if (!PhotonNetwork.connected)
            {
                //On se connecte   
                PhotonNetwork.ConnectUsingSettings(_gameVersion);               
                PhotonNetwork.playerName = namefield.text;
                PlayerPrefs.SetString("name", namefield.text);

                //Callback suivant: OnConnectedToMaster()
            }
        }       
    }

    public void BeginRoom()
    {
        roomList.SetActive(false);
        createRoom.SetActive(true);
    }

    public void BackToMenu()
    {
        PhotonNetwork.Destroy(gameObject);
        PhotonNetwork.Destroy(PermissionsManager.Instance.gameObject);
        Destroy(Mode.Instance.gameObject);
        PhotonNetwork.Disconnect();
        AudioManager.instance.EndMusic("Menu");
        SceneManager.LoadScene(0);
    }

    public override void OnLeftRoom()
    {
        //Origin: LeaveRoom()
        //SceneManager.LoadScene(2);
        RemoveMasterButton();
        currentRoom.SetActive(false);
        roomList.SetActive(true);
        Debug.Log("Left a room");        
    }


    [SerializeField] private GameObject _roomListingPrefab = null;
    private GameObject RoomListingPrefab
    {
        get { return _roomListingPrefab; }
    }

    [SerializeField] private GameObject _roomLayoutGroup = null;
    private GameObject RoomLayoutGroup
    {
        get { return _roomLayoutGroup; }
    }

    private List<RoomListing> _roomListingButtons = new List<RoomListing>();
    private List<RoomListing> RoomListingButtons 
    {
        get { return _roomListingButtons; }
    }

    public override void OnReceivedRoomListUpdate()
    {
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();

        foreach(RoomInfo room in rooms)
        {
            ReceivedRoom(room);
        }
        RemoveOldRooms();
    }

    private void ReceivedRoom(RoomInfo room)
    {
        int index = RoomListingButtons.FindIndex(x => x.RoomName == room.Name);

        if (index == -1)
        {
            if (room.IsVisible && room.PlayerCount < room.MaxPlayers)
            {
                GameObject roomListingObj = Instantiate(RoomListingPrefab);
                roomListingObj.transform.SetParent(RoomLayoutGroup.transform, false);

                RoomListing roomListing = roomListingObj.GetComponent<RoomListing>();
                RoomListingButtons.Add(roomListing);

                index = (RoomListingButtons.Count - 1);
            }
        }

        if (index != -1)
        {
            RoomListing roomListing = RoomListingButtons[index];
            roomListing.SetRoomName(room.Name);
            roomListing.Updated = true;
        }
    }

    private void RemoveOldRooms()
    {
        List<RoomListing> removeRooms = new List<RoomListing>();

        foreach (RoomListing roomListing in RoomListingButtons)
        {
            if (!roomListing.Updated)
                removeRooms.Add(roomListing);
            else
                roomListing.Updated = false;
        }

        foreach (RoomListing roomListing in removeRooms)
        {
            GameObject roomListingObj = roomListing.gameObject;
            RoomListingButtons.Remove(roomListing);
            Destroy(roomListingObj);
        }
    }


    [SerializeField] private GameObject panel = null;
    [SerializeField] private InputField password = null;
    private RoomInfo _room = null;

    public void EnterPassword(RoomInfo room)
    {
        panel.SetActive(true);
        _room = room;
    }

    private void Update()
    {
        if (panel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                panel.SetActive(false);
            }
        }
    }

    public void ValidPassword()
    {
        if (_room != null)
        {
            if (password.text == (string)_room.CustomProperties["password"])
            {
                panel.SetActive(false);
                PhotonNetwork.JoinRoom(_room.Name);
            }
            else
            {
                /*error.text = "Wrong password";
                Debug.Log("Failed to joined " + roomName); 
                StartCoroutine(Waiting());*/
            }
        }
    }


}
