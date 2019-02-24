using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritHead : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject spiritCamera;

    public string spiritName ="ERROR"; //Inititialisé lors de l'instantiation de Spirit
    public int groupIndex = -1; //Inititialisé lors de l'instantiation de Spirit

    private List<GameObject> selectedList;

    //Unity Callback
    void Start()
    {
        if (!photonView.isMine)
        {
            
            this.enabled = false;
        }

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

            Debug.Log("Instantiatoion de: " + go);

            //go.GetComponent<CharaPermissions>().SetGroupMaster(groupIndex)
            go.GetComponent<CharaPermissions>().GetComponent<PhotonView>().RPC("SetGroupMaster",PhotonTargets.All,groupIndex);
        }
    }

    //Clicking and selecting

    private void ClickUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LeftClickUpdate();
        }
        if (Input.GetMouseButtonDown(1))
        {
            RightClickUpdate();
            //BrouillonClicDroit();
        }
    }

    private void LeftClickUpdate()
    {
        Debug.Log("SpiritHead: " + spiritName + " a fait un clic gauche");

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

                ClickOnChara(hit.transform.gameObject);
            }
            else
            {
                DeselectAll();
            }
        }
    }

    private void RightClickUpdate()
    {
        Debug.Log("SpiritHead: " + spiritName + " a fait un clic droit");

        Ray ray = spiritCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Chara"))
            {
                //Action si on clic droit sur un Chara
            }
            else
            {
                ActionMoveAllTo(hit.point);
            }
        }

    }

    //Brouillon RaycastAll

    private void BrouillonClicDroit()
    {
        Ray ray = spiritCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Length > 0)
        {
            RaycastHit hit = GetNextVisibleHit(hits);

            if (hit.transform.CompareTag("Chara"))
            {
                //Action si on clic droit sur un Chara
            }
            else
            {
                ActionMoveAllTo(hit.point);
            }
        }
    }

    private RaycastHit GetNextVisibleHit(RaycastHit[] hits)
    {
        Debug.Log("GetNextVisibleObject: Starting up");
        RaycastHit hit;
        int length = hits.Length;
        int i = 0;
        while(i < length)
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
                    return hit;
                }
                Debug.Log("GetNextVisibleObject: this hit has a renderer which is not enabled");
            }
            i++;
        }
        Debug.Log("GetNextVisibleObject: did not find valid hit");
        return hits[0];
    }

    //Public methods

    public void ClickOnChara(GameObject chara)
    {
        //Debug.Log("SpiritHead: On essaye de selectionné chara");
        if (chara.GetComponent<CharaHead>().LeftClickedOn(spiritName))
        {
            //Debug.Log("SpiritHead: On a réussi ");
            if (!(selectedList.Contains(chara)))
            {
                selectedList.Add(chara);
            }
        }
        else
        {
            //Debug.Log("SpiritHead: On a échoué ");
            if (selectedList.Contains(chara))
            {
                selectedList.Remove(chara);
            } 
        }
        
    }

    public void DeselectChara(GameObject chara)
    {
        chara.GetComponent<CharaHead>().Deselect();
        selectedList.Remove(chara);
    }

    public void DeselectAll()
    {
        foreach(GameObject chara in selectedList)
        {
            chara.GetComponent<CharaHead>().Deselect();
        }
        selectedList.Clear();
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
        selectedList.Clear();
        selectedList.Add(except);
    }

    //Private methods

    private void ActionMoveAllTo(Vector3 destination)
    {
        //Debug.Log("SpiritHead: moving every chara to destination ("+selectedList.Count+")");
        foreach (GameObject Chara in selectedList)
        {
            //Debug.Log("SpiritHead: moving one Chara");
            Chara.GetComponent<CharaHead>().SetDestination(destination);
        }
    }
}
