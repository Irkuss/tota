using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Generator : MonoBehaviour
{
    public NavMeshSurface surface;
    private float tunkLength = 12.6f; //magic number with MagicaVoxel, tunk = technical chunk
    private float nTunkInDistrict = 5.0f;
    private int cityX = 15;
    private int cityY = 15;

    //Matrix où l'on stocke des Nodes
    private GameObject[,] nodeMatrix;
    //Variable aide
    private float districtLength;
    private int borderNorth;
    private int borderSouth;
    private int borderEast;
    private int borderWest;
    private int midX;
    private int midY;

    public float nodeMiddleX;
    public float nodeMiddleY;


    //Qd y augmente -> Nord
    //Qd x augmente -> Est

    // Unity Callback
    void Start()
    {
        //Initialisation des variables aides
        districtLength = tunkLength * nTunkInDistrict;
        borderNorth = cityY - 1;
        borderSouth = 0;
        borderEast = cityX - 1;
        borderWest = 0;
        midX = cityX / 2;
        midY = cityY / 2;

        //Generate
        GenerateCity();

        //Update le NavMeshSurface entierement (fin)
        surface.BuildNavMesh();
    }
    
    

    private void GenerateCity()
    {
        nodeMatrix = new GameObject[cityX,cityY];

        //Place un quadriallage de Node
        PlaceNode();

        //Setup des nodes spécifiques

        NodeHead nodeMiddle = GetPoiMiddle();
        nodeMiddleX = nodeMiddle.x * districtLength;
        nodeMiddleY = nodeMiddle.y * districtLength;


        NodeHead nodePoiSW = GetPoiSouthWest();
        NodeHead nodePoiSE = GetPoiSouthEast();
        NodeHead nodePoiNW = GetPoiNorthWest();
        NodeHead nodePoiNE = GetPoiNorthEast();

        //Get les entrées
        int ouverture = 3; //Compris entre 1 et min(midX,midY), plus ouverture est petit plus la variance est grande

        NodeHead north = GetNodeInRange (ouverture  , borderEast- ouverture , borderNorth   , borderNorth   );
        NodeHead south = GetNodeInRange (ouverture  , borderEast- ouverture , 0             , 0             );
        NodeHead east = GetNodeInRange  (borderEast , borderEast            , ouverture     , borderNorth - ouverture);
        NodeHead west = GetNodeInRange  (0          ,0                      , ouverture     , borderNorth - ouverture);
            //ouvrir les entrées
            north.OpenNorth();
            south.OpenSouth();
            east.OpenEast();
            west.OpenWest();

        //Connectez des nodes avec routes

        ConnectCardinalTo(nodeMiddle,north,south,east,west);

        CreateRoad(nodeMiddle, nodePoiNE);
        CreateRoad(nodeMiddle, nodePoiSE);
        CreateRoad(nodeMiddle, nodePoiNW);
        CreateRoad(nodeMiddle, nodePoiSW);

        ConnectAllBetween(nodePoiSW, nodePoiSE, nodePoiNE, nodePoiNW);
        ConnectAllBetween(north, east, south, west);


        

        //Appel de generate sur chaque node (fin)
        GenerateOnNode();
    }

    

    //Node Getters
    private NodeHead GetNodeInRange(int xMin, int xMax, int yMin, int yMax)
    {
        int x = Random.Range(xMin, xMax + 1);
        int y = Random.Range(yMin, yMax + 1);

        return nodeMatrix[x, y].GetComponent<NodeHead>();
    }

    private NodeHead GetPoiMiddle()
    {
        return GetNodeInRange(
            midX - 1,
            midX + 1,
            cityY / 2 - 1,
            cityY / 2 + 1);
    }

    private NodeHead GetPoiSouthWest()
    {
        return GetNodeInRange(
            1,
            midX - 2,
            1,
            midY - 2);
    }
    private NodeHead GetPoiSouthEast()
    {
        return GetNodeInRange(
            midX + 2,
            borderEast - 1,
            1,
            midY - 2);
    }
    private NodeHead GetPoiNorthWest()
    {
        return GetNodeInRange(
            1,
            midX - 2,
            midY + 2,
            borderNorth - 1);
    }
    private NodeHead GetPoiNorthEast()
    {
        return GetNodeInRange(
            midX + 2,
            borderEast - 1,
            midY + 2,
            borderNorth - 1);
    }

    //Connectors
    private void ConnectCardinalTo(NodeHead node,NodeHead north,NodeHead south,NodeHead east,NodeHead west)
    {
        ConnectPoint(FindPath(north, node));
        ConnectPoint(FindPath(south, node));
        ConnectPoint(FindPath(east, node));
        ConnectPoint(FindPath(west, node));
    }
    private void ConnectAllBetween(NodeHead n1, NodeHead n2, NodeHead n3, NodeHead n4)
    {
        CreateRoad(n1, n2);
        CreateRoad(n2, n3);
        CreateRoad(n3, n4);
        CreateRoad(n4, n1);
    }

    //CreateRoad
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
    private void CreateRoad(NodeHead node1, NodeHead node2)
    {
        ConnectPoint(FindPath(node1, node2));
    }

    //PlaceNode
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

    //GenerateOnNode
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
