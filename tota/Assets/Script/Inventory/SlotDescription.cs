using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string description = "";

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (description == "") return;
        ShowDescription(transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject slot = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Description;
        slot.SetActive(false);
        
    }

    private void ShowDescription(Vector3 position)
    {
        GameObject slot = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Description;
        slot.SetActive(true);
        slot.transform.GetChild(0).GetComponent<Text>().text = description;
        Vector3 pos = new Vector3(Input.mousePosition.x - 3, Input.mousePosition.y + 3, position.z);
        //Vector3 pos = new Vector3(position.x - 3, position.y + 3, position.z);
        slot.transform.position = pos;
    }
}
