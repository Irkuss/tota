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
        public const int packageSize = 6;

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
            return new int[packageSize] {x, y, _north, _south, _east, _west};
        }

        //Public Getters

        public int GetNorth() { return _north; }
        public int GetSouth() { return _south; }
        public int GetEast() { return _east; }
        public int GetWest() { return _west; }

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

        public void OpenNorth() { _north = 1; }
        public void OpenSouth() { _south = 1; }
        public void OpenEast() { _east = 1; }
        public void OpenWest() { _west = 1; }

        //End Generate
        public void Generate(int xOffset, int yOffset)
        {
            //Le nom de la route est important
            //NB: On aurait pu instantié separemment chaque branche du noeud + la piece centrale
            string path = "Roads/eRoad";
            path += _north;
            path += _south;
            path += _east;
            path += _west;
            Instantiate(
                Resources.Load<GameObject>(path), 
                new Vector3(x * c_districtLength + xOffset * c_worldChunkLength, 0, y * c_districtLength + yOffset * c_worldChunkLength), 
                Quaternion.identity);
        }
    }

    public class BuildNode
    {
        //Coordonnée du chunk dans le monde
        private int _x;
        public int X { get => _x; }
        private int _y;
        public int Y { get => _y; }

        //directions dont l'espace est libre (0 = libre, 1 occupé)
        private int _north = 0;
        public int North { get => _north; }
        private int _south = 0;
        public int South { get => _south; }
        private int _east = 0;
        public int East { get => _east; }
        private int _west = 0;
        public int West { get => _west; }

        //All BuildSpotHead can force other BuildSpotHead to not generate on them
        private bool _isFree = true;
        public bool IsFree { get => _isFree; }
        public void SetSlave() { _isFree = false; }

        private bool _isNextToRoad;
        public bool IsNextToRoad { get => _isNextToRoad; }

        public bool generationDecided = false;

        //Origin Node
        private int _posToNode;//0 = SW , 1 = SE , 2 = NE, 3 = NW
        public int PosToNode { get => _posToNode; }
        private int _facingRotation = 0; //0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east
        public int FacingRotation { get => _facingRotation; }

        //Building to build
        private string _pathBuilding = ""; //Chemin du batiment (à partir de Resources)
        private int _buildingSizeType = 0; //0 = 2x2, 1 = 2x4, 2 = 2x4, 3 = 4*4 (à update a chaque qu'on decide d'entendre la taille possible des batiments

        //Nombre d'argument du deuxieme constructeur de BuildNode (prévu pour la Deserialisation)
        public const int c_packageSize = 10;

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

            _isNextToRoad = _north + _south + _east + _west > 0;

            //TEMPORAIRE
            //DecidePath();
        }
        public BuildNode(int xNew, int yNew, int north, int south, int east, int west, bool liberty, int pos, int rotation, int size, string path)
        {
            _x = xNew;
            _y = yNew;

            _north = north;
            _south = south;
            _east = east;
            _west = west;

            _isFree = liberty;

            _posToNode = pos;
            _facingRotation = rotation;
            _buildingSizeType = size;
            
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

        //Appelé dans MasterChoice apres CornerBehavior
        public void SetPath(string path)
        {
            _pathBuilding = path;
        }
        
        //Generate
        public bool Generate(int xOffset, int yOffset)
        {
            //(NB: on utilise pas le retour bool so far)
            //Verifions que le building peut se générer
            if (!CanGenerateBuilding())
            {
                return false;
            }

            //Le building peut se généré, debut de la génération
            //Debug.Log("Generate: Instantiating with path: " + _pathBuilding);

            Vector3 position = new Vector3(
                _x * c_voxChunkLength + 6.3f + xOffset * c_worldChunkLength, 
                0,
                _y * c_voxChunkLength + 6.3f + yOffset * c_worldChunkLength);
            Vector3 offset = CalculateOffset();

            //Instantié et le tourner dans la bonne direction
            GameObject build = Instantiate(Resources.Load<GameObject>(_pathBuilding), position + offset, Quaternion.identity);
            build.transform.Rotate(0, _facingRotation * 90f, 0);
            return true;
        }
        public bool CanGenerateBuilding()
        {
            if (_isFree && (_north + _south + _east + _west > 0))
            {
                //Debug.Log("CanGenerateBuilding: true: (_isFree: " + _isFree + " ) (_isNextToRoad: " + ((_north + _south + _east + _west > 0)) + " )");
                return true;
            }
            //Debug.Log("CanGenerateBuilding: false: (_isFree: " + _isFree + " ) (_isNextToRoad: " + ((_north + _south + _east + _west > 0)) + " )");
            return false;
        }
        private Vector3 CalculateOffset()
        {
            //Bien le placer
            Vector3 offsetRotation = new Vector3();

            switch (_buildingSizeType)
            {
                case 0:
                    offsetRotation.Set(0, 0, 0);
                    break;
                default:
                    switch (_facingRotation)
                    {
                        case 0: //0f = 0 * 90 -> south
                            offsetRotation.Set(0, 0, 0);
                            break;
                        case 1: //90f = 1 * 90 -> west
                            offsetRotation.Set(0, 0, 0);
                            break;
                        case 2: //180f = 2 * 90 -> north
                            offsetRotation.Set(0, 0, 0);
                            break;
                        default://270f = 3 * 90 -> east
                            offsetRotation.Set(0, 0, 0);
                            break;
                    }
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
            if (_isFree)
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

    public class CityNode
    {
        //But de cette class, produire les datas necessaire à une City

        //Les matrices travaillées
        public RoadNode[,] roadMatrix;
        public BuildNode[,] buildMatrix;
        //Valeur aidant au Setup de roadMatrix
        private int borderNorth;
        private int borderEast;
        private int midCoord;
        //Constructeur
        public CityNode()
        {
            Init();

            SetupRoads();

            SetupBuilds();
        }

        //Initialisation générale
        private void Init()
        {
            roadMatrix = new RoadNode[c_districtInWorldChunk, c_districtInWorldChunk];
            buildMatrix = new BuildNode[c_districtInWorldChunk * 2, c_districtInWorldChunk * 2];

            borderNorth = c_districtInWorldChunk - 1;
            borderEast = c_districtInWorldChunk - 1;
            midCoord = c_districtInWorldChunk / 2;
        }

        //Setup roadMatrix
        private void SetupRoads()
        {
            //remplir masterRoads
            InitRoads();

            //Modifier masterRoads
            ModifyRoads();
        }
        #region SetupRoads
        private void InitRoads()
        {
            for (int y = 0; y < c_districtInWorldChunk; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk; x++)
                {
                    roadMatrix[x, y] = new RoadNode(x, y);
                }
            }
        }
        private void ModifyRoads()
        {
            //Setup des nodes spécifiques
            RoadNode nodeMiddle = GetNodeInRange(midCoord - 1, midCoord + 1, midCoord - 1, midCoord + 1);
            RoadNode nodePoiSW = GetNodeInRange(1, midCoord - 2, 1, midCoord - 2);
            RoadNode nodePoiSE = GetNodeInRange(midCoord + 2, borderEast - 1, 1, midCoord - 2);
            RoadNode nodePoiNW = GetNodeInRange(1, midCoord - 2, midCoord + 2, borderNorth - 1);
            RoadNode nodePoiNE = GetNodeInRange(midCoord + 2, borderEast - 1, midCoord + 2, borderNorth - 1);

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
        //Modify Roads
        //Node Getters
        private RoadNode GetNodeInRange(int xMin, int xMax, int yMin, int yMax)
        {
            int x = Random.Range(xMin, xMax + 1);
            int y = Random.Range(yMin, yMax + 1);

            return roadMatrix[x, y];
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
                path.Add(roadMatrix[currX, currY]);
                //On met à jour la distance
                distX = destX - currX;
                distY = destY - currY;
                //On met à jour la distance absolu
                absDistX = (distX > 0) ? distX : -distX;
                absDistY = (distY > 0) ? distY : -distY;
                //Choisir l'axe de déplacement
                if (absDistX > absDistY)
                {
                    if (distX > 0) { currX++; //On se déplace à l'Est
                    }
                    else { currX--; //On se déplace à l'Ouest
                    }
                }
                else
                {
                    if (distY > 0) { currY++;  //On se déplace au Nord
                    }
                    else { currY--;  //On se déplace au Sud
                    }
                }
                //Fin de la boucle, on a mis à jour les coordonnées suivantes
            }
            //Il manque la destination dans path
            path.Add(roadMatrix[destX, destY]);

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

        //Setup buildMatrix
        private void SetupBuilds()
        {
            InitBuilds();

            ModifyBuilds();
        }
        #region SetupBuilds
        private void InitBuilds()
        {
            for (int y = 0; y < c_districtInWorldChunk; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk; x++)
                {
                    BuildAroundNode(roadMatrix[x, y], x, y);
                }
            }
        }
        private void BuildAroundNode(RoadNode roadOrigin, int xInMatrix, int yInMatrix)
        {
            int xChunk = roadOrigin.x * 5;
            int yChunk = roadOrigin.y * 5;
            //South West 0
            buildMatrix[2 * xInMatrix, 2 * yInMatrix] = new BuildNode(xChunk - 2, yChunk - 2, roadOrigin, 0);
            //South East 1
            buildMatrix[2 * xInMatrix + 1, 2 * yInMatrix] = new BuildNode(xChunk + 1, yChunk - 2, roadOrigin, 1);
            //North East 2
            buildMatrix[2 * xInMatrix + 1, 2 * yInMatrix + 1] = new BuildNode(xChunk + 1, yChunk + 1, roadOrigin, 2);
            //North West 3
            buildMatrix[2 * xInMatrix, 2 * yInMatrix + 1] = new BuildNode(xChunk - 2, yChunk + 1, roadOrigin, 3);
        }
        private void ModifyBuilds()
        {
            for (int y = 0; y < c_districtInWorldChunk * 2; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk * 2; x++)
                {
                    MasterChooseBuildsAt(x, y);
                }
            }
        }
        //Modify Builds
        private void MasterChooseBuildsAt(int x, int y)
        {
            BuildNode build = buildMatrix[x, y];
            if (!build.IsFree) return;
            if (build.IsNextToRoad)
            {
                //size --> 0 = 2x2, 1 = 2x4, 2 = 2x4, 3 = 4*4
                //facing rotation --> 0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east
                //pos to Node -->0 = SW , 1 = SE , 2 = NE, 3 = NW

                int pos = build.PosToNode;

                switch (pos)
                {
                    case 0: //SW par rapport à son origine roadNode
                        CornerBehaviorSW(build, x, y);
                        break;
                    case 1: //SE par rapport à son origine roadNode
                        CornerBehaviorSE(build, x, y);
                        break;
                    case 2: //NE par rapport à son origine roadNode
                        CornerBehaviorNE(build, x, y);
                        break;
                    default: //NW par rapport à son origine roadNode
                        CornerBehaviorNW(build, x, y);
                        break;
                }
            }
            else
            {
                //Pas a coté de la route
            }
        }

        private void CornerBehaviorSW(BuildNode build, int x, int y)
        {
            //Common Behavior Start
            int facingRotation = build.FacingRotation;
            //fmd = l'entre est sur l'axe nord-sud
            bool fmd = (facingRotation == 2);
            string file = "Error: did not modified wtf";
            int result;
            //Specific Behavior
            bool southFree = MasterCheckSouth(build, x, y);
            bool westFree = MasterCheckWest(build, x, y);

            if (!southFree) {
                if (!westFree) {
                    //Sud occupé, Ouest occupé -> carré
                    MasterChoice(out result, out file, true, false, false, false);
                    //Pas de slave à mettre
                }
                else {
                    //Sud occupé, Ouest libre -> carré / vertical
                    MasterChoice(out result, out file, true, !fmd, fmd, false);
                    if (result != 0) buildMatrix[x - 1, y].SetSlave();
                }
            }
            else {
                if (!westFree) {
                    //Sud libre, Ouest occupé -> carré / horizontal
                    MasterChoice(out result, out file, true, fmd, false, !fmd);
                    if (result != 0) buildMatrix[x, y - 1].SetSlave();
                }
                else {
                    //Sud libre, Ouest libre -> carré / horizontal / vertical
                    MasterChoice(out result, out file, true, true, fmd, !fmd);
                    switch (result)
                    {
                        case 1:
                            if (fmd) { buildMatrix[x, y - 1].SetSlave(); }
                            else {  buildMatrix[x - 1, y].SetSlave(); }
                            break;
                        case 2: buildMatrix[x - 1, y].SetSlave(); break;
                        case 3: buildMatrix[x, y - 1].SetSlave(); break;
                        default:
                            break;
                    }
                }
            }
            //Debug.Log("CornerBehaviorSW: " +"("+southFree+", "+westFree+") "+ result + " at (" + x + ", " + y + ")");
            //Common Behavior Ending
            build.generationDecided = true;
            build.SetPath(file);
        }
        private void CornerBehaviorSE(BuildNode build, int x, int y)
        {
            //Common Behavior Start
            int facingRotation = build.FacingRotation;
            //fmd = l'entre est sur l'axe nord-sud
            bool fmd = (facingRotation == 2);
            string file = "Error: did not modified wtf";
            int result;
            //Specific Behavior
            bool southFree = MasterCheckSouth(build, x, y);
            bool eastFree = MasterCheckEast(build, x, y);

            if (!southFree) {
                if (!eastFree) {
                    //Sud occupé, Est occupé -> carré
                    MasterChoice(out result, out file, true, false, false, false);
                    //Pas de slave à mettre
                }
                else {
                    //Sud occupé, Est libre -> carré / horizontal
                    MasterChoice(out result, out file, true, !fmd, false, fmd);
                    if (result != 0) buildMatrix[x + 1, y].SetSlave();
                }
            }
            else {
                if (!eastFree) {
                    //Sud libre, Est occupé -> carré / vertical
                    MasterChoice(out result, out file, true, fmd, !fmd, false);
                    if (result != 0) buildMatrix[x, y - 1].SetSlave();
                }
                else {
                    //Sud libre, Est libre -> carré / horizontal / vertical
                    MasterChoice(out result, out file, true, true, !fmd, fmd);
                    switch (result)
                    {
                        case 1:
                            if (fmd) { buildMatrix[x, y - 1].SetSlave(); }
                            else { buildMatrix[x + 1, y].SetSlave(); }
                            break;
                        case 2: buildMatrix[x, y - 1].SetSlave(); break;
                        case 3: buildMatrix[x + 1, y].SetSlave(); break;
                        default: break;
                    }
                }
            }
            //Debug.Log("CornerBehaviorSE: " + "(" + southFree + ", " + eastFree + ") " + result + " at (" + x + ", " + y + ")");
            //Common Behavior Ending
            build.generationDecided = true;
            build.SetPath(file);
        }
        private void CornerBehaviorNE(BuildNode build, int x, int y)
        {
            //Common Behavior Start
            int facingRotation = build.FacingRotation;
            //fmd = l'entre est sur l'axe nord-sud
            bool fmd = (facingRotation == 0);
            string file = "Error: did not modified wtf";
            int result;
            //Specific Behavior
            bool northFree = MasterCheckNorth(build, x, y);
            bool eastFree = MasterCheckEast(build, x, y);

            if (!northFree) {
                if (!eastFree) {
                    //Nord occupé, Est occupé -> carré
                    MasterChoice(out result, out file, true, false, false, false);
                    //Pas de slave à mettre
                }
                else {
                    //Nord occupé, Est libre -> carré / horizontal
                    MasterChoice(out result, out file, true, !fmd, fmd, false);
                    if (result != 0) buildMatrix[x + 1, y].SetSlave();
                }
            }
            else {
                if (!eastFree) {
                    //Nord libre, Est occupé -> carré / vertical
                    MasterChoice(out result, out file, true, fmd, false, !fmd);
                    if (result != 0) buildMatrix[x, y + 1].SetSlave();
                }
                else {
                    //Nord libre, Est libre -> carré / horizontal / vertical
                    MasterChoice(out result, out file, true, true, fmd, !fmd);
                    switch (result)
                    {
                        case 1:
                            if (fmd) { buildMatrix[x, y + 1].SetSlave(); }
                            else { buildMatrix[x + 1, y].SetSlave(); }
                            break;
                        case 2: buildMatrix[x + 1, y].SetSlave(); break;
                        case 3: buildMatrix[x, y + 1].SetSlave(); break;
                        default: break;
                    }
                }
            }
            //Debug.Log("CornerBehaviorNE: " + "(" + northFree + ", " + eastFree + ") " + result + " at (" + x + ", " + y + ")");
            //Common Behavior Ending
            build.generationDecided = true;
            build.SetPath(file);
        }
        private void CornerBehaviorNW(BuildNode build, int x, int y)
        {
            //Common Behavior Start
            int facingRotation = build.FacingRotation;
            //fmd = l'entre est sur l'axe nord-sud
            bool fmd = (facingRotation == 0);
            string file = "Error: did not modified wtf";
            int result;
            //Specific Behavior
            bool northFree = MasterCheckNorth(build, x, y);
            bool westFree = MasterCheckWest(build, x, y);

            if (!northFree) {
                if (!westFree) {
                    //Nord occupé, Ouest occupé -> carré
                    MasterChoice(out result, out file, true, false, false, false);
                }
                else {
                    //Nord occupé, Ouest libre -> carré / horizontal
                    MasterChoice(out result, out file, true, !fmd, false, fmd);
                    if (result != 0) buildMatrix[x - 1, y].SetSlave();
                }
            }
            else {
                if (!westFree) {
                    //Nord libre, Ouest occupé -> carré / vertical
                    MasterChoice(out result, out file, true, fmd, !fmd, false);
                    if (result != 0) buildMatrix[x, y + 1].SetSlave();
                }
                else {
                    //Nord libre, Ouest libre -> carré / horizontal / vertical
                    MasterChoice(out result, out file, true, true, !fmd, fmd);
                    switch (result)
                    {
                        case 1:
                            if (fmd) { buildMatrix[x, y + 1].SetSlave(); }
                            else { buildMatrix[x - 1, y].SetSlave(); }
                            break;
                        case 2: buildMatrix[x, y + 1].SetSlave(); break;
                        case 3: buildMatrix[x - 1, y].SetSlave(); break;
                        default: break;
                    }
                }
            }
            //Debug.Log("CornerBehaviorNW: " + "(" + northFree + ", " + westFree + ") " + result + " at ("+x+", "+y+")");
            //Common Behavior Ending
            build.generationDecided = true;
            build.SetPath(file);
        }

        private void MasterChoice(out int result, out string file, bool f22, bool f24, bool f42left, bool f42right)
        {
            //But de cette fonction:
            //  Faire un choix entre la taille du bâtiment parmis les tailles possibles
            //  Les 4 booléens en paramètres représentent les choix possibles
            //      f22: génération d'un bâtiment de taille 2*2 (petit carré)
            //      f24: génération d'un bâtiment de taille 2*4 (rectangle avec entrée coté de longueur 2)
            //      f42left: génération d'un bâtiment de taille 4*2 (rectangle avec entrée coté de longueur 4 à gauche quand face au Sud)
            //      f42left: génération d'un bâtiment de taille 4*2 (rectangle avec entrée coté de longueur 4 à droite quand face au Nord)
            //  le string "file" pris en paramètres est renvoyé pour modifier le chemin de build
            //  le string "result" représente la décision prise et est renvoyé pour SetSlave() ou non les voisins concernés

            //On crée une array stockant toutes les décisions possibles
            bool[] stock = new bool[4] { f22, f24, f42left, f42right };

            //On compte le nombre de choix valides
            int trueCount = 0;
            foreach (bool b in stock) if (b) trueCount++;

            //Pris de décision au hasard
            int rng = Random.Range(0, trueCount);

            //Result représente l'index de la décision prise
            result = 0;

            //Result augmente jusqu'à qu'on soit arrivé à la décision voulue
            foreach (bool b in stock)
            {
                if (b)
                {
                    if (rng == 0)
                    {
                        break;
                    }
                    rng--;
                }
                result++;
            }

            //Modification de file en fonction du résultat précédent
            switch (result)
            {
                case 0: file = table22.GetRandomPath(); break;
                case 1: file = table24.GetRandomPath(); break;
                case 2: file = table42left.GetRandomPath(); break;
                default: file = table42right.GetRandomPath(); break;
            }
        }

        private bool MasterCheckNorth(BuildNode build, int x, int y)
        {
            if (y == c_districtInWorldChunk * 2 - 1) return false; //Si on a atteint la bordure Nord
            if (build.North == 1) return false; //Si passage au Nord est occupé

            //Si le batiment au Nord a deja decidé sa génération ou qqun a déja décidé pour lui
            if (!buildMatrix[x, y + 1].IsFree || buildMatrix[x, y + 1].generationDecided) return false;
            return true;
        }
        private bool MasterCheckSouth(BuildNode build, int x, int y)
        {
            if (y == 0) return false; //Si on a atteint la bordure Sud
            if (build.South == 1) return false; //Si passage au Sud est occupé

            //Si le batiment au Sud a deja decidé sa génération ou qqun a déja décidé pour lui
            if (!buildMatrix[x, y - 1].IsFree || buildMatrix[x, y - 1].generationDecided) return false;
            return true;
        }
        private bool MasterCheckEast(BuildNode build, int x, int y)
        {
            if (x == c_districtInWorldChunk * 2 - 1) return false; //Si on a atteint la bordure Est
            if (build.East == 1) return false; //Si le passage à l'Est est occupé

            //Si le batiment à l'Est a deja decidé sa génération ou qqun a déja décidé pour lui
            if (!buildMatrix[x + 1, y].IsFree || buildMatrix[x + 1, y].generationDecided) return false;
            return true;
        }
        private bool MasterCheckWest(BuildNode build, int x, int y)
        {
            if (x == 0) return false; //Si on a atteint la bordure Ouest
            if (build.West == 1) return false; //Si le passage à l'Ouest est occupé
                                               //Si le batiment à l'Ouest a deja decidé sa génération ou qqun a déja décidé pour lui
            if (!buildMatrix[x - 1, y].IsFree || buildMatrix[x - 1, y].generationDecided) return false;
            return true;
        }
        #endregion

        //Data encrypt
        public int[] GetRoadData()
        {
            Debug.Log("Generator: Encrypting roads as " + PhotonNetwork.player.NickName);
            int[] package = new int[c_districtInWorldChunk * c_districtInWorldChunk * (RoadNode.packageSize)];

            int x = 0;
            int y = 0;
            int index = 0;
            while (x < c_districtInWorldChunk)
            {
                y = 0;
                while (y < c_districtInWorldChunk)
                {
                    foreach (int n in roadMatrix[x, y].Serialize())
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
        public int[] GetBuildData()
        {
            Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.NickName);
            int[] package = new int[(2 * c_districtInWorldChunk) * (2 * c_districtInWorldChunk) * BuildNode.c_packageSize];

            int x = 0;
            int y = 0;
            int index = 0;
            while (x < 2 * c_districtInWorldChunk)
            {
                y = 0;
                while (y < 2 * c_districtInWorldChunk)
                {
                    foreach (int n in buildMatrix[x, y].SerializeArray())
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
        public string[] GetBuildPathData()
        {
            Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.NickName);
            string[] package = new string[(2 * c_districtInWorldChunk) * (2 * c_districtInWorldChunk)];

            int x = 0;
            int y = 0;
            int index = 0;
            while (x < 2 * c_districtInWorldChunk)
            {
                y = 0;
                while (y < 2 * c_districtInWorldChunk)
                {
                    package[index] = buildMatrix[x, y].SerializeString();
                    index++;
                    y++;
                }
                x++;
            }
            return package;
        }

        //Generateur static (decrypt)
        public static void GenerateOffset(int xOffset, int yOffset, int[] roadData, int[] buildData, string[] buildPathData)
        {
            GenerateRoads(DecryptRoads(roadData), xOffset, yOffset);

            GenerateBuilds(DecryptBuilds(buildData, buildPathData), xOffset, yOffset);
        }

        private static RoadNode[,] DecryptRoads(int[] roadData)
        {
            Debug.Log("Generator: Decrypting roads as " + PhotonNetwork.player.NickName);

            RoadNode[,] tempRoadMatrix = new RoadNode[c_districtInWorldChunk, c_districtInWorldChunk];

            int x = 0;
            int y = 0;
            int index = 0;

            int[] nodeSerialized = new int[RoadNode.packageSize];
            while (x < c_districtInWorldChunk)
            {
                y = 0;
                while (y < c_districtInWorldChunk)
                {
                    //On recupere la version serialisé de la RoadNode
                    for (int i = 0; i < RoadNode.packageSize; i++)
                    {
                        nodeSerialized[i] = roadData[index + i];
                    }
                    //On ajoute la RoadNode dans la matrice prévu à cette effet
                    tempRoadMatrix[x, y] = RoadNode.Deserialize(nodeSerialized);

                    //Incrementation
                    index += RoadNode.packageSize;
                    y++;
                }
                x++;
            }
            return tempRoadMatrix;
        }
        private static void GenerateRoads(RoadNode[,] tempRoadMatrix, int xOffset, int yOffset)
        {
            for (int y = 0; y < c_districtInWorldChunk; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk; x++)
                {
                    tempRoadMatrix[x, y].Generate(xOffset, yOffset);
                }
            }
        }

        private static BuildNode[,] DecryptBuilds(int[] buildData, string[] buildPathData)
        {
            Debug.Log("Generator: Decrypting builds as " + PhotonNetwork.player.NickName);

            BuildNode[,] tempBuildMatrix = new BuildNode[c_districtInWorldChunk * 2, c_districtInWorldChunk * 2];

            int x = 0;
            int y = 0;
            int indexArray = 0;
            int indexPath = 0;

            int[] buildSerialized = new int[BuildNode.c_packageSize];
            while (x < 2 * c_districtInWorldChunk)
            {
                y = 0;
                while (y < 2 * c_districtInWorldChunk)
                {
                    //On recupere la version serialisé de la BuildNode
                    for (int i = 0; i < BuildNode.c_packageSize; i++)
                    {
                        buildSerialized[i] = buildData[indexArray + i];
                    }
                    //On ajoute la BuildNode dans la matrice prévu à cette effet (+version sérialisé de son path)
                    tempBuildMatrix[x, y] = BuildNode.Deserialize(buildSerialized, buildPathData[indexPath]);

                    //Incrementation
                    indexArray += BuildNode.c_packageSize;
                    indexPath++;
                    y++;
                }
                x++;
            }
            return tempBuildMatrix;
        }
        private static void GenerateBuilds(BuildNode[,] tempBuildMatrix, int xOffset, int yOffset)
        {
            for (int y = 0; y < c_districtInWorldChunk * 2; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk * 2; x++)
                {
                    tempBuildMatrix[x, y].Generate(xOffset, yOffset);
                }
            }
        }
    }

    public class WorldNode
    {
        //Coordonnée dans la matrice globale (initialisé par le constructeur)
        public int x;
        public int y;
        //Type de Node
        public WorldBiome biome;
        public WorldLayout layout;
        //Caractéristique des Roads (-1 si pas de route, sinon de 0 à 15 partant du sud ou de l'ouest)
        public int openNorth = -1;
        public int openSouth = -1;
        public int openEast = -1;
        public int openWest = -1;
        public int[] roadData;
        //Caractéristique des Builds
        public int[] buildData;
        public string[] buildPathData;

        //Constructeur
        public WorldNode(int xWorld,int yWorld)
        {
            x = xWorld;
            y = yWorld;
        }
        //Modification par le master
        public bool TrySetBiome(WorldBiome newBiome)
        {
            if (biome == WorldBiome.Undecided)
            {
                biome = newBiome;
                return true;
            }
            return false;
        }
        public void SetLayout(WorldLayout newLayout)
        {
            layout = newLayout;
        }
        public void DecideMasterdata()
        {
            if (layout == WorldLayout.City)
            {
                //Genere une ville
                CityNode city = new CityNode();
                //Update les datas
                roadData = city.GetRoadData();
                buildData = city.GetBuildData();
                buildPathData = city.GetBuildPathData();
            }
        }
        /// <summary>°0:North,1:South,2:East,3:West; value range from 0 to 15 </summary>
        public void Open(int direction, int value)
        {
            switch (direction)
            {
                case 0:
                    openNorth = value;
                    break;
                case 1:
                    openSouth = value;
                    break;
                case 2:
                    openEast = value;
                    break;
                default:
                    openWest = value;
                    break;
            }
        }
        //Envoi (côté master)


        //Réception (coté client)
        public void UpdateType(WorldBiome masterBiome, WorldLayout masterLayout)
        {
            //Appelé par les clients après que le maître ait envoyé les infos sur le type
            biome = masterBiome;
            layout = masterLayout;
        }
        public void UpdateMasterData(int[] roads, int[] builds, string[] buildPaths)
        {
            roadData = roads;
            buildData = builds;
            buildPathData = buildPaths;
        }

        //Generation
        public void Generate()
        {
            GenerateFloor();
            if (layout == WorldLayout.City)
            {
                CityNode.GenerateOffset(x, y, roadData, buildData, buildPathData);
            }

        }
        private void GenerateFloor()
        {
            for (int yCycle = 0; y < c_districtInWorldChunk; yCycle++)
            {
                for (int xCycle = 0; x < c_districtInWorldChunk; xCycle++)
                {
                    Instantiate(
                        Resources.Load<GameObject>("testEmpty"), 
                        new Vector3(xCycle * c_districtLength + x * c_worldChunkLength, -0.5f, yCycle * c_districtLength + y * c_worldChunkLength), 
                        Quaternion.identity);
                }
            }
        }
    }

    public enum WorldBiome
    {
        Undecided = 0,
        Plain = 1,
        Forest = 2,
    }
    public enum WorldLayout
    {
        Undecided = 0,
        Village = 1,
        City = 2,
    }

    #region Attributes

    public NavMeshSurface surface;
    //Constante de génération
    public const float c_voxChunkLength = 12.6f; //magic number with MagicaVoxel, tunk = technical chunk
    public const int c_voxChunkInDistrict = 5;
    public const float c_districtLength = c_voxChunkLength * c_voxChunkInDistrict;
    public const int c_districtInWorldChunk = 16;
    public const float c_worldChunkLength = c_districtLength * c_districtInWorldChunk;

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
    private int borderNorth;
    private int borderEast;
    
    private int midX;
    private int midY;

    public float nodeMiddleX;
    public float nodeMiddleY;

    #endregion

    //Path and directories

    [SerializeField] private buildTable _table22 = null;
    [SerializeField] private buildTable _table24 = null;
    [SerializeField] private buildTable _table42left = null;
    [SerializeField] private buildTable _table42right = null;

    public static buildTable table22 = null;
    public static buildTable table24 = null;
    public static buildTable table42left = null;
    public static buildTable table42right = null;

    private void InitBuildTable()
    {
        table22 = _table22;
        table24 = _table24;
        table42left = _table42left;
        table42right = _table42right;
    }

    // --- IMPORTANT TIPS AND TRICKS ---
    //Quandd y augmente -> on va vers le Nord
    //Quandd x augmente -> on ve vers l' Est

    //Unity Callabcks
    private void Awake()
    {
        InitBuildTable();
    }

    private void Start()
    {
        roadMatrix = new RoadNode[c_districtInWorldChunk, c_districtInWorldChunk];
        buildMatrix = new BuildNode[c_districtInWorldChunk * 2, c_districtInWorldChunk * 2];

        if (PhotonNetwork.isMasterClient)
        {
            MasterInitialisation();

            MasterSetupRoads();
            MasterSetupBuilds();


            if (PhotonNetwork.offlineMode == true)
            {
                roadMatrix = masterRoads;
                buildMatrix = masterBuilds;

                //Saute l'envoi de RPC
                GenerateEnd();
            }
            else
            {
                UpdateRoads();
                UpdateBuilds();
            }
        }
    }

    private void Update()
    {
        if (!receivedPackage && serializedRoads != null && serializedBuilds != null &&  serializedPath != null)
        {
            receivedPackage = true;

            OnReceivedPackage();
        }
    }

    private void OnReceivedPackage()
    {
        //(serializedRoads a été modifié par RPC de meme pour cityX et cityY)
        DecryptRoads();

        DecryptBuilds();

        //Genere avec les informations recues
        GenerateEnd();
    }

    private void GenerateEnd()
    {
        //Genere le sol
        StartCoroutine(GenerateFloor());

        //Generate les routes
        StartCoroutine(GenerateRoads());

        //Generate les bâtiments
        StartCoroutine(GenerateBuilds());
        //Generate Builds appelle OnGenerationEnden à la fin
        
    }

    private void OnGenerationEnded()
    {
        //Update le NavMeshSurface entierement (fin)
        surface.BuildNavMesh();

        //Update le spawnpoint (temporaire)
        GetComponent<CentralManager>().OnGenerationFinished();
        GetComponent<CentralManager>().spawnPoint = new Vector3((c_worldChunkLength) / 2, 1000f, (c_worldChunkLength) / 2);
    }

    //Master Initialisation

    private void MasterInitialisation()
    {
        masterRoads = new RoadNode[c_districtInWorldChunk, c_districtInWorldChunk];
        masterBuilds = new BuildNode[c_districtInWorldChunk * 2, c_districtInWorldChunk * 2];
        
        borderNorth = c_districtInWorldChunk - 1;
        borderEast = c_districtInWorldChunk - 1;
        midX = c_districtInWorldChunk / 2;
        midY = c_districtInWorldChunk / 2;
    }

    //Master  Roads

    private void MasterSetupRoads()
    {
        //remplir masterRoads
        MasterInitRoads();

        //Modifier masterRoads
        MasterModifyRoads();
    }

    private void MasterInitRoads()
    {
        for (int y = 0; y < c_districtInWorldChunk; y++)
        {
            for (int x = 0; x < c_districtInWorldChunk; x++)
            {
                masterRoads[x, y] = new RoadNode(x, y);
            }
        }
    }

    private void MasterModifyRoads()
    {
        //Setup des nodes spécifiques

        RoadNode nodeMiddle = GetPoiMiddle();
        nodeMiddleX = nodeMiddle.x * c_districtLength;
        nodeMiddleY = nodeMiddle.y * c_districtLength;


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

    #region MasterModifyRoads

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
            c_districtInWorldChunk / 2 - 1,
            c_districtInWorldChunk / 2 + 1);
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

    //Master  Builds
    private void MasterSetupBuilds()
    {
        MasterInitBuilds();

        MasterModifyBuilds();
    }

    private void MasterInitBuilds()
    {
        for (int y = 0; y < c_districtInWorldChunk; y++)
        {
            for (int x = 0; x < c_districtInWorldChunk; x++)
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
        masterBuilds[2 * xInMatrix, 2 * yInMatrix] = new BuildNode(xChunk - 2, yChunk - 2, roadOrigin, 0);

        //South East 1
        masterBuilds[2 * xInMatrix + 1, 2 * yInMatrix] = new BuildNode(xChunk + 1, yChunk - 2, roadOrigin, 1);

        //North East 2
        masterBuilds[2 * xInMatrix + 1, 2 * yInMatrix + 1] = new BuildNode(xChunk + 1, yChunk + 1, roadOrigin, 2);

        //North West 3
        masterBuilds[2 * xInMatrix, 2 * yInMatrix + 1] = new BuildNode(xChunk - 2, yChunk + 1, roadOrigin, 3);
    }

    private void MasterModifyBuilds()
    {
        for (int y = 0; y < c_districtInWorldChunk * 2; y++)
        {
            for (int x = 0; x < c_districtInWorldChunk * 2; x++)
            {
                MasterChooseBuildsAt(x, y);
            }
        }
    }

    #region MasterModifyBuilds
    //NB: ça sert a rien d'essayer de comprendre ce qui se passe, faut juste retenir que masterBuilds est modifié

    private void MasterChooseBuildsAt(int x, int y)
    {
        BuildNode build = masterBuilds[x, y];
        if (!build.IsFree)
        {
            return;
        }
        if (build.IsNextToRoad)
        {
            //size --> 0 = 2x2, 1 = 2x4, 2 = 2x4, 3 = 4*4
            //facing rotation --> 0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east
            //pos to Node -->0 = SW , 1 = SE , 2 = NE, 3 = NW

            int pos = build.PosToNode;

            switch (pos)
            {
                case 0: //SW par rapport à son origine roadNode
                    CornerBehaviorSW(build,x,y);
                    break;
                case 1: //SE par rapport à son origine roadNode
                    CornerBehaviorSE(build, x, y);
                    break;
                case 2: //NE par rapport à son origine roadNode
                    CornerBehaviorNE(build, x, y);
                    break;
                default: //NW par rapport à son origine roadNode
                    CornerBehaviorNW(build, x, y);
                    break;
            }
        }
        else
        {
            //Pas a coté de la route
        }
    }

    private void CornerBehaviorSW(BuildNode build, int x, int y)
    {
        //Common Behavior Start
        int facingRotation = build.FacingRotation;
        //fmd = l'entre est sur l'axe nord-sud
        bool fmd = (facingRotation == 2);
        string file = "Error: did not modified wtf";
        int result;
        //Specific Behavior
        
        bool southFree = MasterCheckSouth(build, x, y);
        bool westFree = MasterCheckWest(build, x, y);

        if (!southFree)
        {
            if (!westFree)
            {
                //Sud occupé, Ouest occupé -> carré
                MasterChoice(out result, out file, true, false, false, false);
                //Pas de slave à mettre
            }
            else
            {
                //Sud occupé, Ouest libre -> carré / vertical
                MasterChoice(out result, out file, true, !fmd, fmd, false);
                if (result != 0)
                {
                    masterBuilds[x - 1, y].SetSlave();
                }
            }
        }
        else
        {
            if (!westFree)
            {
                //Sud libre, Ouest occupé -> carré / horizontal
                MasterChoice(out result, out file, true, fmd, false, !fmd);
                if (result != 0)
                {
                    masterBuilds[x, y - 1].SetSlave();
                }
            }
            else
            {
                //Sud libre, Ouest libre -> carré / horizontal / vertical
                MasterChoice(out result, out file, true, true, fmd, !fmd);
                switch (result)
                {
                    case 1:
                        if (fmd)
                        {
                            masterBuilds[x, y - 1].SetSlave();
                        }
                        else
                        {
                            masterBuilds[x - 1, y].SetSlave();
                        }
                        break;
                    case 2:
                        masterBuilds[x - 1, y].SetSlave();
                        break;
                    case 3:
                        masterBuilds[x, y - 1].SetSlave();
                        break;
                    default:
                        break;
                }
            }
        }

        //Debug.Log("CornerBehaviorSW: " +"("+southFree+", "+westFree+") "+ result + " at (" + x + ", " + y + ")");
        //Common Behavior Ending
        build.generationDecided = true;

        build.SetPath(file);
    }
    private void CornerBehaviorSE(BuildNode build, int x, int y)
    {
        //Common Behavior Start
        int facingRotation = build.FacingRotation;
        //fmd = l'entre est sur l'axe nord-sud
        bool fmd = (facingRotation == 2);
        string file = "Error: did not modified wtf";
        int result;
        //Specific Behavior

        bool southFree = MasterCheckSouth(build, x, y);
        bool eastFree = MasterCheckEast(build, x, y);

        if (!southFree)
        {
            if (!eastFree)
            {
                //Sud occupé, Est occupé -> carré
                MasterChoice(out result, out file, true, false, false, false);
                //Pas de slave à mettre
            }
            else
            {
                //Sud occupé, Est libre -> carré / horizontal
                MasterChoice(out result, out file, true, !fmd, false, fmd);
                if (result != 0)
                {
                    masterBuilds[x + 1, y].SetSlave();
                }
            }
        }
        else
        {
            if (!eastFree)
            {
                //Sud libre, Est occupé -> carré / vertical
                MasterChoice(out result, out file, true, fmd, !fmd, false);
                if (result != 0)
                {
                    masterBuilds[x, y - 1].SetSlave();
                }
            }
            else
            {
                //Sud libre, Est libre -> carré / horizontal / vertical
                MasterChoice(out result, out file,  true, true, !fmd, fmd);
                switch (result)
                {
                    case 1:
                        if (fmd)
                        {
                            masterBuilds[x, y - 1].SetSlave();
                        }
                        else
                        {
                            masterBuilds[x + 1, y].SetSlave();
                        }
                        break;
                    case 2:
                        masterBuilds[x, y - 1].SetSlave();
                        break;
                    case 3:
                        masterBuilds[x + 1, y].SetSlave();
                        break;
                    default:
                        break;
                }
            }
        }

        //Debug.Log("CornerBehaviorSE: " + "(" + southFree + ", " + eastFree + ") " + result + " at (" + x + ", " + y + ")");
        //Common Behavior Ending
        build.generationDecided = true;

        build.SetPath(file);
    }
    private void CornerBehaviorNE(BuildNode build, int x, int y)
    {
        //Common Behavior Start
        int facingRotation = build.FacingRotation;
        //fmd = l'entre est sur l'axe nord-sud
        bool fmd = (facingRotation == 0);
        string file = "Error: did not modified wtf";
        int result;
        //Specific Behavior

        bool northFree = MasterCheckNorth(build, x, y);
        bool eastFree = MasterCheckEast(build, x, y);

        if (!northFree)
        {
            if (!eastFree)
            {
                //Nord occupé, Est occupé -> carré
                MasterChoice(out result, out file, true, false, false, false);
                //Pas de slave à mettre
            }
            else
            {
                //Nord occupé, Est libre -> carré / horizontal
                MasterChoice(out result, out file, true, !fmd, fmd, false);
                if (result != 0)
                {
                    masterBuilds[x + 1, y].SetSlave();
                }
            }
        }
        else
        {
            if (!eastFree)
            {
                //Nord libre, Est occupé -> carré / vertical
                MasterChoice(out result, out file, true, fmd, false, !fmd);
                if (result != 0)
                {
                    masterBuilds[x, y + 1].SetSlave();
                }
            }
            else
            {
                //Nord libre, Est libre -> carré / horizontal / vertical
                MasterChoice(out result, out file, true, true, fmd, !fmd);
                switch (result)
                {
                    case 1:
                        if (fmd)
                        {
                            masterBuilds[x, y + 1].SetSlave();
                        }
                        else
                        {
                            masterBuilds[x + 1, y].SetSlave();
                        }
                        break;
                    case 2:
                        masterBuilds[x + 1, y].SetSlave();
                        break;
                    case 3:
                        masterBuilds[x, y + 1].SetSlave();
                        break;
                    default:
                        break;
                }
            }
        }
        //Debug.Log("CornerBehaviorNE: " + "(" + northFree + ", " + eastFree + ") " + result + " at (" + x + ", " + y + ")");

        //Common Behavior Ending
        build.generationDecided = true;

        build.SetPath(file);
    }
    private void CornerBehaviorNW(BuildNode build, int x, int y)
    {
        //Common Behavior Start
        int facingRotation = build.FacingRotation;
        //fmd = l'entre est sur l'axe nord-sud
        bool fmd = (facingRotation == 0);
        string file = "Error: did not modified wtf";
        int result;
        //Specific Behavior

        bool northFree = MasterCheckNorth(build, x, y);
        bool westFree = MasterCheckWest(build, x, y);

        if (!northFree)
        {
            if (!westFree)
            {
                //Nord occupé, Ouest occupé -> carré
                MasterChoice(out result, out file, true, false, false, false);
            }
            else
            {
                //Nord occupé, Ouest libre -> carré / horizontal
                MasterChoice(out result, out file,  true, !fmd, false, fmd);
                if (result != 0)
                {
                    masterBuilds[x - 1, y].SetSlave();
                }
            }
        }
        else
        {
            if (!westFree)
            {
                //Nord libre, Ouest occupé -> carré / vertical
                MasterChoice(out result, out file, true, fmd, !fmd, false);
                if (result != 0)
                {
                    masterBuilds[x, y + 1].SetSlave();
                }
            }
            else
            {
                //Nord libre, Ouest libre -> carré / horizontal / vertical
                MasterChoice(out result, out file, true, true, !fmd, fmd);
                switch (result)
                {
                    case 1:
                        if (fmd)
                        {
                            masterBuilds[x, y + 1].SetSlave();
                        }
                        else
                        {
                            masterBuilds[x - 1, y].SetSlave();
                        }
                        break;
                    case 2:
                        masterBuilds[x, y + 1].SetSlave();
                        break;
                    case 3:
                        masterBuilds[x - 1, y].SetSlave();
                        break;
                    default:
                        break;
                }
            }
        }

        //Debug.Log("CornerBehaviorNW: " + "(" + northFree + ", " + westFree + ") " + result + " at ("+x+", "+y+")");

        //Common Behavior Ending
        build.generationDecided = true;

        build.SetPath(file);
    }
    
    private void MasterChoice(out int result, out string file, bool f22,bool f24,bool f42left, bool f42right)
    {
        //But de cette fonction:
        //  Faire un choix entre la taille du bâtiment parmis les tailles possibles
        //  Les 4 booléens en paramètres représentent les choix possibles
        //      f22: génération d'un bâtiment de taille 2*2 (petit carré)
        //      f24: génération d'un bâtiment de taille 2*4 (rectangle avec entrée coté de longueur 2)
        //      f42left: génération d'un bâtiment de taille 4*2 (rectangle avec entrée coté de longueur 4 à gauche quand face au Sud)
        //      f42left: génération d'un bâtiment de taille 4*2 (rectangle avec entrée coté de longueur 4 à droite quand face au Nord)
        //  le string "file" pris en paramètres est renvoyé pour modifier le chemin de build
        //  le string "result" représente la décision prise et est renvoyé pour SetSlave() ou non les voisins concernés

        //On crée une array stockant toutes les décisions possibles
        bool[] stock = new bool[4] { f22, f24, f42left, f42right };

        //On compte le nombre de choix valides
        int trueCount = 0;
        foreach(bool b in stock) if (b) trueCount++;

        //Pris de décision au hasard
        int rng = Random.Range(0, trueCount);

        //Result représente l'index de la décision prise
        result = 0;

        //Result augmente jusqu'à qu'on soit arrivé à la décision voulue
        foreach (bool b in stock)
        {
            if (b)
            {
                if (rng == 0)
                {
                    break;
                }
                rng--;
            }
            result++;
        }

        //Modification de file en fonction du résultat précédent
        switch (result)
        {
            case 0: file = table22.GetRandomPath(); break;
            case 1: file = table24.GetRandomPath(); break;
            case 2: file = table42left.GetRandomPath(); break;
            default: file = table42right.GetRandomPath(); break;
        }
    }

    private bool MasterCheckNorth(BuildNode build, int x, int y)
    {
        if (y == c_districtInWorldChunk * 2 - 1) return false; //Si on a atteint la bordure Nord
        if (build.North == 1) return false; //Si passage au Nord est occupé

        //Si le batiment au Nord a deja decidé sa génération ou qqun a déja décidé pour lui
        if (!masterBuilds[x, y + 1].IsFree || masterBuilds[x, y + 1].generationDecided) return false; 
        return true;
    }
    private bool MasterCheckSouth(BuildNode build, int x, int y)
    {
        if (y == 0) return false; //Si on a atteint la bordure Sud
        if (build.South == 1) return false; //Si passage au Sud est occupé

        //Si le batiment au Sud a deja decidé sa génération ou qqun a déja décidé pour lui
        if (!masterBuilds[x, y - 1].IsFree || masterBuilds[x, y - 1].generationDecided) return false;
        return true;
    }
    private bool MasterCheckEast(BuildNode build, int x, int y)
    {
        if (x == c_districtInWorldChunk * 2 - 1) return false; //Si on a atteint la bordure Est
        if (build.East == 1) return false; //Si le passage à l'Est est occupé

        //Si le batiment à l'Est a deja decidé sa génération ou qqun a déja décidé pour lui
        if (!masterBuilds[x + 1, y].IsFree || masterBuilds[x + 1, y].generationDecided) return false; 
        return true;
    }
    private bool MasterCheckWest(BuildNode build, int x, int y)
    {
        if (x == 0) return false; //Si on a atteint la bordure Ouest
        if (build.West == 1) return false; //Si le passage à l'Ouest est occupé
        //Si le batiment à l'Ouest a deja decidé sa génération ou qqun a déja décidé pour lui
        if (!masterBuilds[x - 1, y].IsFree || masterBuilds[x - 1, y].generationDecided) return false; 
        return true;
    }
    #endregion

    //Generate
    private IEnumerator GenerateFloor()
    {
        for (int y = 0; y < c_districtInWorldChunk; y++)
        {
            for (int x = 0; x < c_districtInWorldChunk; x++)
            {
                Instantiate(Resources.Load<GameObject>("testEmpty"), new Vector3(x * c_districtLength, -0.5f, y * c_districtLength), Quaternion.identity);
                yield return null;
            }
        }
        //StartCoroutine(GenerateRoads());
    }

    private IEnumerator  GenerateRoads()
    {
        for (int y = 0; y < c_districtInWorldChunk; y++)
        {
            for (int x = 0; x < c_districtInWorldChunk; x++)
            {
                roadMatrix[x, y].Generate(0, 0);
                yield return null;
            }
        }
        //StartCoroutine(GenerateBuilds());
    }

    private IEnumerator GenerateBuilds()
    {
        for (int y = 0; y < c_districtInWorldChunk * 2; y++)
        {
            for (int x = 0; x < c_districtInWorldChunk * 2; x++)
            {
                if(buildMatrix[x, y].Generate(0,0))
                {
                    yield return null;
                }
            }
        }
        OnGenerationEnded();
    }

    
    
    #region  Send Generation via RPC

    //Send roads via RPC
    private void UpdateRoads()
    {
        int[] package = EncryptRoads();

        GetComponent<PhotonView>().RPC("SendRoads",PhotonTargets.AllBuffered,package);
    }
    private int[] EncryptRoads()
    {
        Debug.Log("Generator: Encrypting roads as " + PhotonNetwork.player.NickName);
        int[] package = new int[c_districtInWorldChunk * c_districtInWorldChunk * (RoadNode.packageSize)];

        int x = 0;
        int y = 0;
        int index = 0;
        while (x < c_districtInWorldChunk)
        {
            y = 0;
            while (y < c_districtInWorldChunk)
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
        Debug.Log("Generator: Decrypting roads as " + PhotonNetwork.player.NickName);

        int x = 0;
        int y = 0;
        int index = 0;

        int[] nodeSerialized = new int[RoadNode.packageSize];
        while (x < c_districtInWorldChunk)
        {
            y = 0;
            while (y < c_districtInWorldChunk)
            {
                //On recupere la version serialisé de la RoadNode
                for (int i = 0; i < RoadNode.packageSize; i++)
                {
                    nodeSerialized[i] = serializedRoads[index + i];
                }
                //On ajoute la RoadNode dans la matrice prévu à cette effet
                roadMatrix[x, y] = RoadNode.Deserialize(nodeSerialized);

                //Incrementation
                index += RoadNode.packageSize;
                y++;
            }
            x++;
        }
    }
    [PunRPC] private void SendRoads(int[] package)
    {
        Debug.Log("Generator: Received road package as " + PhotonNetwork.player.NickName);
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
        Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.NickName);
        int[] package = new int[(2 * c_districtInWorldChunk) * (2 * c_districtInWorldChunk) * BuildNode.c_packageSize];

        int x = 0;
        int y = 0;
        int index = 0;
        while (x < 2 * c_districtInWorldChunk)
        {
            y = 0;
            while (y < 2 * c_districtInWorldChunk)
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
        Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.NickName);
        string[] package = new string[(2 * c_districtInWorldChunk) * (2 * c_districtInWorldChunk)];

        int x = 0;
        int y = 0;
        int index = 0;
        while (x < 2 * c_districtInWorldChunk)
        {
            y = 0;
            while (y < 2 * c_districtInWorldChunk)
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
        Debug.Log("Generator: Decrypting builds as " + PhotonNetwork.player.NickName);
        int x = 0;
        int y = 0;
        int indexArray = 0;
        int indexPath = 0;

        int[] buildSerialized = new int[BuildNode.c_packageSize];
        while (x < 2 * c_districtInWorldChunk)
        {
            y = 0;
            while (y < 2 * c_districtInWorldChunk)
            {
                //On recupere la version serialisé de la BuildNode
                for (int i = 0; i < BuildNode.c_packageSize; i++)
                {
                    buildSerialized[i] = serializedBuilds[indexArray + i];
                }
                //On ajoute la BuildNode dans la matrice prévu à cette effet (+version sérialisé de son path)
                buildMatrix[x, y] = BuildNode.Deserialize(buildSerialized, serializedPath[indexPath]);

                //Incrementation
                indexArray += BuildNode.c_packageSize;
                indexPath++;
                y++;
            }
            x++;
        }
    }
    [PunRPC] private void SendBuildsArray(int[] package)
    {
        Debug.Log("Generator: Received build package as " + PhotonNetwork.player.NickName);
        serializedBuilds = package;
    }
    [PunRPC] private void SendBuildsPath(string[] package)
    {
        Debug.Log("Generator: Received buildpath package as " + PhotonNetwork.player.NickName);
        serializedPath = package;
    }

    #endregion
    

}
