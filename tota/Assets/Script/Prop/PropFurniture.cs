using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFurniture : SalvageHandler
{
    //Defining attribute
    [Header("PropFurniture Attribute")]
    public LootTable lootTable = null;
    public bool hasToRandAddLoot = true;
    public float baseTimeFirstLoot = 0.5f;


    //Reference
    private CharaInventory _furnitureInventory;
    private GameObject _inventoryLayout;
    private GameObject _interfaces;
    private Outline _outline;

    //Private attribute
    private bool _firstInteract = true; //Set to false after a chara interact with this furniture
    private bool _isBeingUsed = false;
    private CharaHead _charaUsing = null; //List of charas using that furniture (used to close itself)
    public CharaHead CharaUsing => _charaUsing; //Used to add item in charaUsing
    
    //Command enum
    public enum FurnitureCommand
    {
        Modify,
        FirstInteract,
        Usage,
    }

    //Start
    private void Start()
    {
        //Call the Init for OrganicOpacity
        BeginOpacity();

        //Set the references
        _furnitureInventory = GetComponent<CharaInventory>();
        _inventoryLayout = GameObject.Find("eCentralManager").GetComponent<CentralManager>().InventoryLayout;
        _interfaces = GameObject.Find("eCentralManager").GetComponent<CentralManager>().InventoryList;
        _outline = GetComponent<Outline>();

        _outline.enabled = false;
    }

    private void RandAddLoot()
    {
        //Genere les items dans ce furniture (appelé une seule fois par meuble placé naturellement)
        Item[] itemToAdd = lootTable.GetChosenPropsArray();
        for (int i = 0; i < itemToAdd.Length; i++)
        {
            _furnitureInventory.Add(itemToAdd[i]);
        }
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex)
    {
        if (actionIndex < 0) InteractAI(chara);

        switch (actionIndex)
        {
            case 0: Open(chara); break; //Open
            case 1: Salvage(chara); break;//Salvage
        }
    }
    
    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        if (actionIndex < 0) return CheckAvailabilityAI(chara);

        switch (actionIndex)
        {
            case 0: return !_isBeingUsed; //Open
            case 1: return true; //Salvage
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        if (actionIndex < 0) return GetActionTimeAI(chara);

        switch (actionIndex)
        {
            case 0: return GetOpenTime(chara); //Open
            case 1: return GetSalvageTime(chara); //Salvage
        }
        return 1f;
    }

    //====================AI Interactable====================
    private void InteractAI(CharaHead chara)
    {
        Debug.Log("InteractAI: an Ai is interacting with a furniture TODO");

        CharaInventory charaInv = chara.GetComponent<CharaInventory>();

        foreach(Item itemToSearch in CharaAi.ItemToSearch)
        {
            Debug.Log("InteractAI: searching for " + itemToSearch.nickName);

            while(_furnitureInventory.Contains(itemToSearch))
            {
                Debug.Log("InteractAI: found " + itemToSearch.nickName);
                _furnitureInventory.Remove(itemToSearch);
                charaInv.Add(itemToSearch);
            }
        }
    }
    public bool CheckAvailabilityAI(CharaHead chara)
    {
        //Debug.Log("CheckAvailabilityAI: isBeingUsed: " + _isBeingUsed + " (opposite is " + !_isBeingUsed);
        return !_isBeingUsed; //Open
    }

    public float GetActionTimeAI(CharaHead chara)
    {
        return GetOpenTime(chara); //Open
    }

    //====================Action Method====================
    //Open
    private void Open(CharaHead chara)
    {
        if (!CheckAvailability(chara, 0)) return; //Si au moment d'arriver, le furniture est finalement utilisé annule tout

        //Test Son Open Chest
        AudioManager.instance.Play("Coffre");

        //Marque le furniture comme étant utilisé
        SendToggleUsage(true);
        _charaUsing = chara;

        StartCoroutine(Cor_UpdateClose());
        
        //ouvre l'inventaire
       _interfaces.SetActive(true);

        CharaInventory inv = chara.GetComponent<CharaInventory>();

        if (inv.GetInterface() == null) inv.ToggleInterface(_inventoryLayout, chara.GetComponent<CharaRpg>().GetToolTipInfo());

        _furnitureInventory.ToggleInventory(_inventoryLayout);

        //Generation du loot dans le furniture
        if (_firstInteract && hasToRandAddLoot)
        {
            //Spawn du loot
            RandAddLoot();
            //Train la stat scavenger
            chara.GetComponent<CharaRpg>().TrainStat(CharaRpg.Stat.sk_scavenger, GetActionTime(chara, 0));
            //Indique à tout le monde que cette furniture a été interact
            CommandSend(new int[1] { (int)FurnitureCommand.FirstInteract });
        }
    }
    private void SetFirstInteract()
    {
        //Received after Open
        _firstInteract = false;
    }

    private float GetOpenTime(CharaHead chara)
    {
        if(_firstInteract && hasToRandAddLoot)
        {
            //Temps d'interraction d'une premiere fouille
            return baseTimeFirstLoot * chara.GetComponent<CharaRpg>().GetTimeModifier(CharaRpg.Stat.sk_scavenger);
        }
        //Temps d'interraction d'un furniture déja fouillé
        return 0.5f;
    }

    //(Close : Not an action but happens after Open)
    private IEnumerator Cor_UpdateClose()
    {
        _outline.enabled = true;

        while (_charaUsing != null)
        {
            if (_charaUsing.LastInteractedFocus != this)
            {
                _charaUsing = null;
                SendToggleUsage(false);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        //Debug.Log("Cor_UpdateClose: closing inventory");
        _furnitureInventory.CloseInventory();

        _outline.enabled = false;
    }


    private void SendToggleUsage(bool isUsed)
    {
        CommandSend(new int[2] { (int)FurnitureCommand.Usage, isUsed ? 1 : 0 });
    }
    private void ToggleUsage(bool isUsed)
    {
        _isBeingUsed = isUsed;
    }


    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch ((FurnitureCommand) command[0])
        {
            case FurnitureCommand.Modify: _furnitureInventory.ModifyCountWithId(command[1], command[2]); break;
            case FurnitureCommand.FirstInteract: SetFirstInteract(); break;
            case FurnitureCommand.Usage: ToggleUsage(command[1] == 1); break;
        }
    }
}
