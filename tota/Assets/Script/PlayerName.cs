using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    // Store the PlayerPref Key to avoid typos
    static string playerNamePrefKey = "name";

    // MonoBehaviour method called on GameObject by Unity during initialization phase.
    void Start()
    {
        string defaultName = "";
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }
    }

}
