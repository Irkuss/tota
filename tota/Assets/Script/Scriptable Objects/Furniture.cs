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
        //GameObject go = Instantiate(_inventory);
        //go.transform.SetParent(parent.transform.GetChild(0).GetChild(0), false);
        parent.SetActive(true);
        CharaInventory furniture = manager.gameObject.GetComponent<CharaInventory>();
        furniture.ToggleInventory(parent.transform.GetChild(0).GetChild(0).gameObject);

        foreach(Item item in inventory)
        {
            AddInventory(item, item.stack,furniture);
        }
        
    }

    public void AddInventory(Item item, int value, CharaInventory chara)
    {
        for (int i = 0; i < value; i++)
        {
            //chara.Add(item);           
        }
    }
    
}