using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Generator : MonoBehaviour
{
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
        public RoadNode(int xNew, int yNew, int north, int south, int east, int west)
        {
            x = xNew;
            y = yNew;
            _north = north;
            _south = south;
            _east = east;
            _west = west;
        }

        //Serialize and Deserialize
        public static RoadNode Deserialize(int[] ser)
        {
            return new RoadNode(ser[0], ser[1], ser[2], ser[3], ser[4], ser[5]);
        }

        public int[] Serialize()
        {
            return new int[6] {x, y, _north, _south, _east, _west};
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

        public void Generate(float worldLengthAdaptor)
        {
            InstantiateRoad(worldLengthAdaptor);
        }

        private void InstantiateRoad(float worldLengthAdaptor)
        {
            //Le nom de la route est important
            //NB: On aurait pu instantié separemment chaque branche du noeud + la piece centrale
            string path = "Roads/eRoad";
            path += _north;
            path += _south;
            path += _east;
            path += _west;
            Instantiate(Resources.Load<GameObject>(path), new Vector3(x * worldLengthAdaptor, 0, y * worldLengthAdaptor), Quaternion.identity);
        }
    }
    

    public NavMeshSurface surface;
    private float tunkLength = 12.6f; //magic number with MagicaVoxel, tunk = technical chunk
    private float nTunkInDistrict = 5.0f;
    private int cityX = 15;
    private int cityY = 15;

    //Matrix où l'on stocke des Nodes
    private GameObject[,] depre_nodeMatrix;

    private RoadNode[,] masterRoads; //travaillé par master
    private int[] serializedRoads = null; //envoyé par RPC
    private RoadNode[,] roadMatrix; //décodé après RPC
    private bool receivedPackage = false;

    //Matrix où l'on stocke les emplacements de bâtiments (BuildSpot)
    private GameObject[,] buildSpotMatrix;
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

        //Start

    private void Start()
    {
        districtLength = tunkLength * nTunkInDistrict;
        roadMatrix = new RoadNode[cityX, cityY];

        if (PhotonNetwork.isMasterClient)
        {
            MasterInitialisation();
            MasterSetupRoads();

            UpdateRoads();
        }
    }

    //Initialisation

    private void MasterInitialisation()
    {
        masterRoads = new RoadNode[cityX, cityY];

        
        borderNorth = cityY - 1;
        borderSouth = 0;
        borderEast = cityX - 1;
        borderWest = 0;
        midX = cityX / 2;
        midY = cityY / 2;
    }

    //Main Roads

    private void MasterSetupRoads()
    {
        //remplir masterRoads
        InitMasterRoads();

        //Modifier masterRoads
        GenerateCityRoads();
    }

    private void InitMasterRoads()
    {
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                masterRoads[x, y] = new RoadNode(x, y);
            }
        }
    }

    private void GenerateCityRoads()
    {
        //Setup des nodes spécifiques

        RoadNode nodeMiddle = GetPoiMiddle();
        nodeMiddleX = nodeMiddle.x * districtLength;
        nodeMiddleY = nodeMiddle.y * districtLength;


        RoadNode nodePoiSW = GetPoiSouthWest();
        RoadNode nodePoiSE = GetPoiSouthEast();
        RoadNode nodePoiNW = GetPoiNorthWest();
        RoadNode nodePoiNE = GetPoiNorthEast();

        //Get les entrées
        int ouverture = 3; //Compris entre 1 et min(midX,midY), plus ouverture est petit plus la variance est grande

        RoadNode north = GetNodeInRange(ouverture, borderEast - ouverture, borderNorth, borderNorth);
        RoadNode south = GetNodeInRange(ouverture, borderEast - ouverture, 0, 0);
        RoadNode east = GetNodeInRange(borderEast, borderEast, ouverture, borderNorth - ouverture);
        RoadNode west = GetNodeInRange(0, 0, ouverture, borderNorth - ouverture);
        //ouvrir les entrées
        north.OpenNorth();
        south.OpenSouth();
        east.OpenEast();
        west.OpenWest();

        //Connectez des nodes avec routes

        ConnectCardinalTo(nodeMiddle, north, south, east, west);

        CreateRoad(nodeMiddle, nodePoiNE);
        CreateRoad(nodeMiddle, nodePoiSE);
        CreateRoad(nodeMiddle, nodePoiNW);
        CreateRoad(nodeMiddle, nodePoiSW);

        ConnectAllBetween(nodePoiSW, nodePoiSE, nodePoiNE, nodePoiNW);
        ConnectAllBetween(north, east, south, west);
    }

    //Send via RPC

    private void UpdateRoads()
    {
        int[] package = Encrypt();

        GetComponent<PhotonView>().RPC("SendRoads",PhotonTargets.AllBuffered,package);
    }

    private int[] Encrypt()
    {
        Debug.Log("Generator: Encrypting as " + PhotonNetwork.player.name);
        int[] package = new int[cityX*cityY*6 + 2];

        package[0] = cityX;
        package[1] = cityY;

        int x = 0;
        int y = 0;
        int index = 2;
        while (x < cityX)
        {
            y = 0;
            while (y < cityY)
            {
                foreach (int n in masterRoads[x, y].Serialize())
                {
                    package[index] = n;
                    index++;
                }
                y++;
            }
            x++;
        }

        return package;
    }

    private void Decrypt()
    {
        Debug.Log("Generator: Decrypting as " + PhotonNetwork.player.name);
        cityX = serializedRoads[0];
        cityY = serializedRoads[1];

        int x = 0;
        int y = 0;
        int index = 2;



        int[] nodeSerialized = new int[6];
        while (x < cityX)
        {
            y = 0;
            while (y < cityY)
            {
                for (int i = 0; i < 6; i++)
                {
                    nodeSerialized[i] = serializedRoads[index + i];
                }
                roadMatrix[x, y] = RoadNode.Deserialize(nodeSerialized);

                index += 6;
                y++;
            }
            x++;
        }
    }

    [PunRPC]
    private void SendRoads(int[] package)
    {
        Debug.Log("Generator: Received package as " + PhotonNetwork.player.name);
        serializedRoads = package;
    }
    private void Update()
    {
        if (!receivedPackage && serializedRoads != null)
        {
            //(serializedRoads a été modifié par RPC)
            Decrypt();

            receivedPackage = true;

            //Generate les routes
            GenerateOnNode();

            //Update le NavMeshSurface entierement (fin)
            surface.BuildNavMesh();
        }
    }


    //GenerateOnNode
    private void GenerateOnNode()
    {
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                roadMatrix[x, y].Generate(districtLength);
            }
        }
    }

    //PlaceNode

    #region GenerateCityRoads

    //Node Getters
    private RoadNode GetNodeInRange(int xMin, int xMax, int yMin, int yMax)
    {
        int x = Random.Range(xMin, xMax + 1);
        int y = Random.Range(yMin, yMax + 1);

        return masterRoads[x, y];
    }
    private RoadNode GetPoiMiddle()
    {
        return GetNodeInRange(
            midX - 1,
            midX + 1,
            cityY / 2 - 1,
            cityY / 2 + 1);
    }

    private RoadNode GetPoiSouthWest()
    {
        return GetNodeInRange(
            1,
            midX - 2,
            1,
            midY - 2);
    }
    private RoadNode GetPoiSouthEast()
    {
        return GetNodeInRange(
            midX + 2,
            borderEast - 1,
            1,
            midY - 2);
    }
    private RoadNode GetPoiNorthWest()
    {
        return GetNodeInRange(
            1,
            midX - 2,
            midY + 2,
            borderNorth - 1);
    }
    private RoadNode GetPoiNorthEast()
    {
        return GetNodeInRange(
            midX + 2,
            borderEast - 1,
            midY + 2,
            borderNorth - 1);
    }

    //Connectors
    private void ConnectCardinalTo(RoadNode node, RoadNode north, RoadNode south, RoadNode east, RoadNode west)
    {
        ConnectPoint(FindPath(north, node));
        ConnectPoint(FindPath(south, node));
        ConnectPoint(FindPath(east, node));
        ConnectPoint(FindPath(west, node));
    }
    private void ConnectAllBetween(RoadNode n1, RoadNode n2, RoadNode n3, RoadNode n4)
    {
        CreateRoad(n1, n2);
        CreateRoad(n2, n3);
        CreateRoad(n3, n4);
        CreateRoad(n4, n1);
    }

    //CreateRoad
    private List<RoadNode> FindPath(RoadNode node1, RoadNode node2)
    {
        List<RoadNode> path = new List<RoadNode>();
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
            path.Add(masterRoads[currX, currY]);
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
        path.Add(masterRoads[destX, destY]);

        return path;
    }
    private void ConnectPoint(List<RoadNode> path)
    {
        if (path.Count == 0)
        {
            Debug.Log("ConnectPoint: path is empty");
            return;
        }

        RoadNode previousNode = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            previousNode.OpenTo(path[i]);
            previousNode = path[i];
        }
    }
    private void CreateRoad(RoadNode node1, RoadNode node2)
    {
        ConnectPoint(FindPath(node1, node2));
    }

    #endregion

    #region GenerateCityBuildings

    private void GenerateCityBuildings()
    {
        PlaceBuildSpot();

        GenerateOnBuildSpot();
    }

    private void PlaceBuildSpot()
    {
        NodeHead nodeSource;
        GameObject buildSpot;
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                //InstantiateBuildSpotAroundNode(nodeMatrix[x, y].GetComponent<NodeHead>());
            }
        }
    }

    private void InstantiateBuildSpotAroundNode(NodeHead nodeSource)
    {
        GameObject buildSpot;

        int nodeXPos = nodeSource.x;
        int nodeYPos = nodeSource.y;

        float nodeX = nodeXPos * districtLength; //Position dans la scene
        float nodeY = nodeYPos * districtLength;


        //South West 0
        buildSpot = Instantiate(
            Resources.Load<GameObject>("eBuildSpot"), 
            new Vector3(nodeX - 25.2f, 0f, nodeY - 25.2f), Quaternion.identity);

        buildSpot.GetComponent<BuildSpotHead>().Init(nodeSource,0);

        buildSpotMatrix[2 * nodeXPos, 2 * nodeYPos] = buildSpot;


        //South East 1
        buildSpot = Instantiate(
            Resources.Load<GameObject>("eBuildSpot"), 
            new Vector3(nodeX + 12.6f, 0f, nodeY - 25.2f), Quaternion.identity);

        buildSpot.GetComponent<BuildSpotHead>().Init(nodeSource,1);

        buildSpotMatrix[2 * nodeXPos + 1, 2 * nodeYPos] = buildSpot;


        //North East 2
        buildSpot = Instantiate(
            Resources.Load<GameObject>("eBuildSpot"), 
            new Vector3(nodeX + 12.6f, 0f, nodeY + 12.6f), Quaternion.identity);

        buildSpot.GetComponent<BuildSpotHead>().Init(nodeSource,2);

        buildSpotMatrix[2 * nodeXPos + 1, 2 * nodeYPos + 1] = buildSpot;


        //North West 3
        buildSpot = Instantiate(
            Resources.Load<GameObject>("eBuildSpot"), 
            new Vector3(nodeX - 25.2f, 0f, nodeY + 12.6f), Quaternion.identity);

        buildSpot.GetComponent<BuildSpotHead>().Init(nodeSource,3);

        buildSpotMatrix[2 * nodeXPos, 2 * nodeYPos + 1] = buildSpot;
    }

    private void GenerateOnBuildSpot()
    {
        for (int y = 0; y < 2 * cityY; y++)
        {
            for (int x = 0; x < 2 * cityX; x++)
            {
                buildSpotMatrix[x, y].GetComponent<BuildSpotHead>().Generate();
            }
        }
    }

    #endregion








}
