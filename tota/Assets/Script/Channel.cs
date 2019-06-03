using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Channel : MonoBehaviour
{
    [SerializeField] private GameObject _input = null;
    [SerializeField] private GameObject _messages = null;
    [SerializeField] private GameObject _textPref = null;

    private PermissionsManager _permission;
    private PermissionsManager.Player _player;
    private PermissionsManager.Team _team;
    private string _teamName = "";

    public static bool isWriting;

    private void Awake()
    {
        _permission = PermissionsManager.Instance;
        _player = _permission.GetPlayerWithName(PhotonNetwork.player.NickName);
        if (_player != null)
        {
            _team = _permission.GetTeamWithPlayer(_player);
            if (_team != null)
            {
                _teamName = _team.Name;
            }
        }
    }

    private void Update()
    {
        if (_input.GetComponent<InputField>().isFocused)
        {
            isWriting = true;
        }
        else
        {
            isWriting = false;
        }
    }

    public void ParseInput()
    {
        string mes = _input.GetComponent<InputField>().text;
        _input.GetComponent<InputField>().text = "";
        string[] args = mes.Split(' ');
        string output = "";

        if (args.Length < 1) return;        

        if (args[0] == "/all")
        {
            for (int i = 1; i < args.Length; i++)
            {
                output += " " + args[i];
            }
            if (Mode.Instance.online) gameObject.GetComponent<PhotonView>().RPC("SendReceiveM", PhotonTargets.AllBuffered, output, true, _player.Name);
            else SendReceiveM(output, true, _player.Name);

        }
        else
        {            
            if (Mode.Instance.online) gameObject.GetComponent<PhotonView>().RPC("SendReceiveM", PhotonTargets.AllBuffered, mes, false, _player.Name);
            else SendReceiveM(mes, true, _player.Name);
        }

    }

    [PunRPC]
    private void SendReceiveM(string message, bool general, string player)
    {
        if (general)
        {           
            if (_messages.transform.childCount == 7)
            {
                Destroy(_messages.transform.GetChild(0).gameObject);
            }            
            GameObject mes = Instantiate(_textPref, _messages.transform);            
            
            if (message == "spawnchara")
            {
                _permission.spirit.TryCharaSpawn(true);
                mes.GetComponent<Text>().text = "A chara was spawned";
            }
            else if(message == "spawnrat")
            {
                _permission.spirit.SpawnRat();
                mes.GetComponent<Text>().text = "A rat was spawned";
            }
            else if (message == "spawnai")
            {
                _permission.spirit.SpawnAI();
                mes.GetComponent<Text>().text = "An AI chara was spawned";
            }
            else
            {
                mes.GetComponent<Text>().text = player + " : " + message;
            }
        }
        else
        {
            if (_team != null && _team.ContainsPlayer(_permission.GetPlayerWithName(player)))
            {
                if (_messages.transform.childCount == 7)
                {
                    Destroy(_messages.transform.GetChild(0).gameObject);
                }
                GameObject mes = Instantiate(_textPref, _messages.transform);
                mes.GetComponent<Text>().text = player + " : " + message;
            }
        }
    }
}
