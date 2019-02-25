using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritZoom : Photon.MonoBehaviour
{
    //Init Spirit Body
    [SerializeField]
    private GameObject _spiritCamera;
    
    //Fast Component access
    private Camera _cameraComp;
    private Transform _transformer;

    //Tweakable value (!)
    private float _baseZoomValue = 60.0f;

    private float _zoomSpeed = 10.0f;
    private float _zoomValue; //= baseZoomValue at start

    private float _zoomMin = 45.0f;
    private float _zoomMax = 90.0f;

    //Init Spirit Environment
    private FloorManager _floorManager;

    // Unity Callbacks

    void Start()
    {
        //Cas Photon

        if (!photonView.isMine)
        {
            //Si ce n'est pas au joueur, se désactive
            _spiritCamera.SetActive(false);
            this.enabled = false;
        }
        else
        {
            _spiritCamera.SetActive(true);
            if (Camera.main != null)
            {
                Camera.main.enabled = false;
            }
        }

        //Initialisation

        _floorManager = GameObject.Find("eCentralManager").GetComponent<FloorManager>();
        _transformer = GetComponent<Transform>();
        _cameraComp = _spiritCamera.GetComponent<Camera>();
        _zoomValue = _baseZoomValue;
    }
    

    void Update()
    {
        //Update Player Input (Leftclick,rightclick,middleclick)
        ScrollUpdate();

        ScrollMiddle();

        //Gradually go to desired field of view
        _cameraComp.fieldOfView = Mathf.Lerp(_cameraComp.fieldOfView, _zoomValue, Time.deltaTime * _zoomSpeed);

        //Set desired position
        Vector3 desiredPosition = new Vector3(_transformer.position.x, _floorManager.GetFloorLevel() * 12.6f , _transformer.position.z);
        //Gradually go to desired position
        _transformer.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5.0f);
    }

    //Scroll Update

    private void ScrollUpdate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //Field of view Scroll
                ScrollMaj();
            }
            else
            {
                //Position Scroll
                ScrollNormal();
            }
        }
    }
    private void ScrollNormal()
    {
        //Changement de niveau

        if (Input.GetAxis("Mouse ScrollWheel") > 0)//Scroll vers le haut
        {
            _floorManager.TryIncrease();
        }
        else
        {
            _floorManager.TryDecrease();
        }
    }
    private void ScrollMaj()
    {
        //Zoom de fov

        if (Input.GetAxis("Mouse ScrollWheel") > 0) //Scroll vers le haut
        {
            //On zoom
            _zoomValue -= 5.0f; //Reduit le FoV
        }
        else
        {
            //On dezoom
            _zoomValue += 5.0f;
        }
        _zoomValue = Mathf.Clamp(_zoomValue, _zoomMin, _zoomMax); //Cape le Fov entre zoomMin et zoomMax
    }

    private void ScrollMiddle()
    {
        //Middleclick reset le zoom field of view à sa valeur de départ
        if (Input.GetMouseButtonDown(2))
        {
            _zoomValue = _baseZoomValue;
        }
    }

    //Public
}
