using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFurniture : PropHandler
{
    //Defining attribute
    [Header("PropFurniture Attribute")]
    public LootTable lootTable = null;
    public bool hasToRandAddLoot = true;
    public float baseTimeFirstLoot = 0.5f;

    //Reference
    private CharaInventory _furnitureInventory;
    private GameObject _inventoryLayout;
    private Outline _outline;

    //Private attribute
    private bool _firstInteract = true; //Set to false after a chara interact with this furniture
    private List<CharaHead> _charasUsing; //List of charas using that furniture (used to close itself)
    private bool isFurnitureInvOpen = false;
    
    //Command enum
    public enum FurnitureCommand
    {
        Modify,
        FirstInteract
    }

    //Start
    private void Start()
    {
        //Call the Init for OrganicOpacity
        BeginOpacity();

        //Set the references
        _furnitureInventory = GetComponent<CharaInventory>();
        _inventoryLayout = GameObject.Find("eCentralManager").GetComponent<CentralManager>().InventoryLayout;
        _outline = GetComponent<Outline>();

        _outline.enabled = false;

        //Init the Private attributes
        _charasUsing = new List<CharaHead>();
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
        switch(actionIndex)
        {
            case 0: Open(chara); break; //Open
        }
    }
    
    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return true; //Open
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch(actionIndex)
        {
            case 0: return GetOpenTime(chara); //Open
        }
        return 1f;
    }
    
    //====================Action Method====================
    //Open
    private void Open(CharaHead chara)
    {
        if (_charasUsing.Contains(chara)) return;
        _charasUsing.Add(chara);
        if (!isFurnitureInvOpen)
        {
            StartCoroutine(Cor_UpdateClose());
        }
        //ouvre l'inventaire
        _inventoryLayout.transform.parent.parent.gameObject.SetActive(true);

        CharaInventory inv = chara.GetComponent<CharaInventory>();
        if (inv.GetInterface() == null) inv.ToggleInterface(_inventoryLayout, chara.GetComponent<CharaRpg>().GetToolTipInfo()); //

        _furnitureInventory.ToggleInventory(_inventoryLayout);

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
        return _firstInteract && hasToRandAddLoot
                    ? baseTimeFirstLoot * chara.GetComponent<CharaRpg>().GetTimeModifier(CharaRpg.Stat.sk_scavenger)
                    : 0.5f;
    }

    //(Close : Not an action but happens after Open)
    private IEnumerator Cor_UpdateClose()
    {
        _outline.enabled = true;

        while (_charasUsing.Count > 0)
        {
            List<CharaHead> charaToRemove = new List<CharaHead>();
            //Choisis les charas qui n'interact plus avec ce meuble
            //Debug.Log("Cor_UpdateClose: there is still " + _charasUsing.Count + " charas left next to this furniture");
            foreach (CharaHead ele in _charasUsing)
            {
                if (ele.LastInteractedFocus != this)
                {
                    //Debug.Log("Cor_UpdateClose: removing that chara");
                    charaToRemove.Add(ele);
                }
            }
            //Les enleve de la liste
            foreach (CharaHead chara in charaToRemove)
            {
                _charasUsing.Remove(chara);
            }
            yield return new WaitForSeconds(0.1f);
        }
        isFurnitureInvOpen = false;
        //Debug.Log("Cor_UpdateClose: closing inventory");
        _furnitureInventory.CloseInventory();

        _outline.enabled = false;
    }

    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch ((FurnitureCommand) command[0])
        {
            case FurnitureCommand.Modify: _furnitureInventory.ModifyCountWithId(command[1], command[2]); break;
            case FurnitureCommand.FirstInteract: SetFirstInteract(); break;
        }
    }
}
