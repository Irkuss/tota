using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeHead : Photon.MonoBehaviour
{
    private int north = 0;
    private int south = 0;
    private int east = 0;
    private int west = 0;

    private Random rng;

    void Start()
    {

    }

    public void OpenNorth()
    {
        north = 1;
    }
    public void OpenSouth()
    {
        south = 1;
    }
    public void OpenEast()
    {
        east = 1;
    }
    public void OpenWest()
    {
        west = 1;
    }

    public void Generate()
    {
        Randomize();

        InstantiateRoad();
    }

    private void Randomize()
    {
        int result = Random.Range(0, 3);

        switch(result)
        {
            case 0:
                north = 1;
                south = 1;
                east = 1;
                west = 1;
                break;
            case 1:
                north = 1;
                south = 1;
                break;
            default:
                east = 1;
                west = 1;
                break;
        }
    }

    private void InstantiateRoad()
    {
        string path = "Roads/eRoad";
        path += north;
        path += south;
        path += east;
        path += west;

        PhotonNetwork.Instantiate(path, this.transform.position, this.transform.rotation, 0);
    }
}
