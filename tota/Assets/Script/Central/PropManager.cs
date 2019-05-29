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
        private string _propPath;
        //the referenced gameobject
        private GameObject _go;
        //unique Id
        private int _id;
        public int Id => _id;


        //Constructeur
        public RealProp(Vector3 pos, float rot, int id, string path)
        {
            _propPosition = pos;
            _propRotation = rot;
            _propPath = path;
            _id = id;
            _go = null;
        }
        public RealProp(GameObject go, int id)
        {
            //Already existing prop, never gonna InstantSelf()
            _id = id;
            _go = go;
            _go.GetComponentInChildren<PropHandler>().SetId(_id);
        }

        //Updating the id
        public void UpdateId(int id)
        {
            _id = id;
            if (_go != null)
            {
                //_go.GetComponent<PropHandler>().SetId(_id);
                _go.GetComponentInChildren<PropHandler>().SetId(_id);
            }
        }

        //Instant et destroy

        public void InstantSelf()
        {
            //Debug.Log("InstantSelf: instantiating " + _propPath);
            if (_go != null) return;
            Quaternion rotation = Quaternion.Euler(0, _propRotation, 0);
            try
            {
                _go = Instantiate(Resources.Load<GameObject>(_propPath), _propPosition, rotation);
                _go.GetComponentInChildren<PropHandler>().SetId(_id);
            }
            catch
            {
                Debug.LogError("Failed To Instantiate resources with path " + _propPath);
            }
            
        }

        public void DestroySelf()
        {
            Debug.Log("DestroySelf: try destroying prop ");
            if (_go == null) return;
            Debug.Log("DestroySelf: actually destroying prop with name " + _go.name + " (id: " + _id + ") at coords " + _go.transform.position);
            Destroy(_go);
        }

        //Sending special command
        public void ReceivePropCommand(int[] command, float[] commandFloat, string[] commandString = null)
        {
            if (_go != null) _go.GetComponentInChildren<PropHandler>().CommandReceive(command, commandFloat, commandString);
        }
    }

    //Reference

    public PropTable propTable = null;

    //Attribute
    private static List<RealProp> _props;

    private void Awake()
    {
        _props = new List<RealProp>();
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
    //Placing Props
    public void MassLightPlaceProp(int length, int[] x, int[] z, byte[] rot, int[] propIds)
    {
        Debug.Log("MassLightPlaceProp: mass placing props (" + length + ")");
        //Called by master when generating
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_MassLightPlaceProp", PhotonTargets.AllBuffered, length, x, z, rot, propIds);
        }
        else
        {
            RPC_MassLightPlaceProp(length, x, z, rot, propIds);
        }
        
    }
    [PunRPC] private void RPC_MassLightPlaceProp(int length, int[] x, int[] z, byte[] rot, int[] propIds)
    {
        for (int i = 0; i < length; i++)
        {
            int id = propIds[i];
            Prop pro = propTable.GetPropWithId(id);
            //name = propTable.GetPropWithId(propIds[i]).path;
            AddProp(new Vector3(x[i], 0, z[i]), rot[i] * 90, _props.Count, pro.path);
        }
    }

    public void PlaceProp(Vector3 pos, float rot, string name)
    {
        //Debug.Log("PlaceProp: placing a new prop with path: " + name);
        //Make all players place the new prop
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_PlaceProp", PhotonTargets.AllBuffered, pos.x, pos.y, pos.z, rot, name);
        }
        else
        {
            RPC_PlaceProp(pos.x, pos.y, pos.z, rot, name);
        }
    }
    public void PlaceAlreadyExistingProp(GameObject go, float rot, string name)
    {
        Debug.Log("PlaceProp: ordering placement of an already existing prop with path: " + name + ", its id is " + _props.Count);
        //Make all other players place the new prop (locally update PropManager)
        //create a new prop with correct parameters (NB: no need to update all Ids)
        RealProp prop = new RealProp(go, _props.Count);
        //Add the prop to the list
        _props.Add(prop);
        Vector3 pos = go.transform.position;
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_PlaceProp", PhotonTargets.OthersBuffered, pos.x, pos.y, pos.z, rot, name);
        }       
        
    }
    [PunRPC] private void RPC_PlaceProp(float x, float y, float z, float rot, string path)
    {
        //Debug.Log("RPC_PlaceProp: receiving a prop to place");
        //create a new prop with correct parameters (NB: no need to update all Ids)
        AddProp(new Vector3(x, y, z), rot, _props.Count, path);
    }

    private void AddProp(Vector3 pos, float yRotation, int id, string path)
    {
        RealProp prop = new RealProp(pos, yRotation, id, path);
        //Add the prop to the list
        _props.Add(prop);
        //Update the gameobject
        prop.InstantSelf();
    }

    //Destroy
    public void DestroyProp(int id)
    {
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_DestroyProp", PhotonTargets.AllBuffered, id);
        }
        else
        {
            RPC_DestroyProp(id);
        }
        
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
    //Send command
    public void SendPropCommand(int id, int[] command, float[] commandFloat, string[] commandString)
    {
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_SendPropCommand", PhotonTargets.AllBuffered, id, command, commandFloat, commandString);
        }
        else
        {
            RPC_SendPropCommand(id, command, commandFloat, commandString);
        }
        
    }
    [PunRPC] private void RPC_SendPropCommand(int id, int[] command, float[] commandFloat, string[] commandString)
    {
        RealProp prop = FindPropWithId(id);

        if (prop != null)
        {
            prop.ReceivePropCommand(command, commandFloat, commandString);
        }
    }
}
