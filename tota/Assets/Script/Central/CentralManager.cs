using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CentralManager : Photon.MonoBehaviour
{
    public Generator generator;
    public Vector3 spawnPoint;
    
    //Bouton et interface
    public GameObject tempButton;
    public GameObject toolTip;
    public GameObject pauseMenu;
    [SerializeField] private GameObject _charaRef = null;

    public static bool isPause = false;

    private PermissionsManager permi;
    private PermissionsManager.Team team = null;
    private PermissionsManager.Player player = null;

    [SerializeField] private GameObject _inventoryList = null;
    public GameObject InventoryList { get { return _inventoryList; }}

    [SerializeField] private GameObject _inventoryLayout = null;
    public GameObject InventoryLayout { get { return _inventoryLayout; } }

    [SerializeField] private GameObject _charaLayout = null;
    public GameObject CharaLayout { get { return _charaLayout; } }

    [SerializeField] private GameObject _furnitureInventory = null;
    public GameObject FurnitureInventory { get { return _furnitureInventory; } }

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
    private void Awake()
    {
        permi = PermissionsManager.Instance;
        player = permi.GetPlayerWithName(PhotonNetwork.player.NickName);
        if (player != null)
        {
            team = permi.GetTeamWithPlayer(player);
        }
        
    }

    private void Start()
    {
        tempButton.SetActive(false);
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


    //Special Callbacks
    public void OnGenerationFinished()
    {
        //Appelé par Generator/Start/*Received Package*/GenerateEnd une fois que le monde s'est généré
        tempButton.SetActive(true);
    }

    //Spawn le joueur (appelé par le bouton spawn)
    public void InstantiateSpirit()
    {
        Debug.Log("CentralManager: Instantiation de spirit");

        
        Debug.Log("Teams : ");
        foreach (var team in permi.TeamList)
        {
            Debug.Log(team.Name);
            foreach (var playr in team.PlayerList)
            {
                Debug.Log(playr.Name);
            }
        }

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
        string teamName;

        if (player == null || team == null)
        {
            teamName = "Team" + permi.GetNumberOfTeams();
            //Crée une nouvelle équipe avec comme nom "teamName"
            permi.GetComponent<PhotonView>().RPC("CreateTeam", PhotonTargets.AllBuffered, teamName);
            //Ajoute un nouveau joueur avec comme nom celui du client //TODO pour l'instant chaque joueur joue tout seul
            permi.GetComponent<PhotonView>().RPC("AddNewPlayerToTeam", PhotonTargets.AllBuffered, teamName, PhotonNetwork.player.NickName,true);
            //Recupere le Player crée par AddNewPlayerToTeam
            player = permi.GetPlayerWithName(PhotonNetwork.player.NickName);
        }
        else
        {
            teamName = team.Name;
        }

        //L'attribue à notre spirit nouvellement crée
        spirit.GetComponent<SpiritHead>().InitPermissions(player);

        Debug.Log("CentralManager: This spirit is named " + player.Name + " and is in team " + teamName);

        //Enleve le bouton de spawn
        tempButton.SetActive(false);
        _charaRef.SetActive(true);

        if (player.IsEqual(permi.GetTeamWithPlayer(player).leaderTeam))
        {
            for (int i = 0; i < permi.numberChara; i++)
            {
                spirit.GetComponent<SpiritHead>().TryCharaSpawn(true,_charaLayout);
            }
        }
        
    }
}
