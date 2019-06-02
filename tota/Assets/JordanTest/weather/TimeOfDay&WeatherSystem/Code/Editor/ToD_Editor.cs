﻿using UnityEngine;
using UnityEditor;
using System.Collections;
[ExecuteInEditMode]
[CustomEditor(typeof(ToD_Base))]
public class ToD_Editor : Editor 
{
    public bool bShowTips;
    public override void OnInspectorGUI()
    {
        DrawToDGUI();
        EditorUtility.SetDirty(target);
    }
    private void DrawToDGUI()
    {
        ToD_Base cl = target as ToD_Base;
        // SHOW MORE TIPS
        GUILayout.BeginHorizontal();
        GUILayout.Label("Show more information");
        bShowTips = EditorGUILayout.Toggle(bShowTips, GUILayout.MaxWidth(iMinWidth));
        GUILayout.EndHorizontal();
        // PREFABS FOR LIGHT AND WEATHER MASTER
        EditorGUILayout.HelpBox(("Add gameobjects, lights and materials"), MessageType.None, true);
        if (bShowTips == true)
            EditorGUILayout.HelpBox("Sun: Needs to be a directional light to cover the whole world. \n\nWeather master: This needs to be a prefab with all the weather scripts on it. (See or use example prefab in package)", MessageType.Info, true);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Sun (Directional light): ");
        cl.lSun = EditorGUILayout.ObjectField("", cl.lSun, typeof(Light), true, GUILayout.MaxWidth(iMedWidth)) as Light;
        GUILayout.EndHorizontal();
        // USING WEATHER
        GUILayout.BeginHorizontal();
        GUILayout.Label("Use Moon light: ");
        cl.GetSet_bUseMoon = EditorGUILayout.Toggle(cl.GetSet_bUseMoon, GUILayout.MaxWidth(iMinWidth));
        GUILayout.EndHorizontal();
        if (cl.GetSet_bUseMoon == true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Moon (Directional light): ");
            cl.lMoon = EditorGUILayout.ObjectField("", cl.lMoon, typeof(Light), true, GUILayout.MaxWidth(iMedWidth)) as Light;
            GUILayout.EndHorizontal();
        }
        // USING WEATHER
        GUILayout.BeginHorizontal();
        if (cl.GetSet_bUseWeather == true)
        // DAY CYCLE LENGTH
        GUILayout.BeginHorizontal();
        // STARTING TIME FOR THE GAME
        GUILayout.BeginHorizontal();
        // TIMESET SETTINGS
        GUILayout.BeginHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.BeginHorizontal();