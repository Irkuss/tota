using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavPath : MonoBehaviour
{

    public Camera cameraUsed;
    public NavMeshAgent navMeshAgent;

    public string playeName = "testName";
    private string currentMaster = null;
    private Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();
        outline.OutlineMode = Outline.Mode.SilhouetteOnly;
    }

    void Update()
    {
        if (ClickedOn()) //left click on this
        {
            currentMaster = playeName;
            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        }
        else
        {
            if (Input.GetMouseButtonDown(1)) //rightclick
            {
                currentMaster = null;
                outline.OutlineMode = Outline.Mode.SilhouetteOnly;
            }

            if (currentMaster != null)
            {
                MoveToClick();
            }
        }
    }

    // Update is called once per frame
    public void MoveToClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cameraUsed.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit))
            {
                navMeshAgent.SetDestination(hit.point);
            }
        }

    }

    public bool ClickedOn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cameraUsed.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == this.name)
                {
                    return true;
                }
            }
        }
        return false;
    }




}
