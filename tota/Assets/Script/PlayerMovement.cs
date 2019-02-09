using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : Photon.MonoBehaviour
{
    //public GameObject cameraGO;
    public NavMeshAgent navMeshAgent;

    private Camera _playerCamera;
    

    private void Start()
    {
        /*
        if (photonView.isMine)
        {
            cameraGO.SetActive(true);
            _playerCamera = cameraGO.GetComponent<Camera>();
        }
        else
        {
            cameraGO.SetActive(false);
        }
        */
        _playerCamera = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    navMeshAgent.SetDestination(hit.point);
                }
            }
        }
    }
}
