using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFurniture : PropHandler
{
    [Header("PropFurniture Attribute")]
    //Defining attribute
    public LootTable lootTable = null;


    //Private Attribute
    private CharaInventory _furnitureInventory;
    private GameObject inventoryLayout;


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
        inventoryLayout = GameObject.Find("eCentralManager").GetComponent<CentralManager>().InventoryLayout;
    }
    
    public override void Interact(CharaHead chara, int actionIndex)
    {
        if(!closeCharas.Contains(chara)) closeCharas.Add(chara);
        if(_cor_UpdateClose == null)
        {
            _cor_UpdateClose = Cor_UpdateClose();
            StartCoroutine(_cor_UpdateClose);
        }
        //ouvre l'inventaire
        inventoryLayout.SetActive(true);
        _furnitureInventory.ToggleInventory(inventoryLayout);
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        return true;
    }

    //Close process
    private IEnumerator _cor_UpdateClose = null;

    private IEnumerator Cor_UpdateClose()
    {
        while(closeCharas.Count > 0)
        {
            foreach(CharaHead ele in closeCharas)
            {
                if(Vector3.Distance(transform.position, ele.transform.position) > 4)
                {
                    closeCharas.Remove(ele);
                }
            }
            yield return new WaitForSeconds(2f);
        }
        _cor_UpdateClose = null;
        _furnitureInventory.CloseInventory();
    }

    //Receive
    public override void CommandReceive(int[] command)
    {
        switch ((FurnitureCommand) command[0])
        {
            case FurnitureCommand.Add: _furnitureInventory.AddWithId(command[1]); break;
            case FurnitureCommand.Remove: _furnitureInventory.RemoveWithId(command[1]); break;
            case FurnitureCommand.Modify: _furnitureInventory.ModifyCountWithId(command[1], command[2]); break;
        }
    }
}
