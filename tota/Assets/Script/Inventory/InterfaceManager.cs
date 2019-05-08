using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField] private GameObject _craft = null;
    [SerializeField] private GameObject _slot = null;
    [SerializeField] private RecipeTable _data = null;
    [SerializeField] private GameObject _equipment = null;
    [SerializeField] private GameObject _injuries = null;
    [SerializeField] private GameObject _stats = null;
    [SerializeField] private GameObject _textPref = null;

    [SerializeField] private Sprite _head = null;
    [SerializeField] private Sprite _torso = null;
    [SerializeField] private Sprite _fistL = null;
    [SerializeField] private Sprite _fistR = null;
    [SerializeField] private Sprite _leg = null;

    private GameObject _recipe;

    //public GameObject tooltip => _tooltip;
    public enum SlotIndex
    {
        Item = 0,
        RemoveButton = 1,
        Stack = 2,
        Lock = 3,
        ItemDescription = 4,
        PopDescription = 5,
        MissingInfo = 6
    }

    //Instantion
    public void InstantiateCraft(CharaInventory charaInventory)
    {
        foreach (ItemRecipe recipe in _data.recipes)
        {
            //Instantie le slot (meme slot qu'un inventaire)
            _recipe = Instantiate(_slot, _craft.transform.GetChild(0));
            //Associe l'action de craft et l'image de l'item crafté
            _recipe.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = recipe.result.icon;
            _recipe.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => ClickCraft(charaInventory,recipe));
            //Desactive le bouton de suppression de l'item dans le slot ainsi que celui de description
            _recipe.transform.GetChild((int)SlotIndex.RemoveButton).gameObject.SetActive(false);
            _recipe.transform.GetChild((int)SlotIndex.PopDescription).gameObject.SetActive(false);
            //Lie le compteur du slot au nombre d'item donné par la recipe
            if (recipe.resultCount <= 1)
            {
                //Desactive quand la recipe donne 1 ou moins item
                _recipe.transform.GetChild((int)SlotIndex.Stack).gameObject.SetActive(false);
            }
            else
            {
                //Sinon lie le resultCount au compteur du slot
                _recipe.transform.GetChild((int)SlotIndex.Stack).GetComponent<Text>().text = recipe.resultCount.ToString();
            }
        }
    }
    public void InstantiateEquipment()
    {
        foreach (Transform child in _equipment.transform)
        {
            child.GetChild(1).gameObject.SetActive(false);
            child.GetChild(2).gameObject.SetActive(false);
        }

        _equipment.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = _head;
        _equipment.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = _torso;
        _equipment.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = _torso;
        _equipment.transform.GetChild(3).GetChild(0).GetComponent<Image>().sprite = _fistL;
        _equipment.transform.GetChild(4).GetChild(0).GetComponent<Image>().sprite = _fistR;
        _equipment.transform.GetChild(5).GetChild(0).GetComponent<Image>().sprite = _leg;
    }

    //Updating
    public void UpdateCraft(CharaInventory charaInventory)
    {
        ItemRecipe recipe;
        string missingMsg;
        for (int i = 0; i < _data.recipes.Length; i++)
        {
            recipe = _data.recipes[i];
            _recipe = _craft.transform.GetChild(0).GetChild(i).gameObject;

            missingMsg = "";
            if (!recipe.CanBeCraftedWith(charaInventory.inventory)) missingMsg += "Missing Items\n";
            if (!recipe.CanBeCraftedByWorkshop(charaInventory)) missingMsg += "Missing Workshop\n";
            if (!recipe.CanBeCraftedBySkill(charaInventory)) missingMsg += "" + CharaRpg.statToString[recipe.statUsed] + " level (" + recipe.neededStatLevel + ")\n";

            GameObject recipeMissingInfo = _recipe.transform.GetChild((int)SlotIndex.MissingInfo).gameObject;
            if (missingMsg == "")
            {
                recipeMissingInfo.SetActive(false);
                //Met le lock (image grise transparente)
                _recipe.transform.GetChild((int)SlotIndex.Lock).gameObject.SetActive(false);
            }
            else
            {
                recipeMissingInfo.SetActive(true);
                recipeMissingInfo.GetComponent<Text>().text = missingMsg;
                //Enleve le lock
                _recipe.transform.GetChild((int)SlotIndex.Lock).gameObject.SetActive(true);
            }
        }
    }
    public void UpdateEquipment(CharaInventory charaInventory)
    {
        if (charaInventory.wearables[0] != null)
        {
            GameObject head = _equipment.transform.GetChild(0).gameObject;
            head.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = charaInventory.wearables[0].icon;
            head.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[0].Unequip(charaInventory.gameObject));
            head.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[0].description;
            head.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(head.transform.GetChild((int)SlotIndex.ItemDescription).gameObject));
        }
        else
        {
            GameObject head = _equipment.transform.GetChild(0).gameObject;
            head.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = _head;
            head.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
            head.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
            head.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.wearables[1] != null)
        {
            GameObject torso = _equipment.transform.GetChild(1).gameObject;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = charaInventory.wearables[1].icon;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[1].Unequip(charaInventory.gameObject));
            torso.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[1].description;
            torso.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(torso.transform.GetChild((int)SlotIndex.ItemDescription).gameObject));
        }
        else
        {
            GameObject torso = _equipment.transform.GetChild(1).gameObject;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = _torso;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
            torso.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
            torso.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.wearables[2] != null)
        {
            GameObject torso = _equipment.transform.GetChild(2).gameObject;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = charaInventory.wearables[2].icon;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[2].Unequip(charaInventory.gameObject));
            torso.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[2].description;
            torso.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(torso.transform.GetChild((int)SlotIndex.ItemDescription).gameObject));
        }
        else
        {
            GameObject torso = _equipment.transform.GetChild(2).gameObject;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = _torso;
            torso.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
            torso.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
            torso.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.wearables[3] != null)
        {
            GameObject leg = _equipment.transform.GetChild(5).gameObject;
            leg.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = charaInventory.wearables[3].icon;
            leg.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => charaInventory.wearables[3].Unequip(charaInventory.gameObject));
            leg.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = charaInventory.wearables[3].description;
            leg.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(leg.transform.GetChild((int)SlotIndex.ItemDescription).gameObject));
        }
        else
        {
            GameObject leg = _equipment.transform.GetChild(5).gameObject;
            leg.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = _leg;
            leg.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
            leg.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
            leg.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.equipments[0] != null)
        {
            GameObject left = _equipment.transform.GetChild(3).gameObject;
            left.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = charaInventory.equipments[0].icon;
            left.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => charaInventory.equipments[0].Unequip(charaInventory.gameObject));
            left.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = charaInventory.equipments[0].description;
            left.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(left.transform.GetChild((int)SlotIndex.ItemDescription).gameObject));
        }
        else
        {
            GameObject left = _equipment.transform.GetChild(3).gameObject;
            left.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = _fistL;
            left.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
            left.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
            left.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (charaInventory.equipments[1] != null)
        {
            GameObject right = _equipment.transform.GetChild(4).gameObject;
            right.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = charaInventory.equipments[1].icon;
            if (charaInventory.equipments[0].equipSpace == Equipable.EquipSpace.TwoHanded)
            {
                right.transform.GetChild((int)SlotIndex.Lock).gameObject.SetActive(true);
                right.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().interactable = false;
            }
            else
            {
                right.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => charaInventory.equipments[1].Unequip(charaInventory.gameObject));
            }

            right.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = charaInventory.equipments[1].description;
            right.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(right.transform.GetChild((int)SlotIndex.ItemDescription).gameObject));

        }
        else
        {
            GameObject right = _equipment.transform.GetChild(4).gameObject;
            right.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = _fistR;
            right.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
            right.transform.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
            right.transform.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void UpdateInjuries(string[] injuries)
    {
        foreach (Transform child in _injuries.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject inj = Instantiate(_textPref, _injuries.transform);
        inj.GetComponent<Text>().text = "Injuries :";

        foreach (var str in injuries)
        {
            GameObject i = Instantiate(_textPref, _injuries.transform);
            i.GetComponent<Text>().text = str;
        }
    }
    public void UpdateStats(float[] stats)
    {
        _stats.transform.GetChild(0).GetComponent<Text>().text = "Pain : " + (int)(stats[0] * 100) + " %";
        _stats.transform.GetChild(1).GetComponent<Text>().text = "Consciousness : " + (int)(stats[1] * 100) + " %";
        _stats.transform.GetChild(2).GetComponent<Text>().text = "Movement : " + (int)(stats[2] * 100) + " %";
        _stats.transform.GetChild(3).GetComponent<Text>().text = "Manipulation : " + (int)(stats[3] * 100) + " %";
        _stats.transform.GetChild(4).GetComponent<Text>().text = "BloodStock : " + (stats[4] * 100) / stats[5] + " %";
    }

    //Listener (on button call)
    public void ClickCraft(CharaInventory charaInventory,ItemRecipe recipe)
    {
        //Appelé au moment de cliquer sur le bouton de craft
        if (!recipe.CanBeCraftedBy(charaInventory))
        {
            return;
        }
        else
        {
            charaInventory.GetComponent<CharaHead>().CraftItem(recipe);
        }
    }
    private void ToggleActive(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    //Opening inventory craft when interacting with workshop
    public void ForceOpenCraft(int index)
    {
        GameObject parent = _craft.transform.parent.gameObject;
    }

    

    

    

    
}
