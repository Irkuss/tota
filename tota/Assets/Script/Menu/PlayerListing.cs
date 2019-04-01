using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviour
{
    public PhotonPlayer PhotonPlayer { get; private set; }

    [HideInInspector] public bool isReady = false;

    public GameObject toggle;

    [SerializeField] private Text _playerName = null;
    private Text PlayerName
    {
        get { return _playerName; }
    }

    private void Start()
    {
        if (isReady) toggle.SetActive(true);
        else toggle.SetActive(false);
    }

    public void ApplyPhotonPlayer(PhotonPlayer photonPlayer)
    {
        PhotonPlayer = photonPlayer;
        PlayerName.text = photonPlayer.NickName;
    }
}
