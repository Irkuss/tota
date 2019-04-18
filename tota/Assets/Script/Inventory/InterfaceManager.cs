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

    public void InstantiateCraft()
    {
        foreach (var recipe in _data.recipes)
        {
            _recipe = Instantiate(_slot, _craft.transform.GetChild(0));
            _recipe.transform.GetChild(0).GetComponent<Image>().sprite = recipe.result.icon;
            _recipe.transform.GetChild(2).GetComponent<Text>().text = recipe.resultCount.ToString();
            if (recipe.type != RecipeTable.RecipeType.Base)
            {
                _recipe.transform.GetChild(3).gameObject.SetActive(true);
            }
        }
    }
}
