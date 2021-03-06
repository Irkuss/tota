﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpiritMovement : Photon.MonoBehaviour
{
    //Fast Component access
    private Rigidbody rb;

    //Tweakable value (!)
    private float cameraSpeed = 5.0f;
    private float cameraSpeedBase = 5f;
    private float cameraSpeedFast = 15f;
    private ForceMode fm = ForceMode.VelocityChange;

    // Unity Callbacks

    void Start()
    {
        if (!photonView.isMine)
        {
            this.enabled = false;
        }

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Channel.isWriting) return;
        //Fixed update car on travaille avec de la physique (AddForce)
        cameraSpeed = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ? cameraSpeedFast : cameraSpeedBase ;

        Move();
    }

    //Private Move called in FixedUpdate

    private void Move()
    {
        //Deplacement wasd sur le plan x/z
        //NB: le deplacement vertical est géré dans SpiritZoom
        float moveHorizontal;
        float moveVertical;

        if (Mode.Instance.zqsd)
        {
            moveHorizontal = Input.GetAxis("MoveH"); //Fleches horizontal ou 'q' / 'd'
            moveVertical = Input.GetAxis("MoveV"); //Fleches vertical ou 'z' / 's'
        }
        else
        {
            moveHorizontal = Input.GetAxis("Horizontal"); //Fleches horizontal ou 'a' / 'd'
            moveVertical = Input.GetAxis("Vertical"); //Fleches vertical ou 'w' / 's'
        }

        if(moveHorizontal != 0 || moveVertical != 0)
        {
            if (Mode.Instance.firstTime == 0 && !Mode.Instance.isSkip)
            {
                MoveTuto();
            }

            GameObject _actions = GameObject.Find("eCentralManager").GetComponent<CentralManager>().Actions;
            foreach (Transform child in _actions.transform.GetChild(0).GetChild(0))
            {
                Destroy(child.gameObject);
            }
            _actions.SetActive(false);
        }

        //Vector représentant la direction du mouvement
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        

        rb.AddForce(movement * cameraSpeed, fm);
    }

    private void MoveTuto()
    {
        StartCoroutine(PermissionsManager.Instance.spirit.MoveTuto());
    }
}
