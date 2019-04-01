using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connect : MonoBehaviour
{
    [SerializeField] private Text connectedAs = null;
    [SerializeField] private GameObject edit = null;
    [SerializeField] private GameObject valid = null;
    [SerializeField] private GameObject inputEdit = null;
    [SerializeField] private Text editName = null;

    private void Start()
    {
        connectedAs.text = "Connected as : " + PhotonNetwork.playerName;
    }

    public void Edit()
    {
        edit.SetActive(false);
        valid.SetActive(true);
        inputEdit.SetActive(true);
    }

    public void Valid()
    {     
        if (editName.text != "")
        {         
            PhotonNetwork.playerName = editName.text;
            PlayerPrefs.SetString("name", editName.text);

            edit.SetActive(true);
            valid.SetActive(false);
            inputEdit.SetActive(false);

            connectedAs.text = "Connected as : " + PhotonNetwork.playerName;
        }
                
    }
}
