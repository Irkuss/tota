using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Furniture")]
public class Furniture : ScriptableObject
{
    public string Nickname = "New Furniture";
    public bool usable = true;
    public Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    [SerializeField] private GameObject _inventory = null;

    public void Interact()
    {
        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        GameObject go = Instantiate(_inventory);
        go.transform.SetParent(canvas.transform, false);
    }

    public void AddInventory(Item item, int value)
    {
        inventory.Add(item, value);
    }
    
}
