using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropMarker : MonoBehaviour
{
    //PropMarker marks where the prop chosen by PropSpot is located, it also places the prop

    public PropSpot propSpot = null;
    public bool isDecidingByItself = true;

    void Start()
    {
        if(isDecidingByItself)
        {
            StartPlacement();
        }
    }

    public void StartPlacement()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PlaceChosenProp(propSpot.GetChosenProp());
        }
        Destroy(this.gameObject);
    }

    private void PlaceChosenProp(Prop chosenProp)
    {
        //Dans le cas on a décidé de rien générer, ne fait rien
        if (chosenProp == null) return;


        float propRotation;
        if (propSpot.alignedWithMarker)
        {
            propRotation = transform.rotation.eulerAngles.y;
        }
        else
        {
            propRotation = Random.Range(0, 360);
        }
        
        GameObject.Find("eCentralManager")
            .GetComponent<PropManager>()
            .PlaceProp(
            transform.position,
            propRotation,
            chosenProp.path);
    }
}
