using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public class RealProp
    {
        //defining attributes to gameobject
        private Vector3 _propPosition;
        private float _propRotation;
        private string _propName;
        //the referenced gameobject
        private GameObject _go;
        //unique Id
        private int _id;
        public int Id => _id;


        //Constructeur
        public RealProp(Vector3 pos, float rot, int id, string name)
        {
            _propPosition = pos;
            _propRotation = rot;
            _propName = name;
            _id = id;
            _go = null;
        }
        public RealProp(GameObject go, int id)
        {
            _id = id;
            _go = go;
        }

        //Updating the id
        public void UpdateId(int id)
        {
            _id = id;
            if (_go != null) _go.GetComponent<PropHandler>().SetId(_id);
        }

        //Instant et destroy

        public void InstantSelf()
        {
            if (_go != null) return;
            Quaternion rotation = Quaternion.Euler(0, _propRotation, 0);
            _go = Instantiate(Resources.Load<GameObject>(_propName), _propPosition, rotation);
            _go.GetComponent<PropHandler>().SetId(_id);
        }

        public void DestroySelf()
        {
            if (_go == null) return;
            Destroy(_go);
        }

        //Sending special command
        public void ReceivePropCommand(int[] command)
        {
            if (_go != null) _go.GetComponent<PropHandler>().CommandReceive(command);
        }
    }

    //Reference

    public PropTable propTable = null;

    //Attribute
    private static List<RealProp> _props;

    private static int propCount;

    private void Awake()
    {
        _props = new List<RealProp>();
        propCount = 0;
    }

    //Private

    private RealProp FindPropWithId(int id)
    {
        foreach(RealProp prop in _props)
        {
            if (prop.Id == id)
            {
                return prop;
            }
        }
        return null;
    }

    private void UpdateAllId()
    {
        for (int i = 0; i < _props.Count; i++)
        {
            _props[i].UpdateId(i);
        }
    }

    //Public appelé par n'importe qui + RPc correspondant

    public void LightPlaceProp(int x, int z, byte rot, int propId)
    {
        GetComponent<PhotonView>().RPC("RPC_LightPlaceProp", PhotonTargets.AllBuffered, x, z, rot, propId);
    }
    [PunRPC] private void RPC_LightPlaceProp(int x, int z, byte rot, int propId)
    {
        string name = propTable.GetPropWithId(propId).path;
        RealProp prop = new RealProp(new Vector3(x, 0, z), rot * 90, _props.Count, name);
        //Add the prop to the list
        _props.Add(prop);
        //Update the gameobject
        prop.InstantSelf();
    }

    public void MassLightPlaceProp(int length, int[] x, int[] z, byte[] rot, int[] propIds)
    {
        GetComponent<PhotonView>().RPC("RPC_MassLightPlaceProp", PhotonTargets.AllBuffered, length, x, z, rot, propIds);
    }
    [PunRPC] private void RPC_MassLightPlaceProp(int length, int[] x, int[] z, byte[] rot, int[] propIds)
    {
        RealProp prop;
        string name;
        for (int i = 0; i < length; i++)
        {
            int id = propIds[i];
            Prop pro = propTable.GetPropWithId(id);
            name = pro.path;
            //name = propTable.GetPropWithId(propIds[i]).path;
            prop = new RealProp(new Vector3(x[i], 0, z[i]), rot[i] * 90, _props.Count, name);
            //Add the prop to the list
            _props.Add(prop);
            //Update the gameobject
            prop.InstantSelf();
        }
    }

    public void PlaceProp(Vector3 pos, float rot, string name)
    {
        //Make all players place the new prop
        GetComponent<PhotonView>().RPC("RPC_PlaceProp", PhotonTargets.AllBuffered, pos.x, pos.y, pos.z, rot, name);
    }
    public void PlaceAlreadyExistingProp(GameObject go, float rot, string name)
    {
        //Make all other players place the new prop (locally update PropManager)
        //create a new prop with correct parameters (NB: no need to update all Ids)
        RealProp prop = new RealProp(go, _props.Count);
        //Add the prop to the list
        _props.Add(prop);
        Vector3 pos = go.transform.position;
        GetComponent<PhotonView>().RPC("RPC_PlaceProp", PhotonTargets.OthersBuffered, pos.x, pos.y, pos.z, rot, name);
    }
    [PunRPC] private void RPC_PlaceProp(float x, float y, float z, float rot, string name)
    {
        //Debug.Log("RPC_PlaceProp: receiving a prop to place");
        //create a new prop with correct parameters (NB: no need to update all Ids)
        RealProp prop = new RealProp(new Vector3(x, y, z), rot, _props.Count, name);
        //Add the prop to the list
        _props.Add(prop);
        //Update the gameobject
        prop.InstantSelf();
    }


    public void MassPlaceProp(int length, Vector3[] pos, float[] rot, string[] name)
    {
        float[] xArray = new float[length];
        float[] yArray = new float[length];
        float[] zArray = new float[length];
        for (int i = 0; i < length; i++)
        {
            xArray[i] = pos[i].x;
            yArray[i] = pos[i].y;
            zArray[i] = pos[i].z;
        }
        GetComponent<PhotonView>().RPC("RPC_MassPlaceProp", PhotonTargets.AllBuffered, length, xArray, yArray, zArray, rot, name);
    }
    [PunRPC] private void RPC_MassPlaceProp(int length, float[] x, float[] y, float[] z, float[] rot, string[] name)
    {
        RealProp prop;
        for (int i = 0; i < length; i++)
        {
            prop = new RealProp(new Vector3(x[i], y[i], z[i]), rot[i], _props.Count, name[i]);
            //Add the prop to the list
            _props.Add(prop);
            //Update the gameobject
            prop.InstantSelf();
        }
    }

    public void DestroyProp(int id)
    {
        GetComponent<PhotonView>().RPC("RPC_DestroyProp", PhotonTargets.AllBuffered, id);
    }
    [PunRPC] private void RPC_DestroyProp(int id)
    {
        RealProp prop = FindPropWithId(id);

        if (prop != null)
        {
            //Remove the prop from the list
            _props.Remove(prop);
            //Update all Ids
            UpdateAllId();
            //Update the gameobject
            prop.DestroySelf();
            //Make that reference null (?)
            prop = null;
        }
    }

    public void SendPropCommand(int id, int[] command)
    {
        GetComponent<PhotonView>().RPC("RPC_SendPropCommand", PhotonTargets.AllBuffered, id, command);
    }
    [PunRPC] private void RPC_SendPropCommand(int id, int[] command)
    {
        RealProp prop = FindPropWithId(id);

        if (prop != null)
        {
            prop.ReceivePropCommand(command);
        }
    }
}
