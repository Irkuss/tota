using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyRoom : MonoBehaviour
{
    [SerializeField] private GameObject _listTeams = null;
    [SerializeField] private GameObject _listNoTeam = null;
    [SerializeField] private GameObject _settings = null;

    [SerializeField] private GameObject previous = null;
    [SerializeField] private GameObject current = null;

    [SerializeField] private GameObject launchButton = null;
    [SerializeField] private GameObject forceLaunchButton = null;

    [SerializeField] private GameObject sliderObject = null;

    private Launcher launcher;

    private void Start()
    {
        launcher = GameObject.Find("eLaucher").GetComponent<Launcher>();
        if (PhotonNetwork.isMasterClient)
        {
            launchButton.SetActive(true);
            forceLaunchButton.SetActive(true);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        //Le joueur doit pouvoir quitter la team et la room -> màj pour tout le monde
    }

    public void Ready()
    {
        gameObject.GetComponent<PhotonView>().RPC("ReadyRPC", PhotonTargets.AllBuffered, PhotonNetwork.player);
    }

    [PunRPC]
    private void ReadyRPC(PhotonPlayer photonPlayer)
    {
        launcher.ReadyInRoom(photonPlayer);
    }

    public void Launch()
    {
        if(launcher.LaunchIfReady())
        {
            sliderObject.SetActive(true);
            gameObject.SetActive(false);
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
        }
    }

    public void ForceLaunch()
    {
        sliderObject.SetActive(true);
        gameObject.SetActive(false);
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;
    }

    public void AddToTeam()
    {

    }

    public void AddWithoutTeam()
    {

    }


}
