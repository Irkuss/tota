using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField] private GameObject _craftLeft = null;
    [SerializeField] private RecipeTable _dataLeft = null;

    [SerializeField] private GameObject _craftMidLeft = null;
    [SerializeField] private RecipeTable _dataMidLeft = null;

    [SerializeField] private GameObject _craftMidRight = null;
    [SerializeField] private RecipeTable _dataMidRight = null;

    [SerializeField] private GameObject _craftRight = null;
    [SerializeField] private RecipeTable _dataRight = null;

    [SerializeField] private GameObject _slot = null;    
    [SerializeField] private GameObject _equipment = null;
    [SerializeField] private GameObject _injuries = null;
    [SerializeField] private GameObject _stats = null;
    [SerializeField] private GameObject _textPref = null;

    [SerializeField] private Sprite _head = null;
    [SerializeField] private Sprite _torso = null;
    [SerializeField] private Sprite _fistL = null;
    [SerializeField] private Sprite _fistR = null;
    [SerializeField] private Sprite _leg = null;

    //public GameObject tooltip => _tooltip;
    public enum SlotIndex
    {
        Item = 1,
        RemoveButton = 2,
        Stack = 3,
        Lock = 4,
        ItemDescription = 5,
        PopDescription = 6,
        MissingInfo = 7
    }

    //Instantiation
    public void InstantiateCraft(CharaInventory charaInventory)
    {
        RecipeTable[] allRecipeTable = new RecipeTable[4] { _dataLeft, _dataMidLeft, _dataMidRight, _dataRight };
        GameObject[] allCraftGo = new GameObject[4] { _craftLeft, _craftMidLeft, _craftMidRight, _craftRight };

        GameObject craftSlot;
        for (int i = 0; i < allRecipeTable.Length; i++)
        {
            
            foreach (ItemRecipe recipe in allRecipeTable[i].recipes)
            {
                craftSlot = Instantiate(_slot, allCraftGo[i].transform.GetChild(0));

                //Associe l'action de craft et l'image de l'item crafté
                craftSlot.transform.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = recipe.result.icon;
                craftSlot.transform.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => ClickCraft(charaInventory, recipe));
                //Desactive le bouton de suppression de l'item dans le slot ainsi que celui de description
                craftSlot.transform.GetChild((int)SlotIndex.RemoveButton).gameObject.SetActive(false);
                craftSlot.transform.GetChild((int)SlotIndex.PopDescription).gameObject.SetActive(false);
                //Lie le compteur du slot au nombre d'item donné par la recipe
                if (recipe.resultCount <= 1)
                {
                    //Desactive quand la recipe donne 1 ou moins item
                    craftSlot.transform.GetChild((int)SlotIndex.Stack).gameObject.SetActive(false);
                }
                else
                {
                    //Sinon lie le resultCount au compteur du slot
                    craftSlot.transform.GetChild((int)SlotIndex.Stack).GetComponent<Text>().text = recipe.resultCount.ToString();
                }
            }
        }
    }
    public void InstantiateEquipment()
    {
        foreach (Transform child in _equipment.transform)
        {
            child.GetChild(2).gameObject.SetActive(false);
            child.GetChild(3).gameObject.SetActive(false);
        }

        _equipment.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = _head;
        _equipment.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = _torso;
        _equipment.transform.GetChild(2).GetChild(1).GetComponent<Image>().sprite = _torso;
        _equipment.transform.GetChild(3).GetChild(1).GetComponent<Image>().sprite = _fistL;
        _equipment.transform.GetChild(4).GetChild(1).GetComponent<Image>().sprite = _fistR;
        _equipment.transform.GetChild(5).GetChild(1).GetComponent<Image>().sprite = _leg;
    }

    //Updating
    public void UpdateCraft(CharaInventory charaInventory)
    {
        RecipeTable[] allRecipeTable = new RecipeTable[4] { _dataLeft, _dataMidLeft, _dataMidRight, _dataRight };
        GameObject[] allCraftGo = new GameObject[4] { _craftLeft, _craftMidLeft, _craftMidRight, _craftRight };

        ItemRecipe recipe;
        string missingMsg;
        GameObject craftSlot;

        for (int j = 0; j < allRecipeTable.Length; j++)
        {
            for (int i = 0; i < allRecipeTable[j].recipes.Length; i++)
            {
                recipe = allRecipeTable[j].recipes[i];
                craftSlot = allCraftGo[j].transform.GetChild(0).GetChild(i).gameObject;

                missingMsg = "";
                if (!recipe.CanBeCraftedWith(charaInventory.inventory)) missingMsg += "Missing Items\n";
                if (!recipe.CanBeCraftedByWorkshop(charaInventory)) missingMsg += "Missing Workshop\n";
                if (!recipe.CanBeCraftedBySkill(charaInventory)) missingMsg += "" + CharaRpg.statToString[recipe.statUsed] + " level (" + recipe.neededStatLevel + ")\n";

                GameObject recipeMissingInfo = craftSlot.transform.GetChild((int)SlotIndex.MissingInfo).gameObject;
                if (missingMsg == "")
                {
                    recipeMissingInfo.SetActive(false);
                    //Met le lock (image grise transparente)
                    craftSlot.transform.GetChild((int)SlotIndex.Lock).gameObject.SetActive(false);
                }
                else
                {
                    recipeMissingInfo.SetActive(true);
                    recipeMissingInfo.GetComponent<Text>().text = missingMsg;
                    //Enleve le lock
                    craftSlot.transform.GetChild((int)SlotIndex.Lock).gameObject.SetActive(true);
                }
            }
        }
    }
    public void UpdateEquipment(CharaInventory charaInventory)
    {
        //Debug.Log("UpdateEquipment: ==================updating equipment==================");
        //====================Partie Wearable====================
        if (charaInventory.wearables[0] != null) LinkWearSlot(_equipment.transform.GetChild(0), charaInventory.wearables[0], charaInventory);
        else UnlinkWearSlot(_equipment.transform.GetChild(0), _head);

        if (charaInventory.wearables[1] != null) LinkWearSlot(_equipment.transform.GetChild(1), charaInventory.wearables[1], charaInventory);
        else UnlinkWearSlot(_equipment.transform.GetChild(1), _torso);

        if (charaInventory.wearables[2] != null) LinkWearSlot(_equipment.transform.GetChild(2), charaInventory.wearables[2], charaInventory);
        else UnlinkWearSlot(_equipment.transform.GetChild(2), _torso);

        if (charaInventory.wearables[3] != null) LinkWearSlot(_equipment.transform.GetChild(5), charaInventory.wearables[3], charaInventory);
        else UnlinkWearSlot(_equipment.transform.GetChild(5), _leg);

        //====================Partie Equipable====================
        bool leftSlotIsFilled = charaInventory.equipments[0] != null;
        bool rightSlotIsFilled = charaInventory.equipments[1] != null;

        //Update le slot de gauche
        if (leftSlotIsFilled) LinkWearSlot(_equipment.transform.GetChild(3), charaInventory.equipments[0], charaInventory);
        else UnlinkWearSlot(_equipment.transform.GetChild(3), _fistL);

        //Update le slot de droit
        Transform transRightSlot = _equipment.transform.GetChild(4);

        if (rightSlotIsFilled) LinkWearSlot(transRightSlot, charaInventory.equipments[1], charaInventory);
        else UnlinkWearSlot(_equipment.transform.GetChild(4).gameObject.transform, _fistR);

        //Desactive le slot de droit si le slot de gauche est une arme à deux mains et si le slot est vide
        bool deactivateRightSlot = (leftSlotIsFilled && charaInventory.equipments[0].equipSpace == Equipable.EquipSpace.TwoHanded);

        transRightSlot.GetChild((int)SlotIndex.Lock).gameObject.SetActive(deactivateRightSlot);
        transRightSlot.GetChild((int)SlotIndex.Item).GetComponent<Button>().interactable = !deactivateRightSlot;

        //Debug.Log("UpdateEquipment: ==================ending updating equipment==================");
    }

    private static void LinkWearSlot(Transform transSlot, Item itemWorn, CharaInventory charaInventory)
    {
        transSlot.GetChild(0).GetComponent<SlotDescription>().description = itemWorn.description;
        transSlot.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = itemWorn.icon;
        transSlot.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
        transSlot.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.AddListener(() => itemWorn.Unequip(charaInventory));
        transSlot.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = itemWorn.description;
        transSlot.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.AddListener(() => ToggleActive(transSlot.GetChild((int)SlotIndex.ItemDescription).gameObject));
    }
    private static void UnlinkWearSlot(Transform transSlot, Sprite newSprite)
    {
        transSlot.GetChild(0).GetComponent<SlotDescription>().description = "";
        transSlot.GetChild((int)SlotIndex.Item).GetComponent<Image>().sprite = newSprite;
        transSlot.GetChild((int)SlotIndex.Item).GetComponent<Button>().onClick.RemoveAllListeners();
        transSlot.GetChild((int)SlotIndex.ItemDescription).GetChild(0).GetComponent<Text>().text = "";
        transSlot.GetChild((int)SlotIndex.PopDescription).GetComponent<Button>().onClick.RemoveAllListeners();
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
        _stats.transform.GetChild(5).GetComponent<Text>().text = "Rest : " + (int)(stats[6] /** 100*/) + " %";
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
    private static void ToggleActive(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    //Opening inventory craft when interacting with workshop
    public void ForceOpenCraft(int index)
    {
        _craftLeft.transform.parent.SetAsLastSibling();
        switch (index)
        {
            case 0:
                _craftLeft.transform.SetAsLastSibling();
                break;
            case 1:
                _craftMidLeft.transform.SetAsLastSibling();
                break;
            case 2:
                _craftMidRight.transform.SetAsLastSibling();
                break;
            case 3:
                _craftRight.transform.SetAsLastSibling();
                break;
        }
    }








}
