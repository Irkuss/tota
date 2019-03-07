using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //Un component de Chara

    public GameObject canvas;
    Vector3 inventPosition;
    Quaternion inventRotation;

    private void Start()
    {
        inventRotation = new Quaternion();
        inventPosition = new Vector3(0, 3, 2);
        inventRotation = (Quaternion.Euler(90, 0, 0));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*Vector3 heading = this.transform.position - Camera.main.transform.position;
        if (Vector3.Dot(Camera.main.transform.up, heading) > 0)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(this.transform.position);
            inventory.transform.position = pos;
        }
        else
        {
            // METTRE L'INVENTAIRE AU BON ENDROIT  
            Vector3 pos = Camera.main.WorldToScreenPoint(this.transform.position);
            inventory.transform.position = pos;
            inventory.transform.rotation = inventRotation;
        }*/       
        canvas.transform.position = transform.position + inventPosition;
        canvas.transform.rotation = inventRotation;
    }

    public void DisplayInventory()
    {
        canvas.SetActive(!canvas.activeSelf);
    }

    public void RemoveInventory()
    {
        canvas.SetActive(false);
        Debug.Log("setactive");
    }

}
