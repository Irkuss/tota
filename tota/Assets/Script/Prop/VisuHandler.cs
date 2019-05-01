using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VisuHandler : MonoBehaviour
{
    //VisuHandler: has to be placed on a blueprint construction
    [TextArea]
    [SerializeField] private string _note = "VisuHandler: has to be placed on a blueprint construction";
    //[Header("VisuHandler: has to be placed on a blueprint construction")]
    //Half of the box check dimension
    [Header("Box check dimension")]
    [SerializeField] private float checkX = 0;
    [SerializeField] private float checkY = 0;
    [SerializeField] private float checkZ = 0;
    //Material for valid and filled space
    [Header("Material for valid and filled space(in Material/Visualisation)")]
    [SerializeField] private Material validMat = null;
    [SerializeField] private Material fillMat = null;
    
    private List<Collider> invaderColliders;
    private int originLayer;
    private Vector3 halfDir;
    private Renderer[] renderers;
    private bool hadValidSpace = true;
    private IEnumerator updateColorCor;


    //Init
    private void Awake()
    {
        invaderColliders = new List<Collider>();
        halfDir = new Vector3(checkX/2, checkY/2, checkZ/2);
        originLayer = gameObject.layer;
    }
    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        Material[] materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
        ForceUpdateColor();
        updateColorCor = UpdateColor();

        StartCoroutine(updateColorCor);

    }

    //Brouillon debug
    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y + checkY / 2, transform.position.z), halfDir * 2);
    }

    //Update material (between valid and filled space)
    private IEnumerator UpdateColor()
    {
        while(true)
        {
            if (IsReadyToPlace())
            {
                if (!hadValidSpace)
                {
                    hadValidSpace = true;
                    ChangeMatColor(validMat);
                }
            }
            else
            {
                if (hadValidSpace)
                {
                    hadValidSpace = false;
                    ChangeMatColor(fillMat);
                }
            }
            yield return null;
        }
    }
    public void ForceUpdateColor()
    {
        if (IsReadyToPlace())
        {
            hadValidSpace = true;
            ChangeMatColor(validMat);
        }
        else
        {
            hadValidSpace = false;
            ChangeMatColor(fillMat);
        }
    }
    private void ChangeMatColor(Material mat)
    {
        foreach (Renderer rend in renderers)
        {
            rend.material = mat;
        }
    }

    //Entree et sortie de la visualisation
    public void StartVisualisation() //CALLED BEFORE START WHEN BUILDING
    {
        //Things to do when settings the blueprint
        gameObject.layer = 2; //Set layer to Ignore Raycast
        GetComponent<Collider>().enabled = false;
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = false;
    }
    public void EndVisualisation()
    {
        //Things to do when placing the blueprint (NB: it has to return to the Start state)
        StopCoroutine(updateColorCor);
        ChangeMatColor(validMat);
        Debug.Log("EndVisualisation: setting layer to " + originLayer);
        gameObject.layer = originLayer; //Set layer to layer before it was changed
        GetComponent<Collider>().enabled = true;
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = true;
    }

    //Verification de l'espace de placement
    public bool IsReadyToPlace()
    {
        Vector3 center = new Vector3(transform.position.x, transform.position.y + checkY/2 + 0.5f, transform.position.z);
        Collider[] colls = Physics.OverlapBox(center, halfDir, transform.rotation);
        return colls.Length == 0;
    }
}
