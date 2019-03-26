using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionRoom : MonoBehaviour
{
    [SerializeField] private GameObject previous = null;
    [SerializeField] private GameObject current = null;
    [SerializeField] private GameObject next = null;

    private Toggle passwordToggle;
    private Toggle maxInRoomToggle;
    private Toggle maxInTeamToggle;
    private Slider randomChara;

    [SerializeField] private GameObject passwordObject = null;
    [SerializeField] private GameObject maxInRoom = null;
    [SerializeField] private GameObject maxInTeam = null;

    [SerializeField] private InputField passwordInput = null;
    [SerializeField] private Text maxInRoomInput = null;
    [SerializeField] private Text maxInTeamInput = null;
    [SerializeField] private InputField roomName = null;

    [SerializeField] private GameObject randomCharacters = null;

    private string _password = "";
    private string _maxInRoom = "";
    private string _maxInTeam = "";
    private int _randomChara = 0;

    private void Start()
    {
        passwordToggle = passwordObject.GetComponentInChildren<Toggle>();
        maxInRoomToggle = maxInRoom.GetComponentInChildren<Toggle>();
        maxInTeamToggle = maxInTeam.GetComponentInChildren<Toggle>();
        randomChara = randomCharacters.GetComponentInChildren<Slider>();
    }

    public void PasswordToggle()
    {
        InputField input = passwordObject.GetComponentInChildren<InputField>();
        if (input.IsActive())
        {
            input.enabled = false;
            input.text = "";
        }
        else
        {
            input.enabled = true;
        }
    }

    public void MaxInRoomToggle()
    {
        InputField input = maxInRoom.GetComponentInChildren<InputField>();
        if (input.IsActive())
        {
            input.enabled = false;
            input.text = "";
        }
        else
        {
            input.enabled = true;
        }
    }

    public void MaxInTeamToggle()
    {
        InputField input = maxInTeam.GetComponentInChildren<InputField>();
        if (input.IsActive())
        {
            input.enabled = false;
            input.text = "";
        }
        else
        {
            input.enabled = true;
        }
    }

    public void PasswordEnd()
    {
        _password = passwordInput.text;
    }

    public void MaxInRoomEnd()
    {
        _maxInRoom = maxInRoomInput.text;
    }

    public void MaxInTeamEnd()
    {
        _maxInTeam = maxInTeamInput.text;
    }

    public void CreateRoom()
    {
        //_randomChara = (int) randomChara.value;

        RoomOptions roomOptions = new RoomOptions();

        if (_maxInRoom == "") roomOptions.MaxPlayers = 4;
        else roomOptions.MaxPlayers = byte.Parse(_maxInRoom);

        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("password", _password);

        if (_maxInTeam == "") roomOptions.CustomRoomProperties.Add("maxInTeam", 4);
        else roomOptions.CustomRoomProperties.Add("maxInTeam", int.Parse(_maxInTeam));

        //roomOptions.CustomRoomProperties.Add("charaPerTeam", randomChara.value);

        roomOptions.CustomRoomPropertiesForLobby = new string[]
        {
            "password",
            "maxInTeam",
            "charaPerTeam"
        };

        if (roomName.text != "")
        {
            passwordObject.GetComponentInChildren<InputField>().text = "";
            maxInRoom.GetComponentInChildren<InputField>().text = "";
            maxInTeam.GetComponentInChildren<InputField>().text = "";
            randomChara.value = 0;
            roomName.text = "";
            PhotonNetwork.CreateRoom(roomName.text, roomOptions, default);
        }
    }

    public void Cancel()
    {
        passwordObject.GetComponentInChildren<InputField>().text = "";
        maxInRoom.GetComponentInChildren<InputField>().text = "";
        maxInTeam.GetComponentInChildren<InputField>().text = "";
        randomChara.value = 0;
        roomName.text = "";
        current.SetActive(false);
        previous.SetActive(true);
    }
}
