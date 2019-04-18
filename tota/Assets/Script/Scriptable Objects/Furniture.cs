using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Furniture")]
public class Furniture : ScriptableObject
{
    public string Nickname = "New Furniture";
    public bool usable = true;
    public Item[] inventory;

    [SerializeField] private Dictionary<string, int> _attributes = null;

    //[SerializeField] private GameObject _inventory = null;

    public void Interact(CharaHead chara, GameObject parent, FurnitureManager manager)
    {
        parent.SetActive(true);
        CharaInventory furniture = manager.gameObject.GetComponent<CharaInventory>();
        
        Dictionary<Item, int> _inventory = new Dictionary<Item, int>();
        foreach(Item item in inventory)
        {
            _inventory.Add(item, item.stack);
        }
        furniture.inventory = _inventory;

        furniture.ToggleInventory(parent);

    }

    public void AddInventory(Item item, int value, CharaInventory chara)
    {
        for (int i = 0; i < value; i++)
        {
            chara.Add(item);           
        }
    }
    
}