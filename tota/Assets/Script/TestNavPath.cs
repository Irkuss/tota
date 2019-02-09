using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavPath : MonoBehaviour
{

    public Camera camera;
    public NavMeshAgent navMeshAgent;

    public string playeName = "testName";
    private string currentMaster;

    void Start()
    {
        currentMaster = null;
    }

    void Update()
    {
        if (ClickedOn())
        {
            currentMaster = playeName;
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                currentMaster = null;
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
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
