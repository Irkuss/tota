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
    private CharaMovement _charaMov;
    //Name
    private string _nameFirst = "John";
    public string NameFirst => _nameFirst;
    private string _nameLast = "McCree";
    public string NameLast => _nameLast;
    public string NameFull => _nameFirst + " " + _nameLast;
    //Age
    private int _age;

    //Character
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
        //Addition of Stats
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
        //Addition of to specific stats
        public void AddSpecific(Stat specifiedStat, int modifier)
        {
            _stats[(int)specifiedStat] += modifier;
        }
        //Lanbda multiply
        public void MultiplyAll(float modifier)
        {
            for (int i = 0; i < _stats.Length; i++)
            {
                _stats[i] = Mathf.FloorToInt(_stats[i] * modifier);
            }
        }
        public static Stats GetMultiplyResult(Stats stats, float modifier)
        {
            int[] newStat = new int[stats._stats.Length];
            for (int i = 0; i < newStat.Length; i++)
            {
                newStat[i] = Mathf.FloorToInt(stats._stats[i] * modifier);
            }

            return new Stats(newStat);
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
        _cm = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
    }

    //Init
    private void Start()
    {
        
        _charaMov = GetComponent<CharaMovement>();

        DayNightCycle.onNewHour += UpdateHourly;
    }
    private void OnDestroy()
    {
        DayNightCycle.onNewHour -= UpdateHourly;
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
        //stat*consciousnes+modifier
        return Stats.GetMultiplyResult(_baseStat, _consciousness).GetStat(stat) + _statModifiers.GetStat(stat);
    }
    public string GetQuirksInfo()
    {
        string info = "";

        foreach(Quirk quirk in _quirks)
        {
            info += quirk.quirkName + ", ";
        }

        return info;
    }
    //Random Getters
    public bool GetCheck(Stat ms, int modifier = 0)
    {
        //Lance un dé à 100 faces (de 1 à 100)
        //Si la stat concerné est supérieur ou égal au résultat, alors c'est un réussite, sinon c'est un échec
        return Random.Range(1, 101) <= GetCurrentStat(ms) + modifier;
    }

    //Updated Hourly
    private void UpdateHourly()
    {
        //Hunger
        _hunger = _hunger < _maxHunger ? _hunger + 1 : _maxHunger;
        //tiredness
        //temperature
        UpdateTemperature();
    }

    //Hunger and tiredness
    private int _hunger;
    private int _maxHunger = 20;
    private void InitStatus()
    {
        _hunger = _maxHunger / 5;

        _bodyTemperature = GetOutSideTemperature();
    }
    //Health Attribute
    private readonly int maxBloodStock = 2000;
    private int bloodStockGain = 30;
    private int bloodStock;
    private float _consciousness = 1f; //if reaches 0.1, switch to shock state, *Social/Intelligence
    private float _shockTreshold = 0.1f;
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
    //body temperature
    private int _bodyTemperature;
    private readonly int _minTemp = -5;
    private readonly int _maxTemp = 35;
    private int GetOutSideTemperature()
    {
        return _cm.GetTemperatureAtCoord(transform.position);
    }
    private int GetMinTempResistance()
    {
        return GetComponent<CharaInventory>().GetMinTemperatureModifier();
    }
    private void UpdateTemperature()
    {
        int outsiteTemp = GetOutSideTemperature();
        //Update bodyTemperature
        if(_bodyTemperature != outsiteTemp)
        {
            _bodyTemperature = _bodyTemperature > outsiteTemp ? _bodyTemperature - 1 : _bodyTemperature + 1;
        }
        //Update possible wound related to temperature
    }

    //BodyParts and Wound class/enum
    public enum WoundType
    {
        Fracture,  //treated by a split
        Bruise, //treated by cream1
        Burn,   //treated by cream2
        FrostBite,//treated by FIRE
        Bleeding, //treated by bandage
        DeathBite, //treated by amputation
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
        { WoundType.Bleeding, "Bleeding" },
        { WoundType.DeathBite, "Death Bite" }
    };
    public static Dictionary<WoundType, bool> WoundTypeToIsBleed = new Dictionary<WoundType, bool>()
    {
        { WoundType.Fracture, false },
        { WoundType.Bruise, false },
        { WoundType.Burn, false },
        { WoundType.FrostBite, false },
        { WoundType.Bleeding, true },
        { WoundType.DeathBite, true }
    };
    public static Dictionary<WoundType, float> WoundTypeToPainFactor = new Dictionary<WoundType, float>()
    {
        { WoundType.Fracture, 1f },
        { WoundType.Bruise, 0.75f },
        { WoundType.Burn, 1.5f },
        { WoundType.FrostBite, 0.5f },
        { WoundType.Bleeding, 1f },
        { WoundType.DeathBite, 1f }
    };
    public class Wound
    {
        //Inititial attribute
        public WoundType type;
        public int damage;
        public float painFactor;
        public string origin;
        //Treatment
        public bool isTreated = false;
        //Pain
        public int tempPain = 0;
        //Blood
        public int bloodLose = 0;
        //Infection
        public float deathInfectionLevel = -1;
        public float deathInfectionIncrement = 0;
        //Constructeur
        public Wound(WoundType type, int initialDamage, string origin, float infectionIncrement = 0)
        {
            //Attribute
            this.type = type;
            damage = initialDamage;
            painFactor = WoundTypeToPainFactor[type];
            this.origin = origin;
            //InitialPain
            //tempPain = initialDamage;
            //Bleed
            if (WoundTypeToIsBleed[type]) bloodLose = initialDamage;
            //Infection
            if (infectionIncrement > 0) deathInfectionLevel = 0;
        }
        //Treating the wound
        public void Update(bool isResting = false)
        {
            if (bloodLose != 0)
            {
                tempPain = tempPain > 0 ? tempPain - 1 : 0;
                damage--;
                if (isTreated) damage--;
                if (isResting) damage -= 2;
            }
            deathInfectionLevel += deathInfectionIncrement;
        }
        public void Treat()
        {
            isTreated = true;
            bloodLose = 0;
        }
        //Pain
        public float GetPain()
        {
            Debug.Log("GetPain: getting pain in wound -> " + (damage + tempPain) * painFactor);
            return (damage + tempPain) * painFactor;
        }
        //End COndition
        public bool IsHealed()
        {
            return damage <= 0 && bloodLose <= 0;
        }
        public bool IsFullyInfected()
        {
            return deathInfectionLevel >= 100f;
        }
        //OtherGetters
        public string GetWoundInfo()
        {
            string info = WoundTypeToString[type] + " (from " + origin + ") (" + damage + " damage)";
            if (bloodLose != 0) info += " (bleeding: " + bloodLose + ")";
            if (deathInfectionIncrement > 0) info += " (infection progress: " + deathInfectionLevel +")";
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
            List<Wound> woundToRemove = new List<Wound>();
            foreach (Wound wound in wounds)
            {
                wound.Update(isRested);
                if (wound.IsHealed())
                {
                    woundToRemove.Add(wound);
                }
            }
            //Remove
            foreach(Wound wound in woundToRemove)
            {
                wounds.Remove(wound);
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
                    ClearAllWound();
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
        public void ClearAllWound()
        {
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
            Debug.Log("GetPain: getting pain in bodypart -> " + (totalPainValue / (float)maxHp) * painFactor);
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
        public bool CheckIfInfected()
        {
            foreach(Wound wound in wounds)
            {
                if(wound.deathInfectionLevel > 0)
                {
                    return true;
                }
            }
            return false;
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
            return name + "(" + (maxHp - GetTotalDamage()) + "/" + maxHp + ")";
        }
        public string[] GetWoundsInfo()
        {
            string[] woundsInfo = new string[wounds.Count];
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
        public Wound GetFullyInfectedWound()
        {
            foreach(Wound wound in wounds)
            {
                if(wound.IsFullyInfected())
                {
                    return wound;
                }
            }
            return null;
        }
    }

    //Health Main
    private void InitHealth()
    {
        _bodyParts = new List<BodyPart>();
        _bodyParts.Add(new BodyPart("Head", BodyType.Head,                50, 0.9f));//0
        _bodyParts.Add(new BodyPart("Upper Torso", BodyType.Torso,       400, 0.6f));//1
        _bodyParts.Add(new BodyPart("Lower Torso", BodyType.Torso,       400, 0.6f));//2
        _bodyParts.Add(new BodyPart("Right Leg", BodyType.Leg,           300, 0.7f));//3
        _bodyParts.Add(new BodyPart("Left Leg", BodyType.Leg,            300, 0.7f));//4
        _bodyParts.Add(new BodyPart("Right Feet", BodyType.Feet,         150, 0.7f));//5
        _bodyParts.Add(new BodyPart("Left Feet", BodyType.Feet,          150, 0.7f));//6
        _bodyParts.Add(new BodyPart("Right Shoulder", BodyType.Shoulder, 350, 0.6f));//7
        _bodyParts.Add(new BodyPart("Left Shoulder", BodyType.Shoulder,  350, 0.6f));//8
        _bodyParts.Add(new BodyPart("Right Arm", BodyType.Arm,           300, 0.7f));//9
        _bodyParts.Add(new BodyPart("Left Arm", BodyType.Arm,            300, 0.7f));//10
        _bodyParts.Add(new BodyPart("Right Hand", BodyType.Hand,         150, 0.8f));//11
        _bodyParts.Add(new BodyPart("Left Hand", BodyType.Hand,          150, 0.8f));//12

        bloodStock = maxBloodStock;

        StartCoroutine(Cor_UpdateHealth());
    }
    private IEnumerator Cor_UpdateHealth()
    {
        while(!_isDead)
        {
            UpdateHealth();
            yield return new WaitForSeconds(10);
        }
        foreach(BodyPart bp in _bodyParts)
        {
            bp.ClearAllWound();
        }
    }

    private void UpdateHealth(bool isRested = false)
    {
        int totalBloodLose = 0;
        //Main Update
        foreach (BodyPart bodyPart in _bodyParts)
        {
            bodyPart.Update(isRested || _isInShock);
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
        if (CheckDeath()) return;
        //Make limb falloff
        CheckFalloff();
        //Update Stats
        UpdatePain();
        UpdateConsciousness();
        UpdateMovement();

        //Update l'interface santé
        UpdateInterfaceHealth();
        //Debug:
        Debug.Log("CharaRpg: Blood Lost " + totalBloodLose + ", " + bloodStock + " left, consciousness: " + _consciousness);
        DebugWounds();
    }

    public float[] UpdateStats()
    {
        return new float[5]
        {
            _pain,
            _consciousness,
            _movement,
            bloodStock,
            maxBloodStock
        };
    }

    private bool CheckDeath()
    {
        if(CheckBloodDeath())
        {
            Die();
            return true;
        }
        if (CheckInfectionDeath())
        {
            return true;
        }
        return false;
    }

    private bool CheckBloodDeath()
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
    private bool CheckInfectionDeath()
    {
        Wound infectedWound = null;

        foreach(BodyPart bodyPart in _bodyParts)
        {
            infectedWound = bodyPart.GetFullyInfectedWound();
            if (infectedWound != null) break;
        }

        if(infectedWound != null)
        {
            if(infectedWound.type == WoundType.DeathBite)
            {
                DieZombie();
                return true;
            }
            Die();
            return true;
        }
        return false;
    }
    private void CheckFalloff()
    {
        if (_bodyParts[7].isDestroyed) _bodyParts[9].ForceFalloff();//Right Shoulder -> Right Arm
        if (_bodyParts[9].isDestroyed) _bodyParts[11].ForceFalloff();//Right Arm -> Right Hand

        if (_bodyParts[8].isDestroyed) _bodyParts[10].ForceFalloff();//Left Shoulder -> Left Arm
        if (_bodyParts[10].isDestroyed) _bodyParts[12].ForceFalloff();//Left Arm -> Left Hand

        if (_bodyParts[3].isDestroyed) _bodyParts[5].ForceFalloff();//Right Leg -> Right Feet
        if (_bodyParts[4].isDestroyed) _bodyParts[6].ForceFalloff();//Left Leg -> Left Feet
    }
    private void UpdatePain()
    {
        float totalPain = 0;
        foreach (BodyPart bp in _bodyParts)
        {
            totalPain += bp.GetPain();
        }
        _pain = totalPain;
    }
    private void UpdateConsciousness()
    {
        _consciousness = 1 - _pain;

        _isInShock = _consciousness < _shockTreshold;
    }
    private void UpdateMovement()
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
        Debug.Log("UpdateMovement: movement=" + _movement + ", consciousness=" + _consciousness);
        if (_charaMov != null) _charaMov.ModifyAgentSpeed(_movement * _consciousness);
    }

    private bool CheckIfInfected()
    {
        foreach(BodyPart bp in _bodyParts)
        {
            if(bp.CheckIfInfected())
            {
                return true;
            }
        }
        return false;
    }

    public void Die()
    {
        if(CheckIfInfected())
        {
            DieZombie();
            return;
        }

        _isDead = true;
        _movement = 0;
        //Chara dies
        Debug.Log("======================= CharaRpg: " + NameFull + " has died =======================");
    }
    public void DieZombie()
    {
        _isDead = true;
        _movement = 0;
        //Chara dies by death bite infection
        Debug.Log("======================= CharaRpg: " + NameFull + " has died of death bite infection =======================");
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
        AddWound(wound, bodyPart.name);
    }
    public void AddWound(Wound wound, string bodyPartName)
    {
        SendAddWound((int)wound.type, wound.damage, bodyPartName, wound.origin);
    }

    public void SendAddWound(int woundType, int initialDamage, string bodyPartName, string origin, float infectionIncrement = 0)
    {
        GetComponent<CharaConnect>().SendMsg(
            CharaConnect.CharaCommand.ReceiveAddWound,
            new int[2] { woundType , initialDamage },
            new string[2] { bodyPartName, origin },
            new float[1] { infectionIncrement });
    }
    public void ReceiveAddWound(int woundType, int initialDamage, string bodyPartName, string origin, float infectionIncrement)
    {
        if (initialDamage <= 0) return;

        FindPartWithName(bodyPartName).AddWound(new Wound((WoundType)woundType, initialDamage, origin, infectionIncrement));

        UpdateInterfaceHealth();
    }
    public void LocalAddWound(Wound wound, BodyPart bodyPart)
    {
        if (wound.damage <= 0) return;

        FindPartWithName(bodyPart.name).AddWound(wound);

        UpdateInterfaceHealth();
    }
    //Combat handler
    public void DebugGetRandomDamage(int woundType)
    {
        Debug.Log("DebugGetRandomDamage: attack successful");
        SendAddWound(woundType, Random.Range(1, 10), _bodyParts[Random.Range(1, 13)].name, "DEBUGDAMAGE");
    }
    public void GetAttackedWith(Equipable weapon, int damage)
    {
        BodyPart bodyPart = _bodyParts[Random.Range(1, 13)]; //1-12 
        //<------------------Doit réduire les dégats avec la liste des protections de la partie du corps correspondant
        WoundType type;
        switch(weapon.dmgType)
        {
            case Equipable.DamageType.Mace:
                damage -= GetComponent<CharaInventory>().GetBodyPartMaceResistance(bodyPart.bodyType);
                type = damage > 40 ? WoundType.Fracture : WoundType.Bruise;
                break;
            case Equipable.DamageType.Sharp:
                damage -= GetComponent<CharaInventory>().GetBodyPartSharpResistance(bodyPart.bodyType);
                type = WoundType.Bleeding;
                break;
            default:
                type = WoundType.Bruise;
                break;
        }
        if (damage > 0)
        {
            Wound wound = new Wound(type, damage, weapon.nickName);

            AddWound(wound, bodyPart);
        }
    }
    public void TryDeathBite(int biteDamage)
    {
        //Choisis une partie du corps random
        BodyPart bodyPart = _bodyParts[Random.Range(1, 13)];
        //test de Force
        if (!GetCheck(Stat.ms_strength))
        {
            //Si on rate le test de Force
            //Soustrait les degats normalement (degats de type Sharp)
            biteDamage -= GetComponent<CharaInventory>().GetBodyPartSharpResistance(bodyPart.bodyType);
            if (biteDamage > 0)
            {
                //Si l'armure n'a pas absorbé le coup, on est mordu
                Debug.Log("TryDeathBite: " + NameFull + " got bitten!");
                LocalAddWound(new Wound(WoundType.DeathBite, biteDamage, "zombie", 1.2f), bodyPart);
            }
        }
    }

    //Health Info
    public string[] GetWoundsInfo()
    {
        if (_isDead) return new string[1] { "Dead" };


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
        if (debug == "") debug = " healthy!";
        Debug.Log("DebugWounds: " + debug);
    }
    public void UpdateInterfaceHealth()
    {
        GameObject _interface = GetComponent<CharaInventory>().GetInterface();
        if (_interface != null)
        {
            _interface.GetComponent<InterfaceManager>().UpdateInjuries(GetWoundsInfo());
            _interface.GetComponent<InterfaceManager>().UpdateStats(UpdateStats());
        }
    }
    
    //Action
    public bool Eat(int food)
    {
        if (_hunger > 0)
        {
            _hunger -= food;
            _hunger = _hunger < 0 ? 0 : _hunger;
            UpdateToolTip(); // On appelle l'update du tooltip
            gameObject.GetComponent<CharaInventory>().UpdateStats(GetToolTipInfo());
            //GetComponent<PhotonView>().RPC("SendToolTipInfo", PhotonTargets.AllBuffered, GetToolTipInfo());
            return true;
        }
        return false;
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
    
    //Names static manager
    public static string GetRandomFirstName()
    {
        using (StreamReader prenoms = new StreamReader("Assets/Resources/Database/prenoms.txt"))
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
        using (StreamReader noms = new StreamReader("Assets/Resources/Database/noms.txt"))
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
