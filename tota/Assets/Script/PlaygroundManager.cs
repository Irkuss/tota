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

        SceneManager.LoadScene(0); // Quand on aura mélangé le menu principal et ces scènes là il faudra mettre SceneManager.LoadScene(2);
    }

    public virtual void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected

            PhotonNetwork.LoadLevel("Playground");
        }
    }

    public virtual void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects
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
        PhotonNetwork.Instantiate("PlayerTest", spawnPoint.position, spawnPoint.rotation, 0);

        joinButton.SetActive(false);
    }
}
