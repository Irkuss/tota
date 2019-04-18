using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaRpg : MonoBehaviour
{
    //Ref
    [SerializeField] private QuirkTable _quirkTable;
    private CentralManager _cm;
    //Name
    private string _nameFirst = "John";
    public string NameFirst => _nameFirst;
    private string _nameLast = "McCree";
    public string NameLast => _nameLast;
    public string NameFull => _nameFirst + " " + _nameLast;
    //Age
    private int _age;
    //Stats
    public enum Stat
    {
        //Main stats (de 1 à 100)
        ms_strength,
        ms_intelligence,
        ms_perception,
        ms_mental,
        ms_social,
        //Skills (-1 à 10) (0 de base) (-1 desactive les actions liées) 
        sk_doctor,
        sk_farmer,
        sk_carpenter,
        sk_scavenger,
        sk_electrician,
        sk_marksman,
        //Stats Level
        lv_stamina,
    }
    public const int c_statNumber = 12; //TO UPDATE WHEN ADDING NEW STATS
    public struct Stats
    {
        //Array de stat
        private int[] _stats;

        //Constructor
        public Stats(int[] stats)
        {
            _stats = stats;
        }
        //Getters
        public int GetStat(CharaRpg.Stat stat)
        {
            return _stats[(int)stat];
        }
        //Addition
        public void Add(Stats addedStats)
        {
            for (int i = 0; i < _stats.Length; i++)
            {
                _stats[i] += addedStats._stats[i];
            }
        }
        public void Remove(Stats addedStats)
        {
            for (int i = 0; i < _stats.Length; i++)
            {
                _stats[i] -= addedStats._stats[i];
            }
        }
    }
    private Stats _baseStat;
    private Stats _statModifiers;
    private List<Quirk> _quirks;
    //Status
    private int _hunger;
    private int _maxHunger = 100;

    private void Awake()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Debug.Log("Chara: Adding self to CharaManager");
            //GameObject.Find("eCentralManager").GetComponent<CharaManager>().AddToTeam(this.gameObject);
        }
    }

    //Init
    void Start()
    {
        _cm = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
        
        InitStatus();
    }
    //masterclient init
    public void Init()
    {
        InitStat();
        InitQuirks();
    }
    private void InitQuirks()
    {
        _quirks = new List<Quirk>();
        //Decide les quirks
        SetQuirks();
        //Apply quirks
        ApplyQuirks();
        //DEBUG
        string quirkList = "";
        foreach (Quirk quirk in _quirks)
        {
            quirkList += quirk.quirkName + ", ";
        }
        Debug.Log("InitQuirks: This new chara possesses those quirks: " + quirkList);
        quirkList = "";
        for (int i = 0; i < 5; i++)
        {
            quirkList += GetCurrentStat((Stat)i) + ", ";
        }
        Debug.Log("Stats: " + quirkList);
    }
    private void SetQuirks()
    {
        //Decide quirk number
        int numberPhysical = Random.Range(1, 4); //1 à 3
        int numberMental = Random.Range(2, 5);  //2 à 4
        int numberJob = Random.Range(0, 2);     //0 à 1
        int numberApocExp = Random.Range(0, 2); //0 à 1
        //add Quirk
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.Physical, numberPhysical, _quirks);
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.Mental, numberMental, _quirks);
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.OldJob, numberJob, _quirks);
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.ApocalypseExp, numberApocExp, _quirks);
    }

    //client init
    public void Init(int[] quirks)
    {
        InitStat();
        InitQuirks(quirks);
    }
    private void InitQuirks(int[] quirks)
    {
        _quirks = new List<Quirk>();
        //Deserialize quirks
        foreach(int id in quirks)
        {
            _quirks.Add(_quirkTable.IdToQuirk(id));
        }
        //Apply quirks
        ApplyQuirks();
    }

    //common part of init
    private void InitStat()
    {
        //Stat de base
        int[] baseStat = new int[c_statNumber];
        for (int i = 0; i < 5; i++) baseStat[i] = 50;
        for (int i = 5; i < c_statNumber; i++) baseStat[i] = 0;
        _baseStat = new Stats(baseStat);
        //Stat modifiante
        int[] modifStat = new int[c_statNumber];
        for (int j = 0; j < c_statNumber; j++) modifStat[j] = 0;
        _statModifiers = new Stats(modifStat);
    }
    private void ApplyQuirks()
    {
        //Apply quirks
        foreach (Quirk quirk in _quirks)
        {
            _baseStat.Add(quirk.GetStats());
        }
    }
    private void InitStatus()
    {
        _hunger = _maxHunger / 2;
    }
    //Serialize
    public int[] SerializeQuirks()
    {
        int[] serialized = new int[_quirks.Count];
        for (int i = 0; i < _quirks.Count; i++)
        {
            serialized[i] = _quirkTable.QuirkToId(_quirks[i]);
        }
        return serialized;
    }



    //Getters
    public int GetCurrentStat(Stat stat)
    {
        return _baseStat.GetStat(stat) + _statModifiers.GetStat(stat);
    }

    //Random Getters
    public bool GetCheck(Stat ms)
    {
        //Lance un dé à 100 faces (de 1 à 100)
        //Si la stat concerné est supérieur ou égal au résultat, alors c'est un réussite, sinon c'est un échec
        return Random.Range(1, 101) <= GetCurrentStat(ms);
    }

    //Action
    public void Eat(int food)
    {
        if (_hunger < _maxHunger)
        {
            _hunger += food;
            UpdateToolTip(); // On appelle l'update du tooltip
            //GetComponent<PhotonView>().RPC("SendToolTipInfo", PhotonTargets.AllBuffered, GetToolTipInfo());
        }
    }
    public void UseItem()
    {

    }

    //ToolTip
    public void UpdateToolTip()
    {
        _cm.UpdateToolTip(GetToolTipInfo());
    }
    private string[] GetToolTipInfo()
    {
        return new string[8]
        {
            NameFull,
            GetCurrentStat(Stat.ms_strength).ToString(),
            GetCurrentStat(Stat.ms_intelligence).ToString(),
            GetCurrentStat(Stat.ms_perception).ToString(),
            GetCurrentStat(Stat.ms_mental).ToString(),
            GetCurrentStat(Stat.ms_social).ToString(),
            _hunger.ToString(),
            _maxHunger.ToString()
        };
    }
    /*
    [PunRPC]
    public void SendToolTipInfo(string[] info)
    {
        Strength = int.Parse(info[1]);
        Intelligence = int.Parse(info[2]);
        Perception = int.Parse(info[3]);
        Mental = int.Parse(info[4]);
        Social = int.Parse(info[5]);
        _hunger = int.Parse(info[6]);
        _maxHunger = int.Parse(info[7]);
    }*/


}
