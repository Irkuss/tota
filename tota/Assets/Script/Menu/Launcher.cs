using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : Photon.PunBehaviour
{
    [SerializeField] private PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players and so new room will be created")]
    private byte MaxPlayersPerRoom = 4;

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
    [SerializeField] private Text error = null;
    

    //Unity Callback

    private void Awake()
    {
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        PhotonNetwork.autoJoinLobby = false;
        
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // Force LogLevel
        PhotonNetwork.logLevel = Loglevel;

        error.enabled = false;
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
        error.text = "FAILED TO CREATE THE ROOM";
        StartCoroutine(Waiting());
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        //Origin: SearchRoom()
        error.text = "FAILED TO JOIN THE ROOM";
        Debug.Log("Failed to joined " + roomName);
        StartCoroutine(Waiting());       
    }

    IEnumerator Waiting()
    {
        error.enabled = true;
        yield return new WaitForSeconds(3);
        error.enabled = false;
    }

    [PunRPC]
    public void ActiveMasterButton()
    {
        launchButton.SetActive(true);
        forceLaunchButton.SetActive(true);
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

        Debug.Log("Joined a room");

        //GameObject menu = GameObject.FindGameObjectWithTag("MenuButton");
        //menu.transform.position = new Vector3(menu.transform.position.x - 20, menu.transform.position.y, menu.transform.position.z);

        foreach (Transform child in PlayerLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _teamLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
        for (int i = 0; i < photonPlayers.Length; i++)
        {
            PlayerJoinedRoom(photonPlayers[i]);
        }

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
        if (photonPlayer.IsMasterClient)
        {
            PhotonNetwork.SetMasterClient(PlayerListings[0].PhotonPlayer);
            gameObject.GetComponent<PhotonView>().RPC("ActiveMasterButton", PlayerListings[0].PhotonPlayer);
        }
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
    }

    private void PlayerLeftRoom(PhotonPlayer photonPlayer)
    {
        int index = PlayerListings.FindIndex(x => x.PhotonPlayer == photonPlayer);
        if (index != -1)
        {
            Destroy(PlayerListings[index].gameObject);
            PlayerListings.RemoveAt(index);
        }
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
        return true;
    }

    public void AddTeam(string teamName)
    {
        gameObject.GetComponent<PhotonView>().RPC("CreateTeam", PhotonTargets.AllBuffered, teamName);
        AddToTeam(teamName, PhotonNetwork.player);
    }

    public void AddToTeam(string teamName, PhotonPlayer player)
    {
        int playersInTeam = gameObject.GetComponent<PermissionsManager>().GetTeamWithName(teamName).PlayerList.Count;
        if (playersInTeam < (int) PhotonNetwork.room.CustomProperties["maxInTeam"])
        {
            gameObject.GetComponent<PhotonView>().RPC("AddNewPlayerToTeam", PhotonTargets.AllBuffered, teamName, player.NickName);
            gameObject.GetComponent<PhotonView>().RPC("PlayerLeftNoTeam", PhotonTargets.AllBuffered, player);
        }        
    }

    public void TeamListing(PermissionsManager.Team team)
    {
        if (team == null)
            return;

        //TeamLeftRoom(team);

        GameObject teamListingObj = Instantiate(_teamListingPrefab);
        teamListingObj.transform.SetParent(_teamLayoutGroup.transform, false);

        GameObject teamNameObj = Instantiate(_teamNamePrefab);
        teamNameObj.transform.SetParent(teamListingObj.transform, false);
        teamNameObj.GetComponent<Button>().onClick.AddListener(() => AddToTeam(team.Name, PhotonNetwork.player));
        teamNameObj.GetComponentInChildren<Text>().text = team.Name;

        foreach (var player in team.PlayerList)
        {
            GameObject playerObj = Instantiate(_playerListingPrefab);
            playerObj.transform.SetParent(teamListingObj.transform, false);

            PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
            for (int i = 0; i < photonPlayers.Length; i++)
            {
                if (photonPlayers[i].NickName == player.Name)
                {
                    PlayerListing playerListing = playerObj.GetComponent<PlayerListing>();
                    playerListing.ApplyPhotonPlayer(photonPlayers[i]);
                    PlayerListInTeam.Add(playerListing);
                }
            }
        }
        teams.Add(team);
    }

    private void TeamLeftRoom(PermissionsManager.Team team)
    {
        int index = teams.FindIndex(x => x.Name == team.Name);
        if (index != -1)
        {
            Destroy(_teamLayoutGroup.transform.GetChild(index));
            teams.RemoveAt(index);
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

    public void SearchRoom()
    {
        //Origin: searchButton

        if (roomName != "")
        {
            if (PhotonNetwork.JoinRoom(roomName))
            {

            }
            else
            {
                Debug.Log("On doit renvoyer un message d'erreur");
            }
            
            //Callback suivant: OnJoinedRoom()
            //Callback suivant: OnPhotonJoinedRoomFailed(object[] codeAndMsg)
        }
        else
        {
            error.text = "FAILED TO CREATE THE ROOM";
            StartCoroutine(Waiting());
        }
        
    }

    public void CreateRoom()
    {
        //Origin: createButton
        Debug.Log("Trying to Create");
        if (roomName != "")
        {
            PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom },null);            
            Debug.Log("Created room " + roomName);
        }
        else
        {
            error.text = "FAILED TO CREATE THE ROOM";
            StartCoroutine(Waiting());
        }
        
        //connectRoom.SetActive(true);
        //current.SetActive(false);
    }

    public void BeginRoom()
    {
        roomList.SetActive(false);
        createRoom.SetActive(true);
    }

    public void SetRoomName(string name)
    {
        //Origin: inputField

        Debug.Log("Changed room name to " + name);
        roomName = name;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
        if (PhotonNetwork.connected) PhotonNetwork.Disconnect();
    }

    public override void OnLeftRoom()
    {
        //Origin: LeaveRoom()
        //SceneManager.LoadScene(2);

        foreach(Transform child in _teamLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in PlayerLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        currentRoom.SetActive(false);
        roomList.SetActive(true);
        Debug.Log("Left a room");
    }

    //Public methods

    public void LeaveRoom()
    {
        //Origin: Leave button

        PhotonNetwork.LeaveRoom();

        //Callback suivant: OnLeftRoom()
    }

    public void JoinGame()
    {
        sliderObject.SetActive(true);
        currentRoom.SetActive(false);
        GameObject menu = GameObject.FindGameObjectWithTag("MenuButton");
        menu.transform.position = new Vector3(menu.transform.position.x + 20, menu.transform.position.y, menu.transform.position.z);
    }

    // PARTIE LISTE DES ROOMS A VOIR SI DANS UN AUTRE SCRIPT

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
    [SerializeField] private Text password = null;
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
                error.text = "Wrong password";
                Debug.Log("Failed to joined " + roomName); 
                StartCoroutine(Waiting());
            }
        }
    }


}
