using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    public Text nameText;
    public Text strengthText;
    public Text intelligenceText;
    public Text perceptionText;

    public void UpdateWith(string[] info)
    {
        nameText.text = "Name: " + info[0];
        strengthText.text = "Strength: " + info[1];
        intelligenceText.text = "Intelligence: " + info[2];
        perceptionText.text = "Perception: " + info[3];
    }
}
