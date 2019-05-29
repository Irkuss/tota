using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalvageHandler : PropHandler
{
    //Defining attribute
    [Header("Salvage attribute")]
    public float salvageTime = 2;
    public Item salvagedItem = null;
    public int salvagedItemCount = 1;

    //Start
    private void Start()
    {
        //Call the Init for OrganicOpacity
        BeginOpacity();
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: Salvage(chara); break;//salvage
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return true;//salvage
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return GetSalvageTime(chara);//salvage
        }
        return 1f;
    }
    
    //====================Action Method====================
    //Salvage
    protected void Salvage(CharaHead chara)
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();

        for (int i = 0; i < salvagedItemCount; i++)
        {
            inv.Add(salvagedItem);
        }

        DestroySelf();
    }

    protected float GetSalvageTime(CharaHead chara)
    {
        return salvageTime;
    }
}
