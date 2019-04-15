using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyRoom : MonoBehaviour
{
    [SerializeField] private GameObject _listTeams = null;
    [SerializeField] private GameObject _listNoTeam = null;
    [SerializeField] private GameObject _settings = null;
    [SerializeField] private GameObject _settingPrefab = null;

    [SerializeField] private GameObject previous = null;
    [SerializeField] private GameObject current = null;

    [SerializeField] private GameObject launchButton = null;
    [SerializeField] private GameObject forceLaunchButton = null;

    [SerializeField] private GameObject sliderObject = null;

    [SerializeField] private InputField teamName = null;
    [SerializeField] private GameObject createTeam = null;

    private Launcher launcher;

    private void Start()
    {
        launcher = GameObject.Find("eLaucher").GetComponent<Launcher>();

        GameObject settingObj = Instantiate(_settingPrefab);
        settingObj.transform.SetParent(_settings.transform, false);
        settingObj.GetComponentInChildren<Text>().text = "Password : " + PhotonNetwork.room.CustomProperties["password"];

        GameObject settingObject = Instantiate(_settingPrefab);
        settingObject.transform.SetParent(_settings.transform, false);
        settingObject.GetComponentInChildren<Text>().text = "MaxInRoom : " + PhotonNetwork.room.MaxPlayers;

        GameObject settingObje = Instantiate(_settingPrefab);
        settingObje.transform.SetParent(_settings.transform, false);
        settingObje.GetComponentInChildren<Text>().text = "MaxInTeam : " + PhotonNetwork.room.CustomProperties["maxInTeam"];

        GameObject charaPerTeam = Instantiate(_settingPrefab);
        charaPerTeam.transform.SetParent(_settings.transform, false);
        charaPerTeam.GetComponentInChildren<Text>().text = "CharaPerTeam : " + PermissionsManager.Instance.numberChara;
    }

    public void LeaveRoom()
    {
        for(int i = 1; i < _settings.transform.childCount; i++)
        {
            Destroy(_settings.transform.GetChild(i).gameObject);
        }
        teamName.text = "";

        GameObject launcherD = GameObject.Find("eLaucher");
        GameObject permission = GameObject.Find("PermissionManager");

        PermissionsManager permissions = permission.GetComponent<PermissionsManager>();
        PermissionsManager.Player player = permissions.GetPlayerWithName(PhotonNetwork.player.NickName);
        PermissionsManager.Team team = permissions.GetTeamWithPlayer(player);

        if (player != null && team != null)
        {
            string playerName = player.Name;
            string teamname = player.MyTeamName; 
       
            permission.GetComponent<PhotonView>().RPC("RemovePlayerFromTeam", PhotonTargets.AllBuffered, teamname, playerName);            
        }
        PhotonNetwork.LeaveRoom();        
    }

    public void Ready()
    {
        gameObject.GetComponent<PhotonView>().RPC("ReadyRPC", PhotonTargets.AllBuffered, PhotonNetwork.player);
    }

    [PunRPC]
    private void ReadyRPC(PhotonPlayer photonPlayer)
    {
        GameObject.Find("eLaucher").GetComponent<Launcher>().ReadyInRoom(photonPlayer);
    }

    public void Launch()
    {        
        GameObject launcherD = GameObject.Find("PermissionManager");
        PermissionsManager permissions = launcherD.GetComponent<PermissionsManager>();
        Debug.Log("Teams : ");
        foreach(var team in permissions.TeamList)
        {
            Debug.Log(team.Name);
            foreach(var player in team.PlayerList)
            {
                Debug.Log(player.Name);
            }
        }
        if (launcher.LaunchIfReady())
        {
            sliderObject.SetActive(true);
            gameObject.SetActive(false);
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
        }
    }

    public void ForceLaunch()
    {
        sliderObject.SetActive(true);
        gameObject.SetActive(false);
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;
    }

    public void CreateTeam()
    {
        if (teamName.text != "")
        {            
            launcher.AddTeam(teamName.text);
            teamName.text = "";
        }
    }


}
