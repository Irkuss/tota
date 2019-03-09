using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    //Temporaire


    //On recupere les Component Text qui seront modifiés
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _strengthText;
    [SerializeField] private Text _intelligenceText;
    [SerializeField] private Text _perceptionText;
    [SerializeField] private Text _mentalText;
    [SerializeField] private Text _socialText;
    [SerializeField] private Text _hungerText;

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
}
