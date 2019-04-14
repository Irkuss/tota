using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Zombie))]
public class DrowFOV : Editor
{

    void OnSceneGUI()
    {
        Zombie fow = (Zombie)target;
        Handles.color = Color.black;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.wanderRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.fieldOfViewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.fieldOfViewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.wanderRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.wanderRadius);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visibleTarget.position);
        }
    }

}
