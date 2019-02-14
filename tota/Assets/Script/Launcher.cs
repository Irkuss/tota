﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : MonoBehaviour
{
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players and so new room will be created")]
    public byte MaxPlayersPerRoom = 4;

    public GameObject photonConnectButton;
    public GameObject searchButton;
    public GameObject createButton;
    public GameObject nameField;
    public Text namefield;
    public GameObject inputField;
    public GameObject roomDrop;
    public Dropdown roomDropdown;

    RoomInfo[] rooms;
        
    private string _gameVersion = "1";
    private bool isConnecting;

    
    private string roomName = "";
    private string playerName = "";

    //Unity Callback

    private void Start()
    {
        searchButton.SetActive(false);
        createButton.SetActive(false);
        inputField.SetActive(false);
        roomDrop.SetActive(false);

        // Dropdown for the room 
        rooms = PhotonNetwork.GetRoomList();
        roomDropdown.ClearOptions();

        List<string> names = new List<string>();        
        
        foreach (RoomInfo roomx in rooms)
        {
            if (roomx.MaxPlayers != roomx.PlayerCount)
            {
                names.Add(roomx.Name + " : " + roomx.PlayerCount + " players in");
            }
        }

        roomDropdown.AddOptions(names);
        roomDropdown.RefreshShownValue();

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

    public virtual void OnConnectedToMaster()
    {
        //Origin: ConnectPhoton()

        photonConnectButton.SetActive(false);
        nameField.SetActive(false);
        searchButton.SetActive(true);
        createButton.SetActive(true);
        inputField.SetActive(true);
        roomDrop.SetActive(true);

        Debug.Log("Connected to Master");
    }

    public virtual void OnDisconnectedFromPhoton()
    {
        Debug.Log("No longer connected to Photon");
    }

    public virtual void OnPhotonJoinedRoomFailed(object[] codeAndMsg)
    {
        //Origin: SearchRoom()

        Debug.Log("Failed to joined " + roomName);
    }

    public virtual void OnJoinedRoom()
    {
        //Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        PhotonNetwork.LoadLevel("Playground");
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
}
