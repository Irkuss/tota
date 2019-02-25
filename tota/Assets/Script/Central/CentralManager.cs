using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralManager : Photon.MonoBehaviour
{
    public Generator generator;
    private Vector3 spawnPoint;
    public PermissionsManager permissions;

    public GameObject tempButton;

    private int usingSeed;


    private void Awake()
    {
        /*
        if (PhotonNetwork.isMasterClient)
        {
            int seed = 144;

            Debug.Log("Central Manager: Setting seed: " + seed);

            GetComponent<PhotonView>().RPC("SetRandomSeed", PhotonTargets.AllBuffered,seed);
        }
        Random.InitState(usingSeed);
        */
    }

    [PunRPC]
    private void SetRandomSeed(int seed)
    {
        Debug.Log("Central Manager: " + PhotonNetwork.player.name + " is receiving seed as " + seed);
        usingSeed = seed;
    }



    private void Start()
    {
        spawnPoint = new Vector3(generator.nodeMiddleX, 15.0f, generator.nodeMiddleY);
    }

    //Public methods

    public void InstantiateSpirit()
    {
        Debug.Log("CentralManager: Instantiation de spirit");
        GameObject spirit = PhotonNetwork.Instantiate("Spirit", spawnPoint, Quaternion.identity, 0);
        SpiritHead init = spirit.GetComponent<SpiritHead>();
        init.spiritName = PhotonNetwork.playerName;


        //permissions.AddTeamWithPlayer(PhotonNetwork.playerName); //TODO pour l'instant chaque joueur joue tout seul
        permissions.GetComponent<PhotonView>().RPC("AddTeamWithPlayer", PhotonTargets.All, PhotonNetwork.playerName);

        init.groupIndex = permissions.GetGroupIndex(PhotonNetwork.playerName);

        Debug.Log("CentralManager: This spirit is named " + PhotonNetwork.playerName + " and is in team " + init.groupIndex);

        tempButton.SetActive(false);
    }
}
