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

        if (fow.player != null)
        {
            Handles.color = Color.red;
            Handles.DrawLine(fow.transform.position, fow.player.position);
        }
    }

}
