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

    // Start is called before the first frame update
    void Start()
    {
        GenerateCity();

        surface.BuildNavMesh();
    }

    private void GenerateCity()
    {
        districtLength = tunkLength * nTunkInDistrict;
        nodeMatrix = new GameObject[cityX,cityY];

        PlaceNode();

        GenerateOnNode();
    }

    private void PlaceNode()
    {
        for (int y = 0; y < cityY; y++)
        {
            for (int x = 0; x < cityX; x++)
            {
                Vector3 position = new Vector3(x * districtLength, 0f, y * districtLength);

                nodeMatrix[x, y] = PhotonNetwork.Instantiate("eNode",position,Quaternion.identity,0);
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
