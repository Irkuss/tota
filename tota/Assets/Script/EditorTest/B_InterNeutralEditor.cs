using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(B_InterNeutral))]
public class B_InterNeutralEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        B_InterNeutral myScript = (B_InterNeutral)target;
        if (GUILayout.Button("BuildInterNeutral"))
        {
            myScript.BuildInterNeutral();
        }
    }
}
#endif