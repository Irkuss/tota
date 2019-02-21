using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralManager : Photon.MonoBehaviour
{
    public GameObject tempButton;

    public Transform spawnPoint;
    public PermissionsManager permissions;
    

    //Public methods

    public void InstantiateSpirit()
    {
        Debug.Log("CentralManager: Instantiation de spirit");
        GameObject spirit = PhotonNetwork.Instantiate("Spirit", spawnPoint.position, spawnPoint.rotation, 0);
        SpiritHead init = spirit.GetComponent<SpiritHead>();
        init.spiritName = PhotonNetwork.playerName;


        //permissions.AddTeamWithPlayer(PhotonNetwork.playerName); //TODO pour l'instant chaque joueur joue tout seul
        permissions.GetComponent<PhotonView>().RPC("AddTeamWithPlayer", PhotonTargets.All, PhotonNetwork.playerName);

        init.groupIndex = permissions.GetGroupIndex(PhotonNetwork.playerName);

        Debug.Log("CentralManager: This spirit is named " + PhotonNetwork.playerName + " and is in team " + init.groupIndex);

        tempButton.SetActive(false);
    }
}
