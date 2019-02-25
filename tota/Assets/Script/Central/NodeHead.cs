using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeHead : Photon.MonoBehaviour
{
    // DEPRECATED
    // DEPRECATED
    // DEPRECATED

    public class RoadNode
    {
        //directions dans laquelle la route part (16 combinaisons différentes)
        private int _north = 0;
        private int _south = 0;
        private int _east = 0;
        private int _west = 0;

        //Coordonnée dans la matrice de Generator
        public int x;
        public int y;

        //Constructor

        public RoadNode(int xNew, int yNew)
        {
            x = xNew;
            y = yNew;
        }

        //Public Getters

        public int GetNorth()
        {
            return _north;
        }
        public int GetSouth()
        {
            return _south;
        }
        public int GetEast()
        {
            return _east;
        }
        public int GetWest()
        {
            return _west;
        }

        //Public Setters

        public void OpenTo(RoadNode node)
        {
            if (x > node.x) //on est à l'est de node
            {
                _west = 1;
                node._east = 1;
            }
            else if (x < node.x) //on est à l'ouest de node
            {
                _east = 1;
                node._west = 1;
            }
            else if (y > node.y) //on est au nord de node
            {
                _south = 1;
                node._north = 1;
            }
            else if (y < node.y) //on est au sud de node
            {
                _north = 1;
                node._south = 1;
            }
            else
            {
                Debug.Log("NodeHead: OpenTo: Compared same node coord");
            }
        }

        public void OpenNorth()
        {
            _north = 1;
        }
        public void OpenSouth()
        {
            _south = 1;
        }
        public void OpenEast()
        {
            _east = 1;
        }
        public void OpenWest()
        {
            _west = 1;
        }

        //End Generate

        public void Generate()
        {
            InstantiateRoad();
        }

        private void InstantiateRoad()
        {
            //Le nom de la route est important
            //NB: On aurait pu instantié separemment chaque branche du noeud + la piece centrale
            string path = "Roads/eRoad";
            path += _north;
            path += _south;
            path += _east;
            path += _west;
            PhotonNetwork.Instantiate(path, new Vector3(x, 0, y), Quaternion.identity, 0);
        }
    }



    // DEPRECATED
    // DEPRECATED
    // DEPRECATED


    //directions dans laquelle la route part (16 combinaisons différentes)
    private int _north = 0;
    private int _south = 0;
    private int _east = 0;
    private int _west = 0;

    //Coordonnée dans la matrice de Generator
    public int x = 0;
    public int y = 0;

    //Public Getters

    public int GetNorth()
    {
        return _north;
    }
    public int GetSouth()
    {
        return _south;
    }
    public int GetEast()
    {
        return _east;
    }
    public int GetWest()
    {
        return _west;
    }

    //Public Setters

    public void SetCoord(int xNew, int yNew)
    {
        x = xNew;
        y = yNew;
    }

    public void OpenTo(NodeHead node)
    {
        if (x > node.x) //on est à l'est de node
        {
            _west = 1;
            node._east = 1;
        }
        else if (x < node.x) //on est à l'ouest de node
        {
            _east = 1;
            node._west = 1;
        }
        else if (y > node.y) //on est au nord de node
        {
            _south = 1;
            node._north = 1;
        }
        else if (y < node.y) //on est au sud de node
        {
            _north = 1;
            node._south = 1;
        }
        else
        {
            Debug.Log("NodeHead: OpenTo: Compared same node coord");
        }
    }

    public void OpenNorth()
    {
        _north = 1;
    }
    public void OpenSouth()
    {
        _south = 1;
    }
    public void OpenEast()
    {
        _east = 1;
    }
    public void OpenWest()
    {
        _west = 1;
    }

    //End Generate

    public void Generate()
    {
        InstantiateRoad();
    }

    private void InstantiateRoad()
    {
        //Le nom de la route est important
        //NB: On aurait pu instantié separemment chaque branche du noeud + la piece centrale
        string path = "Roads/eRoad";
        path += _north;
        path += _south;
        path += _east;
        path += _west;
        PhotonNetwork.Instantiate(path, this.transform.position, this.transform.rotation, 0);
    }
}
