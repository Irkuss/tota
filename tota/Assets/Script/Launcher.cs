using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : Photon.PunBehaviour
{
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players and so new room will be created")]
    private byte MaxPlayersPerRoom = 4;

    public GameObject photonConnectButton;
    public GameObject searchButton;
    public GameObject createButton;
    public GameObject nameField;
    public Text namefield;
    public GameObject inputField;
    public GameObject roomDrop;
    public Dropdown roomDropdown;
    public GameObject joinRoomButton;
    public GameObject leaveButton;
    public GameObject playerField;

    public Text playerNames;
    PhotonPlayer[] playerList;

    RoomInfo[] rooms;
        
    private string _gameVersion = "1";
    private bool isConnecting;

    
    private string roomName = "";
    

    //Unity Callback

    private void Start()
    {
        searchButton.SetActive(false);
        createButton.SetActive(false);
        inputField.SetActive(false);
        roomDrop.SetActive(false);
        playerField.SetActive(false);
        joinRoomButton.SetActive(false);
        leaveButton.SetActive(false);
    }

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
        nameField.SetActive(false);
        searchButton.SetActive(true);
        createButton.SetActive(true);
        inputField.SetActive(true);
        roomDrop.SetActive(true);

        // Dropdown for the room 
        rooms = PhotonNetwork.GetRoomList();
        roomDropdown.ClearOptions();
        Debug.Log(rooms.Length);
        List<string> names = new List<string>();

        foreach (RoomInfo roomx in rooms)
        {            
            names.Add(roomx.Name + " : " + roomx.PlayerCount + " players in");                      
        }
        if (names.Count == 0) names.Add("No rooms");

        roomDropdown.AddOptions(names);
        roomDropdown.RefreshShownValue();        

        Debug.Log("Connected to Master");
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("No longer connected to Photon");
    }

    public virtual void OnPhotonJoinedRoomFailed(object[] codeAndMsg)
    {
        //Origin: SearchRoom()

        Debug.Log("Failed to joined " + roomName);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        playerNames.text = "Players :";
        playerList = PhotonNetwork.playerList;

        foreach (var name in playerList)
        {
            playerNames.text += "\n\n" + name.NickName;
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        playerNames.text = "Players :";
        playerList = PhotonNetwork.playerList;

        foreach (var name in playerList)
        {
            playerNames.text += "\n\n" + name.NickName;
        }
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        //PhotonNetwork.LoadLevel("Playground");
        searchButton.SetActive(false);
        createButton.SetActive(false);
        inputField.SetActive(false);
        roomDrop.SetActive(false);

        playerField.SetActive(true);
        joinRoomButton.SetActive(true);
        leaveButton.SetActive(true);


        // Player field où sont écrits le nom des joueurs
        playerNames.text = "Players :";
        playerList = PhotonNetwork.playerList;

        foreach (var name in playerList)
        {
            playerNames.text += "\n\n" + name.NickName;
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
            PhotonNetwork.JoinRoom(roomName);
            
            //Callback suivant: OnJoinedRoom()
            //Callback suivant: OnPhotonJoinedRoomFailed(object[] codeAndMsg)
        }
    }

    public void CreateRoom()
    {
        //Origin: createButton
        Debug.Log("Trying to Create");
        if (roomName != "")
        {
            PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);            
            Debug.Log("Created room " + roomName);
        }
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
        // si il reste au moins un jour dans la room dont on vient 
        // on peut repartir dans cette room 
        //if (names.Length > 0) SceneManager.LoadScene(3);
        //else
        SceneManager.LoadScene(2);
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

        Debug.Log("Instantiation en cours");
        SceneManager.LoadScene(3);
    }
}
