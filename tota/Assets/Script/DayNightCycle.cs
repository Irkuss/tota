using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    //Reference
    private Light _sun;
    [SerializeField] private float _rotationSpeedModifier;

    //Heure
    public float heure;

    //Game status
    private int _dayPassedSinceStart = 1;
    private Seasons _currentSeason;
    private bool _changedHour;

    private float _sliderProgressStatus;

    //Custom event
    public delegate void TimerHour();
    public static event TimerHour onNewHour;

    public delegate void NewSeason(Seasons saison);
    public static event NewSeason onNewseason;

    //day time status
    private bool _isDayTime = true;
    public bool IsDayTime => _isDayTime;

    //Season Attribute
    public enum Seasons
    {
        SUMMER = 0,
        AUTUMM,
        WINTER,
        SPRING
    }
    private Dictionary<Seasons, int> _baseSeasonTemperature = new Dictionary<Seasons, int>()
    {
        {Seasons.SUMMER, 25 },
        {Seasons.AUTUMM, 15 },
        {Seasons.WINTER, -10 },
        {Seasons.SPRING, 20 }
    };
    
    
    //Start
    private void Start()
    {
        _currentSeason = Seasons.SUMMER;
        CallNewSeason();
        CallNewHour();
        _sun = GetComponent<Light>();
        transform.rotation = Quaternion.Euler(0, 60, 0);
        _sliderProgressStatus = 0.5f;
        AudioManager.instance.StartCoroutine("StartMusic", "Solitude");
    }

    //Update
    private void Update()
    {
        DebugForceNextSeason(); //Temp pour test les saisons
        
        Cycle();
        CheckNextSeason();
    }

    private void Cycle()
    {
        if (_sliderProgressStatus >= 1.0f)//Reset le slider à 0 pour faire un cycle
        {
            _sliderProgressStatus = 0;
            _dayPassedSinceStart++;
        }

        if (_changedHour)
        {
            Debug.Log("CallNewHour: starting new hour");
            CallNewHour();
            //Handling daynight changes
            if (heure >= 20f && heure <= 21f)
            {
                Debug.Log("DayNightCycle: Starting Night");
                _isDayTime = false;
                AudioManager.instance.StartCoroutine("EndMusic", "Solitude");
                AudioManager.instance.StartCoroutine("StartMusic", "Nightwalk");
            }
            else if (heure >= 5f && heure <= 6f)
            {
                Debug.Log("DayNightCycle: Starting Day");
                _isDayTime = true;
                AudioManager.instance.StartCoroutine("EndMusic", "Nightwalk");
                AudioManager.instance.StartCoroutine("StartMusic", "Solitude");
            }
        }

        heure = _sliderProgressStatus * 24;
        _sun.transform.rotation = Quaternion.Euler((_sliderProgressStatus * 360) - 90, 60, 0);
        _sliderProgressStatus = _sliderProgressStatus + Time.deltaTime / _rotationSpeedModifier;

        int newHeure = (int)(_sliderProgressStatus * 24); //Calcule l'heure suivante
        _changedHour = newHeure - (int)heure > 0; //Fais la différence pour voir si on a changé d'heure

        //Couleur orangé au lever et coucher de soleil
        float posx = transform.eulerAngles.x;
        if (posx > -10 && posx < 40 || posx > 150 && posx < 190)//Change la couleur en orangé au levé et au couché du soleil
        {
            _sun.color = new Color(1f, 0.772549f, 0.4156863f, 1f);
        }
        else
        {
            _sun.color = new Color(1f, 0.9568627f, 0.8392157f, 1f);
        }
    }

    private void CheckNextSeason()
    {
        if (_dayPassedSinceStart % 20 == 0)//Tous les 20 jours changent de saisons
        {
            ForceNextSeason();
        }
    }
    private void ForceNextSeason()
    {
        int next = ((int)_currentSeason + 1) % 4;
        switch (next)
        {
            case 0: _currentSeason = Seasons.SUMMER; break;
            case 1: _currentSeason = Seasons.AUTUMM; break;
            case 2: _currentSeason = Seasons.WINTER; break;
            case 3: _currentSeason = Seasons.SPRING; break;
        }
        CallNewSeason();
    }

    //Getters
    public int GetCurrentTemperature()
    {
        //Called by all charas every health cycle
        int dayTimeModifier = _isDayTime ? 0 : -4;

        return _baseSeasonTemperature[_currentSeason] + dayTimeModifier;
    }

    //Custom callback
    private void CallNewHour()
    {
        if (onNewHour != null) //Si une personne nous écoute
        {
            onNewHour(); //Declenche le callback chez les spectateurs
        }
    }
    private void CallNewSeason()
    {
        if (onNewseason != null) //Si une personne nous écoute
        {
            onNewseason(_currentSeason); //Declenche le callback chez les spectateurs
        }
    }


    //Debug
    public void DebugForceNextSeason()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ForceNextSeason();
        }
    }

}
