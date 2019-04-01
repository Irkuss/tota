using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisuHandler : MonoBehaviour
{
    [SerializeField] private GameObject visuGO;
    [SerializeField] private GameObject realGO;

    private void Start()
    {
        visuGO.SetActive(true);
        realGO.SetActive(false);
    }

    public void EndVisualisation()
    {
        visuGO.SetActive(false);
        realGO.SetActive(true);
    }
}
