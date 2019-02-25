using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMovement : Photon.MonoBehaviour
{
    //Fast Component access
    private Rigidbody rb;

    //Tweakable value (!)
    private float cameraSpeed = 20.0f;
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
        //Fixed update car on travaille avec de la physique (AddForce)
        Move();
    }

    //Private Move called in FixedUpdate

    private void Move()
    {
        //Deplacement wasd sur le plan x/z
        //NB: le deplacement vertical est géré dans SpiritZoom
        float moveHorizontal = Input.GetAxis("Horizontal"); //Fleches horizontal ou 'a' / 'd'
        float moveVertical = Input.GetAxis("Vertical"); //Fleches vertical ou 'w' / 's'

        //Vector représentant la direction du mouvement
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        

        rb.AddForce(movement * cameraSpeed, fm);
    }
}
