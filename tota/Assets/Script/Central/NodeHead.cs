using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeHead : Photon.MonoBehaviour
{
    private int north = 0;
    private int south = 0;
    private int east = 0;
    private int west = 0;

    public int x = 0;
    public int y = 0;

    private Random rng;

    public void SetCoord(int xNew, int yNew)
    {
        x = xNew;
        y = yNew;
    }

    public void OpenTo(NodeHead node)
    {
        if (x > node.x) //on est à l'est de node
        {
            west = 1;
            node.east = 1;
        }
        else if (x < node.x) //on est à l'ouest de node
        {
            east = 1;
            node.west = 1;
        }
        else if (y > node.y) //on est au nord de node
        {
            south = 1;
            node.north = 1;
        }
        else if (y < node.y) //on est au sud de node
        {
            north = 1;
            node.south = 1;
        }
        else
        {
            Debug.Log("NodeHead: OpenTo: Compared same node coord");
        }
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
        //Randomize();

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
