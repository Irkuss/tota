using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropHandler : Interactable
{
    private int _id;
    public int ID => _id;

    //Called by PropManager when init or updating
    public void SetId(int id)
    {
        _id = id;
    }

    //Used to send special command to all gameobject of this id (ex: Add item to inventory)
    public void CommandSend(byte command)
    {
        GameObject.Find("eCentralManager").GetComponent<PropManager>().SendPropCommand(_id, command);
    }
    //called by PropManager when receiving special command
    // !!! THE PARSING SHOULD OVERWRITE THIS FUNCTION !!!
    public virtual void CommandReceive(byte command)
    {
        //overwrite to parse command
    }

    //Have to be called to destroy this gameobject
    public void DestroySelf()
    {
        GameObject.Find("eCentralManager").GetComponent<PropManager>().DestroyProp(_id);
    }
}
