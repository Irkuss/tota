using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotHandler : SalvageHandler
{
    //Defining attribute
    [Header("Pot Attribute")]
    public Item neededSeeds = null;
    public Item resultItem = null;
    public int resultItemCount = 0;
    public GameObject[] plantStates = null;

    //Private attribute
    private static float neededTimeBetweenSteps = 5f; //Maybe will varied between different plants
    
    //Command enum
    public enum PotCommand
    {
        ResetPot,
        PlantSeed,
    }

    //Status
    private IEnumerator _growCor = null;
    private int _currentState = -1;
    private int LastState => plantStates.Length - 1;

    private bool _isReadyToHarvest = false;

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
            case 0: SendPlantSeed(chara); break;//Plant seeds
            case 1: Harvest(chara); break;//Harvest
            case 2: SendReset(); break;//Destroy Plants
            case 3: Salvage(chara); break;//Salvage
        }
    }

    public override bool CheckAvailability(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return _currentState == -1 && chara.GetComponent<CharaInventory>().Contains(neededSeeds);//plant seeds
            case 1: return _isReadyToHarvest;//Harvest
            case 2: return _currentState != -1;//Destroy Plants
            case 3: return true;//Salvage
        }
        return false;
    }

    public override float GetActionTime(CharaHead chara, int actionIndex = 0)
    {
        switch (actionIndex)
        {
            case 0: return 10 * chara.GetComponent<CharaRpg>().GetTimeModifier(CharaRpg.Stat.sk_farmer);//plant seeds
            case 1: return 10 * chara.GetComponent<CharaRpg>().GetTimeModifier(CharaRpg.Stat.sk_farmer);//Harvest
            case 2: return 5;//Destroy Plants
            case 3: return GetSalvageTime(chara);//Salvage
        }
        return 1f;
    }

    //====================Action Method====================
    //Plant seeds
    private void SendPlantSeed(CharaHead chara)
    {
        chara.GetComponent<CharaRpg>().TrainStat(CharaRpg.Stat.sk_farmer, GetActionTime(chara, 0));
        chara.GetComponent<CharaInventory>().Remove(neededSeeds);

        CommandSend(new int[1] { (int)PotCommand.PlantSeed });
    }

    private void PlantSeed()
    {
        //Received
        Debug.Log("PlantSeed: Planting seeds");
        SwitchToState(0);

        _growCor = Cor_GrowingPlants();
        StartCoroutine(_growCor);
    }
    
    //(Plant Growth : Not an action but happens after PlantSeed)
    private IEnumerator Cor_GrowingPlants()
    {
        while (_currentState != LastState)
        {
            yield return new WaitForSeconds(neededTimeBetweenSteps);
            SwitchToState(_currentState + 1);
        }

        _isReadyToHarvest = true;

    }

    private void SwitchToState(int newState)
    {
        //Change l'état de pousse de la plante

        Debug.Log("SwitchToState: switched from " + _currentState + " to " + newState);
        if (_currentState != newState)
        {
            //Desactive le go de l'ancien état
            if (_currentState > -1) plantStates[_currentState].SetActive(false);
            //Active le go du nouveau état
            if (newState > -1) plantStates[newState].SetActive(true);


            _currentState = newState;
        }
    }

    //Harvest
    private void Harvest(CharaHead chara)
    {
        chara.GetComponent<CharaRpg>().TrainStat(CharaRpg.Stat.sk_farmer, GetActionTime(chara, 1));

        for (int i = 0; i < resultItemCount; i++)
        {
            chara.GetComponent<CharaInventory>().Add(resultItem);
        }
        chara.GetComponent<CharaInventory>().Add(neededSeeds);

        SendReset();
    }

    //Destroy Plants
    private void SendReset()
    {
        CommandSend(new int[1] { (int)PotCommand.ResetPot });
    }

    private void ResetPot()
    {
        if (_growCor != null)
        {
            StopCoroutine(_growCor);
        }
        _isReadyToHarvest = false;
        SwitchToState(-1);
    }

    //Salvage
    
    

    //====================Override PropHandler====================
    public override void CommandReceive(int[] command, float[] commandFloat, string[] commandString = null)
    {
        switch((PotCommand)command[0])
        {
            case PotCommand.ResetPot: ResetPot(); break;
            case PotCommand.PlantSeed: PlantSeed(); break;
        }

    }
}
