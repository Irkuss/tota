using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private Light _sun;
    public float speed;
    public float heure;
    private float _slider;
    private bool _changedHour;

    public delegate void TimerHour();
    public static event TimerHour onNewHour;

    private void UpdateCallback()
    {
        if (onNewHour != null) //Si une personne nous écoute
        {
            onNewHour(); //Declenche le callback chez les spectateurs
        }
    }

    void Start()
    {
        _sun = GetComponent<Light>();
        transform.rotation = Quaternion.Euler(0, 60, 0);
        _slider = 0.5f;

    }

    void Update()
    {
        if (_slider >= 1.0)//Reset le slider à 0 pour faire un cycle
        {
            _slider = 0;
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
}
