using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CentralManager : Photon.MonoBehaviour
{
    public Generator generator;
    public Vector3 spawnPoint;
    public PermissionsManager permissions;

    //Bouton et interface
    public GameObject tempButton;
    public GameObject toolTip;
    public GameObject teamList;
    public GameObject pauseMenu;
    public GameObject nameTeam;
    public GameObject panel;

    public static bool isPause = false;

    public void UpdateToolTip(string[] info)
    {
        toolTip.SetActive(true);
        toolTip.GetComponent<ToolTip>().UpdateTool(info);
    }
    public void DeactivateToolTip()
    {
        toolTip.SetActive(false);
    }


    //Unity Callbacks
    private void Start()
    {
        if (!PhotonNetwork.offlineMode)
        {
            tempButton.SetActive(false);
            //teamList.SetActive(false);
            //nameTeam.SetActive(false);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPause)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        pauseMenu.SetActive(true);
        isPause = true;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        isPause = false;
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(2);
        //PhotonNetwork.Destroy(photonview);
        PhotonNetwork.Disconnect();
    }

    public void Options()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }

    private void TeamListing()
    {
        //foreach(var team in teamLists)
        //{
        //  AddTeam(team):
        //}
    }

    private void AddTeam()
    {
        /*
        if (team == null)
            return;

        //PlayerLeftRoom(photonPlayer);

        GameObject teamListingObj = Instantiate(teamListPrefab);
        teamListingObj.transform.SetParent(TeamLayoutGroup.transform, false);

        teamListingObj.GetComponent<Text>().text = team.ToString();

        TeamListings.Add(teamListing);
         */
    }

    //Special Callbacks
    public void OnGenerationFinished()
    {
        //Appelé par Generator/Start/*Received Package*/GenerateEnd une fois que le monde s'est généré
        tempButton.SetActive(true);
        //teamList.SetActive(true);
        //nameTeam.SetActive(true);
        panel.SetActive(true);
    }

    //Spawn le joueur (appelé par le bouton spawn)
    public void InstantiateSpirit()
    {
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

        string teamName = "Team " + GameObject.Find("eCentralManager").GetComponent<PermissionsManager>().GetNumberOfTeams();


        //Crée une nouvelle équipe avec comme nom "teamName"
        permissions.GetComponent<PhotonView>().RPC("CreateTeam", PhotonTargets.AllBuffered, teamName);
        //Ajoute un nouveau joueur avec comme nom celui du client //TODO pour l'instant chaque joueur joue tout seul
        permissions.GetComponent<PhotonView>().RPC("AddNewPlayerToTeam", PhotonTargets.AllBuffered, teamName, PhotonNetwork.player.NickName);

        //Recupere le Player crée par AddNewPlayerToTeam
        PermissionsManager.Player player = permissions.GetPlayerWithName(PhotonNetwork.player.NickName);

        //L'attribue à notre spirit nouvellement crée
        spirit.GetComponent<SpiritHead>().InitPermissions(player);

        Debug.Log("CentralManager: This spirit is named " + PhotonNetwork.playerName + " and is in team " + teamName);

        //Enleve le bouton de spawn
        tempButton.SetActive(false);
        //teamList.SetActive(false);
        //nameTeam.SetActive(false);
        panel.SetActive(false);
    }
}
