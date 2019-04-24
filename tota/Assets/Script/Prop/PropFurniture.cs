using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFurniture : PropHandler
{
    [Header("PropFurniture Attribute")]
    //Defining attribute
    public LootTable lootTable = null;
    public bool hasToRandAddLoot = true;

    //Private Attribute
    private CharaInventory _furnitureInventory;
    private GameObject _inventoryLayout;
    private Outline _outline;

    private bool firstInteract = true;

    public enum FurnitureCommand
    {
        Add,
        Modify,
        Remove,
    }
    private List<CharaHead> closeCharas;

    //Start
    private void Start()
    {
        closeCharas = new List<CharaHead>();
        _furnitureInventory = GetComponent<CharaInventory>();
        _inventoryLayout = GameObject.Find("eCentralManager").GetComponent<CentralManager>().InventoryLayout;
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    private void RandAddLoot()
    {
        Item[] itemToAdd = lootTable.GetChosenPropsArray(Random.Range(5, 10));
        for (int i = 0; i < itemToAdd.Length; i++)
        {
            _furnitureInventory.Add(itemToAdd[i]);
        }
    }
    
    public override void Interact(CharaHead chara, int actionIndex)
    {
        if (closeCharas.Contains(chara))
        {
            return;
        }
        closeCharas.Add(chara);
        if(!isFurnitureInvOpen)
        {
            StartCoroutine(Cor_UpdateClose());
        }
        //ouvre l'inventaire
        _inventoryLayout.SetActive(true);
        _furnitureInventory.ToggleInventory(_inventoryLayout);
        if (firstInteract && hasToRandAddLoot)
        {
            RandAddLoot();

            firstInteract = false;
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        return true;
    }

    //Close process
    private bool isFurnitureInvOpen = false;

    private IEnumerator Cor_UpdateClose()
    {
        _outline.enabled = true;

        while(closeCharas.Count > 0)
        {
            List<CharaHead> charaToRemove = new List<CharaHead>();

            Debug.Log("Cor_UpdateClose: there is still " + closeCharas.Count + " charas left next to this furniture");
            foreach (CharaHead ele in closeCharas)
            {
                Debug.Log("Cor_UpdateClose: " + Vector3.Distance(_interTransform.position, ele.transform.position) + " is distance between this and a chara");
                if (Vector3.Distance(_interTransform.position, ele.transform.position) > _radius + 1)
                {
                    Debug.Log("Cor_UpdateClose: removing that chara");
                    charaToRemove.Add(ele);
                }
            }

            foreach(CharaHead chara in charaToRemove)
            {
                closeCharas.Remove(chara);
            }
            yield return new WaitForSeconds(0.3f);
        }
        isFurnitureInvOpen = false;
        Debug.Log("Cor_UpdateClose: closing inventory");
        _furnitureInventory.CloseInventory();

        _outline.enabled = false;
    }

    //Receive
    public override void CommandReceive(int[] command, float[] commandFloat)
    {
        switch ((FurnitureCommand) command[0])
        {
            case FurnitureCommand.Add: _furnitureInventory.AddWithId(command[1]); break;
            case FurnitureCommand.Remove: _furnitureInventory.RemoveWithId(command[1]); break;
            case FurnitureCommand.Modify: _furnitureInventory.ModifyCountWithId(command[1], command[2]); break;
        }
    }
}
