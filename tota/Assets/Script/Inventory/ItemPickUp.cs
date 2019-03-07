using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    //Un component d'un item au sol

    //L'item qui est au sol (représenté par un Scriptable Object)
    public Item item;

    /*
    public override void Interact()
    {
        PickUp();
    }
    */

    //Unity callbacks
    public void OnTriggerEnter(Collider other)
    {
        //Appelé quand un Gameobject entre en collision avec nous
        if (other.tag == "Player")
        {
            PickUp(other.gameObject);
            this.GetComponent<Collider>().isTrigger = false;
        }
    }

    void PickUp(GameObject chara)
    {
        bool pickedUp = chara.GetComponent<CharaInventory>().Add(item);
        //bool pickedUp = chara.GetComponent<InventoryManager>().Add(item);
        //bool pickedUp = FindObjectOfType<InventoryManager>().Add(item);
        //bool pickedUp = InventoryManager.instance.Add(item);

        //Si l'object a été pris par le Chara on peut se détruire
        //NB: dans le cas où le Chara n'a plus de place, Add() a retourné false
        if (pickedUp)
        {
            Destroy(gameObject);
        }
    }
}
