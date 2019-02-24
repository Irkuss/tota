using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerManager : MonoBehaviour
{
    public GameObject lobbyCamera;
    public GameObject topCamera;
    public float cameraSpeed;
    public ForceMode fm;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        lobbyCamera.SetActive(false);
        // topCamera.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = lobbyCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray,100.0f);

            Debug.Log("We clicked");
            GetNextVisibleObject(hits);
        }
    }

    private void GetNextVisibleObject(RaycastHit[] hits)
    {
        Debug.Log("GetNextVisibleObject: Starting up");
        RaycastHit hit;
        int length = hits.Length;
        int i = 0;
        
        while (i < length)
        {
            Debug.Log("GetNextVisibleObject: Looking " + i + " on " + length);
            hit = hits[i];
            Debug.Log("GetNextVisibleObject: this hit is named " + hit.transform.gameObject.name);
            
            if (hit.transform.gameObject.GetComponent<Renderer>() != null)
            {
                Debug.Log("GetNextVisibleObject: this hit has a renderer");
                
                if (hit.transform.gameObject.GetComponent<Renderer>().enabled)
                {
                    Debug.Log("GetNextVisibleObject: this hit has a renderer which is enabled");
                    return;
                }
                Debug.Log("GetNextVisibleObject: this hit has a renderer which is not enabled");
                
            }

        }
        Debug.Log("GetNextVisibleObject: did not find valid hit");
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
