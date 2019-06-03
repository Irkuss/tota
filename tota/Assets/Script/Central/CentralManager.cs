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
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject _charaRef = null;
    [SerializeField] private GameObject _options = null;
    [SerializeField] private GameObject _save = null;
    [SerializeField] private GameObject _quit = null;    

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
    public GameObject Channels { get { return _channel; } }

    [SerializeField] private GameObject _tooltipL = null;
    [SerializeField] private GameObject _background = null;

    [SerializeField] private GameObject _build = null;
    public GameObject Build { get { return _build; } }

    [SerializeField] private GameObject _actions = null;
    public GameObject Actions { get { return _actions; } }

    [SerializeField] private GameObject _button = null;
    public GameObject Button { get { return _button; } }

    [SerializeField] private GameObject _canvas = null;
    public GameObject Canvas { get { return _canvas; } }

    [SerializeField] private GameObject _tuto = null;
    public GameObject Tuto { get { return _tuto; } }

    [SerializeField] private RecipeTable _visuData = null;
    public RecipeTable VisuData { get { return _visuData; } }

    [SerializeField] private GameObject _description = null;
    public GameObject Description { get { return _description; } }

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
        _background.SetActive(!_background.activeSelf);
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
        if (Channel.isWriting) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
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
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void Options()
    {
        pauseMenu.SetActive(false);
        _options.SetActive(true);
    }

    public void Controls()
    {
        Move();
        Interface();
        ChannelOption();
        BuildOption();
    }

    public void Move()
    {
        if(_options.transform.GetChild(0).GetChild(0).GetComponent<InputField>().text == "")
        {
            _options.transform.GetChild(0).GetChild(0).GetComponent<InputField>().text = Mode.Instance.zqsd ? "z" : "w";
            return;
        }

        if (_options.transform.GetChild(0).GetChild(0).GetComponent<InputField>().text == "z")
        {
            Mode.Instance.zqsd = true;
        }
    }

    public void Interface()
    {
        if(_options.transform.GetChild(1).GetChild(0).GetComponent<InputField>().text == "")
        {
            _options.transform.GetChild(1).GetChild(0).GetComponent<InputField>().text = Mode.Instance.interfaCe;
            return;
        }

        Mode.Instance.interfaCe = _options.transform.GetChild(1).GetChild(0).GetComponent<InputField>().text;
    }

    public void ChannelOption()
    {
        if (_options.transform.GetChild(2).GetChild(0).GetComponent<InputField>().text == "")
        {
            _options.transform.GetChild(2).GetChild(0).GetComponent<InputField>().text = Mode.Instance.channel;
            return;
        }

        Mode.Instance.channel = _options.transform.GetChild(2).GetChild(0).GetComponent<InputField>().text;
    }

    public void BuildOption()
    {
        if (_options.transform.GetChild(3).GetChild(0).GetComponent<InputField>().text == "")
        {
            _options.transform.GetChild(3).GetChild(0).GetComponent<InputField>().text = Mode.Instance.build;
            return;
        }

        Mode.Instance.build = _options.transform.GetChild(3).GetChild(0).GetComponent<InputField>().text;
    }

    public void Quit()
    {
        if (!Mode.Instance.online)
        {
            pauseMenu.SetActive(false);
            _save.SetActive(true);
            _quit.SetActive(true);
        }

        Application.Quit();
    }

    public void CallSave()
    {
        Save();
        Application.Quit();
    }

    private void Save()
    {
        string path = Application.persistentDataPath + "/save.txt";
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(System.DateTime.Now);
            writer.WriteLine(_charaLayout.transform.childCount);

            foreach(Transform child in _charaLayout.transform)
            {
                GameObject chara = child.GetComponent<LinkChara>().chara;

                writer.WriteLine(chara.transform.position.x + "/" + chara.transform.position.y + "/" + chara.transform.position.z);

                CharaRpg head = chara.GetComponent<CharaRpg>();

                string quirks = "";
                int[] serializeQuirks = head.SerializeQuirks();
                for(int i = 0; i < serializeQuirks.Length - 1; i++)
                {
                    quirks += serializeQuirks[i] + "/";
                }
                quirks += serializeQuirks[serializeQuirks.Length -1];
                writer.WriteLine(quirks);

                writer.WriteLine(head.NameFull);

                float[] healthStats = head.GetHealthStats();
                writer.WriteLine(healthStats[0] + "/" + healthStats[1] + "/" + healthStats[2] +
                    "/" + healthStats[3] + "/" + healthStats[4] + "/" + healthStats[6]);

                string[] info = head.GetToolTipInfo();
                string[] skills = head.GetSkillInfo();

                writer.WriteLine(info[1] + "/" + info[2] + "/" + info[3] + "/" + info[4] +
                    "/" + info[5] + "/" + info[6] + "/" + info[7]);

                writer.WriteLine(skills[0] + "/" + skills[1] + "/" + skills[2] + "/" + skills[3] + "/" +
                    skills[4] + "/" + skills[5] + "/" + skills[6]);

                CharaInventory inventory = chara.GetComponent<CharaInventory>();

                string inv = "";

                foreach(var item in inventory.inventory)
                {
                    inv += item.Key.nickName + " " + item.Value + "/";
                }
                if (inv == "")
                {
                    writer.WriteLine("null");
                }
                else
                {
                    writer.WriteLine(inv);
                }                

                string wearables = "";

                Wearable[] wear = inventory.wearables;
                for (int d = 0; d < wear.Length -1; d++)
                {
                    if (wear[d] == null)
                    {
                        wearables += "null/";
                    }
                    else
                    {
                        wearables += wear[d].nickName + "/";
                    }
                }
                if (wear[wear.Length - 1] == null)
                {
                    wearables += "null";
                }
                else
                {
                    wearables += wear[wear.Length -1].nickName;
                }
                writer.WriteLine(wearables);

                string equipments = "";

                Equipable[] equip = inventory.equipments;
                for (int e = 0; e < equip.Length - 1; e++)
                {
                    if (equip[e] == null)
                    {
                        equipments += "null/";
                    }
                    else
                    {
                        equipments += equip[e].nickName + "/";
                    }
                }
                if (equip[equip.Length - 1] == null)
                {
                    equipments += "null";
                }
                else
                {
                    equipments += equip[equip.Length - 1].nickName;
                }
                writer.WriteLine(equipments);
                writer.WriteLine();
            }
        }
    }

    private void Load(string teamName, string playerName)
    {
        string path = Application.persistentDataPath + "/save.txt";
        if (!File.Exists(path)) return;

        using (StreamReader reader = new StreamReader(path))
        {
            reader.ReadLine();
            int number = int.Parse(reader.ReadLine());
            for(int i = 0; i < number; i++)
            {
                string[] gameobj = reader.ReadLine().Split('/');
                Vector3 pos = new Vector3(float.Parse(gameobj[0]), float.Parse(gameobj[1]), float.Parse(gameobj[2]));

                string[] quirkS = reader.ReadLine().Split('/');
                List<int> listQuirk = new List<int>();
                for(int j = 0; j < quirkS.Length; j++)
                {
                    if(int.TryParse(quirkS[j],out int q))
                    {
                        listQuirk.Add(q);
                    }
                }

                GameObject chara = gameObject.GetComponent<CharaManager>().RPC_SpawnChara(pos.x,pos.y,pos.z,teamName,listQuirk.ToArray(),playerName);
                CharaRpg rpg = chara.GetComponent<CharaRpg>();
                CharaInventory inventory = chara.GetComponent<CharaInventory>();

                string[] name = reader.ReadLine().Split(' ');
                rpg.SetIdentity(name[0], name[1]);

                string[] healthStats = reader.ReadLine().Split('/');
                rpg.SetHealthStats(healthStats);

                string[] info = reader.ReadLine().Split('/');
                string[] skills = reader.ReadLine().Split('/');
                int length = info.Length;
                int[] setInfo = new int[length + skills.Length];
                for(int g = 0; g < length; g++)
                {
                    setInfo[g] = int.Parse(info[g]);
                }
                for (int g = 0; g < skills.Length; g++)
                {
                    setInfo[g + length] = int.Parse(skills[g]);
                }
                rpg.ForceStats(setInfo);
                rpg.UpdateToolTip();

                string[] inv = reader.ReadLine().Split('/');
                foreach(var iteM in inv)
                {
                    string[] items = iteM.Split(' ');
                    if(items.Length >= 2)
                    {
                        string stringItem = "";
                        for(int y = 0; y < items.Length - 2; y++)
                        {
                            stringItem += items[y] + " ";
                        }
                        stringItem += items[items.Length - 2];

                        Item item = inventory.ItemTable.GetItemWithName(stringItem);
                        int value = int.Parse(items[items.Length -1]);

                        if(item != null)
                        {
                            for(int z = 0; z < value; z++)
                            {
                                inventory.Add(item);
                            }
                        }
                    }                    
                }

                string[] wearables = reader.ReadLine().Split('/');
                for(int k = 0; k < wearables.Length; k++)
                {
                    if(wearables[k] == "null")
                    {
                        inventory.wearables[k] = null;
                    }
                    else
                    {
                        Item item = inventory.ItemTable.GetItemWithName(wearables[k]);

                        if (item != null && (Wearable) item != null)
                        {
                            inventory.wearables[k] = (Wearable) item;
                        }
                    }
                    
                }

                string[] equipments = reader.ReadLine().Split('/');
                for (int m = 0; m < equipments.Length; m++)
                {
                    if(equipments[m] == "null")
                    {
                        inventory.equipments[m] = null;
                    }
                    else
                    {
                        Item item = inventory.ItemTable.GetItemWithName(equipments[m]);

                        if (item != null && (Equipable) item != null)
                        {
                            inventory.equipments[m] = (Equipable) item;
                        }
                    }
                    
                }

                reader.ReadLine();
            }

        }
    }

    //Temperature getters
    public int GetTemperatureAtCoord(Vector3 pos)
    {
        //Called by charaRpg
        return dayNightCycle.GetCurrentTemperature() + GetComponent<Generator>().GetPosTempModifier(pos);
    }

    //Special Callbacks
    public void OnGenerationFinished()
    {
        //Appelé par Generator/Start/*Received Package*/GenerateEnd une fois que le monde s'est généré
        if (Mode.Instance.online)
        {
            tempButton.SetActive(true);
            Mode.Instance.isSkip = true;
        }
        else
        {
            _tuto.transform.parent.gameObject.SetActive(true);
            _tuto.transform.GetChild(0).GetComponent<Text>().text = "Welcome in the solo mode of Tales of the Apocalypse. This is a short tutorial for you to understand the main commands of the game";
            _tuto.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(SpawnButton);
            _tuto.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate
            {
                Mode.Instance.isSkip = true;
                SpawnButton();
            });
        }       
    }

    private void SpawnButton()
    {
        tempButton.SetActive(true);
        _tuto.transform.parent.gameObject.SetActive(false);
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
                spirit.GetComponent<SpiritHead>().TryCharaSpawn(true);
            }
        }

        if (!online)
        {
            if (Mode.Instance.load)
            {
                Load(teamName, player.Name);
            }
        }
                
    }
}
