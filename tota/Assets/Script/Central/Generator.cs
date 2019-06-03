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
        public int North => _north;
        public int South => _south;
        public int East => _east;
        public int West => _west;
        public bool IsRoad()
        {
            return (_north != 0 || _south != 0 || _east != 0 || _west != 0);
        }
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

        public bool IsABuilding()
        {
            return !_isFree || (_pathBuilding != "");
        }
        public bool IsOccupied()
        {
            //Utilisé par les Checks
            return !_isFree || generationDecided;
        }

        //Origin Node
        private int _posToNode;//0 = SW , 1 = SE , 2 = NE, 3 = NW
        public int PosToNode { get => _posToNode; }
        private int _facingRotation = 0; //0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east
        public int FacingRotation { get => _facingRotation; }

        //Building to build
        private string _pathBuilding = ""; //Chemin du batiment (à partir de Resources) //Aussi utilisé dans IsABuilding
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
                    InitSetSpace(origin.West, 0, origin.South, 0);
                    break;
                case 1: //1 = SE
                    InitSetSpace(origin.East, 0, 0, origin.South);
                    break;
                case 2: //2 = NE
                    InitSetSpace(0, origin.East, 0, origin.North);
                    break;
                default: //3 = NW
                    InitSetSpace(0, origin.West, origin.North, 0);
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
            if (!CanGenerateBuilding()) return false;

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
            return _isFree && (_north + _south + _east + _west > 0) && _pathBuilding != "";
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
        private WorldLayout _layout;
        //Les matrices travaillées
        public RoadNode[,] roadMatrix;
        public BuildNode[,] buildMatrix;
        //Valeur aidant au Setup de roadMatrix
        private int borderNorth;
        private int borderEast;
        private int midCoord;
        //Ouverture
        private int _openNorth = -1;
        private int _openSouth = -1;
        private int _openEast = -1;
        private int _openWest = -1;
        //Constructeur
        public CityNode(WorldLayout layout, int north, int south, int east, int west)
        {
            //Debug.Log("CityNode: creating new City with " + (int)layout + " as biome");
            _layout = layout;

            _openNorth = north;
            _openSouth = south;
            _openEast = east;
            _openWest = west;

            Init();

            SetupRoads();
            if (_layout != WorldLayout.Empty)
            {
                SetupBuilds();
            }
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
            switch (_layout)
            {
                case WorldLayout.Empty: ModifyRoadsEmpty(); break;
                case WorldLayout.Village: ModifyRoadsVillage(); break;
                case WorldLayout.City: ModifyRoadsCity(); break;
                default: break;
            }
        }
        private void ModifyRoadsEmpty()
        {
            RoadNode north = null;
            RoadNode south = null;
            RoadNode east = null;
            RoadNode west = null;

            RoadNode nodeMiddle = GetNodeInRange(midCoord - 1, midCoord + 1, midCoord - 1, midCoord + 1);

            InitCard(nodeMiddle, ref north, ref south, ref east, ref west);
        }
        private void ModifyRoadsVillage()
        {
            RoadNode north = null;
            RoadNode south = null;
            RoadNode east = null;
            RoadNode west = null;

            RoadNode nodeMiddle = GetNodeInRange(midCoord - 1, midCoord + 1, midCoord - 1, midCoord + 1);

            InitCard(nodeMiddle, ref north, ref south, ref east, ref west);

            //Setup des nodes spécifiques
            RoadNode nodePoiSW;
            RoadNode nodePoiSE;
            RoadNode nodePoiNW;
            RoadNode nodePoiNE;
            InitPoi(1, out nodePoiSW, out nodePoiSE, out nodePoiNW, out nodePoiNE);

            //Connectez des nodes avec routes
            ConnectAllBetween(nodePoiSW, nodePoiSE, nodePoiNE, nodePoiNW);
            ConnectCardinalTo(nodeMiddle, nodePoiNE, nodePoiSE, nodePoiNW, nodePoiSW);
        }
        private void ModifyRoadsCity()
        {
            RoadNode north = null;
            RoadNode south = null;
            RoadNode east = null;
            RoadNode west = null;
            RoadNode nodeMiddle = GetNodeInRange(midCoord - 1, midCoord + 1, midCoord - 1, midCoord + 1);
            InitCard(nodeMiddle, ref north, ref south, ref east, ref west);

            //Setup des nodes spécifiques

            RoadNode nodePoiSW;
            RoadNode nodePoiSE;
            RoadNode nodePoiNW;
            RoadNode nodePoiNE;
            InitPoi(4, out nodePoiSW, out nodePoiSE, out nodePoiNW, out nodePoiNE);

            //Connectez des nodes avec routes
            ConnectCardinalTo(nodeMiddle, north, south, east, west);
            ConnectCardinalTo(nodeMiddle, nodePoiNE, nodePoiSE, nodePoiNW, nodePoiSW);
            ConnectAllBetween(nodePoiSW, nodePoiSE, nodePoiNE, nodePoiNW);
            ConnectAllBetween(north, east, south, west);
            //Random extend some roads of 1 or 2 to a direction they are not open
            ExtendRoads();
            ExtendRoads();
        }
        //Modify Roads Common
        private void InitCard(RoadNode nodeMiddle, ref RoadNode north, ref RoadNode south, ref RoadNode east, ref RoadNode west)
        {
            if (_openNorth != -1)
            {
                north = roadMatrix[_openNorth, borderNorth];
                north.OpenNorth();
            }
            if (_openSouth != -1)
            {
                south = roadMatrix[_openSouth, 0];
                south.OpenSouth();
            }
            if (_openEast != -1)
            {
                east = roadMatrix[borderEast, _openEast];
                east.OpenEast();
            }
            if (_openWest != -1)
            {
                west = roadMatrix[0, _openWest];
                west.OpenWest();
            }
            ConnectCardinalTo(nodeMiddle, north, south, east, west);
        }
        private void InitPoi(int ov, out RoadNode nodePoiSW, out RoadNode nodePoiSE, out RoadNode nodePoiNW, out RoadNode nodePoiNE)
        {
            // ov (overture) entre 1 et 5 (4 conseillée)

            nodePoiSW = GetNodeInRange(midCoord - 2 - ov, midCoord - 2, midCoord - 2 - ov, midCoord - 2);
            nodePoiSE = GetNodeInRange(midCoord + 2, midCoord + 2 + ov, midCoord - 2 - ov, midCoord - 2);
            nodePoiNW = GetNodeInRange(midCoord - 2 - ov, midCoord - 2, midCoord + 2, midCoord + 2 + ov);
            nodePoiNE = GetNodeInRange(midCoord + 2, midCoord + 2 + ov, midCoord + 2, midCoord + 2 + ov);
        }
        //Node Getters
        private RoadNode GetNodeInRange(int xMin, int xMax, int yMin, int yMax)
        {
            int x = Random.Range(xMin, xMax + 1);
            int y = Random.Range(yMin, yMax + 1);

            return roadMatrix[x, y];
        }
        //Extender
        private void ExtendRoads()
        {
            List<RoadNode> roadToExtend = new List<RoadNode>();
            //Choose roads to extends
            RoadNode roadTemp;
            for (int y = 1; y < c_districtInWorldChunk - 1; y++)
            {
                for (int x = 1; x < c_districtInWorldChunk - 1; x++)
                {
                    roadTemp = roadMatrix[x, y];
                    if (roadTemp.IsRoad() && Random.Range(0, c_randExtendRoad) == 0)
                    {
                        roadToExtend.Add(roadTemp);
                    }
                }
            }
            //Extend chosen roads
            foreach (RoadNode road in roadToExtend) Extend(road);
        }
        private void Extend(RoadNode road)
        {
            List<RoadNode> validRoadToExtend = new List<RoadNode>();
            int x = road.x;
            int y = road.y;

            int xExt;
            int yExt;

            //Regarde les nodes valides dans chaque direction
            if (road.North == 0)
            {
                yExt = y + 1;
                while (yExt < c_districtInWorldChunk && yExt <= y + 3)
                {
                    validRoadToExtend.Add(roadMatrix[x, yExt]);
                    yExt++;
                }
            }
            if (road.South == 0)
            {
                yExt = y - 1;
                while (yExt >= 0 && yExt >= y - 3)
                {
                    validRoadToExtend.Add(roadMatrix[x, yExt]);
                    yExt--;
                }
            }
            if (road.East == 0)
            {
                xExt = x + 1;
                while (xExt < c_districtInWorldChunk && xExt <= x + 3)
                {
                    validRoadToExtend.Add(roadMatrix[xExt, y]);
                    xExt++;
                }
            }
            if (road.West == 0)
            {
                xExt = x - 1;
                while (xExt >= 0 && xExt >= x - 3)
                {
                    validRoadToExtend.Add(roadMatrix[xExt, y]);
                    xExt--;
                }
            }
            //Choisis une des nodes valides et connect road avec celle là
            if (validRoadToExtend.Count > 0)
            {
                CreateRoad(road, validRoadToExtend[Random.Range(0, validRoadToExtend.Count)]);
            }

        }
        //Connectors
        private void ConnectCardinalTo(RoadNode node, RoadNode north, RoadNode south, RoadNode east, RoadNode west)
        {
            CreateRoad(north, node);
            CreateRoad(south, node);
            CreateRoad(east, node);
            CreateRoad(west, node);
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
                if (distY == 0 || (Random.Range(0,2) == 0 && distX != 0))//(absDistX > absDistY)
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
            if (node1 == null || node2 == null) return;
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
            int ouverture; //de 0 à 16 * 2 (32)

            switch (_layout)
            {
                case WorldLayout.City:
                    ouverture = 2;
                    break;
                case WorldLayout.Village:
                    ouverture = 8;
                    break;
                default:
                    ouverture = 0;
                    break;
            }

            for (int y = 0; y < c_districtInWorldChunk * 2; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk * 2; x++)
                {
                    //MasterChooseBuildsAt(x, y);
                    MasterDecideIfChoose(x, y, ouverture);
                }
            }
        }
        //Modify Builds
        private void MasterDecideIfChoose(int x, int y, int ouverture)
        {
            if (x < ouverture 
                || x > c_districtInWorldChunk * 2 - ouverture 
                || y < ouverture 
                || y > c_districtInWorldChunk * 2 - ouverture)
            {
                BuildNode build = buildMatrix[x, y];
                build.SetPath("");
            }
            else
            {
                MasterChooseBuildsAt(x, y);
            }
        }
        private void MasterChooseBuildsAt(int x, int y)
        {
            BuildNode build = buildMatrix[x, y];
            if (!build.IsFree || !build.IsNextToRoad) return;
            //size --> 0 = 2x2, 1 = 2x4, 2 = 2x4, 3 = 4*4
            //facing rotation --> 0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east
            //pos to Node -->0 = SW , 1 = SE , 2 = NE, 3 = NW
            int pos = build.PosToNode;
            string file;
            bool openOnAxisNS = build.FacingRotation == 2 || build.FacingRotation == 0;
            switch (pos)
            {
                case 0: //SW par rapport à son origine roadNode
                    file = CornerBehaviorSW(x, y, openOnAxisNS, MasterCheckSouth(x, y), MasterCheckWest(x, y)); break;
                case 1: //SE par rapport à son origine roadNode
                    file = CornerBehaviorSE(x, y, openOnAxisNS, MasterCheckSouth(x, y), MasterCheckEast(x, y)); break;
                case 2: //NE par rapport à son origine roadNode
                    file = CornerBehaviorNE(x, y, openOnAxisNS, MasterCheckNorth(x, y), MasterCheckEast(x, y)); break;
                default: //NW par rapport à son origine roadNode
                    file = CornerBehaviorNW(x, y, openOnAxisNS, MasterCheckNorth(x, y), MasterCheckWest(x, y)); break;
            }
            build.generationDecided = true;
            build.SetPath(file);
        }

        private string CornerBehaviorSW(int x, int y, bool fmd, bool southFree, bool westFree)
        {
            string file;
            int result;
            //Specific Behavior
            if (!southFree) {
                if (!westFree) {
                    //Sud occupé, Ouest occupé -> carré
                    result = MasterChoice(out file, false, false, false);
                }
                else {
                    //Sud occupé, Ouest libre -> carré / vertical
                    result = MasterChoice(out file, !fmd, fmd, false);
                }
            }
            else {
                if (!westFree) {
                    //Sud libre, Ouest occupé -> carré / horizontal
                    result = MasterChoice(out file, fmd, false, !fmd);
                }
                else {
                    if (buildMatrix[x - 1, y - 1].IsOccupied() || Random.Range(0, c_randBigSquare) > 0) {
                        //Sud libre, Ouest libre -> carré / horizontal / vertical
                        result = MasterChoice(out file, true, fmd, !fmd);
                    }
                    else {
                        result = MasterChoice(out file, true, fmd, !fmd, fmd, !fmd);
                    }
                }
            }
            switch (result)
            {
                case 1:
                    if (fmd) { buildMatrix[x, y - 1].SetSlave(); }
                    else { buildMatrix[x - 1, y].SetSlave(); }
                    break;
                case 2: buildMatrix[x - 1, y].SetSlave(); break;
                case 3: buildMatrix[x, y - 1].SetSlave(); break;
                case 4:
                case 5:
                    buildMatrix[x - 1, y - 1].SetSlave();
                    buildMatrix[x - 1, y].SetSlave();
                    buildMatrix[x, y - 1].SetSlave(); break;
                default:
                    break;
            }
            //Debug.Log("CornerBehaviorSW: " +"("+southFree+", "+westFree+") "+ result + " at (" + x + ", " + y + ")");
            return file;
        }
        private string CornerBehaviorSE(int x, int y, bool fmd, bool southFree, bool eastFree)
        {
            string file;
            int result;
            //Specific Behavior
            if (!southFree) {
                if (!eastFree) {
                    //Sud occupé, Est occupé -> carré
                    result = MasterChoice(out file, false, false, false);
                }
                else {
                    //Sud occupé, Est libre -> carré / horizontal
                    result = MasterChoice(out file, !fmd, false, fmd);
                }
            }
            else {
                if (!eastFree) {
                    //Sud libre, Est occupé -> carré / vertical
                    result = MasterChoice(out file, fmd, !fmd, false);
                }
                else {
                    if (buildMatrix[x + 1, y - 1].IsOccupied() || Random.Range(0, c_randBigSquare) > 0) {
                        //Sud libre, Est libre -> carré / horizontal / vertical
                        result = MasterChoice(out file, true, !fmd, fmd);
                    }
                    else {
                        result = MasterChoice(out file, true, !fmd, fmd, !fmd, fmd);
                    }
                }
            }
            switch (result)
            {
                case 1:
                    if (fmd) { buildMatrix[x, y - 1].SetSlave(); }
                    else { buildMatrix[x + 1, y].SetSlave(); }
                    break;
                case 2: buildMatrix[x, y - 1].SetSlave(); break;
                case 3: buildMatrix[x + 1, y].SetSlave(); break;
                case 4:
                case 5:
                    buildMatrix[x + 1, y - 1].SetSlave();
                    buildMatrix[x, y - 1].SetSlave();
                    buildMatrix[x + 1, y].SetSlave(); break;
                default: break;
            }
            //Debug.Log("CornerBehaviorSE: " + "(" + southFree + ", " + eastFree + ") " + result + " at (" + x + ", " + y + ")");
            return file;
        }
        private string CornerBehaviorNE(int x, int y, bool fmd, bool northFree, bool eastFree)
        {
            string file;
            int result;
            //Specific Behavior
            if (!northFree) {
                if (!eastFree) {
                    //Nord occupé, Est occupé -> carré
                    result = MasterChoice(out file, false, false, false);
                }
                else {
                    //Nord occupé, Est libre -> carré / horizontal
                    result = MasterChoice(out file, !fmd, fmd, false);
                }
            }
            else {
                if (!eastFree) {
                    //Nord libre, Est occupé -> carré / vertical
                    result = MasterChoice(out file, fmd, false, !fmd);
                }
                else {
                    if (buildMatrix[x + 1, y + 1].IsOccupied() || Random.Range(0, c_randBigSquare) > 0) {
                        //Nord libre, Est libre -> carré / horizontal / vertical
                        result = MasterChoice(out file, true, fmd, !fmd);
                    }
                    else {
                        result = MasterChoice(out file, true, fmd, !fmd, fmd, !fmd);
                    }
                }
            }
            switch (result)
            {
                case 1:
                    if (fmd) { buildMatrix[x, y + 1].SetSlave(); }
                    else { buildMatrix[x + 1, y].SetSlave(); }
                    break;
                case 2: buildMatrix[x + 1, y].SetSlave(); break;
                case 3: buildMatrix[x, y + 1].SetSlave(); break;
                case 4:
                case 5:
                    buildMatrix[x + 1, y + 1].SetSlave();
                    buildMatrix[x + 1, y].SetSlave();
                    buildMatrix[x, y + 1].SetSlave(); break;
                default: break;
            }
            //Debug.Log("CornerBehaviorNE: " + "(" + northFree + ", " + eastFree + ") " + result + " at (" + x + ", " + y + ")");
            return file;
        }
        private string CornerBehaviorNW(int x, int y, bool fmd, bool northFree, bool westFree)
        {
            string file;
            int result;
            //Specific Behavior
            if (!northFree) {
                if (!westFree) {
                    //Nord occupé, Ouest occupé -> carré
                    result = MasterChoice(out file, false, false, false);
                }
                else {
                    //Nord occupé, Ouest libre -> carré / horizontal
                    result = MasterChoice(out file, !fmd, false, fmd);
                }
            }
            else {
                if (!westFree) {
                    //Nord libre, Ouest occupé -> carré / vertical
                    result = MasterChoice(out file, fmd, !fmd, false);
                }
                else {
                    if (buildMatrix[x - 1, y + 1].IsOccupied() || Random.Range(0, c_randBigSquare) > 0) {
                        //Nord libre, Ouest libre -> carré / horizontal / vertical
                        result = MasterChoice(out file, true, !fmd, fmd);
                    }
                    else {
                        result = MasterChoice(out file, true, !fmd, fmd, !fmd, fmd);
                    }
                }
            }
            switch (result)
            {
                case 1:
                    if (fmd) { buildMatrix[x, y + 1].SetSlave(); }
                    else { buildMatrix[x - 1, y].SetSlave(); }
                    break;
                case 2: buildMatrix[x, y + 1].SetSlave(); break;
                case 3: buildMatrix[x - 1, y].SetSlave(); break;
                case 4:
                case 5:
                    buildMatrix[x - 1, y + 1].SetSlave();
                    buildMatrix[x, y + 1].SetSlave();
                    buildMatrix[x - 1, y].SetSlave(); break;
                default: break;
            }
            //Debug.Log("CornerBehaviorNW: " + "(" + northFree + ", " + westFree + ") " + result + " at ("+x+", "+y+")");
            return file;
        }

        private int MasterChoice(out string file, bool f24, bool f42left, bool f42right, bool f44left = false, bool f44right = false)
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
            bool f22 = true; //On peut toujours faire un petit carré
            bool[] stock = new bool[6] { f22, f24, f42left, f42right, f44left, f44right }; //On crée une array stockant toutes les décisions possibles
            int trueCount = 0; //trueCount: nombre de choix valides
            foreach (bool b in stock) if (b) trueCount++; //On compte le nombre de choix valides
            int rng = Random.Range(0, trueCount); //Pris de décision au hasard
            int result = 0; //Result représente l'index de la décision prise
            //Result augmente jusqu'à qu'on soit arrivé à la décision voulue
            foreach (bool b in stock)
            {
                if (b)
                {
                    if (rng == 0) break;
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
                case 3: file = table42right.GetRandomPath(); break;
                case 4: file = table44left.GetRandomPath(); break;
                case 5: file = table44right.GetRandomPath(); break;
                default: file = "MasterChoice: error occured when parsing result"; break;
            }
            return result;
        }

        private bool MasterCheckNorth(int x, int y)
        {
            if (y == c_districtInWorldChunk * 2 - 1) return false; //Si on a atteint la bordure Nord
            if (buildMatrix[x, y].North == 1) return false; //Si passage au Nord est occupé
            return !buildMatrix[x, y + 1].IsOccupied();
        }
        private bool MasterCheckSouth(int x, int y)
        {
            if (y == 0) return false; //Si on a atteint la bordure Sud
            if (buildMatrix[x, y].South == 1) return false; //Si passage au Sud est occupé
            return !buildMatrix[x, y - 1].IsOccupied();
        }
        private bool MasterCheckEast(int x, int y)
        {
            if (x == c_districtInWorldChunk * 2 - 1) return false; //Si on a atteint la bordure Est
            if (buildMatrix[x, y].East == 1) return false; //Si le passage à l'Est est occupé
            return !buildMatrix[x + 1, y].IsOccupied();
        }
        private bool MasterCheckWest(int x, int y)
        {
            if (x == 0) return false; //Si on a atteint la bordure Ouest
            if (buildMatrix[x, y].West == 1) return false; //Si le passage à l'Ouest est occupé
            return !buildMatrix[x - 1, y].IsOccupied();
        }
        #endregion

        //Data encrypt
        public int[] GetRoadData()
        {
            //Debug.Log("Generator: Encrypting roads as " + PhotonNetwork.player.NickName);
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
            if (_layout == WorldLayout.Empty)
            {
                return null;
            }

            //Debug.Log("Generator: Encrypting builds as " + PhotonNetwork.player.NickName);
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
            if (_layout == WorldLayout.Empty)
            {
                return null;
            }

            //Debug.Log("Generator: Encrypting buildsPaths as " + PhotonNetwork.player.NickName);
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


    }

    public class WorldNode
    {
        //Coordonnée dans la matrice globale (initialisé par le constructeur)
        public int x;
        public int y;
        //GameobjectParent
        private GameObject worldNode;
        //Type de Node
        private WorldBiome _biome;
        public WorldBiome Biome { get => _biome; }
        private WorldLayout _layout;
        public WorldLayout Layout { get => _layout; }
        //Caractéristique des Roads (-1 si pas de route, sinon de 0 à 15 partant du sud ou de l'ouest)
        public int openNorth = -1;
        public int openSouth = -1;
        public int openEast = -1;
        public int openWest = -1;
        public int[] roadData;
        public void OpenTo(WorldNode node, int overture)
        {
            if (x > node.x) //on est à l'est de node
            {
                openWest = overture;
                node.openEast = overture;
            }
            else if (x < node.x) //on est à l'ouest de node
            {
                openEast = overture;
                node.openWest = overture;
            }
            else if (y > node.y) //on est au nord de node
            {
                openSouth = overture;
                node.openNorth = overture;
            }
            else if (y < node.y) //on est au sud de node
            {
                openNorth = overture;
                node.openSouth = overture;
            }
            else
            {
                Debug.Log("NodeHead: OpenTo: Compared same node coord");
            }
        }
        //Caractéristique des Builds
        public int[] buildData;
        public string[] buildPathData;
        //Aide au DecideMasterData
        public bool hasGenerationData = false;
        public bool hasRoadData = false;
        public bool hasBuildData = false;
        public bool hasBuildPathData = false;
        public bool isLoaded = false;

        //Constructeur
        public WorldNode(int xWorld,int yWorld)
        {
            x = xWorld;
            y = yWorld;

            InitWorldNodeGameObject();
        }
        private void InitWorldNodeGameObject()
        {
            worldNode = Instantiate( Resources.Load<GameObject>("worldNode"), new Vector3(x * c_worldChunkLength, 0, y * c_worldChunkLength), Quaternion.identity);
        }
        //Modification par le master
        public void SetBiome(WorldBiome newBiome)
        {
            _biome = newBiome;
        }
        public void SetLayout(WorldLayout newLayout)
        {
            _layout = newLayout;
        }
        public void DecideMasterdata()
        {
            hasGenerationData = true;
            
            //Genere une ville
            CityNode city = new CityNode(_layout,openNorth,openSouth,openEast,openWest);
            //Update les datas
            roadData = city.GetRoadData();
            buildData = city.GetBuildData();
            buildPathData = city.GetBuildPathData();
        }
        /// <summary>°0:North,1:South,2:East,3:West; value range from 0 to 15 </summary>
        //Envoi (côté master)


        //Réception (coté client)
        public void UpdateType(WorldBiome masterBiome, WorldLayout masterLayout)
        {
            //Appelé par les clients après que le maître ait envoyé les infos sur le type
            _biome = masterBiome;
            _layout = masterLayout;
        }
        public void UpdateMasterData(int[] roads, int[] builds, string[] buildPaths)
        {
            hasGenerationData = true;

            roadData = roads;
            buildData = builds;
            buildPathData = buildPaths;
        }
        public void UpdateRoadData(int[] roads)
        {
            hasRoadData = true;
            roadData = roads;
        }
        public void UpdateBuildData(int[] builds)
        {
            hasBuildData = true;
            buildData = builds;
        }
        public void UpdateBuildPathData(string[] buildPaths)
        {
            hasBuildPathData = true;
            buildPathData = buildPaths;
        }
        public bool HasGenerationData()
        {
            hasGenerationData = hasGenerationData || (hasRoadData && hasBuildData && hasBuildPathData);
            return hasGenerationData;
        }

        //Generation
        public IEnumerator Generate(PropManager propManager)
        {
            isLoaded = true;
            GenerateFloor();
            yield return null;
            //Starting Main Generation
            //Generate Roads
            RoadNode[,] tempRoads = DecryptRoads(roadData);
            GenerateRoads(tempRoads, x, y);
            yield return null;
            //Generate Builds
            BuildNode[,] tempBuilds = DecryptBuilds(buildData, buildPathData);
            if (tempBuilds != null)
            {
                for (int yTemp = 0; yTemp < c_districtInWorldChunk * 2; yTemp++)
                {
                    for (int xTemp = 0; xTemp < c_districtInWorldChunk * 2; xTemp++)
                    {
                        tempBuilds[xTemp, yTemp].Generate(x, y);
                    }
                    yield return new WaitForSeconds(0.05f);
                }
                //Generate Streets
                if (_layout == WorldLayout.City) GenerateStreets(tempRoads, tempBuilds, x, y);
            }
            //Ending Main Generation
            yield return null;
            Debug.Log("Generate: Building Navmesh at '" + x + "," + y + "'");
            NavMeshSurface surface = worldNode.GetComponent<NavMeshSurface>();

            //https://forum.unity.com/threads/building-navmeshes-in-separate-thread.489430/


            surface.buildHeightMesh = true;//Lets try this
            surface.BuildNavMesh();
            Debug.Log("Generate: Ending Navmesh at '" + x + "," + y + "'");
            //Genere prop si master
            if (PhotonNetwork.isMasterClient)
            {
                int treeAntiDensity = 200; //The bigger, the lesser tree
                switch (_biome)
                {
                    case WorldBiome.Plain: treeAntiDensity = 60; break;
                    case WorldBiome.Arid: treeAntiDensity = 300; break;
                    case WorldBiome.Forest: treeAntiDensity = 60; break;
                    case WorldBiome.DeepForest: treeAntiDensity = 30; break;
                    default: treeAntiDensity = 500; break;
                }

                Debug.Log("Generate: Starting tree generation as MasterClient at '" + x + "," + y + "'");
                (int, int) position;
                List<(int,int)> treePositions = new List<(int, int)>();
                int rng;
                for (int i = 0; i < c_worldChunkLength; i += 3)
                {
                    for (int j = 0; j < c_worldChunkLength; j += 3)
                    {
                        rng = Random.Range(0, treeAntiDensity);
                        if (rng == 0)
                        {
                            position = (j + x * Mathf.FloorToInt(c_worldChunkLength), i + y * Mathf.FloorToInt(c_worldChunkLength));
                            if (IsOnGround(new Vector3(position.Item1,0, position.Item2), 2)) treePositions.Add(position);
                            //treePositions.Add(position);
                        }
                    }
                    if (i % 50 == 0) yield return null;
                }
                Debug.Log("Generate: Starting tree placement as MasterClient at '" + x + "," + y + "'");
                MasterMassLightPlaceTree(propManager, treePositions);
                Debug.Log("Generate: Ending tree generation as MasterClient at '" + x + "," + y + "'");
            }
        }
        private void GenerateFloor()
        {
            //Decide floor (temporary)
            string floorFile;
            if (_biome == WorldBiome.Arid)
            {
                //Debug.Log("floor file is Arid");
                floorFile = "Floors/floorArid1";
            }
            else
            {
                //Debug.Log("floor file is Plain (" + (int)_biome+ ")");
                floorFile = "Floors/floorPlain1";
            }
            //Place Floor
            GameObject newFloor;
            for (int yCycle = 0; yCycle < c_districtInWorldChunk; yCycle++)
            {
                for (int xCycle = 0; xCycle < c_districtInWorldChunk; xCycle++)
                {
                    newFloor = Instantiate(
                        Resources.Load<GameObject>(floorFile),
                        new Vector3(xCycle * c_districtLength + x * c_worldChunkLength, -0.5f, yCycle * c_districtLength + y * c_worldChunkLength), 
                        Quaternion.identity);

                    newFloor.transform.parent = worldNode.transform;
                }
            }
        }

        //Generateur static (decrypt)
        private static RoadNode[,] DecryptRoads(int[] roadData)
        {
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
        private static BuildNode[,] DecryptBuilds(int[] buildData, string[] buildPathData)
        {
            if (buildData == null && buildPathData == null)
            {
                return null;
            }
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
        private static void GenerateStreets(RoadNode[,] tempRoadMatrix, BuildNode[,] tempBuildMatrix, int xOffset, int yOffset)
        {
            RoadNode road;
            bool isBuildSW;
            bool isBuildSE;
            bool isBuildNE;
            bool isBuildNW;
            int branchCount;
            for (int y = 0; y < c_districtInWorldChunk; y++)
            {
                for (int x = 0; x < c_districtInWorldChunk; x++)
                {
                    road = tempRoadMatrix[x, y];
                    isBuildSW = tempBuildMatrix[2 * x, 2 * y].IsABuilding();
                    isBuildSE = tempBuildMatrix[2 * x + 1, 2 * y].IsABuilding();
                    isBuildNE = tempBuildMatrix[2 * x + 1, 2 * y + 1].IsABuilding();
                    isBuildNW = tempBuildMatrix[2 * x, 2 * y + 1].IsABuilding();
                    branchCount = 0;
                    if (isBuildSW || isBuildSE || isBuildNE || isBuildSW)
                    {
                        Quaternion quater;
                        if ((isBuildNW && isBuildNE) && (road.North == 0)) //N 0 degre
                        {
                            branchCount++;
                            quater = Quaternion.Euler(0, 0, 0);
                            Instantiate(Resources.Load<GameObject>("Roads/fillBranch"),
                            new Vector3(x * c_districtLength + xOffset * c_worldChunkLength, 0, y * c_districtLength + yOffset * c_worldChunkLength),
                            quater);
                        }
                        if ((isBuildNE && isBuildSE) && (road.East == 0)) //E 90
                        {
                            branchCount++;
                            quater = Quaternion.Euler(0, 90, 0);
                            Instantiate(Resources.Load<GameObject>("Roads/fillBranch"),
                            new Vector3(x * c_districtLength + xOffset * c_worldChunkLength, 0, y * c_districtLength + yOffset * c_worldChunkLength),
                            quater);
                        }
                        if ((isBuildSE && isBuildSW) && (road.South == 0)) //S 180
                        {
                            branchCount++;
                            quater = Quaternion.Euler(0, 180, 0);
                            Instantiate(Resources.Load<GameObject>("Roads/fillBranch"),
                            new Vector3(x * c_districtLength + xOffset * c_worldChunkLength, 0, y * c_districtLength + yOffset * c_worldChunkLength),
                            quater);
                        }
                        if ((isBuildSW && isBuildNW) && (road.West == 0)) //W 270
                        {
                            branchCount++;
                            quater = Quaternion.Euler(0, 270, 0);
                            Instantiate(Resources.Load<GameObject>("Roads/fillBranch"),
                            new Vector3(x * c_districtLength + xOffset * c_worldChunkLength, 0, y * c_districtLength + yOffset * c_worldChunkLength),
                            quater);
                        }
                        //Pice Central
                        if ((road.North == 0 && road.South == 0 && road.East == 0 && road.West == 0) && branchCount >= 2)
                        {
                            Instantiate(Resources.Load<GameObject>("Roads/fillCentral"),
                            new Vector3(x * c_districtLength + xOffset * c_worldChunkLength, 0, y * c_districtLength + yOffset * c_worldChunkLength),
                            Quaternion.identity);
                        }
                        //SUBARASHI CONDITION
                        //((isBuildSW && isBuildSE) || (isBuildNE && isBuildNW) || (isBuildSE && isBuildNW) || (isBuildSE && isBuildNE) || (isBuildSW && isBuildNW) || (isBuildSW && isBuildNE))

                    }
                }
            }
        }
        //placement des props
        private bool IsOnGround(Vector3 pos, float radius)
        {
            Collider[] hitColliders = Physics.OverlapSphere(pos, radius);
            bool hitOneGround = false;

            foreach(Collider hit in hitColliders)
            {
                if (!hit.transform.CompareTag("Floor"))
                {
                    return false;
                }
                hitOneGround = true;
            }
            return hitOneGround;
        }
        private void MasterMassLightPlaceTree(PropManager propManager, List<(int, int)> pos)
        {
            int length = pos.Count;
            int[] x = new int[length];
            int[] y = new int[length];
            byte[] rot = new byte[length];
            int[] propIds = new int[length];

            Prop randTree;
            for (int i = 0; i < length; i++)
            {
                x[i] = pos[i].Item1;
                y[i] = pos[i].Item2;
                rot[i] = (byte)Random.Range(0, 4);

                randTree = propTable.GetRandomPropWithType(PropTable.PropType.Tree);
                propIds[i] = propTable.GetIdWithProp(randTree);
            }
            
            propManager.MassLightPlaceProp(length, x, y, rot, propIds);
        }
    }

    public enum WorldBiome
    {
        Undecided = 0, //NB: l'index 0 est le seul biome temporaire
        Plain = 1,
        Forest = 2,
        DeepForest = 3,
        Arid = 4
    }
    private Dictionary<WorldBiome, int> _baseBiomeTemperatureModifier = new Dictionary<WorldBiome, int>()
    {
        {WorldBiome.Undecided, 0 },
        {WorldBiome.Plain, 0 },
        {WorldBiome.Forest, 0 },
        {WorldBiome.DeepForest, 0 },
        {WorldBiome.Arid, +5 }
    };
    public int GetPosTempModifier(Vector3 pos)
    {
        //Trouve les coords dans la matrice de world
        int x = Mathf.FloorToInt(pos.x / c_worldChunkLength);
        int y = Mathf.FloorToInt(pos.y / c_worldChunkLength);
        //Recupere le biome a ces coordonnées
        WorldBiome posBiome;
        try
        {
            posBiome = _world[x, y].Biome;
        }
        catch
        {
            Debug.LogWarning("GetPosTempModifier: Out of range exception for x: " + x + " and y: " + y);
            return 0;
        }
        
        //Return le modifier de temperature specifique à ce biome
        return _baseBiomeTemperatureModifier[posBiome];
    }
    public enum WorldLayout
    {
        Undecided = 0,
        Empty = 1,
        Village = 2,
        City = 3,
    }
    

    public NavMeshSurface surface;
    public PropManager propManager;
    //Constante de génération
    public const float c_voxChunkLength = 12.6f; //magic number with MagicaVoxel, tunk = technical chunk
    public const int c_voxChunkInDistrict = 5;
    public const float c_districtLength = c_voxChunkLength * c_voxChunkInDistrict; //63
    public const int c_districtInWorldChunk = 16;
    public const float c_worldChunkLength = c_districtLength * c_districtInWorldChunk; //1008

    public const int c_randBigSquare = 5;
    public const int c_randExtendRoad = 3;

    #region Table
    //Path and directories
    [SerializeField] private buildTable _table22 = null;
    [SerializeField] private buildTable _table24 = null;
    [SerializeField] private buildTable _table42left = null;
    [SerializeField] private buildTable _table42right = null;
    [SerializeField] private buildTable _table44left = null;
    [SerializeField] private buildTable _table44right = null;
    public static buildTable table22 = null;
    public static buildTable table24 = null;
    public static buildTable table42left = null;
    public static buildTable table42right = null;
    public static buildTable table44left = null;
    public static buildTable table44right = null;
    [SerializeField] private PropTable _propTable = null;
    public static PropTable propTable = null;
    //[SerializeField] private TreeTable _treeTable = null;
    //public static TreeTable treeTable = null;

    private void InitBuildTable()
    {
        table22 = _table22;
        table24 = _table24;
        table42left = _table42left;
        table42right = _table42right;
        table44left = _table44left;
        table44right = _table44right;

        //treeTable = _treeTable;
        propTable = _propTable;
    }
    #endregion

    // --- IMPORTANT TIPS AND TRICKS ---
    //Quandd y augmente -> on va vers le Nord
    //Quandd x augmente -> on ve vers l' Est

    //Attribut du World
    private WorldNode[,] _world = null;

    //Caractéristique de la génération
    private int _worldLength;
    private int _spawnPoint;
    public int SpawnPoint => _spawnPoint;

    //Init les buildTable en tant que static (pour les rendre accessible par CityNode)
    private void Awake()
    {
        InitBuildTable();
    }


    private void Start()
    {
        //Init de worldLength et de spawnpoint
        _worldLength = Mode.Instance.online ? (int)PhotonNetwork.room.CustomProperties["heightMap"] : 1;
        //Debug.Log(_worldLength);
        //_worldLength = PermissionsManager.Instance.heightMap;
        _spawnPoint = (_worldLength - 1) / 2;//(milieu de la map dans la matrice), 10 -> (10-1)/2 -> 4, 2 -> (2-1)/2-> 0, 1 -> (1-1)/2 -> 0
        //Tout le monde Initialise la matrice des WorldNode
        _world = new WorldNode[_worldLength, _worldLength];
        InitWorld();
        
        if (PhotonNetwork.isMasterClient || !Mode.Instance.online)
        {
            Debug.Log("Generator: Starting Init of WorldType as masterClient");
            //Au start le master decide le type des WorldNode
            MasterInitWorldType();
            int[] worldTypeData = MasterGetWorldTypeData();
            //Le master envoie l'entiereté des infos et update la condition de réception
            Debug.Log("Generator: Sending WorldType as masterClient");
            MasterSendWorldType(worldTypeData);
            OnWorldTypeReceived();
        }
        else
        {
            //Les clients attendent les infos du master (Coroutine lancé par le master)
        }
    }
    //InitWorld
    private void InitWorld()
    {
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                _world[x, y] = new WorldNode(x, y);
            }
        }
    }
    #region InitWorld Master Decide World Type
    //Master Decide World Type
    private void MasterInitWorldType()
    {
        Debug.Log("MasterInitWorldType: Deciding biomes");
        MasterInitBiome();
        Debug.Log("MasterInitWorldType: Deciding layout");
        MasterInitLayout();
        Debug.Log("MasterInitWorldType: Deciding Roads");
        MasterInitRoads();
    }
    //Init Biome
    private void MasterInitBiome()
    {
        WorldBiome[,] worldBiomes = new WorldBiome[_worldLength, _worldLength];
        InitWorldBiomes(worldBiomes); //Remplis de Undecided
        Debug.Log("MasterInitBiome: Placing Origin");
        InitWorldBiomesOrigin(worldBiomes); //Marque certain endroit d'origine de biome
        TestPrintBiomes(worldBiomes);
        Debug.Log("MasterInitBiome: Starting Expansion");
        InitWorldBiomesExpansion(worldBiomes); //Cycle through every biomeOrigin and expand until no one can expand anymore)
        TestPrintBiomes(worldBiomes);
        InitWorldBiomesCopy(worldBiomes); //Copy worldBiome biomes into world
    }
    private void InitWorldBiomes(WorldBiome[,] worldBiomes)
    {
        for (int i = 0; i < _worldLength; i++)
        {
            for (int j = 0; j < _worldLength; j++)
            {
                worldBiomes[i, j] = WorldBiome.Undecided;
            }
        }
    }
    private void InitWorldBiomesOrigin(WorldBiome[,] worldBiomes)
    {
        bool placedAtLeastOneBiome = false;
        for (int i = 0; i < _worldLength; i++)
        {
            for (int j = 0; j < _worldLength; j++)
            {
                if (Random.Range(0,100) < 10)
                {
                    worldBiomes[i, j] = GetRandomBiome();
                    placedAtLeastOneBiome = true;
                }
            }
        }
        if (!placedAtLeastOneBiome) worldBiomes[0, 0] = GetRandomBiome();
    }
    private WorldBiome GetRandomBiome()
    {
        int biomeCount = System.Enum.GetNames(typeof(WorldBiome)).Length;
        
        return (WorldBiome)(Random.Range(1, biomeCount)); //1 à 5 pour count = 6
    }
    private void InitWorldBiomesExpansion(WorldBiome[,] worldBiomes)
    {
        bool undecidedLeft = true;
        WorldBiome biomeOrigin;
        while (undecidedLeft)
        {
            WorldBiome[,] worldBiomesCopy = GetCopyWorldBiomes(worldBiomes);
            undecidedLeft = false;
            for (int i = 0; i < _worldLength; i++)
            {
                for (int j = 0; j < _worldLength; j++)
                {
                    if (worldBiomesCopy[i, j] == WorldBiome.Undecided)
                    {
                        //Il reste des endroits à remplir
                        undecidedLeft = true;
                    }
                    else
                    {
                        biomeOrigin = worldBiomes[i, j];
                        InitWorldBiomesTryReplace(worldBiomes, i + 1, j, biomeOrigin);
                        InitWorldBiomesTryReplace(worldBiomes, i - 1, j, biomeOrigin);
                        InitWorldBiomesTryReplace(worldBiomes, i, j + 1, biomeOrigin);
                        InitWorldBiomesTryReplace(worldBiomes, i, j - 1, biomeOrigin);
                    }
                }
            }
        }
    }
    private WorldBiome[,] GetCopyWorldBiomes(WorldBiome[,] worldBiomes)
    {
        WorldBiome[,] worldBiomesCopy = new WorldBiome[_worldLength, _worldLength];

        for (int i = 0; i < _worldLength; i++)
        {
            for (int j = 0; j < _worldLength; j++)
            {
                worldBiomesCopy[i, j] = worldBiomes[i, j];
            }
        }

        return worldBiomesCopy;
    }
    private bool InitWorldBiomesTryReplace(WorldBiome[,] worldBiomes, int x, int y, WorldBiome newBiome)
    {
        if (x >= 0 && y >= 0 && x < _worldLength && y < _worldLength)
        {
            if (worldBiomes[x,y] == WorldBiome.Undecided)
            {
                worldBiomes[x, y] = newBiome;
                return true;
            }
        }
        return false;
    }
    private void InitWorldBiomesCopy(WorldBiome[,] worldBiomes)
    {
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                _world[x, y].SetBiome(worldBiomes[x, y]);
            }
        }
    }
    private void TestPrintBiomes(WorldBiome[,] worldBiomes)
    {
        string line = "";
        for (int i = 0; i < _worldLength; i++)
        {
            for (int j = 0; j < _worldLength; j++)
            {
                line += (int)(worldBiomes[i, j]);
                line += "    ";
            }
            line += "\n";
        }
        Debug.Log(line);
    }
    //Init Layout
    private void MasterInitLayout()
    {
        int rng;
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                rng = Random.Range(0, 10);
                if (rng == 0)
                {
                    _world[x, y].SetLayout(WorldLayout.City);
                }
                else if (rng == 1)
                {
                    _world[x, y].SetLayout(WorldLayout.Village);
                }
                else
                {
                    _world[x, y].SetLayout(WorldLayout.Empty);
                }
            }
        }
        MasterForceLayoutAtSpawnPoint();
    }
    private void MasterForceLayoutAtSpawnPoint()
    {
        _world[_spawnPoint, _spawnPoint].SetLayout(WorldLayout.City);
    }
    //Init Roads
    private void MasterInitRoads()
    {
        MasterConnectCities();
    }
    private List<WorldNode> MasterFindPath(WorldNode node1, WorldNode node2)
    {
        List<WorldNode> path = new List<WorldNode>();
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
            path.Add(_world[currX, currY]);
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
        path.Add(_world[destX, destY]);

        return path;
    }
    private void MasterConnectWorldNode(List<WorldNode> path)
    {
        if (path.Count == 0)
        {
            Debug.Log("MasterConnectPoint: path is empty");
            return;
        }

        WorldNode previousNode = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            previousNode.OpenTo(path[i],Random.Range(5,10));
            previousNode = path[i];
        }
    }
    private void MasterCreateRoad(WorldNode node1, WorldNode node2)
    {
        MasterConnectWorldNode(MasterFindPath(node1, node2));
    }
    private List<WorldNode> MasterFindNonEmpty(WorldNode nodeCenter, int radius)
    {
        int searchSquareLength = radius * 2 + 1;
        List<WorldNode> closeNodes = new List<WorldNode>();
        WorldNode tempNode;
        for (int y = nodeCenter.y - radius; y <= nodeCenter.y + radius; y++)
        {
            for (int x = nodeCenter.x - radius; x <= nodeCenter.x + radius; x++)
            {
                tempNode = MasterCheckNonEmpty(x, y);
                if (tempNode != null)
                {
                    closeNodes.Add(tempNode);
                }
            }
        }
        return closeNodes;
    }
    private WorldNode MasterCheckNonEmpty(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _worldLength && y < _worldLength)
        {
            if (_world[x, y].Layout != WorldLayout.Empty)
            {
                return _world[x, y];
            }
        }
        return null;
    }
    private void MasterConnectToNodeList(WorldNode nodeCenter, List<WorldNode> nodeList)
    {
        foreach(WorldNode node in nodeList)
        {
            MasterCreateRoad(nodeCenter, node);
        }
    }
    private void MasterConnectToCloseNode(WorldNode nodeCenter)
    {
        MasterConnectToNodeList(nodeCenter, MasterFindNonEmpty(nodeCenter, 2));
    }
    private void MasterConnectCities()
    {
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                if(_world[x,y].Layout == WorldLayout.City)
                {
                    MasterConnectToCloseNode(_world[x, y]);
                }
            }
        }
    }
    //Get Data
    private int[] MasterGetWorldTypeData()
    {
        int[] package = new int[_worldLength * _worldLength * 2];

        int index = 0;
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                package[index] = (int)(_world[x,y].Biome);
                package[index + 1] = (int)(_world[x, y].Layout);

                index += 2;
            }
        }

        return package;
    }
    //Master Send World Type
    private void MasterSendWorldType(int[] worldTypeData)
    {
        Debug.Log("MasterSendWorldType: Sending world type Data as master");
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("MasterSendWorldTypeRPC", PhotonTargets.OthersBuffered, _worldLength, worldTypeData);
        }       
        
    }
    [PunRPC] private void MasterSendWorldTypeRPC(int worldLength, int[] worldTypeData)
    {
        _worldLength = worldLength;
        _spawnPoint = (_worldLength - 1) / 2;
        _world = new WorldNode[_worldLength, _worldLength];
        InitWorld();
        Debug.Log("MasterSendWorldTypeRPC: receiving world type Data");
        StartCoroutine(WaitForWorldInit(worldTypeData));
    }
    #endregion

    //Coroutine du start des clients (lancé par le master)
    public IEnumerator WaitForWorldInit(int[] worldTypeData)
    {
        while(_world == null)
        {
            Debug.Log("WaitForWorldInit: waiting for _world to Init");
            yield return new WaitForSeconds(1);
        }
        //Les clients recoivent l'entiereté des infos
        ClientUpdateWorldType(worldTypeData);
    }
    private void ClientUpdateWorldType(int[] worldTypeData)
    {
        Debug.Log("ClientUpdateWorldType: Updating worldtype as client");
        int index = 0;
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                _world[x, y].UpdateType((WorldBiome)(worldTypeData[index]), (WorldLayout)(worldTypeData[index + 1]));

                index += 2;
            }
        }
        OnWorldTypeReceived();
    }
    //Fin de l'init de WorldNode (TRUE START HERE)
    private void OnWorldTypeReceived()
    {
        //The truest Start
        //numbers of chunk left to generate (decrements when a chunk finishes to load)
        chunkLeftToLoad = _worldLength * _worldLength;
        loadingChunkLeftToLoad = chunkLeftToLoad;
        Debug.Log("OnWorldTypeReceived: chunkLeftToLoad " + chunkLeftToLoad);
        //Start the generation of all chunk
        StartCoroutine(LoadAllChunk());

        //Wait for end of generation
        StartCoroutine(OnGenerationEnded());
    }
    private int loadingChunkLeftToLoad;
    private int chunkLeftToLoad = -1;
    private bool loadingChunk = false;
    

    //Debut de l'update permanente
    private IEnumerator LoadAllChunk()
    {
        for (int y = 0; y < _worldLength; y++)
        {
            for (int x = 0; x < _worldLength; x++)
            {
                while (loadingChunk) yield return new WaitForSeconds(0.5f);
                loadingChunk = true;
                ClientLoadChunk(x, y);
            }
        }
    }

    private void LoadChunkAround(int x, int y)
    {
        StartCoroutine(CorLoadChunkAround(x, y));
    }

    private IEnumerator CorLoadChunkAround(int x, int y)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (ChunkHasToBeLoaded(x + i, y + j))
                {
                    while (loadingChunk) yield return null;
                    loadingChunk = true;
                    ClientLoadChunk(x + i, y + j);
                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }
    private bool ChunkHasToBeLoaded(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _worldLength && y < _worldLength)
        {
            if (!_world[x, y].isLoaded)
            {
                return true;
            }
        }
        return false;
    }
    
    //Sending and receiving generation data
    private void ClientLoadChunk(int x, int y)
    {
        GetComponent<CentralManager>().PlaceCameraAbove(x, y);
        //Debug.Log("ClientLoadChunk: Asking to master WorldNode Data");
        //Un client demande la génération d'un chunk au master
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("MasterDataAskedHandler", PhotonTargets.MasterClient, x, y, PhotonNetwork.player.ID);
        }
        else
        {
            MasterDataAskedHandler(x, y);
        }
        
    }
    [PunRPC] private void MasterDataAskedHandler(int x, int y, int IdOfAsker = 0)
    {
        //Debug.Log("MasterDataAskedHandler: Master sending data to " + PhotonPlayer.Find(IdOfAsker).NickName);
        //Appelé par RPC par ClientLoadChunk (seulement effectué par Master)
        WorldNode worldTarget = _world[x, y];
        //Le master regarde s'il a lui meme décidé de la génération
        if (!worldTarget.hasGenerationData)
        {
            //Si non , il décide de la génération
            worldTarget.DecideMasterdata();
        }
        //il envoie les informations de la génération au client
        int[] roads = worldTarget.roadData;
        int[] builds = worldTarget.buildData;
        string[] buildPaths = worldTarget.buildPathData;
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("ClientUpdateDataAt", PhotonPlayer.Find(IdOfAsker), x, y, roads, builds, buildPaths);
        }
        else
        {
            ClientUpdateDataAt(x, y, roads, builds, buildPaths);
        }
        
    }
    [PunRPC] private void ClientUpdateDataAt(int x, int y, int[] roads, int[] builds, string[] buildPaths)
    {
        //Debug.Log("ClientUpdateDataAt: Receiving Generation Data as " + PhotonNetwork.player.NickName);
        //Appelé après ClientLoadChunk (apres etre passé par MasterDataAskedHandler)
        _world[x, y].UpdateMasterData(roads, builds, buildPaths);

        StartCoroutine(ClientGenerateAt(x, y));
    }
    private IEnumerator ClientGenerateAt(int x, int y)
    {
        Debug.Log("ClientGenerateAt: Generating at " + x + "," + y);
        yield return StartCoroutine(_world[x, y].Generate(propManager));
        Debug.Log("ClientGenerateAt: Ended Generation at " + x + "," + y);

        //One chunk has finished to generate
        chunkLeftToLoad--;
        loadingChunk = false;
    }

    //Ending
    private IEnumerator OnGenerationEnded()
    {
        while (loadingChunk || chunkLeftToLoad > 0) yield return new WaitForSeconds(0.5f);

        Debug.Log("OnGenerationEnded: Generation finished");
        CentralManager cm = GetComponent<CentralManager>();

        cm.PlaceCameraAbove(_spawnPoint, _spawnPoint);
        cm.OnGenerationFinished();
    }


    //Getters for loading
    public float GetCurrentStatus()
    {
        if(chunkLeftToLoad < 0) return 0;
        if (chunkLeftToLoad == 0) return 1;
        return  (loadingChunkLeftToLoad - chunkLeftToLoad) / (float)loadingChunkLeftToLoad;
    }


}
