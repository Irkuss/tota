using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpiritHead : Photon.MonoBehaviour
{
    //utilisé pour debugger (à swap avec un scriptable object des que possible)
    private string _charaPath = "CharaYayko";
    [SerializeField] private ItemRecipe bigAppleRecipe = null;
    [SerializeField] private ItemTable itemTable = null;

    [SerializeField] private GameObject _spiritCamera = null;

    private GameObject _inventoryList;
    private GameObject _inventoryLayout;
    private GameObject _charaLayout;
    private GameObject _chara;

    //Le joueur qui contrôle ce Spirit (ne change pas)
    private PermissionsManager _permission = PermissionsManager.Instance;
    private PermissionsManager.Player _playerOwner = null;

    //Liste des Chara selectionnées
    private static List<GameObject> _selectedList;
    public static List<GameObject> SelectedList => _selectedList; 

    //Unity Callback
    void Start()
    {
        if (!photonView.isMine)
        {
            
            this.enabled = false;
        }

        _selectedList = new List<GameObject>();
        CentralManager eManager = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
        _charaLayout = eManager.CharaLayout;
        _inventoryLayout = eManager.InventoryLayout;
        _inventoryList = eManager.InventoryList;

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
        TestCharaSpawn(false,null);

        //Keycode.I check
        TestInventoryAdd();

        //Keycode.O check
        TestCraftBigApple();

        TestBuild();
    }

    public void TryCharaSpawn(bool force,GameObject charaL)
    {
        TestCharaSpawn(force,charaL);
    }

    private void TestCharaSpawn(bool force,GameObject charaL)
    {
        if (Input.GetKeyUp("space") || force)
        {
            //Projection des positions sur le sol
            Vector3 lowPosition = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);

            //Instantiation de Chara
            GameObject go;
            if (PhotonNetwork.offlineMode)
            {
                Debug.Log("SpiritHead: Instantiation du spirit (offline)");
                go = Instantiate(Resources.Load<GameObject>(_charaPath), lowPosition, Quaternion.identity);
                GameObject charaLayout = Instantiate(Resources.Load<GameObject>("CharaRef"));
                charaLayout.GetComponent<LinkChara>().spirit = this;
                charaLayout.GetComponent<LinkChara>().chara = go;
            }
            else
            {
                Debug.Log("SpiritHead: Instantiation du spirit (online)");
                go = PhotonNetwork.Instantiate(_charaPath, lowPosition, Quaternion.identity, 0);
                _chara = go;        
                if (_charaLayout == null)
                {
                    _charaLayout = charaL;
                }
                gameObject.GetComponent<PhotonView>().RPC("InstantiateCharaRef",PhotonTargets.AllBuffered, _playerOwner.Name);
                
            }
            
            //Met ce Chara dans notre équipe (par RPC)
            if (_playerOwner != null)
            {
                go.GetComponent<CharaPermissions>().GetComponent<PhotonView>().RPC("SetTeam", PhotonTargets.AllBuffered, _playerOwner.MyTeamName);
            }
            
        }
    }

    [PunRPC]
    private void InstantiateCharaRef(string playerWhoSent)
    {
        //Debug.Log("Send : " + playerWhoSent + "    Receive : " + _playerOwner.Name);
        PermissionsManager.Team team = _permission.GetTeamWithName(_playerOwner.MyTeamName);

        if (team.ContainsPlayer(_permission.GetPlayerWithName(playerWhoSent)))
        {
            GameObject charaLayout = Instantiate(Resources.Load<GameObject>("CharaRef"));
            charaLayout.transform.SetParent(_charaLayout.transform, false);            
            charaLayout.GetComponent<LinkChara>().spirit = this;
            charaLayout.GetComponent<LinkChara>().chara = _chara;
            charaLayout.GetComponent<LinkChara>().Name.text = _chara.GetComponent<CharaRpg>().FullName;
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
    private GameObject currentBuild = null;
    private Vector3 desiredBuildRotation;
    private void TestBuild()
    {
        //Entree et sortie du buildMode
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (currentBuild == null)
            {
                currentBuild = Instantiate(Resources.Load<GameObject>("testWall"));
                desiredBuildRotation = currentBuild.transform.rotation.eulerAngles;
            }
            else
            {
                Destroy(currentBuild);
            }
        }
        //Gestion du buildMode
        if (currentBuild != null)
        {
            //Deplacement du build
            RaycastHit hit;

            if (ClickedOnSomething(out hit))
            {
                currentBuild.transform.position = hit.point;
            }
            //Rotation du build
            if (Input.GetKeyDown(KeyCode.Alpha1)) desiredBuildRotation = desiredBuildRotation + new Vector3(0, -90, 0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) desiredBuildRotation = desiredBuildRotation + new Vector3(0,  90, 0);

            currentBuild.transform.rotation = Quaternion.Lerp(currentBuild.transform.rotation, Quaternion.Euler(desiredBuildRotation), 0.5f);

            //Placer le bâtiment
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentBuild.GetComponent<VisuHandler>().EndVisualisation();
                currentBuild.transform.rotation = Quaternion.Euler(desiredBuildRotation);
                currentBuild = null;
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
                else
                {
                    ClickOnChara(hit.transform.gameObject);
                }
            }
            else
            {
                GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip();
                foreach (Transform trans in _inventoryLayout.transform)
                {
                    Destroy(trans.gameObject);
                }
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
                foreach(Transform trans in _inventoryLayout.transform)
                {
                    Destroy(trans.gameObject);
                }
                ActionMoveAllTo(hit.point);
            }
        }

    }
    
    //Public methods

    public void ClickOnChara(GameObject chara)
    {
        //Debug.Log("SpiritHead: On essaye de selectionné chara");
        if (chara.GetComponent<CharaHead>().LeftClickedOn(_playerOwner))
        {
            Debug.Log("SpiritHead: On a réussi ");
            if (!(_selectedList.Contains(chara)))
            {
                _selectedList.Add(chara);
                GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(chara.GetComponent<CharaRpg>().GetToolTipInfo());
                chara.GetComponent<CharaInventory>().ToggleInterface(_inventoryLayout);
            }
            else
            {
                _selectedList.Remove(chara);
                GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip();
            }
        }
        else
        {
            GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip();

            Debug.Log("SpiritHead: On a échoué ");
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
        ClickOnChara(exception);
    }

    public bool ContainsChara(GameObject chara)
    {
        return _selectedList.Contains(chara);
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
            _inventoryList.SetActive(!_inventoryList.activeSelf);            
        }
        if(_inventoryLayout.transform.childCount == 0)
        {
            _inventoryList.SetActive(false);
        }
    }

    public void MoveCamera(Vector3 pos)
    {
        float speed = 5;
        float startTime = Time.time;
        Vector3 start = new Vector3(gameObject.transform.position.x,
            gameObject.transform.position.y, gameObject.transform.position.z);

        Vector3 posxz = new Vector3(pos.x, gameObject.transform.position.y, pos.z);
        float journeyLength = Vector3.Distance(start, posxz);

        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;

        gameObject.transform.position = Vector3.Lerp(start, posxz,1/2);

    }

}
