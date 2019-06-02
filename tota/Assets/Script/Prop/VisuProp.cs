using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisuProp : PropHandler
{

    //Defining attribute
    [Header("Construction created with this blueprint (from Resources)")]
    public ItemRecipe recipe;
    
    //Private attribute
    private bool hasAllItems = false;
    
    //Command enum
    public enum VisuCommand
    {
        Completed,
    }

    //Start
    private void Start()
    {
        //Called the Init for OrganicOpacity
        BeginOpacity();
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch(actionIndex)
        {
            case 0: FillNeededItem(chara); break; //Fill needed items
            case 1: Construct(chara); break; //Construct
            case 2: CancelConstruction(chara); break; //Cancel Construction
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return !hasAllItems && recipe.CanBeCraftedWith(chara.GetComponent<CharaInventory>().inventory) && recipe.neededItem.Length != 0; //Fill needed items
            case 1: return (hasAllItems || recipe.neededItem.Length == 0) && recipe.CanBeCraftedBySkill(chara.GetComponent<CharaInventory>()); //Construct
            case 2: return true; //Cancel Construction
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return 1f; //Fill needed items
            case 1: return recipe.GetCraftTime(chara.GetComponent<CharaInventory>()); //Construct
            case 2: return 1f; //Cancel Construction
        }
        return 0f;
    }

    //====================Action Method====================
    //Fill needed items
    public void FillNeededItem(CharaHead chara)
    {
        if (!CheckAvailability(chara, 0)) return; //Si au moment d'arriver, le blueprint est déjà rempli annule tout

        Debug.Log("FillNeededItem: a visu has enough items to be constructed");
        recipe.RemoveNeededItems(chara.GetComponent<CharaInventory>());
        CommandSend(new int[1] { (int)VisuCommand.Completed });
    }

    private void MarkAsCompleted()
    {
        hasAllItems = true;
    }

    //Construct
    public void Construct(CharaHead chara)
    {
        recipe.UpdateTraining(chara.GetComponent<CharaRpg>(), GetActionTime(chara, 1));
        Debug.Log("Construct: a visu has been built");
        //Crée un nouveau prop
        GameObject.Find("eCentralManager").
            GetComponent<PropManager>().
            PlaceProp(transform.position, transform.rotation.eulerAngles.y, recipe.resultPath);
        //Se détruit
        DestroySelf();
    }
    
    //Cancel Construction
    public void CancelConstruction(CharaHead chara)
    {
        if (hasAllItems)
        {
            recipe.Refund(chara.GetComponent<CharaInventory>());
        }

        DestroySelf();
    }

    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch ((VisuCommand)command[0])
        {
            case VisuCommand.Completed: MarkAsCompleted(); break;
        }
    }
}
