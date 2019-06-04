using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharaMovement : MonoBehaviour
{
    //Recuperer le NavMeshAgent Component attaché au Chara
    private NavMeshAgent _navMeshAgent = null;
    public float AgentStoppingDistance => _navMeshAgent.stoppingDistance;
    public bool AgentIsIdling => !_navMeshAgent.hasPath;

    private ThirdPersonCharacter _character = null;
    
    private bool _isRunning = false;
    public bool IsRunning => _isRunning;
    private float _baseAgentWalkingSpeed = 0.7f;
    private float _baseAgentRunningSpeed = 1f;
    
    private float _baseAgentAngularSpeed;

    private float _currentHealthModifier = 1f;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        
        _character = GetComponent<ThirdPersonCharacter>();

        _baseAgentAngularSpeed = _navMeshAgent.angularSpeed;
    }

    private void Update()
    {
        //Update le ThirdPersonCharacter
        _navMeshAgent.updateRotation = false;
        if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
        {
            _character.Move(_navMeshAgent.desiredVelocity, false, false);
        }
        else
        {
            _character.Move(Vector3.zero, false, false);
        }
    }

    public bool ReachedDestination()
    {
        if(!_navMeshAgent.pathPending)
        {
            if(_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                if(!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
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
        _navMeshAgent.stoppingDistance = newStop;
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
        _navMeshAgent.speed = _isRunning
            ? _baseAgentRunningSpeed * _currentHealthModifier
            : _baseAgentWalkingSpeed * _currentHealthModifier;
        
        _navMeshAgent.angularSpeed = _baseAgentAngularSpeed * _currentHealthModifier;
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

    //Update to destination

    private void SetDestination(Vector3 dest, bool isRunning)
    {
        int runInt = isRunning ? 1 : 0;

        dest = CorrectDestination(dest);

        GetComponent<CharaConnect>().SendMsg(CharaConnect.CharaCommand.RPC_SetDestination, new int[1] { runInt}, null, new float[3] { dest.x, dest.y, dest.z });
        //GetComponent<PhotonView>().RPC("RPC_SetDestination", PhotonTargets.AllBuffered, dest.x, dest.y, dest.z);
    }
    public static Vector3 CorrectDestination(Vector3 dest)
    {
        //Debug.Log("CorrectDestination: Correcting at " + dest);
        if(NavMesh.SamplePosition(dest, out NavMeshHit hit, 4, -1))
        {
            //Debug.Log("CorrectDestination: Found closest at " + hit.position);
            return hit.position;
        }

        Debug.LogWarning("CorrectDestination: Could not find valid close point on navMesh");
        return dest;
    }
    public void RPC_SetDestination(int runInt, float x, float y, float z)
    {
        _isRunning = runInt == 1 ? true : false;
        UpdateSpeed();

        //if (GetComponent<CharaPermissions>().Team != null) Debug.Log("SetDestination: setting to destination");

        _navMeshAgent.SetDestination(new Vector3(x, y, z));

        StartCoroutine(WaitForPathCompletion());
    }

    private IEnumerator WaitForPathCompletion()
    {
        while(_navMeshAgent.pathPending)
        {
            yield return null;
        }
        if (!GetComponent<CharaRpg>().ShouldBeDown()) GetComponent<CharaHead>().SwitchState(true);

        //if (GetComponent<CharaPermissions>().Team != null) Debug.Log("SetDestination: updating corners");
        UpdateCorners();
    }

    private void UpdateCorners()
    {
        Vector3[] corners = _navMeshAgent.path.corners;

        List<DoorHandler> doorsOnPath = new List<DoorHandler>();

        /*
        string debug = "UpdateCorners: updating " + corners.Length + " corners";

        foreach (Vector3 vec in corners) debug += vec + " ";

        Debug.Log(debug);
        */

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 v1 = i == 0 ? transform.position : corners[i - 1];
            Vector3 v2 = corners[i];

            float distance = Vector3.Distance(v1, v2);

            //Debug.Log("UpdateCorners: drawing line from " + v1 + " to " + v2);
            Debug.DrawLine(v1 + Vector3.up, v2 + Vector3.up, Color.red, 1.5f);

            if (Physics.Raycast(v1 + Vector3.up, v2 - v1, out RaycastHit hit, distance))
            {
                //Debug.Log("UpdateCorners: hitting something when checking for door");
                DoorHandler doorComp = hit.transform.GetComponent<DoorHandler>();
                if(doorComp != null)
                {
                    //Debug.Log("UpdateCorners: that something is a door");
                    if(!doorsOnPath.Contains(doorComp)) doorsOnPath.Add(doorComp);
                }
            }
        }

        GetComponent<CharaHead>().doorsOnPath = doorsOnPath;
    }
}
