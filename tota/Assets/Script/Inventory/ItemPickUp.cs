using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickUp : Interactable
{
    //Un component d'un item au sol

    //L'item qui est au sol (représenté par un Scriptable Object)
    public Item item;
  
    public override void Interact(CharaHead chara, int actionIndex)
    {
        PickUp(chara.gameObject);
    }

    //Unity callbacks
    void PickUp(GameObject chara)
    {
        bool pickedUp = chara.GetComponent<CharaInventory>().Add(item);

        //Si l'object a été pris par le Chara on peut se détruire
        //NB: dans le cas où le Chara n'a plus de place, Add() a retourné false
        if (pickedUp)
        {
            GetComponent<PhotonView>().RPC("MasterDestroySelf", PhotonTargets.MasterClient);
            //Destroy(gameObject);
        }
    }

    [PunRPC] private void MasterDestroySelf()
    {
        Debug.Log("Confirmed received RPC MasterDestroySelf");
        //PhotonNetwork.Destroy(gameObject);
    }
}
