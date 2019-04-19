using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharaRpg : MonoBehaviour
{
    //Ref
    [SerializeField] private QuirkTable _quirkTable;
    private static QuirkTable quirkTable;
    private CentralManager _cm;
    //Name
    private string _nameFirst = "John";
    public string NameFirst => _nameFirst;
    private string _nameLast = "McCree";
    public string NameLast => _nameLast;
    public string NameFull => _nameFirst + " " + _nameLast;
    //Age
    private int _age;

    //Character
    public static StreamReader prenoms = new StreamReader("Assets/Resources/Database/prenoms.txt");
    public static StreamReader noms = new StreamReader("Assets/Resources/Database/noms.txt");

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
    
    //Status Santé WIP
    private int _hunger;
    private int _maxHunger = 100;

    private float _consciousness = 100f; //if reaches 0, die
    private float _movement = 100f; //
    private float _totalPain = 10f;
    private List<BodyPart> _bodyParts;


    public class Wound
    {
        public enum WoundType
        {
            Burn,   //treated by burn cream (infect)
            Break,  //treated by a split (attelle)
            Bruise, //treated by cream
            Bite,   //treated by bandage (bleed) (infect)
            GunShot,//have to be operated (bleed) (infect)
            Cut,    // (bleed) (infect)
            Stab,   //(bleed) (infect)
        }
        public enum WoundSeverity
        {
            minor,
            moderate,
            serious,
            severe,
            critical,
            maximal

        }
        private WoundType _woundType;

        public Wound(WoundType woundType)
        {
            _woundType = woundType;
        }
    }

    public class BodyPart
    {
        private List<Wound> _wounds;
        private string _partName;

        public BodyPart(string name)
        {
            _partName = name;
            _wounds = new List<Wound>();
        }
    }

    public class Leg : BodyPart
    {
        public Leg(string name) : base(name)
        {
            
        }
    }

    public void InitHealth()
    {
        _bodyParts = new List<BodyPart>();
        _bodyParts.Add(new Leg("Right leg"));
    }

    //Init Awake
    private void Awake()
    {
        quirkTable = _quirkTable;
        _nameFirst = GetRandomFirstName();
        _nameLast = GetRandomLastName();
    }

    //Init
    void Start()
    {
        _cm = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
        
        InitStatus();
    }
    //client init (appelé par CharaManager)
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
            gameObject.GetComponent<CharaInventory>().UpdateStats(GetToolTipInfo());
            //GetComponent<PhotonView>().RPC("SendToolTipInfo", PhotonTargets.AllBuffered, GetToolTipInfo());
        }
    }
    public void UseItem()
    {

    }

    //ToolTip
    public void UpdateToolTip()
    {
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(GetToolTipInfo());
    }
    public string[] GetToolTipInfo()
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

    public static string GetRandomFirstName()
    {
        using (prenoms)
        {
            int index = Random.Range(1, 12437) + 1;
            for(int i = 0; i < index; i++)
            {
                prenoms.ReadLine();
            }
            return prenoms.ReadLine();
        }
    }

    public static string GetRandomLastName()
    {
        using (noms)
        {
            int index = Random.Range(1, 1000) + 1;
            for (int i = 0; i < index; i++)
            {
                noms.ReadLine();
            }
            return noms.ReadLine();
        }
    }
}
