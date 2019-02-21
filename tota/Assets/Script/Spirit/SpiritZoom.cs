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

    private float zoomSpeed = 10.0f;
    private float zoomValue = 60.0f;

    private float zoomMin = 45.0f;
    private float zoomMax = 110.0f;

    private float movementModifier = 4.0f;

    //private GameObject eManager;
    private FloorManager floorManager;
    

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.isMine)
        {
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

        floorManager = GameObject.Find("eCentralManager").GetComponent<FloorManager>();

        cameraComp = spiritCamera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        ScrollUpdate();

        cameraComp.fieldOfView = Mathf.Lerp(cameraComp.fieldOfView, zoomValue, Time.deltaTime * zoomSpeed);
    }

    private void ScrollUpdate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //Changement de niveau

                if (Input.GetAxis("Mouse ScrollWheel") > 0)//Scroll vers le haut
                {
                    floorManager.TryDecrease();
                }
                else
                {
                    floorManager.TryIncrease();
                }
            }
            else
            {
                //Zoom régulier
                if (Input.GetAxis("Mouse ScrollWheel") > 0) //Scroll vers le haut
                {
                    //On zoom
                    zoomValue -= 5.0f; //Reduit le FoV
                    if (zoomValue < 60.0f && zoomValue == Mathf.Clamp(zoomValue, zoomMin, zoomMax))
                    {
                        movement.cameraSpeed -= movementModifier; //Pour un zoom suffisamment élevé, on diminue la vitesse de la camera
                    }
                }
                else
                {
                    //On dezoom
                    if (zoomValue < 60.0f) //Augmente le FoV
                    {
                        movement.cameraSpeed += movementModifier; //L'inverse, on regagne la vitesse perdu
                    }
                    zoomValue += 5.0f;
                }
                zoomValue = Mathf.Clamp(zoomValue, zoomMin, zoomMax); //Cape le Fov entre zoomMin et zoomMax
            }
        }
    }

    //Public
}
