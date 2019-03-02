using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    //Temporaire


    //On recupere les Component Text qui seront modifiés
    public Text nameText;
    public Text strengthText;
    public Text intelligenceText;
    public Text perceptionText;

    //Appelé après avoir cliqué sur un Chara dans SpiritHead.cs/ClickOnChara()
    public void UpdateWith(string[] info)
    {
        nameText.text = "Name: " + info[0];
        strengthText.text = "Strength: " + info[1];
        intelligenceText.text = "Intelligence: " + info[2];
        perceptionText.text = "Perception: " + info[3];
    }
}
