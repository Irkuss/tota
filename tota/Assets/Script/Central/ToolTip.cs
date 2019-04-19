﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    //Temporaire

    [SerializeField] private Text _name = null;
    //On recupere les Component Text qui seront modifiés
    [SerializeField] private Text _carpenter = null;
    [SerializeField] private Text _doctor = null;
    [SerializeField] private Text _electrician = null;
    [SerializeField] private Text _farmer = null;
    [SerializeField] private Text _marksman = null;
    [SerializeField] private Text _scavenger = null;
    [SerializeField] private Text _stamina = null;


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
    public void UpdateSkills(string[] info)
    {
        _carpenter.text = "Carpenter : " + info[0] + " / 10";
        _doctor.text = "Doctor : " + info[1] + " / 10";
        _electrician.text = "Electrician : " + info[2] + " / 10";
        _farmer.text = "Farmer : " + info[3] + " / 10";
        _marksman.text = "Marksman : " + info[4] + " / 10";
        _scavenger.text = "Scavenger : " + info[5] + " / 10";
        _stamina.text = "Stamina : " + info[6] + " / 10";
    }


    public void UpdateTool(string[] info)
    {
        if (_name != null)
        {
            _name.text = "Name : " + info[0];
        }
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
