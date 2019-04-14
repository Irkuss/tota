using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Interactable
{
    public Item woodRaw;

    public override void Interact(CharaHead chara)
    {
        //ajoute du bois au joueur
        for (int i = 0; i < Random.Range(4, 7); i++)
        {
            chara.GetComponent<CharaInventory>().Add(woodRaw);
        }
        //Se détruit
        GetComponent<PropHandler>().DestroySelf();
    }
}
