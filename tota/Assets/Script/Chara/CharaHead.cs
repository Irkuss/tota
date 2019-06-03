using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CharaHead : Photon.PunBehaviour
{
    //Chara Family (Component attaché à Chara)
    private CharaMovement _movement = null;
    private CharaPermissions _permissions = null;
    private CharaOutline _outline = null;
    private CharaAi _aiHead = null;

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
        CentralManager central = GameObject.Find("eCentralManager").GetComponent<CentralManager>();
        GameObject canvas = central.Canvas;

        fillObj = Instantiate(_fill, canvas.transform);
        fillObj.SetActive(false);
    }
    private void Start()
    {
        _movement = GetComponent<CharaMovement>();
        _permissions = GetComponent<CharaPermissions>();
        _outline = GetComponent<CharaOutline>();
        _aiHead = GetComponent<CharaAi>();

        _baseStoppingDistance = _movement.AgentStoppingDistance;

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
        if(_permissions.Team == null && PhotonNetwork.isMasterClient)
        {
            _aiHead.UpdateAi();
        }


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
            if (!_permissions.HasOwner)
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
    private IEnumerator _cor_interaction = null;
    private IEnumerator _cor_craftingItem = null;
    public ItemRecipe recipeBeingCrafted = null;
    public bool isCraftingItem => _cor_craftingItem != null;
    private IEnumerator _cor_useItem = null;
    private IEnumerator _cor_waitAction = null;

    //Focus
    private Interactable _focus;
    private Interactable _lastInteractedFocus = null;
    public Interactable LastInteractedFocus => _lastInteractedFocus;
    public bool IsFree => _cor_interaction == null && _cor_craftingItem == null && _cor_useItem == null;

    public void SetFocus(Interactable inter, int actionIndex)
    {
        RemoveFocus(true, false);

        bool isRunning = _movement.IsRunning;
        _focus = inter;
        //Prise de décision sur la speed and stoping distance of agent
        if (actionIndex >= 0 && _focus.IsDoWhileAction[actionIndex])
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

        _cor_interaction = Cor_Interaction(actionIndex);
        StartCoroutine(_cor_interaction);
    }
    public void CraftItem(ItemRecipe recipe)
    {
        //Called when crafting an item (when the craft is already available)
        RemoveFocus(false);

        _cor_craftingItem = Cor_CraftingItem(recipe);
        StartCoroutine(_cor_craftingItem);
    }
    public void UseItem(Item item)
    {
        //Called by food, equipable, wearable
        RemoveFocus(true);

        _cor_useItem = Cor_UseItem(item);
        StartCoroutine(_cor_useItem);
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
        if (_focus != null || _cor_interaction != null)
        {
            StopCoroutine(_cor_interaction);
            _cor_interaction = null;
            _focus = null;
        }
    }
    private void ForceEndCraft()
    {
        if (_cor_craftingItem != null)
        {
            StopCoroutine(_cor_craftingItem);
            _cor_craftingItem = null;

            recipeBeingCrafted = null;
        }
    }
    private void ForceEndWaitAction()
    {
        if (_cor_waitAction != null)
        {
            StopCoroutine(_cor_waitAction);
            _cor_waitAction = null;
        }
        fillObj.SetActive(false);
        needFill = false;
        fillObj.GetComponent<Image>().fillAmount = 0;
    }

    private IEnumerator Cor_Interaction(int actionIndex)
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
            _cor_waitAction = WaitAction(actionTime);
            yield return StartCoroutine(_cor_waitAction);
        }
        
        //Interragis une fois le waiting time passé
        _lastInteractedFocus = _focus;

        StartCoroutine(LootAtFocus());

        //Remove le focus (avant d'interragir pour permettre de relancer à la fin
        RemoveFocus(false);

        _lastInteractedFocus.Interact(this, actionIndex);
        
        GetComponent<CharaInventory>().UpdateCraft(); //Update les recipe au cas ou on interagis avec un workshop

        if (_lastInteractedFocus != null && actionIndex >= 0 && _lastInteractedFocus.IsDoWhileAction[actionIndex])
        {
            //L'action est à continuer
            yield return new WaitForSeconds(0.4f);
            SetFocus(_lastInteractedFocus, actionIndex);
        }
    }
    private IEnumerator Cor_CraftingItem(ItemRecipe recipe)
    {
        recipeBeingCrafted = recipe;

        float recipeTime = recipe.GetCraftTime(GetComponent<CharaInventory>());
        Debug.Log("Cor_CraftingItem: starting craft of time " + recipeTime);
        _cor_waitAction = WaitAction(recipeTime);

        yield return StartCoroutine(_cor_waitAction);

        recipe.CraftWith(GetComponent<CharaInventory>());
        recipe.UpdateTraining(GetComponent<CharaRpg>(), recipeTime);

        recipeBeingCrafted = null;
        _cor_craftingItem = null;
    }
    private IEnumerator Cor_UseItem(Item item)
    {
        _cor_waitAction = WaitAction(item.GetUseTime());

        yield return StartCoroutine(_cor_waitAction);

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

    private IEnumerator LootAtFocus()
    {
        Debug.Log("LootAtFocus: starting coroutine, " 
            + transform.position + " to " + _focus.transform.position 
            + " (" + (_focus.transform.position - transform.position) + ")");
        Vector3 direction = (_focus.InterTransform.position - transform.position).normalized;

        Debug.Log("LootAtFocus: direction " + direction);

        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        float desiredYRotation = desiredRotation.eulerAngles.y;
        Debug.Log("LootAtFocus: desired Rotation " + desiredYRotation);

        float currentRotation = transform.eulerAngles.y;


        while (!DoorHandler.FloatEqual(transform.eulerAngles.y, desiredYRotation))
        {
            currentRotation = Mathf.LerpAngle(transform.eulerAngles.y, desiredYRotation, 0.5f);

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentRotation, transform.eulerAngles.z);

            yield return null;
        }
        Debug.Log("LootAtFocus: ending coroutine with rotation " + transform.eulerAngles.y + " (aiming for " + desiredYRotation + ")");
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
    public List<DoorHandler> doorsOnPath = null;//Mis a jours depuis CharaMovement quand on décide d'un chemin

    private IEnumerator UpdateForceOpenDoor()
    {
        //Called in Start

        //Ouvre les portes en marchant

        while(true)
        {
            
            //Verifie s'il y a qqchose devant le chara
            Debug.DrawRay(transform.position, transform.forward * 1f);
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit possibleDoorHit, 1f))
            {
                DoorHandler doorHandler = possibleDoorHit.transform.GetComponent<DoorHandler>();
                //Verifie si ce qqchose est une porte
                if (doorHandler != null)
                {
                    Debug.Log("UpdateForceOpenDoor: Close to a door");

                    if(doorsOnPath.Contains(doorHandler))
                    {
                        Debug.Log("UpdateForceOpenDoor: door on path, trying to oopen door");
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
                    else
                    {
                        Debug.Log("UpdateForceOpenDoor: door not on path");
                    }
                }
            }
            
            yield return null;
        }
        
    }
}
