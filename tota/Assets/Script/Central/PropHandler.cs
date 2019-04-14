using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropHandler : MonoBehaviour
{
    private int _id;
    public int ID => _id;

    public void SetId(int id)
    {
        _id = id;
    }

    public void DestroySelf()
    {
        GameObject.Find("eCentralManager").GetComponent<PropManager>().DestroyProp(_id);
    }
}
