using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public Item item;

    /*
    public override void Interact()
    {
        PickUp();
    }
    */

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PickUp(other.gameObject);
            this.GetComponent<Collider>().isTrigger = false;
        }

    }

    void PickUp(GameObject player)
    {
        bool pickedUp = player.GetComponent<InventoryManager>().Add(item);
        //bool pickedUp = FindObjectOfType<InventoryManager>().Add(item);
        //bool pickedUp = InventoryManager.instance.Add(item);
        if (pickedUp)
        {
            Destroy(gameObject);
        }
    }
}
