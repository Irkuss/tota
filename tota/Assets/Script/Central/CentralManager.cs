using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

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
    private bool online;

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

    [SerializeField] private GameObject _canvas = null;
    public GameObject Canvas { get { return _canvas; } }

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
        online = Mode.Instance.online;
        permi = PermissionsManager.Instance;
        if (online)
        {           
            player = permi.GetPlayerWithName(PhotonNetwork.player.NickName);
            if (player != null)
            {
                team = permi.GetTeamWithPlayer(player);
            }
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
        if (!Mode.Instance.online)
        {
            Save();
        }

        Application.Quit();
    }

    private void Save()
    {
        string path = Application.persistentDataPath + "save.txt";

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(_charaLayout.transform.childCount);

            foreach(Transform child in _charaLayout.transform)
            {
                GameObject chara = child.GetComponent<LinkChara>().chara;

                writer.WriteLine(chara.transform.position.x + "/" + chara.transform.position.y + "/" + chara.transform.position.z);

                CharaRpg head = chara.GetComponent<CharaRpg>();

                string quirks = "";
                //foreach(var quirk in head.GetQuirksInfo().Split(',',' '))
                //{
                //    quirks += quirk + "/";
                //}

                foreach (var quirk in head.SerializeQuirks())
                {
                    quirks += quirk + "/";
                }
                writer.WriteLine(quirks);

                writer.WriteLine(head.NameFull);

                float[] healthStats = head.UpdateStats();
                writer.WriteLine(healthStats[0] + "/" + healthStats[1] + "/" + healthStats[2] +
                    "/" + healthStats[3] + "/" + healthStats[4]);

                string[] info = head.GetToolTipInfo();
                string[] skills = head.GetSkillInfo();

                writer.WriteLine(info[0] + "/" + info[1] + "/" + info[2] + "/" + info[3] + "/" + info[4] +
                    "/" + info[5] + "/" + info[6] + "/" + info[7]);

                writer.WriteLine(skills[0] + "/" + skills[1] + "/" + skills[2] + "/" + skills[3] + "/" +
                    skills[4] + "/" + skills[5] + "/" + skills[6]);

                CharaInventory inventory = chara.GetComponent<CharaInventory>();

                string inv = "";

                foreach(var item in inventory.inventory)
                {
                    inv += item.Key.nickName + " " + item.Value + "/";
                }
                writer.WriteLine(inv);

                string wearables = "";

                foreach(var wear in inventory.wearables)
                {
                    wearables += wear.nickName;
                }
                writer.WriteLine(wearables);

                string equipments = "";

                foreach (var equip in inventory.equipments)
                {
                    equipments += equip.nickName;
                }
                writer.WriteLine(equipments);
            }
        }
    }

    private void Load(string teamName, string playerName)
    {
        string path = Application.persistentDataPath + "save.txt";
        if (!File.Exists(path)) return;

        using (StreamReader reader = new StreamReader(path))
        {
            int number = int.Parse(reader.ReadLine());
            for(int i = 0; i < number; i++)
            {
                string[] gameobj = reader.ReadLine().Split('/');
                Vector3 pos = new Vector3(int.Parse(gameobj[0]), int.Parse(gameobj[1]), int.Parse(gameobj[2]));

                string[] quirkS = reader.ReadLine().Split('/');
                int[] quirks = new int[quirkS.Length];
                for(int j = 0; j < quirkS.Length; j++)
                {
                    quirks[j] = int.Parse(quirkS[j]);
                }

                GameObject chara = gameObject.GetComponent<CharaManager>().RPC_SpawnChara(pos.x,pos.y,pos.z,teamName,quirks,playerName);
                CharaRpg rpg = chara.GetComponent<CharaRpg>();
                CharaInventory inventory = chara.GetComponent<CharaInventory>();

                string[] name = reader.ReadLine().Split(' ');
                rpg.SetIdentity(name[0], name[1]);

                string[] healthStats = reader.ReadLine().Split('/');
                rpg.SetStats(healthStats);

                string[] info = reader.ReadLine().Split('/');
                string[] skills = reader.ReadLine().Split('/');

                string[] inv = reader.ReadLine().Split('/');
                foreach(var iteM in inv)
                {
                    string[] items = iteM.Split(' ');
                    string item = items[0];                    
                    int value = int.Parse(items[1]);
                }


                string[] wearables = reader.ReadLine().Split('/');
                for(int k = 0; k < wearables.Length; k++)
                {
                    Item item = inventory.ItemTable.GetItemWithName(wearables[k]);

                    if (item != null && (Wearable) item != null)
                    {
                        inventory.wearables[k] = (Wearable) item;
                    }
                }

                string[] equipments = reader.ReadLine().Split('/');
                for (int m = 0; m < equipments.Length; m++)
                {
                    Item item = inventory.ItemTable.GetItemWithName(wearables[m]);

                    if (item != null && (Equipable)item != null)
                    {
                        inventory.equipments[m] = (Equipable)item;
                    }
                }
            }

        }
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

        //Instantiate the spirit
        GameObject spirit;
        float spawnValue = (generator.SpawnPoint + 0.5f) * Generator.c_worldChunkLength;
        Vector3 spawnPosition = new Vector3(spawnValue, c_cameraStartHeight, spawnValue);
        if (!online || PhotonNetwork.offlineMode)
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
            if (!online)
            {
                teamName = "MyTeam";
                permi.CreateTeam(teamName);
                permi.AddNewPlayerToTeam(teamName, PhotonNetwork.player.NickName, true);
            }
            else
            {
                teamName = "Team" + permi.GetNumberOfTeams();
                //Crée une nouvelle équipe avec comme nom "teamName"
                permi.GetComponent<PhotonView>().RPC("CreateTeam", PhotonTargets.AllBuffered, teamName);
                //Ajoute un nouveau joueur avec comme nom celui du client //TODO pour l'instant chaque joueur joue tout seul
                permi.GetComponent<PhotonView>().RPC("AddNewPlayerToTeam", PhotonTargets.AllBuffered, teamName, PhotonNetwork.player.NickName, true);
                
            }

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
