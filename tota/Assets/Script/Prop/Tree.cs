using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : PropHandler
{
    //Defining attribute
    [Header("Tree Attribute")]
    public Item woodRaw;
    public float baseCutTime = 10;

    //Start
    private void Start()
    {
        BeginOpacity();
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex)
    {
        Debug.Log("Tree: Interacting");
        Cut(chara);
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        return baseCutTime * chara.GetComponent<CharaRpg>().GetTimeModifier(CharaRpg.Stat.ms_strength);
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        return true;
    }

    //====================Action Method====================
    //Cut
    private void Cut(CharaHead chara)
    {
        //ajoute du bois au joueur
        for (int i = 0; i < Random.Range(4, 7); i++)
        {
            chara.GetComponent<CharaInventory>().Add(woodRaw);
        }
        //Se détruit
        DestroySelf();
    }
}
