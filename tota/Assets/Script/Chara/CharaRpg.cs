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
    }
    //client init (appelé par CharaManager)
    public void Init(int[] quirks)
    {
        InitStat();
        InitQuirks(quirks);
        InitHealth();
        InitStatus();
    } // <-------------True Start ---------
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



    //Hunger
    private int _hunger;
    private int _maxHunger = 20;
    private void InitStatus()
    {
        _hunger = _maxHunger / 2;
    }
    //Health Attribute
    private int maxBloodStock = 3000;
    private int bloodStockGain = 30;
    private int bloodStock;
    private float _consciousness = 1f; //if reaches 0, switch to shock state, *Social/Intelligence
    private float _pain = 0f;
    private float _tempPain = 0f; //augmenté au moment d'ajouter une blesure, tend vers 0 à chaque Update
    private float _globalPainFactor = 1f;
    private float _manipulation = 1f; //*craft and construction
    private float _movement = 1f; //*walk speed, = min(feet1, leg1) + min(feet2, leg2)
    //Status
    private bool _isInShock = false;
    private bool _isDead = false;
    //Bodyparts
    private List<BodyPart> _bodyParts;

    //Health class and enum
    public enum WoundType
    {
        Fracture,  //treated by a split
        Bruise, //treated by cream1
        Burn,   //treated by cream2
        FrostBite,//treated by FIRE
        Bleeding//treated by bandage
    }
    public enum BodyType
    {
        Head,
        Torso,
        Leg,
        Feet,
        Shoulder,
        Arm,
        Hand,
    }
    public static Dictionary<WoundType, string> WoundTypeToString = new Dictionary<WoundType, string>()
    {
        { WoundType.Fracture, "Fracture" },
        { WoundType.Bruise, "Bruise" },
        { WoundType.Burn, "Burn" },
        { WoundType.FrostBite, "FrostBite" },
        { WoundType.Bleeding, "Bleeding" }
    };
    public static Dictionary<WoundType, bool> WoundTypeToIsBleed = new Dictionary<WoundType, bool>()
    {
        { WoundType.Fracture, false },
        { WoundType.Bruise, false },
        { WoundType.Burn, false },
        { WoundType.FrostBite, false },
        { WoundType.Bleeding, true }
    };
    public static Dictionary<WoundType, float> WoundTypeToPainFactor = new Dictionary<WoundType, float>()
    {
        { WoundType.Fracture, 1f },
        { WoundType.Bruise, 0.75f },
        { WoundType.Burn, 1.5f },
        { WoundType.FrostBite, 0.5f },
        { WoundType.Bleeding, 1f }
    };
    public class Wound
    {
        //Inititial attribute
        public WoundType type;
        public int damage;
        public float painFactor;
        public string origin;

        public bool isTreated = false;
        public int bloodLose = 0;
        //Constructeur
        public Wound(WoundType type, int initialDamage, string origin)
        {
            this.type = type;
            damage = initialDamage;
            painFactor = WoundTypeToPainFactor[type];
            this.origin = origin;
            if (WoundTypeToIsBleed[type])
            {
                bloodLose = initialDamage;
            }
        }
        //Treating the wound
        public void Update(bool isResting = false)
        {
            if (bloodLose != 0)
            {
                damage--;
                if (isTreated) damage--;
                if (isResting) damage -= 2;
            }
        }
        public void Treat()
        {
            isTreated = true;
            bloodLose = 0;
        }
        //Pain
        public float GetPain()
        {
            return damage * painFactor;
        }
        //End COndition
        public bool IsHealed()
        {
            return damage <= 0 && bloodLose <= 0;
        }
        //OtherGetters
        public string GetWoundInfo()
        {
            string info = WoundTypeToString[type] + " (from " + origin + ") (" + damage + " damage)";
            if (bloodLose != 0) info += " (bleeding: " + bloodLose + ")";
            return info;
        }
    }
    public class BodyPart
    {
        //Defining
        public string name;
        public BodyType bodyType;
        public int maxHp;
        public float painFactor;
        //Wounds
        public List<Wound> wounds;
        public bool isDestroyed = false;
        //Constructor
        public BodyPart(string name, BodyType bodyType, int maxHp, float painFactor)
        {
            this.name = name;
            this.bodyType = bodyType;
            this.maxHp = maxHp;
            this.painFactor = painFactor;
            wounds = new List<Wound>();
        }
        //Adding wound
        public void AddWound(Wound wound)
        {
            wounds.Add(wound);
        }
        //Updating wound
        public void Update(bool isRested)
        {
            foreach (Wound wound in wounds)
            {
                wound.Update(isRested);
                if (wound.IsHealed())
                {
                    wounds.Remove(wound);
                }
            }
        }
        public void TreatAllWoundsOfType(WoundType type)
        {
            foreach (Wound wound in wounds)
            {
                if (wound.type == type)
                {
                    wound.Treat();
                }
            }
        }
        //End Condition
        public bool CheckDestroyed()
        {
            if (isDestroyed)
            {
                return true;
            }
            else
            {
                int totalDmg = 0;
                foreach (Wound wound in wounds)
                {
                    totalDmg += wound.damage;
                }
                if (totalDmg >= maxHp)
                {
                    isDestroyed = true;
                    wounds.Clear();
                    return true;
                }
                return false;
            }
        }
        public bool IsFullyHealed()
        {
            return GetTotalDamage() == 0;
        }
        public void ForceFalloff()
        {
            isDestroyed = true;
            wounds.Clear();
        }
        //OtherGetters
        public float GetPain()
        {
            float totalPainValue = 0;
            foreach(Wound wound in wounds)
            {
                totalPainValue += wound.GetPain();
            }

            return (totalPainValue / (float)maxHp) * painFactor;
        }
        public int GetTotalBloodLose()
        {
            int totalBloodLose = 0;
            foreach (Wound wound in wounds)
            {
                totalBloodLose += wound.bloodLose;
            }
            return totalBloodLose;
        }
        public float GetFuncPurcent()
        {
            int currHp = maxHp - GetTotalDamage();
            return ((float)currHp) / ((float)maxHp);
        }
        //Info Getters
        private int GetTotalDamage()
        {
            int totalDamage = 0;
            foreach (Wound wound in wounds)
            {
                totalDamage += wound.damage;
            }
            return totalDamage;
        }
        public string GetPartInfo()
        {
            return name + "(" + GetTotalDamage() + "/" + maxHp + ")";
        }
        public string[] GetWoundsInfo()
        {
            string[] woundsInfo = new string[wounds.Count];
            Wound wound;
            for (int i = 0; i < wounds.Count; i++)
            {
                woundsInfo[i] = wounds[i].GetWoundInfo();
            }
            return woundsInfo;
        }
        public string GetMissingInfo()
        {
            return name + " has been destroyed";
        }
    }

    //Health Main
    public void InitHealth()
    {
        _bodyParts = new List<BodyPart>();
        _bodyParts.Add(new BodyPart("Head", BodyType.Head,                50, 0.9f));//0
        _bodyParts.Add(new BodyPart("Upper Torso", BodyType.Torso,       500, 0.6f));//1
        _bodyParts.Add(new BodyPart("Lower Torso", BodyType.Torso,       500, 0.6f));//2
        _bodyParts.Add(new BodyPart("Right Leg", BodyType.Leg,           300, 0.7f));//3
        _bodyParts.Add(new BodyPart("Left Leg", BodyType.Leg,            300, 0.7f));//4
        _bodyParts.Add(new BodyPart("Right Feet", BodyType.Feet,         200, 0.7f));//5
        _bodyParts.Add(new BodyPart("Left Feet", BodyType.Feet,          200, 0.7f));//6
        _bodyParts.Add(new BodyPart("Right Shoulder", BodyType.Shoulder, 400, 0.6f));//7
        _bodyParts.Add(new BodyPart("Left Shoulder", BodyType.Shoulder,  400, 0.6f));//8
        _bodyParts.Add(new BodyPart("Right Arm", BodyType.Arm,           300, 0.7f));//9
        _bodyParts.Add(new BodyPart("Left Arm", BodyType.Arm,            300, 0.7f));//10
        _bodyParts.Add(new BodyPart("Right Hand", BodyType.Hand,         200, 0.8f));//11
        _bodyParts.Add(new BodyPart("Left Hand", BodyType.Hand,          200, 0.8f));//12

        bloodStock = maxBloodStock;

        StartCoroutine(Cor_UpdateHealth());
    }
    public IEnumerator Cor_UpdateHealth()
    {
        while(true)
        {
            UpdateHealth();
            yield return new WaitForSeconds(5);
        }
    }

    public void UpdateHealth(bool isRested = false)
    {
        int totalBloodLose = 0;
        //Main Update
        foreach (BodyPart bodyPart in _bodyParts)
        {
            bodyPart.Update(isRested);
            if(!bodyPart.CheckDestroyed()) totalBloodLose += bodyPart.GetTotalBloodLose();
        }
        //Blood lose
        if (totalBloodLose == 0)
        {
            bloodStock += bloodStockGain;
            if (bloodStock > maxBloodStock) bloodStock = maxBloodStock;
        }
        else
        {
            bloodStock -= totalBloodLose;
        }
        //Check Death
        if (CheckDeath())
        {
            Die();
            return;
        }
        //Make limb falloff
        CheckFalloff();
        //Update Stats
        UpdatePain();
        UpdateMovement();


        //Debug:
        Debug.Log("CharaRpg: Blood Lost " + totalBloodLose + ", " + bloodStock + " left");
        DebugWounds();
    }
    
    public bool CheckDeath()
    {
        //Destroyed BodyPart
        if (_bodyParts[0].isDestroyed || _bodyParts[1].isDestroyed || _bodyParts[2].isDestroyed)
        {
            return true;
        }
        //BloodLose
        if (bloodStock <= 0)
        {
            return true;
        }
        //Infection and condition
        //--
        return false;
    }
    public void CheckFalloff()
    {
        if (_bodyParts[7].isDestroyed) _bodyParts[9].ForceFalloff();//Right Shoulder -> Right Arm
        if (_bodyParts[9].isDestroyed) _bodyParts[11].ForceFalloff();//Right Arm -> Right Hand

        if (_bodyParts[8].isDestroyed) _bodyParts[10].ForceFalloff();//Left Shoulder -> Left Arm
        if (_bodyParts[10].isDestroyed) _bodyParts[12].ForceFalloff();//Left Arm -> Left Hand

        if (_bodyParts[3].isDestroyed) _bodyParts[5].ForceFalloff();//Right Leg -> Right Feet
        if (_bodyParts[4].isDestroyed) _bodyParts[6].ForceFalloff();//Left Leg -> Left Feet
    }
    public void UpdatePain()
    {
        float totalPain = 0;
        foreach (BodyPart bp in _bodyParts)
        {
            totalPain += bp.GetPain();
        }

        _pain = _tempPain + totalPain;
        _tempPain = _tempPain > 0 ? _tempPain - 1 : 0;

        if (_pain > 0.9f) _isInShock = true;
    }
    public void UpdateMovement()
    {
        if (_isDead || _isInShock)
        {
            _movement = 0f;
        }
        else
        {
            _movement = 1 - 0.5f * (2 - _bodyParts[3].GetFuncPurcent() - _bodyParts[4].GetFuncPurcent())
                      - 0.25f * (2 - _bodyParts[5].GetFuncPurcent() - _bodyParts[6].GetFuncPurcent());
            if (_movement <= 0) _movement = 0f;
        }
    }

    public void Die()
    {
        //Chara dies
        Debug.Log("======================= CharaRpg: " + NameFull + " has died =======================");
    }
    //Finder
    public List<BodyPart> FindPartWithType(BodyType type)
    {
        List<BodyPart> bodyPartWithType = new List<BodyPart>();

        foreach (BodyPart bodyPart in _bodyParts)
        {
            if (bodyPart.bodyType == type)
            {
                bodyPartWithType.Add(bodyPart);
            }
        }
        return bodyPartWithType;
    }
    public BodyPart FindPartWithName(string name)
    {
        foreach (BodyPart bodyPart in _bodyParts)
        {
            if (bodyPart.name == name)
            {
                return bodyPart;
            }
        }
        return null;
    }

    //Wound
    public void AddWound(Wound wound, BodyPart bodyPart)
    {
        bodyPart.AddWound(wound);
    }
    public void AddWound(Wound wound, string bodyPartName)
    {
        FindPartWithName(bodyPartName).AddWound(wound);
    }

    public void ReceiveAddWound(int woundType, int initialDamage, string bodyPartName, string origin)
    {
        _tempPain += initialDamage;
        FindPartWithName(bodyPartName).AddWound(new Wound((WoundType)woundType, initialDamage, origin));
    }
    //Combat handler
    public void GetAttackedWith(Equipable weapon, int damage)
    {
        int bodyPart = Random.Range(1, 13); //1-12
        //<------------------Doit réduire les dégats avec la liste des protections de la partie du corps correspondant
        WoundType type;
        switch(weapon.dmgType)
        {
            case Equipable.DamageType.Mace:
                //damage -= protectionPourDamageDeTypeMace
                type = damage > 40 ? WoundType.Fracture : WoundType.Bruise;
                break;
            case Equipable.DamageType.Sharp:
                //damage -= protectionPourDamageDeTypeSharp
                type = WoundType.Bleeding;
                break;
            default:
                type = WoundType.Bruise;
                break;
        }

        Wound wound = new Wound(type, damage, weapon.nickName);

        AddWound(wound, _bodyParts[bodyPart]);
    }

    //Health Info
    public string[] GetWoundsInfo()
    {
        List<string> woundsInfo = new List<string>();
        List<string> missingInfo = new List<string>();
        //Get l'information
        //Wounds
        foreach (BodyPart bp in _bodyParts)
        {
            if (!bp.IsFullyHealed() && !bp.isDestroyed)
            {
                woundsInfo.Add(bp.GetPartInfo());
                foreach (string s in bp.GetWoundsInfo())
                {
                    woundsInfo.Add(s);
                }
            }
        }
        //Missing
        foreach (BodyPart bp in _bodyParts)
        {
            if (bp.isDestroyed)
            {
                missingInfo.Add(bp.GetMissingInfo());
            }
        }
        //Parse en array
        string[] woundsInfoArray = new string[woundsInfo.Count];
        for (int i = 0; i < woundsInfo.Count; i++)
        {
            woundsInfoArray[i] = woundsInfo[i];
        }
        return woundsInfoArray;
    }
    public void DebugWounds()
    {
        string debug = "";
        foreach (string s in GetWoundsInfo())
        {
            debug += s;
        }
        Debug.Log("DebugWounds: " + debug);

        GameObject _interface = gameObject.GetComponent<CharaInventory>().GetInterface();
        if (_interface != null)
        {
            _interface.GetComponent<InterfaceManager>().UpdateInjuries(GetWoundsInfo());
        }
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
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateSkills(GetSkillInfo());
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

    public string[] GetSkillInfo()
    {
        return new string[7]
        {
            GetCurrentStat(Stat.sk_carpenter).ToString(),
            GetCurrentStat(Stat.sk_doctor).ToString(),
            GetCurrentStat(Stat.sk_electrician).ToString(),
            GetCurrentStat(Stat.sk_farmer).ToString(),
            GetCurrentStat(Stat.sk_marksman).ToString(),
            GetCurrentStat(Stat.sk_scavenger).ToString(),
            GetCurrentStat(Stat.lv_stamina).ToString(),
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
