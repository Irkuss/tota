using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpiritZoom : Photon.MonoBehaviour
{
    //Init Spirit Body
    [SerializeField] private GameObject _spiritCamera = null;
    
    //Fast Component access
    private Camera _cameraComp;
    private Transform _transformer;

    private static Camera _cam;
    public static Camera cam => _cam;

    //Tweakable value (!)
    private float _heightOffset = 10f;

    private float _baseZoomValue = 60.0f;

    private float _zoomSpeed = 10.0f;
    private float _zoomValue; //= baseZoomValue at start

    private float _zoomMin = 45.0f;
    private float _zoomMax = 90.0f;

    //Init Spirit Environment
    private FloorManager _floorManager;

    // Unity Callbacks

    private void Awake()
    {
        _cam = _spiritCamera.GetComponent<Camera>();
    }

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
        Vector3 desiredPosition = new Vector3(_transformer.position.x, _floorManager.GetFloorLevel() * FloorManager.c_chunkHeight + _heightOffset , _transformer.position.z);
        //Gradually go to desired position
        _transformer.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5.0f);
    }

    //Scroll Update

    private void ScrollUpdate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Mode.Instance.firstTime == 1 && !Mode.Instance.isSkip)
            {
                StartCoroutine(ZoomTuto());
            }

            GameObject _actions = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Actions;
            foreach (Transform child in _actions.transform.GetChild(0).GetChild(0))
            {
                Destroy(child.gameObject);
            }
            _actions.SetActive(false);

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

    private IEnumerator ZoomTuto()
    {
        yield return new WaitForSeconds(3f);

        GameObject tuto = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Tuto;
        tuto.SetActive(true);
        tuto.transform.GetChild(0).GetComponent<Text>().text = "Now that you understood the bases of the movements you can really enter into the game : Press space to spawn a character.";
        Mode.Instance.firstTime = 2;
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
