using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritHead : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject spiritCamera;

    // Start is called before the first frame update
    void Start()
    {
        

        if (photonView.isMine)
        {
            spiritCamera.SetActive(true);
            if (Camera.main != null)
            {
                Camera.main.enabled = false;
            }
        }
        else
        {
            spiritCamera.SetActive(false);
            //GetComponent<SpiritMovement>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
