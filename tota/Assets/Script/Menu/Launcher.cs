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

    public GameObject photonConnectButton;

    public GameObject currentRoom;
    public GameObject waitingRoom;
    
    public GameObject nameField;
    public Text namefield;

    public GameObject sliderObject;

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

        waitingRoom.SetActive(true);

        nameField.SetActive(false);             

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


    public override void OnJoinedRoom()
    {        
        waitingRoom.SetActive(false);
        currentRoom.SetActive(true);
        GameObject menu = GameObject.FindGameObjectWithTag("MenuButton");
        menu.transform.position = new Vector3(menu.transform.position.x - 40, menu.transform.position.y, menu.transform.position.z);

        /*foreach (Transform child in PlayerLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }*/

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
        sliderObject.SetActive(true);
        currentRoom.SetActive(false);
        GameObject menu = GameObject.FindGameObjectWithTag("MenuButton");
        menu.transform.position = new Vector3(menu.transform.position.x + 40, menu.transform.position.y, menu.transform.position.z);
    }
}
