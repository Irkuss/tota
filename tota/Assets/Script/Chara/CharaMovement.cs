using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharaMovement : MonoBehaviour
{
    //Recuperer le NavMeshAgent Component attaché au Chara
    public NavMeshAgent navMeshAgent;
    public ThirdPersonCharacter character;

    private void Start()
    {
        
    }

    private void Update()
    {
        navMeshAgent.updateRotation = false;
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            character.Move(navMeshAgent.desiredVelocity, false, false);
        }
        else
        {
            character.Move(Vector3.zero, false, false);
        }
    }

    //Deplacement clic droit
    public void MoveTo(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }

    public void SetStoppingDistance(float newStop)
    {
        navMeshAgent.stoppingDistance = newStop;
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
