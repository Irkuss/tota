using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class PlaygroundManager : MonoBehaviour
{
    public Transform spawnPoint;
    [SerializeField]
    private GameObject joinButton;
    [SerializeField]
    private GameObject nameInputField;

    public PermissionsManager permissions;

    //Unity Callback

    private void Start()
    {
    }

    //Photon Callback

    public virtual void OnLeftRoom()
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
        Debug.Log(PhotonNetwork.player.NickName + " has joined the room");

        Debug.Log("Instantiation en cours");

        InstantiateSpirit();

        joinButton.SetActive(false);
        nameInputField.SetActive(false);
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
