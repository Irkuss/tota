using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    private int floorLevel = 8;

    private int maxFloorLevel = 8; //A MODIFIER SI ON A DES BATIMENTS AVEC PLUS D'ETAGES

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
            Debug.Log("FloorManager: downed to " + floorLevel);
        }
    }

    public void TryIncrease()
    {
        if (floorLevel > 0)
        {
            floorLevel--;
            Debug.Log("FloorManager: upped to " + floorLevel);
        }
    }

    public void TrySet(int newLevel)
    {
        if (newLevel >= 0 && newLevel <= maxFloorLevel)
        {
            floorLevel = newLevel;
        }
    }
}
