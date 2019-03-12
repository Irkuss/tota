using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    private static float _chunkHeight = 8.0f;
    public static float ChunkHeight { get => _chunkHeight; }

    private int floorLevel = 8;

    private int maxFloorLevel = 8; //A MODIFIER SI ON A DES BATIMENTS AVEC PLUS D'ETAGES

    //Callback creation: when floorLevel is modified

    public delegate void UpdateFloorLevel(int floorLv);
    public static event UpdateFloorLevel onFloorLevelChanged;

    private void UpdateCallback()
    {
        if (onFloorLevelChanged != null) //Si une personne nous écoute
        {
            onFloorLevelChanged(floorLevel); //Declenche le callback chez les spectateurs
        }
    }

    //Getters

    public int GetFloorLevel()
    {
        return floorLevel;
    }

    public int GetMaxFloorLevel()
    {
        return maxFloorLevel;
    }

    //Setters

    public void TryDecrease()
    {
        if (floorLevel < maxFloorLevel)
        {
            floorLevel++;
            //Debug.Log("FloorManager: downed to " + floorLevel);

            //Call callbacks to everyone listening
            UpdateCallback();
        }
    }

    public void TryIncrease()
    {
        if (floorLevel > 0)
        {
            floorLevel--;
            //Debug.Log("FloorManager: upped to " + floorLevel);

            //Call callbacks to everyone listening
            UpdateCallback();
        }
    }

    public void TrySet(int newLevel)
    {
        if (newLevel >= 0 && newLevel <= maxFloorLevel)
        {
            floorLevel = newLevel;
            onFloorLevelChanged(floorLevel);
        }
    }
}
