using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    //Temporaire


    //On recupere les Component Text qui seront modifiés
    [SerializeField] private Text _nameText = null;
    [SerializeField] private Text _strengthText = null;
    [SerializeField] private Text _intelligenceText = null;
    [SerializeField] private Text _perceptionText = null;
    [SerializeField] private Text _mentalText = null;
    [SerializeField] private Text _socialText = null;
    [SerializeField] private Text _hungerText = null;


    [SerializeField] private Slider _strengthSlider = null;
    [SerializeField] private Slider _intelligenceSlider = null;
    [SerializeField] private Slider _perceptionSlider = null;
    [SerializeField] private Slider _mentalSlider = null;
    [SerializeField] private Slider _socialSlider = null;
    [SerializeField] private Slider _hungerSlider = null;

    [SerializeField] private Image _strengthFill = null;
    [SerializeField] private Image _intelligenceFill = null;
    [SerializeField] private Image _perceptionFill = null;
    [SerializeField] private Image _mentalFill = null;
    [SerializeField] private Image _socialFill = null;
    [SerializeField] private Image _hungerFill = null;

    //Appelé après avoir cliqué sur un Chara dans SpiritHead.cs/ClickOnChara()
    public void UpdateWith(string[] info)
    {
        _nameText.text = "Name: " + info[0];

        _strengthText.text = "Strength: " + info[1];
        _intelligenceText.text = "Intelligence: " + info[2];
        _perceptionText.text = "Perception: " + info[3];
        _mentalText.text = "Mental: " + info[4];
        _socialText.text = "Social: " + info[5];

        _hungerText.text = "Hunger: " + info[6] + "/" + info[7];

    }

    public void UpdateTool(string[] info)
    {
        _nameText.text = "Name: " + info[0];

        _strengthSlider.value = int.Parse(info[1]);
        _intelligenceSlider.value = int.Parse(info[2]);
        _perceptionSlider.value = int.Parse(info[3]);
        _mentalSlider.value = int.Parse(info[4]);
        _socialSlider.value = int.Parse(info[5]);
        _hungerSlider.maxValue = int.Parse(info[7]);
        _hungerSlider.value = int.Parse(info[6]);

        List<Slider> sliders = new List<Slider> { _strengthSlider, _intelligenceSlider, _perceptionSlider, _perceptionSlider, _socialSlider, _hungerSlider };
        List<Image> fill = new List<Image> { _strengthFill, _intelligenceFill, _perceptionFill, _perceptionFill, _socialFill, _hungerFill };
        for(int i = 0; i < fill.Count; i++)
        {
            if (sliders[i].value < 20)
            {
                fill[i].color = Color.red;
            }
            else if (sliders[i].value >= 20 && sliders[i].value < 50)
            {
                fill[i].color = Color.yellow;
            }
            else
            {
                fill[i].color = Color.green;
            }        
        }
    }
}
