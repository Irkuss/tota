using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private Light _sun;
    public float speed;
    public float heure;
    public int day = 1;
    private Seasons _season;
    private float _slider;
    private bool _changedHour;

    public delegate void TimerHour();
    public static event TimerHour onNewHour;

    public delegate void NewSeason(Seasons saison);
    public static event NewSeason onNewseason;

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
        {Seasons.AUTUMM, 10 },
        {Seasons.WINTER, -5 },
        {Seasons.SPRING, 15 }
    };
    public int GetCurrentSeasonTemperature()
    {
        return _baseSeasonTemperature[_season];
    }

    private void UpdateCallback()
    {
        if (onNewHour != null) //Si une personne nous écoute
        {
            onNewHour(); //Declenche le callback chez les spectateurs
        }

        if (onNewseason != null) //Si une personne nous écoute
        {
            onNewseason(_season); //Declenche le callback chez les spectateurs
        }
    }

    void Start()
    {
        _season = Seasons.SUMMER;
        UpdateCallback();
        _sun = GetComponent<Light>();
        transform.rotation = Quaternion.Euler(0, 60, 0);
        _slider = 0.5f;
        AudioManager.instance.StartCoroutine("StartMusic", "Solitude");
    }

    void Update()
    {
        Cycle();
        NextSeason();
        if (heure > 16f && heure < 17f && _changedHour)
        {
            Debug.Log("DayNightCycle: Night");
            AudioManager.instance.StartCoroutine("EndMusic", "Solitude");
            AudioManager.instance.StartCoroutine("StartMusic", "Nightwalk");
        }
        if (heure > 5f && heure < 6f && _changedHour)
        {
            Debug.Log("DayNightCycle: Day");
            AudioManager.instance.StartCoroutine("EndMusic", "Nightwalk");
            AudioManager.instance.StartCoroutine("StartMusic","Solitude");
        }
            
    }

    void Cycle()
    {
        if (_slider >= 1.0)//Reset le slider à 0 pour faire un cycle
        {
            _slider = 0;
            day++;
        }

        if (_changedHour)
        {
            UpdateCallback();
        }

        heure = _slider * 24;
        _sun.transform.rotation = Quaternion.Euler((_slider * 360) - 90, 60, 0);
        _slider = _slider + Time.deltaTime / speed;

        int newHeure = (int)(_slider * 24); //Calcule l'heure suivante
        _changedHour = newHeure - (int)heure > 0; //Fais la différence pour voir si on a changé d'heure

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

    void NextSeason()
    {
        if (day >= 20)
        {
            day = 1;
            int next = ((int)_season + 1) % 4;
            switch (next)
            {
                case 0: _season = Seasons.SUMMER; break;
                case 1: _season = Seasons.AUTUMM; break;
                case 2: _season = Seasons.WINTER; break;
                case 3: _season = Seasons.SPRING; break;
            }
            UpdateCallback();
        }
    }
}
