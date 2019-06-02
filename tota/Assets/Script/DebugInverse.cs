using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugInverse : MonoBehaviour
{
    public Transform linkedTransformToCheck = null;




    private void Update()
    {
        if(linkedTransformToCheck != null) Debug.Log(linkedTransformToCheck.InverseTransformPoint(transform.position));
    }
}
