using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public const float c_chunkHeight = 8.0f;

    private int _floorLevel = 8;
    public int FloorLevel => _floorLevel;

    private readonly int _maxFloorLevel = 8; //A MODIFIER SI ON A DES BATIMENTS AVEC PLUS D'ETAGES

    //Callback creation: when floorLevel is modified

    public delegate void UpdateFloorLevel(int floorLv);
    public static event UpdateFloorLevel onFloorLevelChanged;

    private void UpdateCallback()
    {
        if (onFloorLevelChanged != null) //Si une personne nous écoute
        {
            onFloorLevelChanged(_floorLevel); //Declenche le callback chez les spectateurs
        }
    }

    //Getters

    public int GetFloorLevel()
    {
        return _floorLevel;
    }

    public int GetMaxFloorLevel()
    {
        return _maxFloorLevel;
    }

    //Setters

    public void TryDecrease()
    {
        if (_floorLevel < _maxFloorLevel)
        {
            _floorLevel++;
            //Debug.Log("FloorManager: downed to " + floorLevel);

            //Call callbacks to everyone listening
            UpdateCallback();
        }
    }

    public void TryIncrease()
    {
        if (_floorLevel > 0)
        {
            _floorLevel--;
            //Debug.Log("FloorManager: upped to " + floorLevel);

            //Call callbacks to everyone listening
            UpdateCallback();
        }
    }

    public void TrySet(int newLevel)
    {
        if (newLevel >= 0 && newLevel <= _maxFloorLevel)
        {
            _floorLevel = newLevel;
            onFloorLevelChanged(_floorLevel);
        }
    }
}
