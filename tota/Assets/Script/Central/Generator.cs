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

        //Nombre d'argument du deuxieme constructeur de RoadNode (prévu pour la Deserialisation)
        private static int _packageSize = 6;
        public static int GetPackageSize()
        {
            return _packageSize;
        }

        //Constructor

        public RoadNode(int xNew, int yNew)
        {
            //Master creation
            x = xNew;
            y = yNew;
        }
        public RoadNode(int xNew, int yNew, int north, int south, int east, int west)
        {
            //part of the Deserialization process
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

    public class BuildNode
    {
        //Coordonnée du chunk dans le monde
        private int _x;
        private int _y;

        //directions dont l'espace est libre (0 = libre, 1 occupé)
        private int _north = 0;
        private int _south = 0;
        private int _east = 0;
        private int _west = 0;

        //All BuildSpotHead can force other BuildSpotHead to not generate on them
        public bool isFree = true;

        //Origin Node
        private int _posToNode;//0 = SW , 1 = SE , 2 = NE, 3 = NW
        private int _facingRotation = 0; //0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east

        //Building to build
        private string _pathBuilding = ""; //Chemin du batiment (à partir de Resources)
        private int _buildingSizeType = 0; //0 = 2x2, 1 = 2x4, 2 = 2x4, 3 = 4*4 (à update a chaque qu'on decide d'entendre la taille possible des batiments

        //Nombre d'argument du deuxieme constructeur de BuildNode (prévu pour la Deserialisation)
        private static int _packageSize = 10;
        public static int GetPackageSize()
        {
            return _packageSize;
        }

        //Constructeur
        public BuildNode(int xNew, int yNew, RoadNode origin, int pos)
        {
            _x = xNew;
            _y = yNew;

            _posToNode = pos;

            switch (_posToNode)
            {
                case 0: //0 = SW
                    InitSetSpace(origin.GetWest(), 0, origin.GetSouth(), 0);
                    break;
                case 1: //1 = SE
                    InitSetSpace(origin.GetEast(), 0, 0, origin.GetSouth());
                    break;
                case 2: //2 = NE
                    InitSetSpace(0, origin.GetEast(), 0, origin.GetNorth());
                    break;
                default: //3 = NW
                    InitSetSpace(0, origin.GetWest(), origin.GetNorth(), 0);
                    break;
            }
            InitRotation();

            //TEMPORAIRE
            DecidePath();
        }
        public BuildNode(int xNew, int yNew, int north, int south, int east, int west, bool liberty, int pos, int rotation, int size, string path)
        {
            _x = xNew;
            _y = yNew;

            _north = north;
            _south = south;
            _east = east;
            _west = west;

            isFree = liberty;

            _posToNode = pos;
            _facingRotation = rotation;
            _buildingSizeType = size;

            _pathBuilding = path;
        }

        public void SetPath(string path)
        {
            _pathBuilding = path;
        }

        private void InitSetSpace(int north, int south, int east, int west)
        {
            _north = north;
            _south = south;
            _east = east;
            _west = west;
        }
        private void InitRotation()
        {
            List<int> validRotation = new List<int>();
            if (_south > 0) validRotation.Add(0);
            if (_west > 0) validRotation.Add(1);
            if (_north > 0) validRotation.Add(2);
            if (_east > 0) validRotation.Add(3);

            if (validRotation.Count > 0)
            {
                _facingRotation = validRotation[Random.Range(0, validRotation.Count)];
            }
        }

        private void DecidePath()
        {
            //Choisir quel building construire
            int rng = Random.Range(0, 3);
            //rng = 0;
            string path = "Buildings/build22/test" + rng;

            _pathBuilding = path;
        }
        
        //Generate
        public void Generate(float worldLengthAdaptor)
        {
            if (!CanGenerateItself())
            {
                return;
            }
            Vector3 position = new Vector3(_x * worldLengthAdaptor, 0, _y * worldLengthAdaptor);

            Vector3 offset = CalculateOffset();

            //Instantié et le tourner dans la bonne direction
            GameObject build = Instantiate(Resources.Load<GameObject>(_pathBuilding), position + offset, Quaternion.identity);
            build.transform.Rotate(0, _facingRotation * 90f, 0);
        }
        private bool CanGenerateItself()
        {
            if (isFree)
            {
                if (_north + _south + _east + _west > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        private Vector3 CalculateOffset()
        {
            //Bien le placer
            Vector3 offsetRotation = new Vector3();

            switch (_facingRotation)
            {
                case 0: //0f = 0 * 90 -> south
                        //Debug.Log("facing South Moving not");
                    offsetRotation.Set(0, 0, 0);
                    break;
                case 1: //90f = 1 * 90 -> west
                        //Debug.Log("facing west Moving north");
                    offsetRotation.Set(0, 0, 12.6f);
                    break;
                case 2: //180f = 2 * 90 -> north
                        //Debug.Log("facing north Moving east and north");
                    offsetRotation.Set(12.6f, 0, 12.6f);
                    break;
                default://270f = 3 * 90 -> east
                        //Debug.Log("facing east Moving east");
                    offsetRotation.Set(12.6f, 0, 0);
                    break;
            }

            return offsetRotation;
        }

        //Serialize and Deserialize
        public static BuildNode Deserialize(int[] ser,string path)
        {
            bool liberty;
            if (ser[6] == 1)
            {
                liberty = true;
            }
            else
            {
                liberty = false;
            }

            return new BuildNode(ser[0], ser[1], ser[2], ser[3], ser[4], ser[5], liberty, ser[7], ser[8], ser[9], path);
        }

        public int[] SerializeArray()
        {
            int isFreeToInt;
            if (isFree)
            {
                isFreeToInt = 1;
            }
            else
            {
                isFreeToInt = 0;
            }
            return new int[10] { _x, _y, _north, _south, _east, _west, isFreeToInt, _posToNode, _facingRotation, _buildingSizeType };

        }
        public string SerializeString()
        {
            return _pathBuilding;
        }
    }

    #region Attributes

    public NavMeshSurface surface;
    private float tunkLength = 12.6f; //magic number with MagicaVoxel, tunk = technical chunk
    private float nTunkInDistrict = 5.0f;
    private int cityX = 15;
    private int cityY = 15;

    //Matrix où l'on stocke des Nodes
    private RoadNode[,] masterRoads; //matrice travaillée par Master
    private int[] serializedRoads = null; //envoyé par RPC
    private RoadNode[,] roadMatrix; //décodé après RPC, commun à tous

    private bool receivedPackage = false;

    //Matrix où l'on stocke les emplacements de bâtiments (BuildSpot)
    private BuildNode[,] masterBuilds; //matrice travaillée par Master
    private int[] serializedBuilds = null; //envoyé par RPC
    private string[] serializedPath = null; //envoyé par RPC
    private BuildNode[,] buildMatrix; //décodé après RPC, commun à tous

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

    #endregion


    // --- IMPORTANT TIPS AND TRICKS ---
    //Quandd y augmente -> on va vers le Nord
    //Quandd x augmente -> on ve vers l' Est

    //Unity Callabcks

    private void Start()
    {
        districtLength = tunkLength * nTunkInDistrict;
        roadMatrix = new RoadNode[cityX, cityY];
        buildMatrix = new BuildNode[cityX * 2, cityY * 2];

        if (PhotonNetwork.isMasterClient)
        {
            MasterInitialisation();

            MasterSetupRoads();
            MasterSetupBuilds();

            UpdateRoads();
            UpdateBuilds();
        }
    }

    private void Update()
    {
        if (!receivedPackage && serializedRoads != null && serializedBuilds != null &&  serializedPath != null)
        {
            receivedPackage = true;

            //(serializedRoads a été modifié par RPC de meme pour cityX et cityY)
            DecryptRoads();

            DecryptBuilds();

            //Generate les routes
            GenerateRoads();

            //Generate les bâtiments
            GenerateBuilds();

            //Update le NavMeshSurface entierement (fin)
            surface.BuildNavMesh();
        }
    }

    //Master Initialisation

    private void MasterInitialisation()
    {
        masterRoads = new RoadNode[cityX, cityY];
        masterBuilds = new BuildNode[cityX * 2, cityY * 2];


        borderNorth = cityY - 1;
        borderSouth = 0;
        borderEast = cityX - 1;
        borderWest = 0;
        midX = cityX / 2;
        midY = cityY / 2;
    }

    //Master  Roads

    private void MasterSetupRoads()
    {
        //remplir masterRoads
        InitMasterRoads();

        //Modifier masterRoads
        MasterModifyRoads();
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

    private void MasterModifyRoads()
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

    //Master  Builds
    private void MasterSetupBuilds()
    {
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                MasterBuildAroundNode(masterRoads[x, y], x, y);
            }
        }
    }

    private void MasterBuildAroundNode(RoadNode roadOrigin, int xInMatrix, int yInMatrix)
    {
        int xChunk = roadOrigin.x * 5;
        int yChunk = roadOrigin.y * 5;

        //South West 0
        //(nodeX - 25.2f, 0f, nodeY - 25.2f)

        masterBuilds[2 * xInMatrix, 2 * yInMatrix] = new BuildNode(xChunk - 2, yChunk - 2, roadOrigin, 0);

        //South East 1
        //(nodeX + 12.6f, 0f, nodeY - 25.2f)

        masterBuilds[2 * xInMatrix + 1, 2 * yInMatrix] = new BuildNode(xChunk + 1, yChunk - 2, roadOrigin, 1);

        //North East 2
        //(nodeX + 12.6f, 0f, nodeY + 12.6f)

        masterBuilds[2 * xInMatrix + 1, 2 * yInMatrix + 1] = new BuildNode(xChunk + 1, yChunk + 1, roadOrigin, 2);

        //North West 3
        //(nodeX - 25.2f, 0f, nodeY + 12.6f)

        masterBuilds[2 * xInMatrix, 2 * yInMatrix + 1] = new BuildNode(xChunk - 2, yChunk + 1, roadOrigin, 3);
    }

    #region  Send via RPC

    //Send roads via RPC
    private void UpdateRoads()
    {
        int[] package = EncryptRoads();

        GetComponent<PhotonView>().RPC("SendRoads",PhotonTargets.AllBuffered,package);
    }
    private int[] EncryptRoads()
    {
        Debug.Log("Generator: Encrypting roads as " + PhotonNetwork.player.name);
        int[] package = new int[cityX*cityY*(RoadNode.GetPackageSize()) + 2];

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
    private void DecryptRoads()
    {
        Debug.Log("Generator: Decrypting roads as " + PhotonNetwork.player.name);
        cityX = serializedRoads[0];
        cityY = serializedRoads[1];

        int x = 0;
        int y = 0;
        int index = 2;

        int[] nodeSerialized = new int[RoadNode.GetPackageSize()];
        while (x < cityX)
        {
            y = 0;
            while (y < cityY)
            {
                //On recupere la version serialisé de la RoadNode
                for (int i = 0; i < RoadNode.GetPackageSize(); i++)
                {
                    nodeSerialized[i] = serializedRoads[index + i];
                }
                //On ajoute la RoadNode dans la matrice prévu à cette effet
                roadMatrix[x, y] = RoadNode.Deserialize(nodeSerialized);

                //Incrementation
                index += RoadNode.GetPackageSize();
                y++;
            }
            x++;
        }
    }
    [PunRPC]
    private void SendRoads(int[] package)
    {
        Debug.Log("Generator: Received road package as " + PhotonNetwork.player.name);
        serializedRoads = package;
    }

    //Send buildings via RPC
    private void UpdateBuilds()
    {
        GetComponent<PhotonView>().RPC("SendBuildsArray", PhotonTargets.AllBuffered, EncryptBuildsArray());
        GetComponent<PhotonView>().RPC("SendBuildsPath", PhotonTargets.AllBuffered, EncryptBuildsPath());
    }
    private int[] EncryptBuildsArray()
    {
        Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.name);
        int[] package = new int[(2 * cityX) * (2 * cityY) * BuildNode.GetPackageSize()];

        int x = 0;
        int y = 0;
        int index = 0;
        while (x < 2 * cityX)
        {
            y = 0;
            while (y < 2 * cityY)
            {
                foreach (int n in masterBuilds[x, y].SerializeArray())
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
    private string[] EncryptBuildsPath()
    {
        Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.name);
        string[] package = new string[(2 * cityX) * (2 * cityY)];

        int x = 0;
        int y = 0;
        int index = 0;
        while (x < 2 * cityX)
        {
            y = 0;
            while (y < 2 * cityY)
            {
                package[index] = masterBuilds[x, y].SerializeString();
                index++;
                y++;
            }
            x++;
        }
        return package;
    }
    private void DecryptBuilds()
    {
        Debug.Log("Generator: Decrypting builds as " + PhotonNetwork.player.name);
        int x = 0;
        int y = 0;
        int indexArray = 0;
        int indexPath = 0;

        int[] buildSerialized = new int[BuildNode.GetPackageSize()];
        while (x < 2 * cityX)
        {
            y = 0;
            while (y < 2 * cityY)
            {
                //On recupere la version serialisé de la BuildNode
                for (int i = 0; i < BuildNode.GetPackageSize(); i++)
                {
                    buildSerialized[i] = serializedBuilds[indexArray + i];
                }
                //On ajoute la BuildNode dans la matrice prévu à cette effet (+version sérialisé de son path)
                buildMatrix[x, y] = BuildNode.Deserialize(buildSerialized, serializedPath[indexPath]);

                //Incrementation
                indexArray += BuildNode.GetPackageSize();
                indexPath++;
                y++;
            }
            x++;
        }
    }
    [PunRPC]
    private void SendBuildsArray(int[] package)
    {
        Debug.Log("Generator: Received build package as " + PhotonNetwork.player.name);
        serializedBuilds = package;
    }
    [PunRPC]
    private void SendBuildsPath(string[] package)
    {
        Debug.Log("Generator: Received buildpath package as " + PhotonNetwork.player.name);
        serializedPath = package;
    }

    #endregion

    //Generate
    private void GenerateRoads()
    {
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                roadMatrix[x, y].Generate(districtLength);
            }
        }
    }

    private void GenerateBuilds()
    {
        for (int y = 0; y < cityY * 2; y++)
        {
            for (int x = 0; x < cityX * 2; x++)
            {
                buildMatrix[x, y].Generate(tunkLength);
            }
        }
    }

    //PlaceNode

    #region MasterGenerateCityRoads

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








}
