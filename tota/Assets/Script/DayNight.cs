using UnityEngine;

public class DayNight : MonoBehaviour
{
    Light sun;
    public float speed;
    public float heure;
    public float slider;
    private bool changedHour;

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
        sun = GetComponent<Light>();
        transform.rotation = Quaternion.Euler(0, -30, 0);
        slider = 0.5f;
        
    }
    
    void Update()
    {
        if (slider >= 1.0)//Reset le slider à 0 pour faire un cycle
        {
            slider = 0;
        }

        if(changedHour)
        {
            UpdateCallback();
        }

        heure = slider * 24;
        sun.transform.rotation = Quaternion.Euler((slider * 360) - 90, 0, 0);
        slider = slider + Time.deltaTime / speed;

        int newHeure = (int)(slider * 24); //Calcule l'heure suivante
        changedHour = newHeure - (int)heure > 0; //Fais la différence pour voir si on a changé d'heure
        
        float posx = transform.eulerAngles.x;
        if (posx > -10 && posx < 40 || posx > 150 && posx < 190)//Change la couleur en orangé au levé et au couché du soleil
        {
            sun.color = new Color(1f, 0.772549f, 0.4156863f, 1f);
        }
        else
        {
            sun.color = new Color(1f, 0.9568627f, 0.8392157f, 1f);
        }
    }
}
