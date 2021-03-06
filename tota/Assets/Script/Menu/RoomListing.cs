﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    private Launcher _launcher;

    [SerializeField] private Text _roomName;
    private Text roomName
    {
        get { return _roomName; }
    }

    public string RoomName { get; private set; }
    public bool Updated { get; set; }

    private void Start()
    {
        _launcher = GameObject.Find("eLaucher").GetComponent<Launcher>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClickJoinRoom(roomName.text));
    }

    private void OnDestroy()
    {
        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
    }

    public void SetRoomName(string text)
    {
        RoomName = text;
        roomName.text = RoomName;
    }

    public void OnClickJoinRoom(string roomName)
    {
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();

        foreach (RoomInfo room in rooms)
        {
            if (room.Name == roomName)
            {
                if((string) room.CustomProperties["password"] != "")
                {
                    _launcher.EnterPassword(room);
                }
                else PhotonNetwork.JoinRoom(roomName);
            }
        }        
    }
}
