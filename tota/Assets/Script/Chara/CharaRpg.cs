using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharaRpg : MonoBehaviour
{
    //====================Structure====================
    //Stat Structure
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

    public static Dictionary<Stat, string> statToString = new Dictionary<Stat, string>
    {
        //Main stats (de 1 à 100)
        { Stat.ms_strength, "Strength"},
        { Stat.ms_intelligence, "Intelligence"},
        { Stat.ms_perception, "Perception"},
        { Stat.ms_mental, "Mental"},
        { Stat.ms_social, "Social"},
        //Skills (-1 à 10) (0 de base) (-1 desactive les actions liées) 
        { Stat.sk_doctor, "Doctor"},
        { Stat.sk_farmer, "Farmer"},
        { Stat.sk_carpenter, "Carpenter"},
        { Stat.sk_scavenger, "Scavenger"},
        { Stat.sk_electrician, "Electrician"},
        { Stat.sk_marksman, "Marksman"},
        //Stats Level
        { Stat.lv_stamina, "Stamina"},
    };

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

    //Health Structure
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
    public class Wound
    {
        //Inititial attribute
        public WoundInfo.WoundType type;
        public int damage;
        public float painFactor;
        public string origin;
        //Treatment
        public bool needToBeOperated = false;
        public bool isTreated = false;
        //Pain
        public int tempPain = 0;
        //Blood
        public int bloodLose = 0;
        //Infection
        public float deathInfectionLevel = -1;
        public float deathInfectionIncrement = 0;
        //Constructeur
        public Wound(WoundInfo.WoundType type, int initialDamage, string origin, float infectionIncrement = 0)
        {
            //Attribute
            this.type = type;
            damage = initialDamage;
            painFactor = woundTable.GetInfo(type).painFactor;
            this.origin = origin;
            this.needToBeOperated = woundTable.GetInfo(type).hasToBeOperated;
            //InitialPain
            tempPain = initialDamage;
            //Bleed
            if (woundTable.GetInfo(type).makesBleed) bloodLose = initialDamage;
            //Infection
            deathInfectionIncrement = infectionIncrement;
            if (infectionIncrement > 0) deathInfectionLevel = 0;
        }
        //Treating the wound
        public void Update(bool isResting, bool isBelowMinTemp)
        {
            //Temperature temporaire
            tempPain = tempPain > 0 ? tempPain - 1 : 0;
            //Frostbite increment
            if (type == WoundInfo.WoundType.FrostBite && isBelowMinTemp)
            {
                damage++;
                return;
            }
            //Healing process (impossible if bleeding)
            if (bloodLose == 0)
            {
                damage--;
                if (isTreated) damage--;
                if (isResting) damage -= 2;
            }
            //Infection level
            deathInfectionLevel += deathInfectionIncrement;
            if (deathInfectionLevel > 100f) deathInfectionLevel = 100f;
        }
        public void Treat()
        {
            isTreated = true;
            bloodLose = 0;
        }
        //Pain
        public float GetPain()
        {
            //Debug.Log("GetPain: getting pain in wound -> " + (damage + tempPain) * painFactor);
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
            string info = woundTable.GetInfo(type).nickName + " (" + origin + ") (dmg: " + damage + ")";
            if (bloodLose != 0) info += " (bleed: " + bloodLose + ")";
            if (deathInfectionIncrement > 0) info += " (infection: " + deathInfectionLevel + "%)";
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
        public void Update(bool isRested, bool isBelowMinTemp)
        {
            List<Wound> woundToRemove = new List<Wound>();
            foreach (Wound wound in wounds)
            {
                wound.Update(isRested, isBelowMinTemp);
                if (wound.IsHealed())
                {
                    woundToRemove.Add(wound);
                }
            }
            //Remove
            foreach (Wound wound in woundToRemove)
            {
                wounds.Remove(wound);
            }
        }
        public void TreatAllWoundsOfType(WoundInfo.WoundType type)
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
            foreach (Wound wound in wounds)
            {
                totalPainValue += wound.GetPain();
            }
            //Debug.Log("GetPain: getting pain in bodypart -> " + (totalPainValue / (float)maxHp) * painFactor);
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
            if (isDestroyed) return 0;

            int currHp = maxHp - GetTotalDamage();
            return ((float)currHp) / ((float)maxHp);
        }
        public bool CheckIfInfected()
        {
            foreach (Wound wound in wounds)
            {
                if (wound.deathInfectionLevel > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public int GetCountWoundsOfType(WoundInfo.WoundType type)
        {
            int count = 0;
            foreach (Wound wound in wounds)
            {
                if (wound.type == type)
                {
                    count++;
                }
            }
            return count;
        }
        public bool HasWoundsOfType(WoundInfo.WoundType type)
        {
            foreach (Wound wound in wounds)
            {
                //A: le type correspond, B: on saigne, C: le type est Bleeding
                if (wound.type == type && (wound.bloodLose != 0 || type != WoundInfo.WoundType.Bleeding))
                {
                    return true;
                }
            }
            return false;
        }
        public float GetMaxInfection()
        {
            float maxInfectionLevel = 0f;

            foreach (Wound wound in wounds)
            {
                if (maxInfectionLevel < wound.deathInfectionLevel)
                {
                    maxInfectionLevel = wound.deathInfectionLevel;
                }
            }
            return maxInfectionLevel;
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
            foreach (Wound wound in wounds)
            {
                if (wound.IsFullyInfected())
                {
                    return wound;
                }
            }
            return null;
        }
    }


    //====================Static Methods====================
    //Stat Helper
    public static bool IsMainStat(Stat stat)
    {
        return (int)stat < 5;
    }
    //Float Helper
    private static bool FloatIsSup(float a, float b, float epsilon)
    {
        return a > b && a > b + epsilon;
    }
    //Names static Generation
    private static string GetRandomFirstName()
    {
        using (StreamReader prenoms = new StreamReader("Assets/Resources/Database/prenoms.txt"))
        {
            int index = Random.Range(1, 12437) + 1;
            for (int i = 0; i < index; i++)
            {
                prenoms.ReadLine();
            }
            return prenoms.ReadLine();
        }
    }
    private static string GetRandomLastName()
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


    //====================Attribute====================
    //Reference
    [SerializeField] private QuirkTable _quirkTable;
    private static QuirkTable quirkTable;
    [SerializeField] private WoundTable _woundTable;
    public static WoundTable woundTable;
    
    private CentralManager _cm;
    private CharaMovement _charaMov;
    
    //Identité
    private string _nameFirst = "John";
    public string NameFirst => _nameFirst;

    private string _nameLast = "McCree";
    public string NameLast => _nameLast;

    public string NameFull => _nameFirst + _nameLast;

    public void SetIdentity(string first, string last)
    {
        _nameFirst = first;
        _nameLast = last;
    }

    //Quirk
    private List<Quirk> _quirks;

    //Stats
    private Stats _baseStat;


    //====================Init====================
    //Init Awake
    private void Awake()
    {
        woundTable = _woundTable;
        quirkTable = _quirkTable;
        _nameFirst = GetRandomFirstName();
        _nameLast = GetRandomLastName();
        _cm = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
    }
    private void Start()
    {
        _charaMov = GetComponent<CharaMovement>();

        DayNightCycle.onNewHour += UpdateHourly;
    }
    private void OnDestroy()
    {
        DayNightCycle.onNewHour -= UpdateHourly; //Making sure there is no possible memory leak
    }

    //client init (appelé par CharaManager)
    public void Init(int[] quirks)
    {
        InitStat();
        InitQuirks(quirks);
        InitHealth();
    }
    private void InitQuirks(int[] quirks)
    {
        _quirks = new List<Quirk>();
        //Deserialize quirks
        foreach (int id in quirks)
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

    //Force setters
    public void ForceStats(int[] statsForced)
    {
        //Force stats when loading a saved chara

        int[] baseStat = new int[c_statNumber];
        for (int i = 0; i < 5; i++) baseStat[i] = statsForced[i];

        _hunger = statsForced[5];        

        baseStat[(int)Stat.sk_carpenter] = statsForced[7];
        baseStat[(int)Stat.sk_doctor] = statsForced[8];
        baseStat[(int)Stat.sk_electrician] = statsForced[9];
        baseStat[(int)Stat.sk_farmer] = statsForced[10];
        baseStat[(int)Stat.sk_marksman] = statsForced[11];
        baseStat[(int)Stat.sk_scavenger] = statsForced[12];

        //no stamina here
    }
    public void SetHealthStats(string[] stats)
    {
        _pain = float.Parse(stats[0]);
        _consciousness = float.Parse(stats[1]);
        _movement = float.Parse(stats[2]);
        _manipulation = float.Parse(stats[3]);
        _bloodStock = int.Parse(stats[4]);
    }


    //====================Stats Getters====================
    //Stable getters
    public int GetCurrentStat(Stat stat)
    {
        if(IsMainStat(stat))
        {
            return Stats.GetMultiplyResult(_baseStat, _consciousness).GetStat(stat);
        }
        return _baseStat.GetStat(stat);
    }
    
    public float GetTimeModifier(Stat stat)
    {
        float statValue = GetCurrentStat(stat);
        Debug.Log("GetTimeModifier: statValue " + statValue);
        if (IsMainStat(stat))
        {
            Debug.Log("GetTimeModifier: returning " + (1.75f - statValue * 0.015f));
            return 1.75f - statValue * 0.015f; //0 -> 1.75, 50 -> 1, 100 -> 0.25
        }
        Debug.Log("GetTimeModifier: returning " + (1f - statValue * 0.075f));
        return 1f - statValue * 0.075f; //0 -> 1, 10 -> 0.25
    }

    public float GetStrengthModifier()
    {
        return  0.25f + GetCurrentStat(Stat.ms_strength) * 0.015f; //0 -> 0.25, 50 -> 1, 100 -> 1.75
    }

    //Dice throwers
    public bool GetCheck(Stat ms, int modifier = 0)
    {
        //Lance un dé à 100 faces (de 1 à 100)
        //Si la stat concerné est supérieur ou égal au résultat, alors c'est un réussite, sinon c'est un échec
        return Random.Range(1, 101) <= GetCurrentStat(ms) + modifier;
    }


    //====================Experience System====================
    //Experience Attribute
    private Dictionary<Stat, float> _statExp = new Dictionary<Stat, float>
    {
        //Main stats (de 1 à 100)
        { Stat.ms_strength, 0},
        { Stat.ms_intelligence, 0},
        { Stat.ms_perception, 0},
        { Stat.ms_mental, 0},
        { Stat.ms_social, 0},
        //Skills (-1 à 10) (0 de base) (-1 desactive les actions liées) 
        { Stat.sk_doctor, 0},
        { Stat.sk_farmer, 0},
        { Stat.sk_carpenter, 0},
        { Stat.sk_scavenger, 0},
        { Stat.sk_electrician, 0},
        { Stat.sk_marksman, 0},
        //Stats Level
        { Stat.lv_stamina, 0},
    };


    //Experience Getters (%)
    private float GetTrainingPurcent(Stat trainedStat)
    {
        return _statExp[trainedStat] / 10 * (1 + GetCurrentStat(trainedStat));
    }

    //Training action
    public void TrainStat(Stat trainedStat, float trainValue)
    {
        if(trainValue > 0)
        {
            GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.TrainStat, new int[1] { (int)trainedStat }, null, new float[1] { trainValue });
        }
    }
    public void RPC_TrainStat(Stat trainedStat, float trainValue)
    {
        //Called with CharaCommand.TrainStat
        _statExp[trainedStat] += trainValue;

        UpdateStatTraining(trainedStat);

        Debug.Log("TrainStat: " + 
            statToString[trainedStat] + " is now " + _statExp[trainedStat] + 
            " (added " + trainValue + ")-> " + GetTrainingPurcent(trainedStat) + "%");
    }
    //Level up update
    private void UpdateStatTraining(Stat trainedStat)
    {
        if(_statExp[trainedStat] > 10 * (1 + GetCurrentStat(trainedStat)) )
        {
            _statExp[trainedStat] = 0;
            _baseStat.AddSpecific(trainedStat, 1);
            Debug.Log("TrainStat: " + statToString[trainedStat] + " Level up !");
            UpdateToolTip();
        }
    }


    //====================Health System====================

    //==========Health attribute==========
    //Health Status
    private bool _isInShock = false;

    private bool _isDead = false;
    public bool IsDead => _isDead;

    //Bodyparts (also holds wounds)
    private List<BodyPart> _bodyParts;

    //Hunger
    private const int c_maxHunger = 20;

    private int _hunger = c_maxHunger / 5;
    public int Hunger => _hunger;

    //Temperature
    private readonly int _minTemp = -5;
    private readonly int _maxTemp = 35;

    private float _feltTemperature;

    //Consciousness
    private float _consciousness = 1f; //if reaches 0.1, switch to shock state, *Social/Intelligence
    private float _shockTreshold = 0.1f;

    //Rest
    private const int c_secInHour = 41;
    private static int maxRest = 18 * c_secInHour; //16 * 41 (41 -> nb de sec en 1h)
    private static int maxSleepDeprivationProgressBeforeNextLevel = 24 * c_secInHour;

    private int _rest = 12 * c_secInHour;
    public float RestPurcent => _rest / maxRest;

    private int _sleepDeprivationLevel = 0;
    private int _sleepDeprivationProgress = 0;

    //Pain
    private float _pain = 0f;
    private float _globalPainFactor = 1f;

    //Blood
    private readonly int _maxBloodStock = 2000; //Devrait etre 5000mL irl
    private int _bloodStockGain = 30;
    private int _bloodStock;

    //Manipulation
    private float _manipulation = 1f; //*craft and construction
    public float Manipulation => _manipulation;

    //Movement
    private float _movement = 1f; //*walk speed, = min(feet1, leg1) + min(feet2, leg2)
    
    //==========Health init==========
    private void InitHealth()
    {
        _bodyParts = new List<BodyPart>();
        _bodyParts.Add(new BodyPart("Head", BodyType.Head, 50, 0.9f));//0
        _bodyParts.Add(new BodyPart("Upper Torso", BodyType.Torso, 400, 0.6f));//1
        _bodyParts.Add(new BodyPart("Lower Torso", BodyType.Torso, 400, 0.6f));//2
        _bodyParts.Add(new BodyPart("Right Leg", BodyType.Leg, 300, 0.7f));//3
        _bodyParts.Add(new BodyPart("Left Leg", BodyType.Leg, 300, 0.7f));//4
        _bodyParts.Add(new BodyPart("Right Feet", BodyType.Feet, 150, 0.7f));//5
        _bodyParts.Add(new BodyPart("Left Feet", BodyType.Feet, 150, 0.7f));//6
        _bodyParts.Add(new BodyPart("Right Shoulder", BodyType.Shoulder, 350, 0.6f));//7
        _bodyParts.Add(new BodyPart("Left Shoulder", BodyType.Shoulder, 350, 0.6f));//8
        _bodyParts.Add(new BodyPart("Right Arm", BodyType.Arm, 300, 0.7f));//9
        _bodyParts.Add(new BodyPart("Left Arm", BodyType.Arm, 300, 0.7f));//10
        _bodyParts.Add(new BodyPart("Right Hand", BodyType.Hand, 150, 0.8f));//11
        _bodyParts.Add(new BodyPart("Left Hand", BodyType.Hand, 150, 0.8f));//12

        _bloodStock = _maxBloodStock;

        InitFeltTemperature();

        StartCoroutine(Cor_UpdateHealth());
    }
    
    private void InitFeltTemperature()
    {
        _feltTemperature = GetOutSideTemperature();
    }

    //==========Health update==========
    //Hunger update
    private void UpdateHourly()
    {
        //Gain Hunger each hour
        _hunger = _hunger < c_maxHunger ? _hunger + 1 : c_maxHunger;
    }

    //Main Coroutine (see here to change health update delay)
    private IEnumerator Cor_UpdateHealth()
    {
        while (!_isDead)
        {
            UpdateHealth();
            yield return new WaitForSeconds(1);
        }
        //Death handler
        foreach (BodyPart bp in _bodyParts)
        {
            bp.ClearAllWound();
        }
    }

    //Main HealthUpdate (called every 1 second)
    private void UpdateHealth()
    {
        UpdateWounds();
        UpdateTemperature();
        UpdateTiredness();

        //Update things that comes from updates above
        UpdateHealthStatus();
    }

    //Wound Update
    private int cycleBeforeUpdatingWounds = 0;

    private void UpdateWounds()
    {
        if (cycleBeforeUpdatingWounds > 0)
        {
            cycleBeforeUpdatingWounds--;
            return;
        }
        cycleBeforeUpdatingWounds = 10;

        bool isRested = false;
        int totalBloodLose = 0;
        foreach (BodyPart bodyPart in _bodyParts)
        {
            bodyPart.Update(isRested || _isInShock, IsBelowMinTemp());
            if (!bodyPart.CheckDestroyed()) totalBloodLose += bodyPart.GetTotalBloodLose();
        }
        //Blood lose
        if (totalBloodLose == 0)
        {
            _bloodStock += _bloodStockGain;
            if (_bloodStock > _maxBloodStock) _bloodStock = _maxBloodStock;
        }
        else
        {
            _bloodStock -= totalBloodLose;
        }
    }

    //Temperature Update
    private int _cycleBeforeAddingFrostbite = 0;

    private void UpdateTemperature()
    {
        int outsiteTemp = GetOutSideTemperature();
        //Update bodyTemperature
        if (_feltTemperature != outsiteTemp)
        {
            _feltTemperature = FloatIsSup(_feltTemperature, outsiteTemp, 1) ? _feltTemperature - 0.5f : _feltTemperature + 0.5f;
        }
        //Update possible wound related to temperature
        if (IsBelowMinTemp()) //Si il fait trop froid
        {
            if (_cycleBeforeAddingFrostbite == 0)
            {
                _cycleBeforeAddingFrostbite = 50;
                //Ajoute une Frostbite
                AddRandomFrostBite();
            }
            else
            {
                _cycleBeforeAddingFrostbite--;
            }
        }

    }
    private bool IsBelowMinTemp()
    {
        return _feltTemperature + GetMinTempResistance() <= _minTemp;
    }
    
    private void AddRandomFrostBite()
    {
        if (!PhotonNetwork.isMasterClient) return;

        Debug.Log("AddRandomFrostBite: Adding Frostbite wound to " + NameFull);

        BodyPart bodyPart = _bodyParts[Random.Range(3, 13)]; //3-12 (not taking head nor torso

        Wound frostBiteWound = new Wound(WoundInfo.WoundType.FrostBite, 5, "(Cold Temperature)");

        AddWound(frostBiteWound, bodyPart);

    }

    //Tiredness Update
    private void UpdateTiredness()
    {
        if (IsSleeping() || _isInShock)
        {
            if (_sleepDeprivationLevel > 0)
            {
                _sleepDeprivationProgress += -8;
                if (_sleepDeprivationProgress == 0)
                {
                    _sleepDeprivationLevel--;
                    _sleepDeprivationProgress = maxSleepDeprivationProgressBeforeNextLevel;
                }
            }
            else
            {
                _rest += 4;
            }

            //Debug.Log("UpdateTiredness: resting (" + _rest + ")");

            if (_rest >= maxRest) _rest = maxRest;
        }
        else
        {
            _rest += -1;

            //Debug.Log("UpdateTiredness: not resting (" + _rest + ")");
            if (_rest < 0)
            {
                _rest = 0;
                //Tired status

                _sleepDeprivationProgress++;
                if (_sleepDeprivationLevel > maxSleepDeprivationProgressBeforeNextLevel)
                {
                    _sleepDeprivationProgress = 0;
                    _sleepDeprivationLevel++;

                    if (_sleepDeprivationLevel >= 5)
                    {
                        Die();
                    }
                }
            }
        }
    }
    private bool IsSleeping()
    {
        Interactable focus = GetComponent<CharaHead>().LastInteractedFocus;

        if (focus != null)
        {
            return focus is BedHandler;
        }
        return false;
    }

    //HealthStatus Update
    private void UpdateHealthStatus()
    {
        //Check Death
        if (_isDead || CheckDeath()) return;
        //Make limb falloff
        CheckFalloff();
        //Update Stats
        UpdatePain();
        UpdateConsciousness();
        UpdateMovement();
        UpdateManipulation();

        //Update l'interface santé
        UpdateInterfaceHealth();
        //Debug:
        //Debug.Log("CharaRpg: Blood Lost " + totalBloodLose + ", " + bloodStock + " left, consciousness: " + _consciousness);
        DebugWounds();
    }
    
    private bool CheckDeath()
    {
        if (CheckBloodDeath())
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
        if (_bloodStock <= 0)
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

        foreach (BodyPart bodyPart in _bodyParts)
        {
            infectedWound = bodyPart.GetFullyInfectedWound();
            if (infectedWound != null) break;
        }

        if (infectedWound != null)
        {
            if (infectedWound.type == WoundInfo.WoundType.DeathBite)
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
        _pain = totalPain * _globalPainFactor;
        if (_isDead) _pain = 0f;
    }

    private void UpdateConsciousness()
    {
        _consciousness = 1 - _pain;
        float maxInfection = 0f;
        float infection;
        foreach (BodyPart bp in _bodyParts)
        {
            infection = bp.GetMaxInfection();
            if (maxInfection < infection)
            {
                maxInfection = infection;
            }
        }
        _consciousness = _consciousness * (float)_bloodStock / (float)_maxBloodStock;
        _consciousness -= (maxInfection / 100f);


        if (_consciousness < 0) _consciousness = 0;

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
    }
    private void UpdateManipulation()
    {
        if (_isDead || _isInShock)
        {
            _manipulation = 0f;
        }
        else
        {
            _manipulation = 1 - 0.5f * (2 - _bodyParts[11].GetFuncPurcent() - _bodyParts[12].GetFuncPurcent()) //Mains
                      - 0.3f * (4 - _bodyParts[7].GetFuncPurcent() //Rigth Shoulder
                                  - _bodyParts[8].GetFuncPurcent() //Left Shoulder
                                  - _bodyParts[9].GetFuncPurcent() //Right Arm
                                  - _bodyParts[10].GetFuncPurcent()); //Left Arm
            if (_manipulation <= 0) _manipulation = 0f;
        }
        //Debug.Log("UpdateMovement: movement=" + _movement + ", consciousness=" + _consciousness);
        if (_charaMov != null) _charaMov.ModifyAgentSpeed(_movement * _consciousness);
    }

    //==========Death==========
    private void Die()
    {
        _isDead = true;
        _movement = 0;
        //Chara dies
        PermissionsManager.Instance.spirit.CharaDie(gameObject);

        if (CheckIfInfected())
        {
            DieZombie();
            return;
        }
        Debug.Log("======================= CharaRpg: " + NameFull + " has died =======================");
    }
    private void DieZombie()
    {
        //Chara dies by death bite infection
        Debug.Log("======================= CharaRpg: " + NameFull + " has died of death bite infection =======================");
    }

    private bool CheckIfInfected()
    {
        foreach (BodyPart bp in _bodyParts)
        {
            if (bp.CheckIfInfected())
            {
                return true;
            }
        }
        return false;
    }
    
    //==========Health public methods==========
    //Wound Adder
    public void AddWound(Wound wound, BodyPart bodyPart)
    {
        AddWound(wound, bodyPart.name);
    }
    public void AddWound(Wound wound, string bodyPartName)
    {
        SendAddWound((int)wound.type, wound.damage, bodyPartName, wound.origin, wound.deathInfectionIncrement);
    }

    private void SendAddWound(int woundType, int initialDamage, string bodyPartName, string origin, float infectionIncrement = 0)
    {
        GetComponent<CharaConnect>().SendMsg(
            CharaConnect.CharaCommand.ReceiveAddWound,
            new int[2] { woundType, initialDamage },
            new string[2] { bodyPartName, origin },
            new float[1] { infectionIncrement });
    }
    
    public void ReceiveAddWound(int woundType, int initialDamage, string bodyPartName, string origin, float infectionIncrement)
    {
        if (initialDamage <= 0) return;

        FindPartWithName(bodyPartName).AddWound(new Wound((WoundInfo.WoundType)woundType, initialDamage, origin, infectionIncrement));

        UpdateHealthStatus();
        UpdateInterfaceHealth();
    }
    //Wound treatment
    public int GetCountWoundsOfType(WoundInfo.WoundType type)
    {
        int count = 0;
        foreach (BodyPart bodyPart in _bodyParts)
        {
            count += bodyPart.GetCountWoundsOfType(type);
        }
        return count;
    }
    public bool HasWoundOfType(WoundInfo.WoundType type)
    {
        foreach (BodyPart bodyPart in _bodyParts)
        {
            //Debug.Log("HasWoundOfType: Checking " + bodyPart.name);
            if (bodyPart.HasWoundsOfType(type))
            {
                //Debug.Log("HasWoundOfType: found wounds of type in " + bodyPart.name);
                return true;
            }
        }
        return false;
    }

    public void TreatAllWoundsOfType(WoundInfo.WoundType type)
    {
        foreach (BodyPart bodyPart in _bodyParts)
        {
            bodyPart.TreatAllWoundsOfType(type);
        }
        UpdateHealthStatus();
    }

    public bool IsInfected()
    {
        foreach (BodyPart bodyPart in _bodyParts)
        {
            if (bodyPart.CheckIfInfected())
            {
                return true;
            }
        }
        return false;
    }
    public void AmputateEveryInfectedPart()
    {
        foreach (BodyPart bodyPart in _bodyParts)
        {
            if (bodyPart.CheckIfInfected())
            {
                bodyPart.ForceFalloff();
            }
        }
        UpdateHealthStatus();
    }

    //Hunger (Eat)
    public bool Eat(int food)
    {
        //Called by food item when clicking in charainventory
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

    //==========Health Getters==========
    private int GetOutSideTemperature()
    {
        return _cm.GetTemperatureAtCoord(transform.position);
    }
    private int GetMinTempResistance()
    {
        return GetComponent<CharaInventory>().GetMinTemperatureModifier();
    }
    
    //bodyPart string Finder
    private BodyPart FindPartWithName(string name)
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


    //====================Public Combat Action====================
    public void GetAttackedWith(Equipable weapon, int damage)
    {
        BodyPart bodyPart = _bodyParts[Random.Range(0, 13)]; //1-12 
        //<------------------Doit réduire les dégats avec la liste des protections de la partie du corps correspondant
        WoundInfo.WoundType type;
        switch(weapon.dmgType)
        {
            case Equipable.DamageType.Mace:
                damage -= GetComponent<CharaInventory>().GetBodyPartMaceResistance(bodyPart.bodyType);
                type = damage > 40 ? WoundInfo.WoundType.Fracture : WoundInfo.WoundType.Bruise;
                break;
            case Equipable.DamageType.Sharp:
                damage -= GetComponent<CharaInventory>().GetBodyPartSharpResistance(bodyPart.bodyType);
                type = WoundInfo.WoundType.Bleeding;
                break;
            default:
                type = WoundInfo.WoundType.Bruise;
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
                AddWound(new Wound(WoundInfo.WoundType.DeathBite, biteDamage, "zombie", 1.2f), bodyPart);
            }
        }
    }


    //====================Health Interface Methods====================
    private void UpdateInterfaceHealth()
    {
        GameObject _interface = GetComponent<CharaInventory>().GetInterface();
        if (_interface != null)
        {
            _interface.GetComponent<InterfaceManager>().UpdateInjuries(GetWoundsInfo());
            _interface.GetComponent<InterfaceManager>().UpdateStats(GetHealthStats());
        }
    }

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
        string[] woundsInfoArray = new string[woundsInfo.Count + missingInfo.Count];
        int i = 0;
        for (; i < woundsInfo.Count; i++)
        {
            woundsInfoArray[i] = woundsInfo[i];
        }
        for (int j = 0; j < missingInfo.Count; j++)
        {
            woundsInfoArray[i] = missingInfo[j];
            i++;
        }
        return woundsInfoArray;
    }
    public float[] GetHealthStats()
    {
        return new float[7]
        {
            _pain,
            _consciousness,
            _movement,
            _manipulation,
            _bloodStock,
            _maxBloodStock,
            _rest
        };
    }


    //====================ToolTip Methods====================
    //Main Tooltip Update (use tooltip getters)
    public void UpdateToolTip()
    {
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(GetToolTipInfo(),GetQuirksInfo());
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateSkills(GetSkillInfo());
    }

    //Tooltip getters
    public string GetQuirksInfo()
    {
        string info = "";

        foreach (Quirk quirk in _quirks)
        {
            info += quirk.quirkName + ", ";
        }

        return info;
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
            c_maxHunger.ToString()
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


    //====================Debug====================
    public void DebugGetRandomDamage(int woundType)
    {
        Debug.Log("DebugGetRandomDamage: attack successful");
        SendAddWound(woundType, Random.Range(1, 10), _bodyParts[Random.Range(1, 13)].name, "DEBUGDAMAGE");
    }

    public void DebugWounds()
    {
        string debug = "";
        foreach (string s in GetWoundsInfo())
        {
            debug += s;
        }
        if (debug == "") debug = " healthy!";
        //Debug.Log("DebugWounds: " + debug);
        //Debug.Log("DebugWounds: felt temperature by " + NameFull + ", " + _feltTemperature + " (outside temp: " + GetOutSideTemperature() + ")");
    }

}
