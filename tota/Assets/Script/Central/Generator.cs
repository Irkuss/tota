using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Generator : MonoBehaviour
{
    public NavMeshSurface surface;
    private float tunkLength = 12.6f; //magic number with MagicaVoxel, tunk = technical chunk
    private float nTunkInDistrict = 5.0f;
    public int cityX = 4;
    public int cityY = 5;
    private GameObject[,] nodeMatrix;
    private float districtLength;

    
    //Qd y augmente -> Nord
    //Qd x augmente -> Est

    // Start is called before the first frame update
    void Start()
    {
        GenerateCity();

        

        surface.BuildNavMesh();
    }

    private GameObject GetNodeNorth()
    {
        return nodeMatrix[  cityX / 2   , cityY - 1 ];
    }
    private GameObject GetNodeSouth()
    {
        return nodeMatrix[  cityX / 2   , 0         ];
    }
    private GameObject GetNodeEast()
    {
        return nodeMatrix[  cityX - 1   , cityY / 2 ];
    }
    private GameObject GetNodeWest()
    {
        return nodeMatrix[  0           , cityY / 2 ];
    }

    private void GenerateCity()
    {
        districtLength = tunkLength * nTunkInDistrict;
        nodeMatrix = new GameObject[cityX,cityY];

        PlaceNode();

        NodeHead nodeTarget1 = nodeMatrix[4, 7].GetComponent<NodeHead>();
        NodeHead nodeTarget2 = nodeMatrix[2, 3].GetComponent<NodeHead>();

        ConnectCardinalTo(nodeTarget1);
        ConnectCardinalTo(nodeTarget2);

        GenerateOnNode();
    }

    private void ConnectCardinalTo(NodeHead node)
    {
        NodeHead north = GetNodeNorth().GetComponent<NodeHead>();
        NodeHead south = GetNodeSouth().GetComponent<NodeHead>();
        NodeHead east = GetNodeEast().GetComponent<NodeHead>();
        NodeHead west = GetNodeWest().GetComponent<NodeHead>();

        ConnectPoint(FindPath(north, node));
        ConnectPoint(FindPath(south, node));
        ConnectPoint(FindPath(east, node));
        ConnectPoint(FindPath(west, node));
    }

    private List<NodeHead> FindPath(NodeHead node1, NodeHead node2)
    {
        List<NodeHead> path = new List<NodeHead>();
        //Current X and Y
        int currX = node1.x;
        int currY = node1.y;
        //Destination X and Y
        int destX = node2.x;
        int destY = node2.y;
        //Distance between Destination and Current
        int distX; //Si distX positif, la destination est à l'Est
        int distY; //Si distY positif, la destination est au Nord

        int absDistX;
        int absDistY;

        while (currX != destX || currY != destY)
        {
            //On ajoute la node là où on se trouve
            path.Add(nodeMatrix[currX, currY].GetComponent<NodeHead>());
            //On met à jour la distance
            distX = destX - currX;
            distY = destY - currY;
            //On met à jour la distance absolu
            absDistX = (distX > 0) ? distX : -distX;
            absDistY = (distY > 0) ? distY : -distY;
            //Choisir l'axe de déplacement
            if (absDistX > absDistY)
            {
                if (distX > 0)
                {
                    currX++; //On se déplace à l'Est
                }
                else
                {
                    currX--; //On se déplace à l'Ouest
                }
            }
            else
            {
                if (distY > 0)
                {
                    currY++;  //On se déplace au Nord
                }
                else
                {
                    currY--;  //On se déplace au Sud
                }
            }
            //Fin de la boucle, on a mis à jour les coordonnées suivantes
        }
        //Il manque la destination dans path
        path.Add(nodeMatrix[destX, destY].GetComponent<NodeHead>());

        return path;
    }

    private void ConnectPoint(List<NodeHead> path)
    {
        if (path.Count == 0)
        {
            Debug.Log("ConnectPoint: path is empty");
            return;
        }

        NodeHead previousNode = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            previousNode.OpenTo(path[i]);
            previousNode = path[i];
        }
    }

    private void PlaceNode()
    {
        GameObject node;
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                Vector3 position = new Vector3(x * districtLength, 0f, y * districtLength);

                node = PhotonNetwork.Instantiate("eNode",position,Quaternion.identity,0);
                nodeMatrix[x, y] = node;
                node.GetComponent<NodeHead>().SetCoord(x, y);
            }
        }
    }

    private void GenerateOnNode()
    {
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                nodeMatrix[x, y].GetComponent<NodeHead>().Generate();
            }
        }
    }
}
