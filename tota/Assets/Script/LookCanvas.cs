using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCanvas : MonoBehaviour
{

    // Update is called once per frame
    /*void Update()
    {
        if (SpiritZoom.cam == null) return;
        transform.LookAt(transform.position + SpiritZoom.cam.transform.rotation * Vector3.forward,
            SpiritZoom.cam.transform.rotation * Vector3.up);
    }*/

    private void Awake()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
