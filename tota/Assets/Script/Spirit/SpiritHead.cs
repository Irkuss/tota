using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpiritHead : Photon.MonoBehaviour
{
    //utilisé pour debugger (à swap avec un scriptable object des que possible)

    private string _charaPath = "CharaMarc";
    [SerializeField] private ItemRecipe bigAppleRecipe = null;
    [SerializeField] private ItemTable itemTable = null;

    //[SerializeField] private GameObject _spiritCamera = null;
    private Camera _spiritCamera = null;

    private GameObject _inventoryList;
    private GameObject _inventoryLayout;
    private GameObject _charaLayout;
    private GameObject _chara;
    private GameObject _channel;
    private GameObject _build;
    private GameObject _actions;
    private GameObject _button;
    private GameObject _tutos;
    private GameObject _tuto;
    private RecipeTable _visuData;
    private GameObject _charaRef;

    private Mode mode;

    //Le joueur qui contrôle ce Spirit (ne change pas)
    private PermissionsManager _permission = PermissionsManager.Instance;
    private PermissionsManager.Player _playerOwner = null;

    //Liste des Chara selectionnées
    private static List<GameObject> _selectedList;
    public static List<GameObject> SelectedList => _selectedList;

    private void Awake()
    {
        if (!photonView.isMine)
        {

            this.enabled = false;
        }
        else
        {
            PermissionsManager.Instance.spirit = this;
        }

        _selectedList = new List<GameObject>();
        CentralManager eManager = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
        _charaLayout = eManager.CharaLayout;
        _inventoryLayout = eManager.InventoryLayout;
        _inventoryList = eManager.InventoryList;
        _channel = eManager.Channels;
        _build = eManager.Build;
        _actions = eManager.Actions;
        _button = eManager.Button;
        _tuto = eManager.Tuto;
        _tutos = _tuto.transform.parent.gameObject;
        _visuData = eManager.VisuData;
        _charaRef = Resources.Load<GameObject>("CharaRef");
        
        mode = Mode.Instance;

        _tuto.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
        _tuto.transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();

        _tuto.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => _tuto.SetActive(false));
        _tuto.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate
        {
            mode.isSkip = true;
            _tutos.SetActive(false);
        });

        if (!mode.online && !mode.isSkip)
        {
            StartCoroutine(FirstStepTuto());
        }

        InitiateBuild();
    }

    private IEnumerator FirstStepTuto()
    {
        yield return new WaitForSeconds(1f);

        _tutos.SetActive(true);
        _tuto.transform.GetChild(0).GetComponent<Text>().text = "Now you can move yourself using the wqsd keys or the directional arrows";
    }
    //Unity Callback
    void Start()
    {
        _spiritCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (Channel.isWriting) return;

        if (!_isInBuildMode)
        {
            //Normal Right Left click check
            ClickUpdate();
            DoubleClickUpdate();
        }

        //Do all test functions
        TestAll();

        //Keycode.E Check
        InventoryUpdate();

        DisplayChannel();
    }
    //TestFunction
    private void TestAll()
    {
        //Space Bar check
        TestCharaSpawn(false);

        //Keycode.I check
        TestInventoryAdd();

        TestBuildInput();
    }

    public void TryCharaSpawn(bool force)
    {
        TestCharaSpawn(force);
    }

    private void TestCharaSpawn(bool force)
    {
        if (Input.GetKeyUp("space") || force)
        {
            //Projection des positions sur le sol
            Vector3 lowPosition = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);

            if (Input.GetKey(KeyCode.R))
            {
                if (PhotonNetwork.offlineMode)
                {
                    Instantiate(Resources.Load("Prop/ratProp"), lowPosition, Quaternion.identity);
                }
                else
                {
                    PhotonNetwork.Instantiate("Prop/ratProp", lowPosition, Quaternion.identity, 0);
                }
                return;
            }

            bool setToAI = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            string teamOfNewChara = setToAI ? "" : _playerOwner.MyTeamName;

            if (setToAI) Debug.Log("TestCharaSpawn: spawning AI chara");


            GameObject.Find("eCentralManager").GetComponent<CharaManager>().SpawnChara(lowPosition, teamOfNewChara, _playerOwner.Name);

            if (mode.firstTime == 2)
            {
                StartCoroutine(SpawnTuto());

            }
        }
    }

    private IEnumerator SpawnTuto()
    {
        yield return new WaitForSeconds(1f);

        _tutos.SetActive(true);
        _tuto.transform.GetChild(0).GetComponent<Text>().text = "Nice, you have created a new character. You can select him by clicking left on him.";
        mode.firstTime = 3;
    }

    public void SpawnRat()
    {
        Vector3 lowPosition = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);
        Instantiate(Resources.Load("Prop/ratProp"), lowPosition, Quaternion.identity);
    }

    public void SpawnAI()
    {
        Vector3 lowPosition = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);
        GameObject.Find("eCentralManager").GetComponent<CharaManager>().SpawnChara(lowPosition, "", _playerOwner.Name);
    }

    public void InstantiateCharaRef(string playerWhoSent,GameObject chara)
    {
        PermissionsManager.Player player = _permission.GetPlayerWithName(PhotonNetwork.player.NickName);
        PermissionsManager.Team team = _permission.GetTeamWithName(player.MyTeamName);

        if (team.ContainsPlayer(_permission.GetPlayerWithName(playerWhoSent)))
        {
            GameObject charaLayout = Instantiate(_charaRef);
            charaLayout.transform.SetParent(_charaLayout.transform, false);
            charaLayout.GetComponent<LinkChara>().spirit = this;
            charaLayout.GetComponent<LinkChara>().chara = chara;
            charaLayout.GetComponent<LinkChara>().Name.text = chara.GetComponent<CharaRpg>().NameFull;
        }
    }

    public void CharaDie(GameObject chara) 
    {
        foreach(Transform charaRef in _charaLayout.transform)
        {
            if(charaRef.GetComponent<LinkChara>().chara == chara)
            {
                Destroy(charaRef.gameObject);
            }
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

    //BuildSystem
    private bool _isInBuildMode = false;
    public bool IsInBuildMode => _isInBuildMode;
    private IEnumerator _currentBuildModeCor;
    private GameObject _currentBuild = null;

    private void InitiateBuild()
    {
        foreach (ItemRecipe recipe in _visuData.recipes)
        {
            if (recipe.visuSprite == null || recipe.visuPath == "") return;

            GameObject visuSlot = Instantiate(Resources.Load<GameObject>("BuildPref"), _build.transform.GetChild(1).GetChild(0).GetChild(0));
            if (recipe != null) visuSlot.GetComponent<SlotDescription>().description = recipe.description;
            visuSlot.GetComponent<Image>().sprite = recipe.visuSprite;
            visuSlot.GetComponent<Button>().onClick.AddListener(() => EnterBuildMode(recipe.visuPath));
        }
    }

    private void TestBuildInput()
    {
        //Entree et sortie du buildMode
        //if (Input.GetKeyDown(KeyCode.B))
        if(Input.inputString == mode.build)
        {
            if (_build.activeSelf)
            {
                if(_isInBuildMode) ForceExitBuildMode();
                _build.SetActive(false);
            }
            else
            {
                //EnterBuildMode("testWall");
                _build.SetActive(true);
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
    public const float c_doubleClickDelay = 0.5f;
    private bool _isReadyToDoubleRightClick = false;
    private float _timeWhenDoubleRightClick;

    private void ClickUpdate()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0)) LeftClickUpdate();
            if (Input.GetMouseButtonDown(1)) RightClickUpdate();
        }
    }
    private void DoubleClickUpdate()
    {
        if(_isReadyToDoubleRightClick)
        {
            if (Time.time - _timeWhenDoubleRightClick > c_doubleClickDelay)
            {
                //Debug.Log("DoubleClickUpdate: double right click expired");
                _isReadyToDoubleRightClick = false;
            }
        }
    }

    private bool DEPRE_ClickedOnSomething(out RaycastHit hit)
    {
        //Useful function for the rest of SpiritHead
        Ray ray = _spiritCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return true;
        }
        Debug.Log("ClickedOnSomething: Unexpected spiritRaycast hit nothing");
        return false;
    }

    private bool ClickedOnSomething(out RaycastHit hit)
    {
        //Debug.Log("==========ClickedOnSomething: Starting new Raycast==========");
        //VER2_iterativeCasting_ClickedOnSomething
        
        
        Ray ray = _spiritCamera.ScreenPointToRay(Input.mousePosition);

        Vector3 direction = ray.direction; //Will not change as the raycast goes through unvalid hits
        Vector3 origin = ray.origin; //Will update after hitting an unvalid hit

        bool hitSomethingValid = false;

        int maxStep = 20;
        int step = 0;
        //Debug.Log("ClickedOnSomething: Starting direction is " + direction + ", starting origin is " + origin);
        while (!hitSomethingValid && step < maxStep)
        {
            //Color debugColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            //Debug.DrawRay(origin, direction * 30, debugColor, 3f);
            if (Physics.Raycast(origin, direction, out RaycastHit possibleHit))
            {
                if(IsHitValid(possibleHit))
                {
                    //Debug.Log("ClickedOnSomething: clicked on semething valid - Step done " + step);
                    //Si on a cliqué sur un mur, on vérifie s'il n'y pas une porte plus bas
                    if(possibleHit.transform.GetComponent<FloorOpacity>() != null)
                    {
                        //Debug.Log(("ClickedOnSomething: Clicked on wall, checking below for any possible door"));
                        //Debug.DrawRay(possibleHit.point, Vector3.down * 3.95f, debugColor, 3);
                        if (Physics.Raycast(possibleHit.point, Vector3.down, out RaycastHit possibleDoorHit, 3.95f))
                        {
                            //Debug.Log(("ClickedOnSomething: found something below"));
                            if (possibleDoorHit.transform.GetComponent<DoorHandler>() != null)
                            {
                                //Debug.Log(("ClickedOnSomething: found a door below! returning the door"));
                                hit = possibleDoorHit;
                                return true;
                            }
                        }
                    }
                    hit = possibleHit;
                    return true;
                }
                else
                {
                    //Debug.Log("ClickedOnSomething: Cycling from " + origin + " to " + possibleHit.point);
                    origin = possibleHit.point + direction * 0.02f;
                }
            }
            else
            {
                break;
            }
            step++;
        }
        if (step == maxStep) Debug.LogWarning("ClickedOnSomething: Unexpected reached maxStep ! ");
        Debug.LogWarning("ClickedOnSomething: Unexpected spiritRaycast hit nothing");
        hit = new RaycastHit();
        return false;
    }
    public bool IsHitValid(RaycastHit hit, bool searchForFloorOpacityOnly = false)
    {
        int currentFloorLevel = GetComponent<SpiritZoom>().FloorManagerRef.FloorLevel;


        GeneralOpacity generalOpacity = hit.transform.GetComponent<GeneralOpacity>();
        if (generalOpacity != null)
        {
            if(searchForFloorOpacityOnly && generalOpacity.GetComponent<FloorOpacity>() == null)
            {
                return false;
            }

            if(generalOpacity.IsBelowFloor(currentFloorLevel))
            //if(generalOpacity.CurrentFloorLevel <= currentFloorLevel)
            {

                //Debug.Log("IsHitValid: Returning hit with name " + hit.transform.name + " at floor " + currentFloorLevel);
                return true;
            }
            //Debug.Log("IsHitValid: Cycled through '" + hit.transform.name);
            return false;
        }
        //Debug.Log("IsHitValid: Returning hit wihtout general opacity (" + hit.transform.name + ")");
        return true;
    }

    private void LeftClickUpdate()
    {
        RaycastHit hit;
        foreach (Transform child in _actions.transform.GetChild(0).GetChild(0))
        {
            Destroy(child.gameObject);
        }
        _actions.SetActive(false);

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

                if(mode.firstTime == 3 && !mode.isSkip)
                {
                    StartCoroutine(MoveCharaTuto());                    
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

    private IEnumerator MoveCharaTuto()
    {
        yield return new WaitForSeconds(1f);

        _tutos.SetActive(true);
        _tuto.transform.GetChild(0).GetComponent<Text>().text = "You can also move your character. Once you have selected him you can right click on a position and your character will walk until this point";
        mode.firstTime = 4;
    }

    private void RightClickUpdate()
    {
        RaycastHit hit;
        if (ClickedOnSomething(out hit))
        {
            //Debug.Log("RightClickUpdate: clicked on something");
            Interactable inter = hit.collider.GetComponent<Interactable>();

            //Debug.Log("RightClickUpdate: clicking on " + hit.transform.name);
            if (inter != null)
            {
                GeneralActionHandler(inter);
            }
            else
            {
                //Debug.Log("RightClickUpdate: removing focus, pointing a destination to charas");
                foreach (Transform child in _actions.transform.GetChild(0).GetChild(0))
                {
                    Destroy(child.gameObject);
                }
                _actions.SetActive(false);
                RemoveFocusAll();

                if(!_isReadyToDoubleRightClick)
                {
                    //Fait marcher les charas dans le cas ou on click
                    ActionMoveAllTo(hit.point, false);

                    _isReadyToDoubleRightClick = true;
                    _timeWhenDoubleRightClick = Time.time;
                }
                else
                {
                    //Fait courir les charas dans le cas ou on double click droit
                    ActionMoveAllTo(hit.point, true);
                }

                if(mode.firstTime == 4 && !mode.isSkip && _selectedList.Count != 0)
                {
                    StartCoroutine(FinalTuto());
                    
                }
            }
        }

    }

    private IEnumerator FinalTuto()
    {
        yield return new WaitForSeconds(1f);

        _tutos.SetActive(true);
        _tuto.transform.GetChild(0).GetComponent<Text>().text = "WOW some informations about your character appear on the screen. You can see more details by pressing the E key.\n"
            + "Other keys : \n"
            + "Press B to open the build mode.\n"
            + "Press ESCAPE to open the pause menu.";
        mode.firstTime = 5;
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

                chara.GetComponent<CharaRpg>().UpdateToolTip();
                
                chara.GetComponent<CharaInventory>().ToggleInterface(_inventoryLayout, chara.GetComponent<CharaRpg>().GetToolTipInfo());
            }
            else
            {
                Debug.Log("Was remove from selected");
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

    //Charas order to selected chara (Right Click action)

    private void ActionMoveAllTo(Vector3 destination, bool isRunning)
    {
        //Deplace tous les charas selectionnés à une position donnée
        float stopDistance = 0.2f + (_selectedList.Count - 1) * 0.4f; //Temporaire
        foreach (GameObject chara in _selectedList)
        {
            chara.GetComponent<CharaHead>().SetDestination(destination, isRunning);
            chara.GetComponent<CharaHead>().SetStopDistance(stopDistance);
        }
    }

    private void GeneralActionHandler(Interactable inter)
    {
        if (_selectedList.Count == 0) return; //Do nothing if no charas are selected
        if (inter.ActionLength == 0) return; //Do nothing if no interaction exists
        //Processus de décision l'index d'action
        //Ouvre le dropDown Menu
        //Récupére les noms d'actions des strings (l'index du nom correspond à l'index d'action)
        //Vérifie que les actions sont available à tous les charas selectionnés (IsActionIndexAvailableByAll déjà implémenté juste en dessous)
        //Si une action n'est pas available à au moins un Chara selectionné, elle est grisée,
        //sinon elle est disponible
        //Appelle IndexActionHandler avec inter et l'index d'action choisi par le joueur
        foreach (Transform child in _actions.transform.GetChild(0).GetChild(0))
        {
            Destroy(child.gameObject);
        }

        Vector3 vec = new Vector3(inter.transform.position.x, inter.transform.position.y + 3, inter.transform.position.z);
        _actions.transform.position = _spiritCamera.GetComponent<Camera>().WorldToScreenPoint(vec);

        _actions.SetActive(true);
        for (int i = 0; i < inter.ActionLength; i++)
        {
            bool isAvailable = IsActionIndexAvailableByAll(inter, i);

            if (isAvailable || !inter.MakesActionNotAppearWhenUnavailable[i])
            {
                GameObject act = Instantiate(_button, _actions.transform.GetChild(0).GetChild(0));
                act.transform.GetChild(0).GetComponent<Text>().text = inter.PossibleActionNames[i];
                act.GetComponent<Button>().onClick.AddListener(() => Act(inter, act.transform.GetChild(0).GetComponent<Text>().text));

                if (!isAvailable)
                {
                    act.GetComponent<Image>().color = Color.grey;
                    act.GetComponent<Button>().interactable = false;
                }
            }
        }

    }

    public void Act(Interactable inter,string action)
    {
        List<string> actions = new List<string>();
        foreach(string actionName in inter.PossibleActionNames)
        {
            actions.Add(actionName);
        }
        int i = actions.IndexOf(action);


        IndexActionHandler(inter, i);
        foreach (Transform child in _actions.transform.GetChild(0).GetChild(0))
        {
            Destroy(child.gameObject);
        }
        _actions.SetActive(false);
    }


    public bool IsActionIndexAvailableByAll(Interactable inter, int actionIndex)
    {
        foreach (GameObject Chara in _selectedList)
        {
            if(!inter.CheckAvailability(Chara.GetComponent<CharaHead>(),actionIndex))
            {
                return false;
            }
        }
        return true;
    }

    public void IndexActionHandler(Interactable inter, int actionIndex)
    {
        //Debug.Log("IndexActionHandler: Index chosen, giving order to all charas");
        if(inter.IsDistanceAction[actionIndex]) //Si l'action en question est une action à distance (on a déjà verifié qu'elle était available)
        {
            SetInteractAll(inter, actionIndex);
        }
        else
        {
            SetFocusAll(inter, actionIndex);
        }

    }

    private void SetFocusAll(Interactable inter, int actionIndex)
    {
        //Focus tous les charas selectionnés sur un objet interactible
        foreach (GameObject Chara in _selectedList)
        {
            Chara.GetComponent<CharaHead>().SetFocus(inter, actionIndex);
        }
    }
    private void SetInteractAll(Interactable inter, int actionIndex)
    {
        //Fais interragir tous les charas selectionnés sur un objet interactible
        foreach (GameObject Chara in _selectedList)
        {
            inter.Interact(Chara.GetComponent<CharaHead>(), actionIndex);
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
        //if (Input.GetKeyDown(KeyCode.E))
        if (Input.inputString == mode.interfaCe)
        {
            _inventoryList.SetActive(!_inventoryList.activeSelf);            
        }
        if(_inventoryLayout.transform.childCount == 0)
        {
            _inventoryList.SetActive(false);
        }
    }

    public void ForceOpenCraft(GameObject chara, int index)
    {
        chara.GetComponent<CharaInventory>().ToggleInterface(_inventoryLayout, chara.GetComponent<CharaRpg>().GetToolTipInfo());
        GameObject _interface = chara.GetComponent<CharaInventory>().GetInterface();
        if (_interface != null)
        {
            _interface.GetComponent<InterfaceManager>().ForceOpenCraft(index);
            _inventoryList.SetActive(true);
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

        gameObject.transform.position = Vector3.Lerp(start, posxz,1);

    }

    private void DisplayChannel()
    {
        //if (Input.GetKeyDown(KeyCode.C))
        if (Input.inputString == mode.channel)
        {
            _channel.SetActive(!_channel.activeSelf);
        }
    }

}
