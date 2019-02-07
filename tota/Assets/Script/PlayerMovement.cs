using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : Photon.MonoBehaviour
{
    private Camera camera;
    public NavMeshAgent navMeshAgent;

    private void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    navMeshAgent.SetDestination(hit.point);
                }
            }
        }
    }
}
