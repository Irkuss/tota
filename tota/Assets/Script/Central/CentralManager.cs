using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CentralManager : Photon.MonoBehaviour
{
    //Generation, spirit spawn et camera Start

    public Generator generator;

    public const float c_cameraStartHeight = 800;
    public float cameraStartDownAngle = 90f;

    //reference
    private DayNightCycle dayNightCycle = null;
    //Bouton et interface
    public GameObject tempButton;
    public GameObject toolTip;
    public GameObject pauseMenu;
    [SerializeField] private GameObject _charaRef = null;
    [SerializeField] private GameObject _options = null;

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

    [SerializeField] private GameObject _channel = null;
    public GameObject Channel { get { return _channel; } }

    [SerializeField] private GameObject _tooltipL = null;

    [SerializeField] private GameObject _build = null;
    public GameObject Build { get { return _build; } }

    [SerializeField] private GameObject _actions = null;
    public GameObject Actions { get { return _actions; } }

    [SerializeField] private GameObject _button = null;
    public GameObject Button { get { return _button; } }

    public void UpdateToolTip(string[] info,string quirks)
    {
        toolTip.SetActive(true);
        toolTip.GetComponent<ToolTip>().UpdateToolTip(info,quirks);
    }

    public void UpdateSkills(string[] info)
    {
        toolTip.GetComponent<ToolTip>().UpdateSkills(info);
    }

    public void DeactivateToolTip()
    {
        toolTip.SetActive(false);
    }

    public void ClipDown()
    {
        _tooltipL.SetActive(!_tooltipL.activeSelf);
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
        dayNightCycle = GameObject.Find("Directional Light").GetComponent<DayNightCycle>();
    }

    public void PlaceCameraAbove(int x, int y)
    {
        Transform cameraTransform = Camera.main.gameObject.transform;

        cameraTransform.position =
            new Vector3(
                (x + 0.5f) * Generator.c_worldChunkLength,
                c_cameraStartHeight,
                (y + 0.5f) * Generator.c_worldChunkLength);
        cameraTransform.rotation = Quaternion.Euler(new Vector3(cameraStartDownAngle, 0, 0));
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
        PhotonNetwork.Destroy(gameObject);
        PhotonNetwork.Destroy(PermissionsManager.Instance.gameObject);
        PhotonNetwork.Destroy(Mode.Instance.gameObject);
        SceneManager.LoadScene(2);
    }

    public void Options()
    {
        pauseMenu.SetActive(false);
        _options.SetActive(true);
    }

    public void Controls()
    {
        if (_options.transform.GetChild(0).GetChild(0).GetComponent<InputField>().text == "z")
        {
            Mode.Instance.zqsd = true;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
    //Temperature getters
    public int GetTemperatureAtCoord(Vector3 pos)
    {
        //Called by charaRpg
        return dayNightCycle.GetCurrentSeasonTemperature() + GetComponent<Generator>().GetPosTempModifier(pos);
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
        float spawnValue = (generator.SpawnPoint + 0.5f) * Generator.c_worldChunkLength;
        Vector3 spawnPosition = new Vector3(spawnValue, c_cameraStartHeight, spawnValue);
        if (PhotonNetwork.offlineMode)
        {
            spirit = Instantiate(Resources.Load<GameObject>("Spirit"), spawnPosition, Quaternion.identity);
        }
        else
        {
            spirit = PhotonNetwork.Instantiate("Spirit", spawnPosition, Quaternion.identity, 0);
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
