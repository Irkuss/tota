using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string description = "";
    IEnumerator _wait = null;

    public void OnPointerEnter(PointerEventData eventData)
    {        
        if (description == "") return;
        _wait = Wait();
        StartCoroutine(_wait);        
    }

    public void OnPointerExit(PointerEventData eventData)
    {        
        if (_wait != null) StopCoroutine(_wait);
        GameObject slot = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Description;
        slot.SetActive(false);
        
    }

    private void ShowDescription(Vector3 position)
    {
        GameObject slot = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Description;
        slot.SetActive(true);
        slot.transform.GetChild(0).GetComponent<Text>().text = description;
        Vector3 pos = new Vector3(Input.mousePosition.x - 5, Input.mousePosition.y - 10, position.z);
        //Vector3 pos = new Vector3(position.x - 3, position.y + 3, position.z);
        slot.transform.position = pos;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        ShowDescription(transform.position);
    }
}
