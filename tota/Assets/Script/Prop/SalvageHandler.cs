using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalvageHandler : PropHandler
{
    //Defining attribute
    [Header("Salvage attribute")]
    public float salvageTime = 2;

    public Item salvagedItem1 = null;
    public int salvagedItemCount1 = 1;

    public Item salvagedItem2 = null;
    public int salvagedItemCount2 = 1;

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

        if(salvagedItem1 != null)
        {
            for (int i = 0; i < salvagedItemCount1; i++)
            {
                inv.Add(salvagedItem1);
            }
        }

        if (salvagedItem2 != null)
        {
            for (int i = 0; i < salvagedItemCount2; i++)
            {
                inv.Add(salvagedItem2);
            }
        }

        DestroySelf();
    }

    protected float GetSalvageTime(CharaHead chara)
    {
        return salvageTime;
    }
}
