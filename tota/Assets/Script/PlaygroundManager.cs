using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class PlaygroundManager : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject joinButton;

    //Unity Callback

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

        PhotonNetwork.Instantiate("PlayerTest", spawnPoint.position, spawnPoint.rotation, 0);

        joinButton.SetActive(false);
    }
}
