using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDeactivator : MonoBehaviour
{
    //Activor and deactivator
    protected bool _canTakeDecision = false;
    private List<CharaHead> _activatorCharas = new List<CharaHead>();

    public void ForceActivate(CharaHead activatorChara)
    {
        //if (!_canTakeDecision) Debug.Log("ForceActivate: A zombie has been activated");
        //Called by CharaHead in CheckForAi

        if (!_activatorCharas.Contains(activatorChara))
        {
            //ajoute le chara en tant qu'activator
            _activatorCharas.Add(activatorChara);
            _canTakeDecision = true;
        }
    }
    protected void CheckDeactivate()
    {
        List<CharaHead> activatorToRemove = new List<CharaHead>();
        //Verifie la distance de chaque activateur
        foreach (CharaHead activatorChara in _activatorCharas)
        {
            if (activatorChara == null || Vector3.Distance(transform.position, activatorChara.transform.position) > CharaHead.c_radiusToActivate)
            {
                activatorToRemove.Add(activatorChara);
            }
        }
        //Supprimes les charas eloignées des activateurs
        foreach (CharaHead deactivator in activatorToRemove)
        {
            _activatorCharas.Remove(deactivator);
        }
        //End condition
        if (_activatorCharas.Count == 0)
        {
            //Debug.Log("CheckDeactivate: A zombie has been deactivated");
            _canTakeDecision = false;
        }
    }
}
