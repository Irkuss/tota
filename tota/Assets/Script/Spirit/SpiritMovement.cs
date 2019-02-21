using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMovement : Photon.MonoBehaviour
{
    
    public float cameraSpeed;
    private float cameraSpeedBaseValue = 20.0f;
    [SerializeField]
    private ForceMode fm;
    private Rigidbody rb;

    void Start()
    {
        if (!photonView.isMine)
        {
            this.enabled = false;
        }

        rb = GetComponent<Rigidbody>();

        cameraSpeed = cameraSpeedBaseValue;
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.AddForce(movement * cameraSpeed, fm);
    }
}
