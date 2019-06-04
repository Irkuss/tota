using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirecampHandler : PropHandler
{
    //Defining attribute
    private int fireDuration = CharaRpg.c_secInHour * 2;
    private int _maxFuel = 3;

    [Header("Firecamp attribute")]
    //Reference
    [SerializeField] private GameObject fireGo = null;
    [SerializeField] private Item _fuelItem = null;
    [SerializeField] private Item _rawMeat = null;
    [SerializeField] private Item _cookedMeat = null;

    //Private attribute
    private bool _isLit = false;
    private int _fuelLeft = 0;
    private IEnumerator _cor_fireEnding = null;

    //Command enum
    public enum FireCommand
    {
        Lit,
        Unlit,
    }

    //Start
    private void Start()
    {
        //Call the Init for OrganicOpacity
        BeginOpacity();

        SetFireState(false);
    }

    //====================Override Interactable====================
    public override void Interact(CharaHead chara, int actionIndex)
    {
        switch (actionIndex)
        {
            case 0: Fuel(chara); break; //Start a fire / Add fuel
            case 1: Cook(chara); break; //Cook
            case 2: PutOutFire(); break;//Put out fire
            case 3: DestroyFirecamp(); break;//Destroy firecamp
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return CheckFuel(chara, actionIndex); //Start a fire / Add fuel
            case 1: return CheckCook(chara, actionIndex); //Cook
            case 2: return _isLit; //Put out fire
            case 3: return !_isLit; //Destroy firecamp
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return _isLit ? 1 : 4; //Start a fire 4 / Add fuel
            case 1: return 7; //Cook 7
            case 2: return 1; //Put out fire 1
            case 3: return 2; //Destroy firecamp 2
        }
        return base.GetActionTime(chara, actionIndex);
    }

    //====================Action Method====================
    //Add fuel
    private void Fuel(CharaHead chara)
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();

        if (!inv.Contains(_fuelItem)) return;

        inv.Remove(_fuelItem);

        CommandSend(new int[1] { (int)FireCommand.Lit });
    }
    private void ReceiveFuel()
    {
        if (!_isLit)
        {
            SetFireState(true);

            //Lance le decompte
            _cor_fireEnding = Cor_FireEnding();

            StartCoroutine(_cor_fireEnding);
        }
        else
        {
            _fuelLeft++;
        }
    }

    private bool CheckFuel(CharaHead chara, int actionIndex)
    {
        if(!_isLit)
        {
            _possibleActionNames[actionIndex] = "Start a fire";

            if(chara.GetComponent<CharaInventory>().Contains(_fuelItem))
            {
                //Only possible if chara has needed fuel item (and fire is not lit)
                return true;
            }
        }
        else
        {
            //Add fuel
            _possibleActionNames[actionIndex] = "Add fuel (" + _fuelLeft + "/" + _maxFuel + ")";

            if(_fuelLeft < _maxFuel)
            {
                if (chara.GetComponent<CharaInventory>().Contains(_fuelItem))
                {
                    //Only possible if fuel stock is not fuel and if chara has needed fuel item
                    return true;
                }
            }
        }
        return false;
    }

    //Fire ending
    private IEnumerator Cor_FireEnding()
    {
        while(_fuelLeft >= 0)
        {
            Debug.Log("Cor_FireEnding: starting fire with " + _fuelLeft + " fuel Left");
            yield return new WaitForSeconds(fireDuration);
            _fuelLeft--;
        }

        SetFireState(false);

        _fuelLeft = 0;
    }

    private void SetFireState(bool setToLit)
    {
        //Debug.Log("SetFireState: setting fire state to " + setToLit);

        _isLit = setToLit;
        fireGo.SetActive(setToLit);
    }

    //Cook
    private void Cook(CharaHead chara)
    {
        CharaInventory inv = chara.GetComponent<CharaInventory>();

        if(inv.Contains(_rawMeat))
        {
            inv.Remove(_rawMeat);

            inv.Add(_cookedMeat);
        }
    }

    private bool CheckCook(CharaHead chara, int actionIndex)
    {
        if(_isLit)
        {
            _makesActionNotAppearWhenUnavailable[actionIndex] = false;

            if(chara.GetComponent<CharaInventory>().Contains(_rawMeat))
            {
                return true;
            }
        }
        _makesActionNotAppearWhenUnavailable[actionIndex] = true;

        return false;
    }

    //Put out fire
    private void PutOutFire()
    {
        CommandSend(new int[1] { (int)FireCommand.Unlit });
    }

    private void ReceivePutOutFire()
    {
        SetFireState(false);

        if(_cor_fireEnding != null)
        {
            StopCoroutine(_cor_fireEnding);
        }
    }

    //DestroyFirecamp()
    private void DestroyFirecamp()
    {
        DestroySelf();
    }
    
    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch ((FireCommand)command[0])
        {
            case FireCommand.Lit: ReceiveFuel(); break;
            case FireCommand.Unlit: ReceivePutOutFire(); break;
        }
    }

    //====================Override Organic Opacity====================

    protected override bool ShouldParticleGameObjectBeActive()
    {
        return _isLit;
    }
}
