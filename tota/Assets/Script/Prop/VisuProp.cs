using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisuProp : PropHandler
{

    //Path for the construction created with this blueprint (from Resources)
    [Header("Construction created with this blueprint (from Resources)")]
    public ItemRecipe recipe;

    private bool hasAllItems = false;

    public enum VisuCommand
    {
        Completed,
    }

    //Basic prop method
    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch(actionIndex)
        {
            case 0: FillNeededItem(chara); break;
            case 1: Construct(chara); break;
            case 2: CancelConstruction(chara); break;
        }
    }
    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return !hasAllItems && recipe.CanBeCraftedWith(chara.GetComponent<CharaInventory>().inventory) && recipe.neededItem.Length != 0;
            case 1: return (hasAllItems || recipe.neededItem.Length == 0) && recipe.CanBeCraftedBySkill(chara.GetComponent<CharaInventory>());
            case 2: return true; //Cancel Construction
        }
        return false;
    }
    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return 1f; //FillNeededItem
            case 1: return recipe.GetCraftTime(chara.GetComponent<CharaInventory>()); //Construct
            case 2: return 1f; //Cancel Construction
        }
        return 0f;
    }
    //Construction method (Interaction)

    public void FillNeededItem(CharaHead chara)
    {
        if (!CheckAvailability(chara, 0)) return; //Si au moment d'arriver, le blueprint est déjà rempli annule tout

        Debug.Log("FillNeededItem: a visu has enough items to be constructed");
        recipe.RemoveNeededItems(chara.GetComponent<CharaInventory>());
        CommandSend(new int[1] { (int)VisuCommand.Completed });
    }

    public void Construct(CharaHead chara)
    {
        Debug.Log("Construct: a visu has been built");
        //Crée un nouveau prop
        GameObject.Find("eCentralManager").
            GetComponent<PropManager>().
            PlaceProp(transform.position, transform.rotation.eulerAngles.y, recipe.resultPath);
        //Se détruit
        DestroySelf();
    }
    public void CancelConstruction(CharaHead chara)
    {
        if(hasAllItems)
        {
            recipe.Refund(chara.GetComponent<CharaInventory>());
        }

        DestroySelf();
    }
    //Network
    private void MarkAsCompleted()
    {
        hasAllItems = true;
    }

    public override void CommandReceive(int[] command, float[] commandFloat)
    {
        switch ((VisuCommand)command[0])
        {
            case VisuCommand.Completed: MarkAsCompleted(); break;
        }
    }
}
