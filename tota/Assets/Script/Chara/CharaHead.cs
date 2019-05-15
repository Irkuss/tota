using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CharaHead : Photon.PunBehaviour
{
    //Chara Family (Component attaché à Chara)
    [SerializeField] private CharaMovement _movement = null;
    [SerializeField] private CharaPermissions _permissions = null;
    [SerializeField] private CharaOutline _outline = null;

    //Autre ref
    [SerializeField] private LayerMask _aiActivationLayer;
    private float _baseStoppingDistance;
    //Searched in Start
    private GameObject _eManager;
    private PermissionsManager _permManager;

    [SerializeField] private GameObject _fill;
    private GameObject fillObj;
    private bool needFill;

    //Unity Callbacks
    private void Awake()
    {
        _baseStoppingDistance = _movement.navMeshAgent.stoppingDistance;
        CentralManager central = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
        GameObject canvas = central.Canvas;

        fillObj = Instantiate(_fill, canvas.transform);
        fillObj.SetActive(false);
    }
    private void Start()
    {
        _eManager = GameObject.Find("eCentralManager"); //pas ouf comm methode, mieux vaux avec un tag
        _permManager = PermissionsManager.Instance;

        if(PhotonNetwork.isMasterClient)
        {
            StartCoroutine(CheckForAi());
            StartCoroutine(UpdateForceOpenDoor());
        }
    }

    //Activer les IA
    public const float c_radiusToActivate = 80f;
    
    private IEnumerator CheckForAi()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);

            Collider[] allAi = Physics.OverlapSphere(transform.position, c_radiusToActivate, _aiActivationLayer);
            //Debug.Log("CheckForAi: got " + allAi.Length + " zombies in detection sphere");
            foreach(Collider aiCollider in allAi)
            {
                Zombie zombieComp = aiCollider.GetComponent<Zombie>();
                if(zombieComp != null)
                {
                    //Debug.Log("CheckForAi: Activating a zombie in range");
                    zombieComp.ForceActivate(this);
                }
            }

        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, c_radiusToActivate);
    }

    

    private void Update()
    {
        if (needFill)
        {
            Vector3 vec = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 3, gameObject.transform.position.z);
            fillObj.transform.position = SpiritZoom.cam.WorldToScreenPoint(vec);
        }
    }
    //Clic Gauche

    public bool LeftClickedOn(PermissionsManager.Player playerWhoClickedUs)
    {
        //LeftClickedOn renvoie true, si le Spirit a réussi a slectionné Chara, false sinon
        //NB: Il renvoie aussi false lors de la deselection

        if (playerWhoClickedUs == null || gameObject.GetComponent<CharaRpg>().IsDead) return false;

        //Debug.Log("Chara: I have been clicked by "+ playerWhoClickedUs.Name);

        PermissionsManager.Team team = _permissions.GetTeam();
        if (team == null) return false;

        if (team.ContainsPlayer(playerWhoClickedUs))
        {
            //Si le joueur qui a cliqué sur Chara appartient à notre équipe
            if (!_permissions.HasOwner())
            {
                //Si personne controle Chara, le joueur prend controle de Chara
                //Debug.Log("Chara: now controlled by "+ playerWhoClickedUs.Name + " (was empty)");

                SelectAsPlayer(playerWhoClickedUs);
                return true;
            }
            else if (_permissions.IsOwner(playerWhoClickedUs.Name))
            {
                //Si le joueur qui nous controle a cliqué sur Chara -> deselect
                //Debug.Log("Chara:deselected by " + playerWhoClickedUs.Name);
                Deselect();
                return false;
                
            }
            else
            {
                //Si un joueur de notre équipe controle deja Chara, personne ne peut l'override
                return false;
            }
        }
        else
        {
            //Si le joueur qui a cliqué sur Chara n'appartient pas à notre équipe
            //Debug.Log("Chara: Access Denied: " + playerWhoClickedUs.Name + " is not in team " + _permissions.GetTeam().Name);
            return false;
        }
    }

    private void SelectAsPlayer(PermissionsManager.Player player)
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOwner, null, new string[1]{ player.Name}, null);
        //_permissions.GetComponent<PhotonView>().RPC("SetOwner", PhotonTargets.AllBuffered, player.Name);


        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOutlineToSelected, null, null, null);
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.ChangeColorTo, player.LinkedColor, null, null);
        //_outline.GetComponent<PhotonView>().RPC("SetOutlineToSelected", PhotonTargets.AllBuffered);
        //_outline.GetComponent<PhotonView>().RPC("ChangeColorTo", PhotonTargets.AllBuffered, player.LinkedColor);
    }

    public void Deselect()
    {
        //Appelé par SpiritHead (par une des 3 fonctions Deselect: DeselectChara(), DeselectAll(), DeselectAllExcept())
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOwnerNull, null, null, null);
        //_permissions.GetComponent<PhotonView>().RPC("SetOwnerNull", PhotonTargets.AllBuffered);

        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.SetOutlineToNotSelected, null, null, null);
        //_outline.GetComponent<PhotonView>().RPC("SetOutlineToNotSelected", PhotonTargets.AllBuffered);

        //NB: removed "if (!EventSystem.current.IsPointerOverGameObject())" and moved it to SpiritHead
        //RemoveInventoryOnDeselected(this.gameObject);
        CloseInventoryOnDeselected();
        GameObject.Find("eCentralManager").GetComponent<CentralManager>().DeactivateToolTip();
    }

    //Clic Droit

    public void RightClickedOn(string spiritName)
    {

    }

    public void SetDestination(Vector3 destination, bool isRunning)
    {
        _movement.MoveTo(destination, isRunning);
    }

    public void SetStopDistance(float newStop)
    {
        _movement.SetStoppingDistance(newStop);
    }

    private void CloseInventoryOnDeselected()
    {
        GetComponent<CharaInventory>().CloseInterface();
        //Debug.Log("CharaHead: closing inventory of a deselected Chara");
    }

    //Focus on Interactable
    private IEnumerator _checkFocusDistanceCor = null;
    private IEnumerator _craftingItemCor = null;
    private IEnumerator _useItemCor = null;
    private IEnumerator _waitActionCor = null;

    //Focus
    private Interactable _focus;
    private Interactable _lastInteractedFocus = null;
    public Interactable LastInteractedFocus => _lastInteractedFocus;
    

    public void SetFocus(Interactable inter, int actionIndex)
    {
        RemoveFocus(true, false);

        bool isRunning = _movement.IsRunning;
        _focus = inter;
        //Prise de décision sur la speed and stoping distance of agent
        if (_focus.IsDoWhileAction[actionIndex])
        {
            SetStopDistance(_focus.Radius * 2f / 3f);

            Debug.Log("SetFocus: is a do while action");
            CharaMovement movementCompOfFocus = _focus.transform.GetComponent<CharaMovement>();
            if (movementCompOfFocus != null) isRunning = movementCompOfFocus.IsRunning;
        }
        else
        {
            SetStopDistance(_baseStoppingDistance);
        }
        //Starting coroutine
        _movement.MoveToInter(_focus, isRunning);

        _checkFocusDistanceCor = CheckDistanceInter(actionIndex);
        StartCoroutine(_checkFocusDistanceCor);
    }
    public void CraftItem(ItemRecipe recipe)
    {
        //Called when crafting an item (when the craft is already available)
        RemoveFocus(false);

        _craftingItemCor = Cor_CraftingItem(recipe);
        StartCoroutine(_craftingItemCor);
    }
    public void UseItem(Item item)
    {
        //Called by food, equipable, wearable
        RemoveFocus(true);

        _useItemCor = Cor_UseItem(item);
        StartCoroutine(_useItemCor);
    }

    public void RemoveFocus(bool alsoRemoveLastInter = true, bool resetRunning = true)
    {
        //Called when setting a destination or when craftin an item (or when using an item)
        ForceRemoveFocus();
        ForceEndCraft();
        ForceEndWaitAction();

        _movement.StopAgent(resetRunning);
        if (alsoRemoveLastInter) _lastInteractedFocus = null; //tfalse when crafting an item only to keepworkshop busy (or when interacting with a focus oc)

        GetComponent<CharaInventory>().UpdateCraft();
    }
    private void ForceRemoveFocus()
    {
        if (_focus != null || _checkFocusDistanceCor != null)
        {
            StopCoroutine(_checkFocusDistanceCor);
            _checkFocusDistanceCor = null;
            _focus = null;
        }
    }
    private void ForceEndCraft()
    {
        if (_craftingItemCor != null)
        {
            StopCoroutine(_craftingItemCor);
            _craftingItemCor = null;
        }
    }
    private void ForceEndWaitAction()
    {
        if (_waitActionCor != null)
        {
            StopCoroutine(_waitActionCor);
            _waitActionCor = null;
        }
        fillObj.SetActive(false);
        needFill = false;
        fillObj.GetComponent<Image>().fillAmount = 0;
    }

    private IEnumerator CheckDistanceInter(int actionIndex)
    {
        if (_focus == null) Debug.LogWarning("CheckDistanceInter: Unexpected null focus");

        //Verification de la distance par rapport au focus
        while (Vector3.Distance(transform.position, _focus.InterTransform.position) > _focus.Radius * 0.8f)
        {
            if(_focus.isMoving)
            {
                bool isRunning = false;
                CharaMovement movementCompOfFocus = _focus.transform.GetComponent<CharaMovement>();
                if (movementCompOfFocus != null) isRunning = movementCompOfFocus.IsRunning;

                _movement.MoveToInter(_focus, isRunning); //Update les positions si on sait qu'il bouge
            }
            yield return new WaitForSeconds(0.5f);
        }
        //Debug.Log("CharaHead: reached Inter, starting waiting time of " + _focus.GetActionTime(this, actionIndex));

        _movement.StopAgent();

        //Interragis avec l'Interactable une fois proche
        float actionTime = _focus.GetActionTime(this, actionIndex);
        if (actionTime > 0)
        {
            _waitActionCor = WaitAction(actionTime);
            yield return StartCoroutine(_waitActionCor);
        }

        //Interragis une fois le waiting time passé
        _lastInteractedFocus = _focus;
        _focus.Interact(this, actionIndex);
        
        GetComponent<CharaInventory>().UpdateCraft(); //Update les recipe au cas ou on interagis avec un workshop

        if (_focus.IsDoWhileAction[actionIndex])
        {
            //L'action est à continuer
            yield return new WaitForSeconds(0.4f);
            SetFocus(_focus, actionIndex);
        }
        else
        {
            //Fin de l'action
            //Reset le focus
            RemoveFocus(false);
        }
    }
    private IEnumerator Cor_CraftingItem(ItemRecipe recipe)
    {
        float recipeTime = recipe.GetCraftTime(GetComponent<CharaInventory>());
        Debug.Log("Cor_CraftingItem: starting craft of time " + recipeTime);
        _waitActionCor = WaitAction(recipeTime);

        yield return StartCoroutine(_waitActionCor);

        recipe.CraftWith(GetComponent<CharaInventory>());
        recipe.UpdateTraining(GetComponent<CharaRpg>(), recipeTime);
    }
    private IEnumerator Cor_UseItem(Item item)
    {
        _waitActionCor = WaitAction(item.GetUseTime());

        yield return StartCoroutine(_waitActionCor);

        CharaInventory inv = GetComponent<CharaInventory>();

        if (item.UseAsChara(inv))
        {
            inv.Remove(item);
        }

    }
    private IEnumerator WaitAction(float waitingTime)
    {
        Image fill = fillObj.GetComponent<Image>();
        needFill = true;
        fillObj.SetActive(true);

        float startTime = Time.time;
        //Fill
        float progressTime = Time.time - startTime;
        while (progressTime < waitingTime)
        {
            yield return null;
            fill.fillAmount = progressTime / waitingTime;
            progressTime = Time.time - startTime;
        }
        //End Fill
        fillObj.SetActive(false);
        fill.fillAmount = 0;
        needFill = false;
    }

    

    //Special Interact
    public bool TryAddItemToFurniture(Item item)
    {
        if (_lastInteractedFocus != null)
        {
            CharaInventory furnitureInv = _lastInteractedFocus.GetComponent<CharaInventory>();
            if (furnitureInv != null)
            {
                return furnitureInv.Add(item);
            }
        }
        return false;
    }

    //ForceOpenDoor
    private IEnumerator UpdateForceOpenDoor()
    {
        //Called in Start
        while(true)
        {
            //Si on n'a pas ou on n'interragis pas avec une porte
            if ((_focus == null || _focus.GetComponent<DoorHandler>() == null) && (_lastInteractedFocus == null || _lastInteractedFocus.GetComponent<DoorHandler>() == null))
            {
                //Verifie s'il y a qqchose devant le chara
                Debug.DrawRay(transform.position, transform.forward * 1f);
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit possibleDoorHit, 1f))
                {
                    DoorHandler doorHandler = possibleDoorHit.transform.GetComponent<DoorHandler>();
                    //Verifie si ce qqchose est une porte
                    if (doorHandler != null)
                    {
                        if (doorHandler.CanForceOpen(this))
                        {
                            //Ouvre la porte
                            doorHandler.ForceOpen(this);
                        }
                        else
                        {
                            //N'ouvre pas la porte et arrete l'agent
                            _movement.StopAgent();
                            yield return new WaitForSeconds(0.5f);//Important pour ne pas bloquer le chara
                        }
                    }
                }
            }
            yield return null;
        }
        
    }
}
