using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharaMovement : MonoBehaviour
{
    //Recuperer le NavMeshAgent Component attaché au Chara
    public NavMeshAgent navMeshAgent;

    //Deplacement clic droit
    public void MoveTo(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }


    //Deplacement vers un Interactable
    public void MoveToInter(Interactable inter)
    {
        navMeshAgent.SetDestination(inter.InterTransform.position);
    }

    public void StopAgent()
    {
        navMeshAgent.SetDestination(transform.position);
    }
}
