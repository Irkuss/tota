using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField] private GameObject _craft = null;
    [SerializeField] private GameObject _slot = null;
    [SerializeField] private GameObject _tooltip = null;
    [SerializeField] private RecipeTable _data = null;
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
}
