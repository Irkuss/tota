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

    //UnityCallback
    private void Start()
    {
        _strength = Random.Range(1, 20) * 5;
        _intelligence = Random.Range(1, 20) * 5;
        _perception = Random.Range(1, 20) * 5;
    }
    //Public Getters
    public string GetFullName()
    {
        return _nameFirst + " " + _nameLast;
    }
    public string[] GetToolTipInfo()
    {
        return new string[4] { GetFullName(), _strength.ToString(), _intelligence.ToString(), _perception.ToString() };
    }
    
}
