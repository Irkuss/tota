﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpiritHead : Photon.MonoBehaviour
{
    //utilisé pour debugger (à swap avec un scriptable object des que possible)
    private string _charaPath = "CharaTanguy";
    [SerializeField] private ItemRecipe bigAppleRecipe = null;
    [SerializeField] private ItemTable itemTable = null;

    [SerializeField] private GameObject _spiritCamera = null;

    //Le joueur qui contrôle ce Spirit (ne change pas)
    private PermissionsManager.Player _playerOwner;

    //Liste des Chara selectionnées
    private List<GameObject> _selectedList;

    //Unity Callback
    void Start()
    {
        if (!photonView.isMine)
        {
            
            this.enabled = false;
        }

        _selectedList = new List<GameObject>();
    }

    void Update()
    {
        //Right Left click check
        ClickUpdate();

        //Do all test functions
        TestAll();

        //Keycode.E Check
        InventoryUpdate();
    }

    private void TestAll()
    {
        //Space Bar check
        TestCharaSpawn();

        //Keycode.I check
        TestInventoryAdd();

        //Keycode.O check
        TestCraftBigApple();
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
                go = Instantiate(Resources.Load<GameObject>(_charaPath), lowPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log("SpiritHead: Instantiation du spirit (online)");
                go = PhotonNetwork.Instantiate(_charaPath, lowPosition, Quaternion.identity, 0);
            }
            
            //Met ce Chara dans notre équipe (par RPC)
            go.GetComponent<CharaPermissions>().GetComponent<PhotonView>().RPC("SetTeam", PhotonTargets.AllBuffered, _playerOwner.MyTeamName);
        }
    }
    private void TestInventoryAdd()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Item item = itemTable.GetRandomItem();
            foreach (GameObject Chara in _selectedList)
            {
                Chara.GetComponent<CharaInventory>().Add(item);
            }
        }
    }
    private void TestCraftBigApple()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            foreach (GameObject Chara in _selectedList)
            {
                Debug.Log("TestCraftBigApple: Trying to craft bigApple");
                CharaInventory charaInv = Chara.GetComponent<CharaInventory>();
                if (bigAppleRecipe.CanBeCraftedWith(charaInv.inventory))
                {
                    Debug.Log("TestCraftBigApple: crafted bigApple");
                    bigAppleRecipe.CraftWith(charaInv);
                }
                else
                {
                    Debug.Log("TestCraftBigApple: failed to craft bigApple");
                }
                
            }
        }
    }

    //Permisssions initialisation

    public void InitPermissions(PermissionsManager.Player player)
    {
        _playerOwner = player;
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

    private bool ClickedOnSomething(out RaycastHit hit)
    {
        Ray ray = _spiritCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            return true;
        }
        return false;
    }

    private void LeftClickUpdate()
    {
        RaycastHit hit;

        if (ClickedOnSomething(out hit))
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
        RaycastHit hit;

        if (ClickedOnSomething(out hit))
        {
            if (hit.transform.CompareTag("Interactable"))
            {
                Interactable inter = hit.collider.GetComponent<Interactable>();
                if (inter != null)
                {
                    SetFocusAll(inter);
                }
            }
            else
            {
                RemoveFocusAll();
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
        if (chara.GetComponent<CharaHead>().LeftClickedOn(_playerOwner))
        {
            //Debug.Log("SpiritHead: On a réussi ");
            if (!(_selectedList.Contains(chara)))
            {
                _selectedList.Add(chara);
            }
        }
        else
        {
            //Debug.Log("SpiritHead: On a échoué ");
            if (_selectedList.Contains(chara))
            {
                _selectedList.Remove(chara);
            } 
        }
        
    }

    public void DeselectChara(GameObject chara)
    {
        chara.GetComponent<CharaHead>().Deselect();
        _selectedList.Remove(chara);
    }

    public void DeselectAll()
    {
        foreach(GameObject chara in _selectedList)
        {
            chara.GetComponent<CharaHead>().Deselect();
        }
        _selectedList.Clear();
    }

    public void DeselectAllExcept(GameObject exception)
    {
        foreach (GameObject chara in _selectedList)
        {
            if (chara != exception)
            {
                chara.GetComponent<CharaHead>().Deselect();
            }
        }
        _selectedList.Clear();
        _selectedList.Add(exception);
    }

    //Private methods

    private void ActionMoveAllTo(Vector3 destination)
    {
        float stopDistance = 0.2f + (_selectedList.Count - 1) * 0.4f;


        //Debug.Log("SpiritHead: moving every chara to destination ("+selectedList.Count+")");
        foreach (GameObject Chara in _selectedList)
        {
            //Debug.Log("SpiritHead: moving one Chara");
            Chara.GetComponent<CharaHead>().SetDestination(destination);
            Chara.GetComponent<CharaHead>().SetStopDistance(stopDistance);
        }
    }

    private void SetFocusAll(Interactable inter)
    {
        foreach (GameObject Chara in _selectedList)
        {
            //Debug.Log("SpiritHead: moving one Chara");
            Chara.GetComponent<CharaHead>().SetFocus(inter);
        }
    }

    private void RemoveFocusAll()
    {
        foreach (GameObject Chara in _selectedList)
        {
            //Debug.Log("SpiritHead: moving one Chara");
            Chara.GetComponent<CharaHead>().RemoveFocus();
        }
    }

    //Inventory

    private void InventoryUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //Toggle l'inventaire dans chaque Chara selectionné
            //(l'ouvre s'il est fermé, le ferme s'il est ouvert)
            foreach (GameObject Chara in _selectedList)
            {
                //Chara.GetComponent<Inventory>().DisplayInventory();
                Chara.GetComponent<CharaInventory>().ToggleInventory();
            }
        }
    }

}
