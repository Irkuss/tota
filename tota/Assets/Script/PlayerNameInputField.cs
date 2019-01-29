using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Demande que tout GameObject de cette classe a avoir un Component "InputField"
[RequireComponent(typeof(InputField))] 

public class PlayerNameInputField : MonoBehaviour
{
    //README
    //
    //Explication du Script:
    //A start, on verifie si le joueur possède déjà un nom stocké dans les Prefs
    //-Si oui, on réattribue ce nom au joueur
    //         on affiche ce nom dans le InputField
    //-Si non, defaultName est attribué au nom du joueur
    //         le PlaceHolder de InputField est tout de même laissé
    //Le joueur peut changer de nom en modifiant InputField
    //Ce nom est stocké dans les prefs, et devient le nom du joueur
    
    
    // Store the PlayerPref Key to avoid typos
    
    static string playerNamePrefKey = "PlayerName";

    void Start ()
    {
        string defaultName = "defaultName";
        InputField _inputField = this.GetComponent<InputField>();

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.playerName = defaultName;
	}

    public void SetPlayerName(string inputText)
    {
        PhotonNetwork.playerName = inputText + " ";
        //NB: force a trailing space string in case value is an empty string, else playerName would not be updated.


        PlayerPrefs.SetString(playerNamePrefKey, inputText);
    }


}
