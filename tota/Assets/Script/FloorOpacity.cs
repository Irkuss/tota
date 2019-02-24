using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorOpacity : MonoBehaviour
{
    public int currentFloor = 0;
    //private FloorManager floorManager;

    
    void Start()
    {
        //subscribe to onFloorLevelChanged
        FloorManager.onFloorLevelChanged += UpdateRenderer;
        
    }

    private void UpdateRenderer(int newFloorLevel) //Subscribed to onFloorLevelChanged
    {
        if (currentFloor > newFloorLevel)
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }
    }
}
