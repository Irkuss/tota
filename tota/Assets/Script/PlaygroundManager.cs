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
        names = PhotonNetwork.otherPlayers;
            
        for(int i = 0; i < names.Length; i++)
        {

            playerNames.text += "\n\n" + names[i].NickName;
        }
    }

    public void Update()
    {
        playerNames.text = "Players : ";
        names = PhotonNetwork.otherPlayers;

        for (int i = 0; i < names.Length; i++)
        {

            playerNames.text += "\n\n" + names[i].NickName;
        }
    }

    //Photon Callback

    public virtual void OnLeftRoom()
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



        PhotonNetwork.Instantiate("PlayerTest", spawnPoint.position, spawnPoint.rotation, 0);



        joinButton.SetActive(false);
        background.SetActive(false);
    }
}
