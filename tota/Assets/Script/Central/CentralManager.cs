using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralManager : Photon.MonoBehaviour
{
    public Generator generator;
    public Vector3 spawnPoint;
    public bool generationIsFinished = false;
    public PermissionsManager permissions;

    public GameObject tempButton;

    private int usingSeed;

    //Spawn le joueur (appelé par le bouton spawn)
    public void InstantiateSpirit()
    {
        if (!generationIsFinished)
        {
            return;
        }
        Debug.Log("CentralManager: Instantiation de spirit");
        //Instantiate the spirit
        GameObject spirit;
        if (PhotonNetwork.offlineMode)
        {
            spirit = Instantiate(Resources.Load<GameObject>("Spirit"), spawnPoint, Quaternion.identity);
        }
        else
        {
            spirit = PhotonNetwork.Instantiate("Spirit", spawnPoint, Quaternion.identity, 0);
        }
        


        //Initialise le Spirit
        //Crée une nouvelle équipe et ajoute le spirit dans cette équipe//TODO pour l'instant chaque joueur joue tout seul
        permissions.GetComponent<PhotonView>().RPC("AddTeamWithPlayer", PhotonTargets.All, PhotonNetwork.playerName);

        SpiritHead init = spirit.GetComponent<SpiritHead>();

        init.spiritName = PhotonNetwork.playerName;
        init.groupIndex = permissions.GetGroupIndex(PhotonNetwork.playerName);

        Debug.Log("CentralManager: This spirit is named " + PhotonNetwork.playerName + " and is in team " + init.groupIndex);

        //Enleve le bouton de spawn
        tempButton.SetActive(false);
    }
}
