using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaPermissions : MonoBehaviour
{

    //Index du groupe de joueurs qui peut controler Chara
    private int groupMasterIndex = -1; //Initialisé lors de l'instantiation

    //Nom du Spirit qui controle Chara, null si personne ne la controle
    private string spiritMasterName = null;


    //Public Methods

        //Getters
    public int GetGroupMasterIndex()
    {
        return groupMasterIndex;
    }

    public string GetSpiritMasterName()
    {
        return spiritMasterName;
    }

    public bool HasGroupMaster()
    {
        return (groupMasterIndex >= 0);
    }

    public bool HasSpiritMaster()
    {
        return (spiritMasterName != null);
    }

        //Setters

    public void SetGroupMaster(int index) //
    {
        if (index >= 0)
        {
            groupMasterIndex = index;
        }
    }

    public void SetGroupMasterNull() //Probably useless
    {
        if (groupMasterIndex >= 0)
        {
            groupMasterIndex = -1;
        }
    }

    public void SetSpiritMasterNull()
    {
        if (spiritMasterName != null)
        {
            spiritMasterName = null;
        }
    }

    public void SetSpiritMaster(string spiritName)
    {
        //Change la personne qui a le controle de Chara
        spiritMasterName = spiritName;
    }

        //Others

    public bool IsSpiritMaster(string spiritName)
    {
        if (spiritMasterName != null)
        {
            return spiritMasterName == spiritName;
        }
        return false;
    }
}
