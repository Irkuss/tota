using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaRpg : MonoBehaviour
{
    //Character
    private string _nameFirst = "John";
    private string _nameLast = "McCree";

    public string FullName
    {
        get { return _nameFirst + _nameLast; }
    }

    //Stats
    private int _strength;
    public int Strength
    {
        get => _strength;
    }
    private int _intelligence;
    public int Intelligence
    {
        get => _intelligence;
    }
    private int _perception;
    public int Perception
    {
        get => _perception;
    }
    private int _mental;
    public int Mental
    {
        get => _mental;
    }
    private int _social;
    public int Social
    {
        get => _social;
    }


    //Status
    private int _hunger;
    private int _maxHunger = 100;
    //UnityCallback
    void Start()
    {
        InitStat();

        InitStatus();

    }

    private void InitStat()
    {
        _strength = Random.Range(1, 20) * 5;
        _intelligence = Random.Range(1, 20) * 5;
        _perception = Random.Range(1, 20) * 5;
        _mental = Random.Range(1, 20) * 5;
        _social = Random.Range(1, 20) * 5;
    }
    private void InitStatus()
    {
        _hunger = _maxHunger / 2;
    }
    //Public Getters
    public string GetFullName()
    {
        return _nameFirst + " " + _nameLast;
    }
    public string[] GetToolTipInfo()
    {
        return new string[8] 
        {
            GetFullName(),
            _strength.ToString(),
            _intelligence.ToString(),
            _perception.ToString(),
            _mental.ToString(),
            _social.ToString(),
            _hunger.ToString(),
            _maxHunger.ToString()
        };
    }

    [PunRPC]
    public void SendToolTipInfo(string[] info)
    {
        _strength = int.Parse(info[1]);
        _intelligence = int.Parse(info[2]);
        _perception = int.Parse(info[3]);
        _mental = int.Parse(info[4]);
        _social = int.Parse(info[5]);
        _hunger = int.Parse(info[6]);
        _maxHunger = int.Parse(info[7]);
    }

    public void Eat(int food)
    {
        if (_hunger < _maxHunger)
        {
            _hunger += food;
            GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(GetToolTipInfo()); // On appelle l'update du tooltip
            //GetComponent<PhotonView>().RPC("SendToolTipInfo", PhotonTargets.AllBuffered, GetToolTipInfo());
        }
    }

    public void UseItem()
    {

    }
}
