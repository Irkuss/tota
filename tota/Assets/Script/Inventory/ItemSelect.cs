using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSelect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Item item;
    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    Transform startParent; 


    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        transform.position = startPosition;

    }

    /*
    public void SelectItem()
    {
        bool pickedUp = player.GetComponent<InventoryManager>().Add(item);
        //bool pickedUp = FindObjectOfType<InventoryManager>().Add(item);
        //bool pickedUp = InventoryManager.instance.Add(item);
        if (pickedUp)
        {
            Destroy(gameObject);
        }
    }*/
}
