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
    private string _teamName = "" ;

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

    public void ParseInput()
    {
        string mes = _input.GetComponent<InputField>().text;
        _input.GetComponent<InputField>().text = "";
        string[] args = mes.Split(' ');
        string output = "";

        if (args.Length <= 1) return;
        if (_team == null && args[0] != "#general") return;

        if(args[0] == "#general")
        {
            for(int i = 1; i < args.Length; i++)
            {
                output += args[i];
            }
            gameObject.GetComponent<PhotonView>().RPC("SendReceive", PhotonTargets.AllBuffered, output, true, _player.Name);
            
        }
        else if(args[1] == "#" + _teamName)
        {
            for (int i = 1; i < args.Length; i++)
            {
                output += args[i];
            }
            gameObject.GetComponent<PhotonView>().RPC("SendReceive", PhotonTargets.AllBuffered, output, false, _player.Name);
        }
        
    }

    [PunRPC]
    private void SendReceive(string message,bool general,string player)
    {
        if (general)
        {
            if (_messages.transform.childCount == 7)
            {
                Destroy(_messages.transform.GetChild(0).gameObject);
            }
            GameObject mes = Instantiate(_textPref, _messages.transform);
            mes.GetComponent<Text>().text = player + " : " + message;
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
