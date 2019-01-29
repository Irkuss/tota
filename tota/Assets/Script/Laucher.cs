using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laucher : Photon.PunBehaviour {

    //===ATTRIBUTE

    //======PUBLIC ATTRIBUTE

    public PhotonLogLevel LogLevel = PhotonLogLevel.Informational;
    public byte maxPlayerInRooms = 4;

    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    public GameObject controlPanel;

    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    public GameObject progressLabel;

    //======PRIVATE ATTRIBUTE

    private string _gameVersion = "1";

    //===METHOD
    //======CALLBACK METHOD

    void Start()
    {
        //Desactive le msg de progression et active le bouton Play et L'inputField
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;

        PhotonNetwork.logLevel = LogLevel;
    }
    
    //======PUBLIC METHOD

    
    public void Connect()
    {
        // Start the connection process.
        // - If already connected, we attempt joining a random room
        // - if not yet connected, Connect this application instance to Photon Cloud Network

        //Ecran de chargement
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        if (PhotonNetwork.connected)
        {
            //we need at this point to attempt joining a Random Room. 
            //If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            //we must first and foremost connect to Photon Online Server
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }
	}


    //======CALLBACK PHOTON METHODE OVERRIDE

    public override void OnConnectedToMaster()
    {
        //Une fois connecté au serveur Master
        Debug.Log("Connecté au serveur Master");

        //Essaye de rejoindre une salle random
        PhotonNetwork.JoinRandomRoom();
    }
    
    public override void OnPhotonRandomJoinFailed (object[] codeAndMessage)
    {
        //Lorsqu'on ne trouve pas de salle disponible (vide ou inexistente)
        Debug.Log("Aucune salle disponible, création d'une nouvelle salle en cours");

        //Create new room
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerInRooms }, null);//shrug: maxPlayers avec une majuscule ?

        Debug.Log("Une nouvelle salle a été créée");
    }
    
    public override void OnJoinedRoom()
    {
        //Lorsqu'un joueur rejoint une salle

        Debug.Log("WE IN BOIS");
    }

    public override void OnDisconnectedFromPhoton()
    {
        //Retour Menu
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        Debug.LogWarning("Erreur: Deconnecté de Photon Cloud");
    }
}
