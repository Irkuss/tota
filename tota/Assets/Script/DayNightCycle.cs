using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private Light _sun;
    public float speed;
    public float heure;
    public int day = 1;
    public Seasons season;
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

    private void UpdateCallback()
    {
        if (onNewHour != null) //Si une personne nous écoute
        {
            onNewHour(); //Declenche le callback chez les spectateurs
        }

        if (onNewseason != null) //Si une personne nous écoute
        {
            onNewseason(season); //Declenche le callback chez les spectateurs
        }
    }

    void Start()
    {
        season = Seasons.SUMMER;
        UpdateCallback();
        speed = 100f;
        _sun = GetComponent<Light>();
        transform.rotation = Quaternion.Euler(0, 60, 0);
        _slider = 0.5f;
        FindObjectOfType<AudioManager>().StartCoroutine("StartMusic", "Solitude");
    }

    void Update()
    {
        Cycle();
        NextSeason();
        if (heure > 16f && heure < 17f && _changedHour)
        {
            Debug.Log("Night");
            FindObjectOfType<AudioManager>().StartCoroutine("EndMusic", "Solitude");
            FindObjectOfType<AudioManager>().StartCoroutine("StartMusic", "Nightwalk");
        }
        if (heure > 5f && heure < 6f && _changedHour)
        {
            Debug.Log("Day");
            FindObjectOfType<AudioManager>().StartCoroutine("EndMusic", "Nightwalk");
            FindObjectOfType<AudioManager>().StartCoroutine("StartMusic","Solitude");
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
            int next = ((int)season + 1) % 4;
            switch (next)
            {
                case 0: season = Seasons.SUMMER; break;
                case 1: season = Seasons.AUTUMM; break;
                case 2: season = Seasons.WINTER; break;
                case 3: season = Seasons.SPRING; break;
            }
            UpdateCallback();
        }
    }
}
