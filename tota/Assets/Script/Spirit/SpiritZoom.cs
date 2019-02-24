using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritZoom : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject spiritCamera;
    [SerializeField]
    private SpiritMovement movement;

    
    private Camera cameraComp;
    private Transform transformer;

    private float baseZoomValue = 60.0f;

    private float zoomSpeed = 10.0f;
    private float zoomValue; //= baseZoomValue at start

    private float zoomMin = 45.0f;
    private float zoomMax = 90.0f;

    private float movementModifier = 4.0f;

    //private GameObject eManager;
    private FloorManager floorManager;

    // Unity Callbacks

    void Start()
    {
        //Cas Photon

        if (!photonView.isMine)
        {
            //Si ce n'est pas au joueur, se désactive
            spiritCamera.SetActive(false);
            this.enabled = false;
        }
        else
        {
            spiritCamera.SetActive(true);
            if (Camera.main != null)
            {
                Camera.main.enabled = false;
            }
        }

        //Initialisation

        floorManager = GameObject.Find("eCentralManager").GetComponent<FloorManager>();
        transformer = GetComponent<Transform>();
        cameraComp = spiritCamera.GetComponent<Camera>();
        zoomValue = baseZoomValue;
    }
    

    void Update()
    {
        ScrollUpdate();

        ScrollMiddle();


        cameraComp.fieldOfView = Mathf.Lerp(cameraComp.fieldOfView, zoomValue, Time.deltaTime * zoomSpeed);

        Vector3 desiredPosition = new Vector3(transformer.position.x, floorManager.GetFloorLevel() * 12.6f , transformer.position.z);

        transformer.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5.0f);
    }

    //Scroll Update

    private void ScrollUpdate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                ScrollMaj();
            }
            else
            {
                ScrollNormal();
            }
        }
    }
    private void ScrollNormal()
    {
        //Changement de niveau

        if (Input.GetAxis("Mouse ScrollWheel") > 0)//Scroll vers le haut
        {
            floorManager.TryIncrease();
        }
        else
        {
            floorManager.TryDecrease();
        }
    }
    private void ScrollMaj()
    {
        //Zoom de fov

        if (Input.GetAxis("Mouse ScrollWheel") > 0) //Scroll vers le haut
        {
            //On zoom
            zoomValue -= 5.0f; //Reduit le FoV
        }
        else
        {
            //On dezoom
            zoomValue += 5.0f;
        }
        zoomValue = Mathf.Clamp(zoomValue, zoomMin, zoomMax); //Cape le Fov entre zoomMin et zoomMax
    }

    private void ScrollMiddle()
    {
        if (Input.GetMouseButtonDown(2))
        {
            zoomValue = baseZoomValue;
        }
    }

    //Public
}
