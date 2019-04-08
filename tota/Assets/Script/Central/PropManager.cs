using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public class Prop
    {
        private Vector3 _propPosition;
        private int _propRotation;

        private GameObject go;

        private int _id;
        public int Id => _id;

        public Prop(Vector3 pos, int rot, int id)
        {
            _propPosition = pos;
            _propRotation = rot;
            _id = id;
        }

        public void InstantSelf()
        {
            Instantiate()
        }

        public void DestroySelf()
        {

        }
    }

    private List<Prop> _props;

    private static int propCount;

    private void Awake()
    {
        _props = new List<Prop>();
        propCount = 0;
    }

    //Private

    private Prop FindPropWithId(int id)
    {
        foreach(Prop prop in _props)
        {
            if (prop.Id == id)
            {
                return prop;
            }
        }
        return null;
    }

    private void RemoveProp(Prop prop)
    {
        _props.Remove(prop);


        prop = null;
    }

    //Public

    public void PlaceProp(Vector3 pos, int rot)
    {

    }

    public void DestroyProp(int id)
    {
        GetComponent<PhotonView>().RPC("RPC_DestroyProp", PhotonTargets.AllBuffered, id);
    }

    [PunRPC] private void RPC_DestroyProp(int id)
    {
        Prop prop = FindPropWithId(id);

        if (prop != null)
        {
            RemoveProp(prop);
        }
    }
}
