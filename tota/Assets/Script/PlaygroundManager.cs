using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlaygroundManager : Photon.PunBehaviour
{
    public Transform spawnPoint;
    [SerializeField]
    private GameObject joinButton;
    [SerializeField]
    private GameObject nameInputField;

    public PermissionsManager permissions;
    public GameObject background;

    public Text playerNames;
    PhotonPlayer[] names;


    //Unity Callback

    public void Start()
    {
        playerNames.text = "Players :";
        names = PhotonNetwork.playerList;
        
        foreach(var name in names)
        {
            playerNames.text += "\n\n" + name.NickName;
        }
    }

    //Photon Callback

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        playerNames.text = "Players :";
        names = PhotonNetwork.playerList;

        foreach (var name in names)
        {
            playerNames.text += "\n\n" + name.NickName;
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        playerNames.text = "Players :";
        names = PhotonNetwork.playerList;

        foreach (var name in names)
        {
            playerNames.text += "\n\n" + name.NickName;
        }
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
        Debug.Log(PhotonNetwork.player.NickName + " has joined the room");

        Debug.Log("Instantiation en cours");

        InstantiateSpirit();

        joinButton.SetActive(false);
        nameInputField.SetActive(false);

        background.SetActive(false);
    }

    //Private methods

    private void InstantiateSpirit()
    {
        GameObject spirit = PhotonNetwork.Instantiate("Spirit", spawnPoint.position, spawnPoint.rotation, 0);
        SpiritHead init = spirit.GetComponent<SpiritHead>();
        init.spiritName = PhotonNetwork.playerName;
        
        //TODO pour l'instant chaque joueur joue tout seul
        permissions.AddTeamWithPlayer(PhotonNetwork.playerName);
    }
}
