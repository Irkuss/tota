using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaRpg : MonoBehaviour
{
    //Character
    private string _nameFirst = "John";
    private string _nameLast = "McCree";
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
    private float timer = 0f;
    
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

    public void Eat(int food)
    {
        if (_hunger > 0)
        {
            _hunger -= food;
            GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(GetToolTipInfo()); // On appelle l'update du tooltip
        }
    }

    public void UseItem()
    {

    }

    public void RemoveCharaTeam()
    {
        if (_hunger == _maxHunger)
        {
            GetComponent<CharaPermissions>().SetTeamNull();
        }
    }
    
}
