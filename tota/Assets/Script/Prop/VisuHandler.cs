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
    public void StartVisualisation()
    {
        //Things to do when settings the blueprint
    }

    public void EndVisualisation()
    {
        //Things to do when placing the blueprint (NB: it has to return to the Start state)
    }

    public void Construct()
    {
        visuGO.SetActive(false);
        realGO.SetActive(true);
    }
}
