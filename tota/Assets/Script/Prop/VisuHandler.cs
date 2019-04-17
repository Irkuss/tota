using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VisuHandler : MonoBehaviour
{
    private List<Collider> invaderColliders;
    private int originLayer;
    [SerializeField] private float checkX;
    [SerializeField] private float checkY;
    [SerializeField] private float checkZ;
    private Vector3 halfDir;
    private Material[] materials;
    private Renderer[] renderers;
    private bool hadValidSpace = true;
    private IEnumerator updateColorCor;

    [SerializeField] private Material validMat;
    [SerializeField] private Material fillMat;

    private Color validColor;
    private Color fillColor;

    public string path;

    //Init

    private void Awake()
    {
        validColor = Color.cyan;// new Color(114, 255, 255);
        fillColor = Color.red;// new Color(255, 16, 16);
        invaderColliders = new List<Collider>();
        halfDir = new Vector3(checkX/2, checkY/2, checkZ/2);
    }
    private void Start()
    {
        originLayer = gameObject.layer;
        renderers = GetComponentsInChildren<Renderer>();
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
        ForceUpdateColor();
        updateColorCor = UpdateColor();

        StartCoroutine(updateColorCor);

    }
    //Update
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

    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y + checkY / 2, transform.position.z), halfDir*2);
    }

    //Entree et sortie de la visualisation
    public void StartVisualisation()
    {
        //Things to do when settings the blueprint

        gameObject.layer = 2; //Set layer to Ignore Raycast
        GetComponent<NavMeshObstacle>().enabled = false;
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = false;
    }

    public void EndVisualisation()
    {
        //Things to do when placing the blueprint (NB: it has to return to the Start state)
        StopCoroutine(updateColorCor);
        ChangeMatColor(validMat);

        gameObject.layer = originLayer; //Set layer to layer before it was changed
        GetComponent<NavMeshObstacle>().enabled = true;
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = true;
    }

    private void ChangeMatColor(Color col)
    {
        foreach (Material mat in materials)
        {
            mat.color = col;
        }
    }
    private void ChangeMatColor(Material mat)
    {
        foreach(Renderer rend in renderers)
        {
            rend.material = mat;
        }
    }

    //Verification de l'espace de placement
    public bool IsReadyToPlace()
    {
        Vector3 center = new Vector3(transform.position.x, transform.position.y + checkY/2 + 0.5f, transform.position.z);
        Collider[] colls = Physics.OverlapBox(center, halfDir, transform.rotation);
        return colls.Length == 0;
    }

    //RealGo
    public void Construct()
    {
        //Crée un nouveau prop
        GameObject.Find("eCentralManager").
            GetComponent<PropManager>().
            PlaceProp(transform.position, transform.rotation.eulerAngles.y, path);
        GetComponent<PropHandler>().DestroySelf();
    }
}
