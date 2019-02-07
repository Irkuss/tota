using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlaygroundManager : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject joinButton;
    public GameObject background;

    public Text playerNames;

    PhotonPlayer[] names;

    //Unity Callback

    public void Start()
    {
        playerNames.text = "Players : ";
        names = PhotonNetwork.playerList;
        List<string> listName = new List<string>();

        for(int i = 0; i < names.Length; i++)
        {
            playerNames.text += '\n' + names[i].NickName;
        }
    }

    //Photon Callback

    public virtual void OnLeftRoom()
    {
        //Origin: LeaveRoom()

        SceneManager.LoadScene(2);        
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
        background.SetActive(false);
    }
}
