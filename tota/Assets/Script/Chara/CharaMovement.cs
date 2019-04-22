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

    private float _baseAgentSpeed;
    private float _baseAgentAngularSpeed;

    private void Start()
    {
        _baseAgentSpeed = navMeshAgent.speed;
        _baseAgentAngularSpeed = navMeshAgent.angularSpeed;
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
        SetDestination(position);
    }

    public void SetStoppingDistance(float newStop)
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetStoppingDistance, null, null, new float[1] { newStop });
        //GetComponent<PhotonView>().RPC("RPC_SetStoppingDistance", PhotonTargets.AllBuffered, newStop);
    }
    public void RPC_SetStoppingDistance(float newStop)
    {
        navMeshAgent.stoppingDistance = newStop;
    }
    /*[PunRPC] private void RPC_SetStoppingDistance(float newStop)
    {
        navMeshAgent.stoppingDistance = newStop;
    }*/

    //Modification depuis CharaRpg
    public void ModifyAgentSpeed(float speedModifier)
    {
        navMeshAgent.speed = _baseAgentSpeed * speedModifier;
        navMeshAgent.angularSpeed = _baseAgentAngularSpeed * speedModifier;
    }

    //Deplacement vers un Interactable
    public void MoveToInter(Interactable inter)
    {
        SetDestination(inter.InterTransform.position);
    }

    public void StopAgent()
    {
        SetDestination(transform.position);
    }

    //Update to network
    //Update current destination

    private void SetDestination(Vector3 dest)
    {
        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetDestination, null, null, new float[3] { dest.x, dest.y, dest.z });
        //GetComponent<PhotonView>().RPC("RPC_SetDestination", PhotonTargets.AllBuffered, dest.x, dest.y, dest.z);
    }
    public void RPC_SetDestination(float x, float y, float z)
    {
        navMeshAgent.SetDestination(new Vector3(x, y, z));
    }
    /*
    [PunRPC] private void RPC_SetDestination(float x, float y, float z)
    {
        navMeshAgent.SetDestination(new Vector3(x, y, z));
    }*/
}
