using UnityEngine;

public class DayNight : MonoBehaviour
{
    Light sun;
    public float speed;
    public float heure;
    public float slider;

    void Start()
    {
        sun = GetComponent<Light>();
        transform.rotation = Quaternion.Euler(0, -30, 0);
        slider = 0.5f;
    }


    void Update()
    {
        if (slider >= 1.0)
        {
            slider = 0;
        }

        heure = slider * 24;
        sun.transform.rotation = Quaternion.Euler((slider * 360) - 90, 0, 0);
        slider = slider + Time.deltaTime / speed;

        float posx = transform.eulerAngles.x;
        if (posx > -10 && posx < 40 || posx > 150 && posx < 190)
        {
            sun.color = new Color(1f, 0.772549f, 0.4156863f, 1);
        }
        else
        {
            sun.color = new Color(1f, 0.9568627f, 0.8392157f, 1f);
        }
    }
}
