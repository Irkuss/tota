﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpiritHead : Photon.MonoBehaviour
{
    //utilisé pour debugger (à swap avec un scriptable object des que possible)
    private string charaPath = "CharaYanis";
    [SerializeField]
    private ItemTable itemTable;

    [SerializeField]
    private GameObject spiritCamera;

    //Le joueur qui contrôle ce Spirit (ne change pas)
    private PermissionsManager.Player playerOwner;

    //Liste des Chara selectionnées
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
        //Right Left click check
        ClickUpdate();

        //Space Bar check
        TestCharaSpawn();

        //Keycode.I check
        TestInventoryAdd();

        //Keycode.E Check
        InventoryUpdate();
    }

    private void TestCharaSpawn()
    {
        if (Input.GetKeyUp("space"))
        {
            //Projection des positions sur le sol
            Vector3 lowPosition = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);

            //Instantiation de Chara
            GameObject go;
            if (PhotonNetwork.offlineMode)
            {
                Debug.Log("SpiritHead: Instantiation du spirit (offline)");
                go = Instantiate(Resources.Load<GameObject>(charaPath), lowPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log("SpiritHead: Instantiation du spirit (online)");
                go = PhotonNetwork.Instantiate(charaPath, lowPosition, Quaternion.identity, 0);
            }
            
            //Met ce Chara dans notre équipe (par RPC)
            go.GetComponent<CharaPermissions>().GetComponent<PhotonView>().RPC("SetTeam", PhotonTargets.AllBuffered, playerOwner.MyTeamName);
        }
    }

    private void TestInventoryAdd()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            TestAddToAll(itemTable.GetRandomItem());
        }
    }

    //Permisssions initialisation

    public void InitPermissions(PermissionsManager.Player player)
    {
        playerOwner = player;
    }

    //Clicking and selecting

    private void ClickUpdate()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                LeftClickUpdate();
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RightClickUpdate();
            }
        }
    }

    private void LeftClickUpdate()
    {
        //Debug.Log("SpiritHead: " + spiritName + " a fait un clic gauche");

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
                GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip(); //pas beau
                DeselectAll();
            }
        }
    }

    private void RightClickUpdate()
    {
        //Debug.Log("SpiritHead: " + spiritName + " a fait un clic droit");

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
    
    //Public methods

    public void ClickOnChara(GameObject chara)
    {
        //TEMPORAIRE, update le tooltip
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(chara.GetComponent<CharaRpg>().GetToolTipInfo());

        //Debug.Log("SpiritHead: On essaye de selectionné chara");
        if (chara.GetComponent<CharaHead>().LeftClickedOn(playerOwner))
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

    public void DeselectAllExcept(GameObject exception)
    {
        foreach (GameObject chara in selectedList)
        {
            if (chara != exception)
            {
                chara.GetComponent<CharaHead>().Deselect();
            }
        }
        selectedList.Clear();
        selectedList.Add(exception);
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

    //Inventory

    private void InventoryUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //Toggle l'inventaire dans chaque Chara selectionné
            //(l'ouvre s'il est fermé, le ferme s'il est ouvert)
            foreach (GameObject Chara in selectedList)
            {
                //Chara.GetComponent<Inventory>().DisplayInventory();
                Chara.GetComponent<CharaInventory>().ToggleInventory();
            }
        }
    }

    private void TestAddToAll(Item item)
    {
        foreach (GameObject Chara in selectedList)
        {
            //Chara.GetComponent<Inventory>().DisplayInventory();
            Chara.GetComponent<CharaInventory>().Add(item);
        }
    }

}
