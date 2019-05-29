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
    
    private bool _isRunning = false;
    public bool IsRunning => _isRunning;
    private float _baseAgentWalkingSpeed = 0.7f;
    private float _baseAgentRunningSpeed = 1f;
    
    private float _baseAgentAngularSpeed;

    private float _currentHealthModifier = 1f;

    private void Start()
    {
        _baseAgentAngularSpeed = navMeshAgent.angularSpeed;
    }

    private void Update()
    {
        //Update le ThirdPersonCharacter
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
    public void MoveTo(Vector3 position, bool isRunning)
    {
        //Debug.Log("MoveTo: moving");
        SetDestination(position, isRunning);
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

    //Modification depuis CharaRpg
    public void ModifyAgentSpeed(float speedModifier)
    {
        //Called by UpdateHealth every delay seconds
        _currentHealthModifier = speedModifier;
        UpdateSpeed();
    }
    private void UpdateSpeed()
    {
        navMeshAgent.speed = _isRunning
            ? _baseAgentRunningSpeed * _currentHealthModifier
            : _baseAgentWalkingSpeed * _currentHealthModifier;
        
        navMeshAgent.angularSpeed = _baseAgentAngularSpeed * _currentHealthModifier;
    }

    //Deplacement vers un Interactable
    public void MoveToInter(Interactable inter, bool isRunning = false)
    {
        //Debug.Log("MoveToInter: moving");
        SetDestination(inter.InterTransform.position, isRunning);
    }

    public void StopAgent(bool resetRunning = true)
    {
        //Debug.Log("StopAgent: moving");
        SetDestination(transform.position, !resetRunning && _isRunning);
    }

    //Update to network
    //Update current destination

    private void SetDestination(Vector3 dest, bool isRunning)
    {
        int runInt = isRunning ? 1 : 0;

        dest = CorrectDestination(dest);
        if(dest == null)
        {
            Debug.LogWarning("SetDestination: No destination close were found");
            return;
        }

        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetDestination, new int[1] { runInt}, null, new float[3] { dest.x, dest.y, dest.z });
        //GetComponent<PhotonView>().RPC("RPC_SetDestination", PhotonTargets.AllBuffered, dest.x, dest.y, dest.z);
    }
    private static Vector3 CorrectDestination(Vector3 dest)
    {
        //Debug.Log("CorrectDestination: Correcting at " + dest);
        NavMeshHit hit;
        
        if(NavMesh.SamplePosition(dest, out hit, 4, -1))
        {
            //Debug.Log("CorrectDestination: Found closest at " + hit.position);
            return hit.position;
        }

        //Debug.LogWarning("CorrectDestination: Could not find valid close point on navMesh");
        return dest;
    }
    public void RPC_SetDestination(int runInt, float x, float y, float z)
    {
        _isRunning = runInt == 1 ? true : false;
        UpdateSpeed();
        navMeshAgent.SetDestination(new Vector3(x, y, z));
    }
}
