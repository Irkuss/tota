using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatInteract : Interactable
{
    public Item ratLoot;

    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch(actionIndex)
        {
            case 0: Catch(chara); break;
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return true;
        }
        return false;
    }

    private void Catch(CharaHead chara)
    {
        chara.GetComponent<CharaInventory>().Add(ratLoot);
        GetComponent<Rat>().KillSelf();
    }
}
