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
    private Toggle heightMapToggle;
    private Slider randomChara;

    [SerializeField] private GameObject passwordObject = null;
    [SerializeField] private GameObject maxInRoom = null;
    [SerializeField] private GameObject maxInTeam = null;
    [SerializeField] private GameObject heightMap = null;

    [SerializeField] private InputField passwordInput = null;
    [SerializeField] private Text maxInRoomInput = null;
    [SerializeField] private Text maxInTeamInput = null;
    [SerializeField] private InputField roomName = null;
    [SerializeField] private InputField heightMapInput = null;

    [SerializeField] private GameObject randomCharacters = null;

    [SerializeField] private GameObject _dropdown = null;

    private string _password = "";
    private string _maxInRoom = "";
    private string _maxInTeam = "";
    private string _heightMap = "";
    private int _randomChara = 0;
    private string _room = "";

    private void Start()
    {
        passwordToggle = passwordObject.GetComponentInChildren<Toggle>();
        maxInRoomToggle = maxInRoom.GetComponentInChildren<Toggle>();
        maxInTeamToggle = maxInTeam.GetComponentInChildren<Toggle>();
        heightMapToggle = heightMap.GetComponentInChildren<Toggle>();
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

    public void HeightMapToggle()
    {
        InputField input = heightMap.GetComponentInChildren<InputField>();
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
        RoomOptions roomOptions = new RoomOptions();

        if (_maxInRoom == "") roomOptions.MaxPlayers = 4;
        else roomOptions.MaxPlayers = byte.Parse(_maxInRoom);

        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("password", _password);

        if (_maxInTeam == "") roomOptions.CustomRoomProperties.Add("maxInTeam", 4);
        else roomOptions.CustomRoomProperties.Add("maxInTeam", int.Parse(_maxInTeam));

        if (heightMapInput.text == "")
        {
            roomOptions.CustomRoomProperties.Add("heightMap", 1);
            PermissionsManager.Instance.heightMap = 1;
        }
        else
        {
            roomOptions.CustomRoomProperties.Add("heightMap", int.Parse(heightMapInput.text));
            PermissionsManager.Instance.heightMap = int.Parse(heightMapInput.text);
        }

        roomOptions.CustomRoomProperties.Add("numberChara", (int)randomChara.value);
        PermissionsManager.Instance.numberChara = (int) randomChara.value;

        int mode = _dropdown.GetComponent<Dropdown>().value;
        roomOptions.CustomRoomProperties.Add("mode", mode);

        roomOptions.CustomRoomPropertiesForLobby = new string[]
        {
            "password",
            "maxInTeam",
            "heightMap",
            "numberChara",
            "mode"
        };

        switch (mode)
        {
            case 0:            
                Mode.Instance.Solo();
                break;
            case 1:
                Mode.Instance.Cold();
                break;
            case 2:
                Mode.Instance.Zombie();
                break;
        }

        if (roomName.text != "")
        {
            passwordObject.GetComponentInChildren<InputField>().text = "";
            maxInRoom.GetComponentInChildren<InputField>().text = "";
            maxInTeam.GetComponentInChildren<InputField>().text = "";
            heightMapInput.text = "";
            randomChara.value = 0;
            _room = roomName.text;
            roomName.text = "";
            PhotonNetwork.CreateRoom(_room, roomOptions, default);
        }
    }

    public void Cancel()
    {
        passwordObject.GetComponentInChildren<InputField>().text = "";
        maxInRoom.GetComponentInChildren<InputField>().text = "";
        maxInTeam.GetComponentInChildren<InputField>().text = "";
        heightMapInput.text = "";
        randomChara.value = 0;
        roomName.text = "";
        current.SetActive(false);
        previous.SetActive(true);
    }
}
