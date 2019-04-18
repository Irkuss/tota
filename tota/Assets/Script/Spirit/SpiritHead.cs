using System.Collections;
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
        if (!_isInBuildMode)
        {
            //Normal Right Left click check
            ClickUpdate();
        }

        //Do all test functions
        TestAll();

        //Keycode.E Check
        InventoryUpdate();
    }
    //TestFunction
    private void TestAll()
    {
        //Space Bar check
        TestCharaSpawn(false, null);

        //Keycode.I check
        TestInventoryAdd();

        //Keycode.O check
        TestCraftBigApple();

        TestBuildInput();
    }

    public void TryCharaSpawn(bool force, GameObject charaL)
    {
        TestCharaSpawn(force, charaL);
    }

    private void TestCharaSpawn(bool force, GameObject charaL)
    {
        if (Input.GetKeyUp("space") || force)
        {
            //Projection des positions sur le sol
            Vector3 lowPosition = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);
            GameObject.Find("eCentralManager").GetComponent<CharaManager>().SpawnChara(lowPosition, _playerOwner.MyTeamName);
            /*
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
                gameObject.GetComponent<PhotonView>().RPC("InstantiateCharaRef", PhotonTargets.AllBuffered, _playerOwner.Name);

            }

            //Met ce Chara dans notre équipe (par RPC)
            if (_playerOwner != null)
            {
                go.GetComponent<CharaPermissions>().SetTeam(_playerOwner.MyTeamName);
            }*/

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
            charaLayout.GetComponent<LinkChara>().Name.text = _chara.GetComponent<CharaRpg>().NameFull;
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

    //BuildSystem
    private bool _isInBuildMode = false;
    public bool IsInBuildMode => _isInBuildMode;
    private IEnumerator _currentBuildModeCor;
    private GameObject _currentBuild = null;

    private void TestBuildInput()
    {
        //Entree et sortie du buildMode
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_isInBuildMode)
            {
                ForceExitBuildMode();
            }
            else
            {
                EnterBuildMode("testWall");
            }
        }
    }

    //Public Build, entrée et sortie du buildmode
    public void EnterBuildMode(string buildPath)
    {
        if (_isInBuildMode)
        {
            ForceExitBuildMode();
        }
        //Debut de l'entrée du buildMode
        _isInBuildMode = true;
        //Init le go
        _currentBuild = Instantiate(Resources.Load<GameObject>(buildPath));
        _currentBuild.GetComponent<VisuHandler>().StartVisualisation();
        Vector3 desiredRotation = _currentBuild.transform.rotation.eulerAngles;
        //Init la coroutine
        _currentBuildModeCor = BuildMode(buildPath, desiredRotation);
        StartCoroutine(_currentBuildModeCor);
    }
    public void ForceExitBuildMode()
    {
        //Debut de la sortie du buildMode
        _isInBuildMode = false;
        //Detruit le go
        Destroy(_currentBuild);
        _currentBuild = null;
        //Stop la coroutine
        StopCoroutine(_currentBuildModeCor);
    }
    //Private Build, boucle du buildMode
    private IEnumerator BuildMode(string buildPath, Vector3 desiredRotation)
    {
        while(_isInBuildMode)
        {
            //Deplacement du build
            RaycastHit hit;
            if (ClickedOnSomething(out hit)) _currentBuild.transform.position = hit.point;
            //Rotation du build
            if (Input.GetKeyDown(KeyCode.Alpha1)) desiredRotation = desiredRotation + new Vector3(0, -90, 0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) desiredRotation = desiredRotation + new Vector3(0, 90, 0);
            _currentBuild.transform.rotation = Quaternion.Lerp(_currentBuild.transform.rotation, Quaternion.Euler(desiredRotation), 0.5f);
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                //Clic droit, quitter le buildMode
                if (Input.GetMouseButtonDown(1)) ForceExitBuildMode();
                //Clic gauche, placez le currentBuild
                else if (Input.GetMouseButtonDown(0))
                {
                    _currentBuild.transform.rotation = Quaternion.Euler(desiredRotation);
                    if (_currentBuild.GetComponent<VisuHandler>().IsReadyToPlace())
                    {
                        Debug.Log("BuildMode: Placing object");
                        _currentBuild.GetComponent<VisuHandler>().EndVisualisation();
                        GameObject.Find("eCentralManager").
                            GetComponent<PropManager>().
                            PlaceAlreadyExistingProp(_currentBuild, _currentBuild.transform.rotation.eulerAngles.y, buildPath);
                        //Met fin proprement
                        _isInBuildMode = false;
                        _currentBuild = null;
                    }
                    else
                    {
                        Debug.Log("BuildMode: Failed to place object");
                    }
                }
            }
            yield return null;
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
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0)) LeftClickUpdate();
            if (Input.GetMouseButtonDown(1)) RightClickUpdate();
        }
    }

    private bool ClickedOnSomething(out RaycastHit hit)
    {
        //Useful function for the rest of SpiritHead
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
            //Si on clique gauche sur qqchose
            if (hit.transform.CompareTag("Chara"))
            {
                //Si on clique gauche sur un Chara
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
                //Si on ne clique gauche par sur un Chara
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
            Debug.Log("RightClickUpdate: clicked on something");
            if (hit.transform.CompareTag("Interactable"))
            {
                Interactable inter = hit.collider.GetComponent<Interactable>();
                if (inter != null)
                {
                    Debug.Log("RightClickUpdate: Setting focus");
                    SetFocusAll(inter);
                }
            }
            else
            {
                Debug.Log("RightClickUpdate: removing focus, pointing a destination to charas");
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
                //GameObject.Find("eCentralManager").GetComponent<CentralManager>().UpdateToolTip(chara.GetComponent<CharaRpg>().GetToolTipInfo());
                chara.GetComponent<CharaRpg>().UpdateToolTip();

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

    //Charas order to selected chara

    private void ActionMoveAllTo(Vector3 destination)
    {
        //Deplace tous les charas selectionnés à une position donnée
        float stopDistance = 0.2f + (_selectedList.Count - 1) * 0.4f; //Temporaire
        foreach (GameObject Chara in _selectedList)
        {
            Chara.GetComponent<CharaHead>().SetDestination(destination);
            Chara.GetComponent<CharaHead>().SetStopDistance(stopDistance);
        }
    }

    private void SetFocusAll(Interactable inter)
    {
        //Focus tous les charas selectionnés sur un objet interactible
        foreach (GameObject Chara in _selectedList)
        {
            Chara.GetComponent<CharaHead>().SetFocus(inter);
        }
    }

    private void RemoveFocusAll()
    {
        //Enleve le focus tous les charas selectionnés
        foreach (GameObject Chara in _selectedList)
        {
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
