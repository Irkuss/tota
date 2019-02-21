using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorOpacity : MonoBehaviour
{
    public int floor;
    private FloorManager floorManager;

    
    void Start()
    {
        floorManager = GameObject.Find("eCentralManager").GetComponent<FloorManager>();
    }

    private void Update()
    {
        if (floor > floorManager.GetFloorLevel())
        {
            GetComponent<Renderer>().enabled = false;
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
        }
    }
}
