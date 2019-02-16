using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritHead : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject spiritCamera;
    public string spiritName ="ERROR"; //Inititialisé lors de l'instantiation de Spirit

    private List<GameObject> selectedList;

    //Unity Callback
    void Start()
    {

        getRightCamera();

        selectedList = new List<GameObject>();
    }

    void Update()
    {
        ClickUpdate();

        TestBtw();
        
    }
    private void TestBtw()
    {
        if (Input.GetKeyUp("space"))
        {
            GameObject go = PhotonNetwork.Instantiate(
                "Chara", 
                new Vector3(gameObject.transform.position.x, 
                            1, 
                            gameObject.transform.position.z), 
                Quaternion.identity, 
                0);

            Debug.Log("Instantiaton WOW de: " + go);

            go.GetComponent<CharaPermissions>().SetGroupMaster(0);
        }
    }

    //Clicking and selecting

    private void ClickUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LeftClickUpdate();
        }
        //if (Input.GetMouseButtonDown(1))
        //{
        //    RightClickUpdate();
        //}
    }

    private void LeftClickUpdate()
    {
        Ray ray = spiritCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
            

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Chara"))
            {
                if (!(Input.GetKey(KeyCode.LeftShift)))
                {
                    DeselectAllExcept(hit.transform.gameObject);
                }
                SelectChara(hit.transform.gameObject);
            }
            //
        }
    }

    private void RightClickUpdate()
    {
        //TODO TEST
        foreach(GameObject go in selectedList)
        {
            //go.GetComponent<CharaHead>().Deselect();
        }
        
        Ray ray = spiritCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == this.name)
            {
                //
            }
        }
        
    }

    //Public methods

    public void SelectChara(GameObject chara)
    {
        Debug.Log("On selectionne chara");
        chara.GetComponent<CharaHead>().LeftClickedOn(spiritName);
        if (!(selectedList.Contains(chara)))
        {
            selectedList.Add(chara);
        }
    }

    public void DeselectChara(GameObject chara)
    {
        //chara.GetComponent<CharaHead>().Deselect();
        selectedList.Remove(chara);
    }

    public void DeselectAll()
    {
        foreach(GameObject chara in selectedList)
        {
            chara.GetComponent<CharaHead>().Deselect();
        }
    }

    public void DeselectAllExcept(GameObject except)
    {
        foreach (GameObject chara in selectedList)
        {
            if (chara != except)
            {
                chara.GetComponent<CharaHead>().Deselect();
            }
        }
    }

    //Private methods

    private void getRightCamera()
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
        }
    }
    
    
}
