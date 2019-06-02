using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropHandler : Interactable
{
    private int _id;
    public int ID => _id;

    private void Start()
    {
        BeginOpacity();
    }

    //Called by PropManager when init or updating
    public void SetId(int id)
    {
        _id = id;
    }

    //Used to send special command to all gameobject of this id (ex: Add item to inventory)
    public void CommandSend(int[] command, float[] commandFloat = null, string[] commandString = null)
    {
        GameObject.Find("eCentralManager").GetComponent<PropManager>().SendPropCommand(_id, command, commandFloat, commandString);
    }
    //called by PropManager when receiving special command
    // !!! THE PARSING SHOULD OVERWRITE THIS FUNCTION !!!
    public virtual void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        //overwrite to parse command
    }
    

    //Have to be called to destroy this gameobject
    public void DestroySelf()
    {
        Debug.Log("DestroySelf: ordering destroying self, " + name + " (id: " + _id + ")");
        GameObject.Find("eCentralManager").GetComponent<PropManager>().DestroyProp(_id);
    }
}
