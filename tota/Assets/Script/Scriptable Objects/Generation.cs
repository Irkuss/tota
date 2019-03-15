using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation")]
public class Generation : ScriptableObject
{
    public int worldLength = 0;
    [Range(0, 100)] public float worldPolisDensity = 0f;
    [Range(0, 100)] public float worldBiomeDensity = 0f;
}
