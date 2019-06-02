using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropMarkerMaster : MonoBehaviour
{
    [Header("Chance for the childs PropMarker to not destroy themselves")]
    public int randChanceToLetChildrenSpawn = 0;

    private void Start()
    {
        if(PhotonNetwork.isMasterClient)
        {
            if(Random.Range(0,100) < randChanceToLetChildrenSpawn)
            {
                PropMarker[] pms = GetComponentsInChildren<PropMarker>();

                foreach(PropMarker pm in pms)
                {
                    pm.StartPlacement();
                }
            }
        }
        Destroy(this.gameObject);
    }
}
