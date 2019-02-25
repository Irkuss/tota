using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSpotHead : MonoBehaviour
{
    //DEPRECATED

    //directions dont l'espace est libre (0 = libre, 1 occupé)
    private int _north = 0;
    private int _south = 0;
    private int _east = 0;
    private int _west = 0;

    //All BuildSpotHead can force other BuildSpotHead to not generate on them

    public bool isFree = true;

    //Origin Node
    private NodeHead nodeOrigin;
    private int posToNode;//0 = SW , 1 = SE , 2 = NE, 3 = NW
    public int facingRotation = 0; //0f = south, 90f = (1) west, 180f = (2) north, 270f = (3) east
    

    //Initialisation

    public void Init(NodeHead node,int pos)
    {
        nodeOrigin = node;
        posToNode = pos;

        switch (posToNode)
        {
            case 0: //0 = SW
                InitSetSpace(nodeOrigin.GetWest(), 0, nodeOrigin.GetSouth(), 0);
                break;
            case 1: //1 = SE
                InitSetSpace(nodeOrigin.GetEast(), 0, 0, nodeOrigin.GetSouth());
                break;
            case 2: //2 = NE
                InitSetSpace(0, nodeOrigin.GetEast(), 0, nodeOrigin.GetNorth());
                break;
            default: //3 = NW
                InitSetSpace(0, nodeOrigin.GetWest(), nodeOrigin.GetNorth(), 0);
                break;
        }
        InitRotation();

    }

    private void InitSetSpace(int north,int south,int east,int west)
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
            facingRotation = validRotation[Random.Range(0, validRotation.Count)];
        }
    }
    

    //End Generate

    public void Generate()
    {
        if(isFree)
        {
            if (_north + _south + _east + _west > 0)
            {
                //Choisir quel building construire
                int rng = Random.Range(0,3);
                //rng = 0;
                string path = "Buildings/build22/test" + rng;


                //Bien le placer
                Vector3 offsetRotation = new Vector3();

                switch (facingRotation)
                {
                    case 0: //0f = 0 * 90 -> south
                        //Debug.Log("facing South Moving not");
                        offsetRotation.Set(0, 0, 0);
                        break;
                    case 1: //90f = 1 * 90 -> west
                        //Debug.Log("facing west Moving north but moved east");
                        offsetRotation.Set(0, 0, 12.6f);
                        break;
                    case 2: //180f = 2 * 90 -> north
                        //Debug.Log("facing north Moving east and north");
                        offsetRotation.Set(12.6f, 0, 12.6f);
                        break;
                    default://270f = 3 * 90 -> east
                        //Debug.Log("facing east Moving east but moved north");
                        offsetRotation.Set(12.6f, 0, 0);
                        break;
                }

                
                //Instantié et le tourner dans la bonne direction
                GameObject build = Instantiate(Resources.Load<GameObject>(path), this.transform.position + offsetRotation, this.transform.rotation);

                build.transform.Rotate(0, facingRotation * 90f, 0);
            }
            
            //Instantiate("Buildings/build22/test22tower", this.transform.position, this.transform.rotation, 0);
        }
    }
}
