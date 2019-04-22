﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField] private GameObject _craft = null;
    [SerializeField] private GameObject _slot = null;
    [SerializeField] private GameObject _tooltip = null;
    [SerializeField] private RecipeTable _data = null;
    [SerializeField] private GameObject _equipment = null;
    [SerializeField] private GameObject _injuries = null;

    private GameObject _recipe;

    public GameObject tooltip => _tooltip;

    public void InstantiateCraft(CharaInventory charaInventory)
    {
        foreach (var recipe in _data.recipes)
        {
            _recipe = Instantiate(_slot, _craft.transform.GetChild(0));
            _recipe.transform.GetChild(0).GetComponent<Image>().sprite = recipe.result.icon;
            _recipe.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => ClickCraft(charaInventory,recipe));
            _recipe.transform.GetChild(1).gameObject.SetActive(false);
            _recipe.transform.GetChild(2).GetComponent<Text>().text = recipe.resultCount.ToString();

            if (recipe.type != RecipeTable.RecipeType.Base)
            {
                _recipe.transform.GetChild(3).gameObject.SetActive(true);
            }
        }
    }

    public void ClickCraft(CharaInventory charaInventory,ItemRecipe recipe)
    {
        if (!recipe.CanBeCraftedWith(charaInventory.inventory) || recipe.type != RecipeTable.RecipeType.Base)
        {
            return;
        }
        else
        {
            recipe.CraftWith(charaInventory);
        }
    }

    public void UpdateCraft(CharaInventory charaInventory)
    {
        int i = 0;
        foreach(var recipe in _data.recipes)
        {
            _recipe = _craft.transform.GetChild(0).GetChild(i).gameObject;
            if (!recipe.CanBeCraftedWith(charaInventory.inventory) || recipe.type != RecipeTable.RecipeType.Base)
            {
                _recipe.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                _recipe.transform.GetChild(1).gameObject.SetActive(true);
                _recipe.transform.GetChild(1).GetComponent<Button>().interactable = false;
                _recipe.transform.GetChild(1).GetComponent<Image>().color = Color.green;
            }
            i++;
        }
    }

    public void InstantiateEquipment()
    {
        foreach(Transform child in _equipment.transform)
        {
            child.GetChild(1).gameObject.SetActive(false);
            child.GetChild(2).gameObject.SetActive(false);
        }
    }

    private void Active(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    public void UpdateEquipment(CharaInventory charaInventory)
    {
        if (charaInventory.wearables[0] != null)
        {
            GameObject head = _equipment.transform.GetChild(0).gameObject;
            head.transform.GetChild(0).GetComponent<Image>().sprite = charaInventory.wearables[0].icon;
            head.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[0].Unequip(charaInventory.gameObject));
            head.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[0].description;
            head.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => Active(head.transform.GetChild(4).gameObject));
        }
        else
        {
            GameObject head = _equipment.transform.GetChild(0).gameObject;
            head.transform.GetChild(0).GetComponent<Image>().sprite = null;
            head.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            head.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "";
            head.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.wearables[1] != null)
        {
            GameObject torso = _equipment.transform.GetChild(1).gameObject;
            torso.transform.GetChild(0).GetComponent<Image>().sprite = charaInventory.wearables[1].icon;
            torso.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[1].Unequip(charaInventory.gameObject));
            torso.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[1].description;
            torso.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => Active(torso.transform.GetChild(4).gameObject));
        }
        else
        {
            GameObject torso = _equipment.transform.GetChild(1).gameObject;
            torso.transform.GetChild(0).GetComponent<Image>().sprite = null;
            torso.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            torso.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "";
            torso.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.wearables[2] != null)
        {
            GameObject torso = _equipment.transform.GetChild(2).gameObject;
            torso.transform.GetChild(0).GetComponent<Image>().sprite = charaInventory.wearables[2].icon;
            torso.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[2].Unequip(charaInventory.gameObject));
            torso.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[2].description;
            torso.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => Active(torso.transform.GetChild(4).gameObject));
        }
        else
        {
            GameObject torso = _equipment.transform.GetChild(2).gameObject;
            torso.transform.GetChild(0).GetComponent<Image>().sprite = null;
            torso.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            torso.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "";
            torso.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if(charaInventory.wearables[3] != null)
        {
            GameObject leg = _equipment.transform.GetChild(5).gameObject;
            leg.transform.GetChild(0).GetComponent<Image>().sprite = charaInventory.wearables[3].icon;
            leg.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[3].Unequip(charaInventory.gameObject));
            leg.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[3].description;
            leg.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => Active(leg.transform.GetChild(4).gameObject));
        }
        else
        {
            GameObject leg = _equipment.transform.GetChild(5).gameObject;
            leg.transform.GetChild(0).GetComponent<Image>().sprite = null;
            leg.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            leg.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "";
            leg.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.equipments[0] != null)
        {
            GameObject left = _equipment.transform.GetChild(3).gameObject;
            left.transform.GetChild(0).GetComponent<Image>().sprite = charaInventory.equipments[0].icon;
            left.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => charaInventory.equipments[0].Unequip(charaInventory.gameObject));
            left.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = charaInventory.equipments[0].description;
            left.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => Active(left.transform.GetChild(4).gameObject));
        }
        else
        {
            GameObject left = _equipment.transform.GetChild(3).gameObject;
            left.transform.GetChild(0).GetComponent<Image>().sprite = null;
            left.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            left.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "";
            left.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.equipments[1] != null)
        {
            GameObject right = _equipment.transform.GetChild(4).gameObject;
            right.transform.GetChild(0).GetComponent<Image>().sprite = charaInventory.equipments[1].icon;
            if (charaInventory.equipments[0].equipSpace == Equipable.EquipSpace.TwoHanded)
            {
                right.transform.GetChild(3).gameObject.SetActive(true);
                right.transform.GetChild(0).GetComponent<Button>().interactable = false;
            }
            else
            {
                right.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => charaInventory.equipments[1].Unequip(charaInventory.gameObject));                
            }

            right.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = charaInventory.equipments[1].description;
            right.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => Active(right.transform.GetChild(4).gameObject));

        }
        else
        {
            GameObject right = _equipment.transform.GetChild(4).gameObject;
            right.transform.GetChild(0).GetComponent<Image>().sprite = null;
            right.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            right.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = "";
            right.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void UpdateInjuries(string[] injuries)
    {
        string inj = "Injuries :\n";
        foreach(var str in injuries)
        {
            inj += str + "\n";
        }
        _injuries.GetComponent<Text>().text = inj;
    }
}
