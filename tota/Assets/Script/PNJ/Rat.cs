using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rat : AiDeactivator
{
    //Defining attribute
    private float _wanderRadius = 6;
    private float _viewRadius = 6;

    private float _slowSpeed = 1f;
    private float _fastSpeed = 5f;

    //Reference
    private NavMeshAgent _agent;
    private PhotonView _photon;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    //Private Attribute
    private Vector3 _wanderPoint;

    private List<Transform> _visibleTargets;
    private Transform _closestPlayer;

    private bool isRunningAway = false;

    // Start is called before the first frame update
    private void Start()
    {
        _visibleTargets = new List<Transform>();
        _agent = GetComponent<NavMeshAgent>();
        _photon = GetComponent<PhotonView>();
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(UpdateDelay());
        }
    }
    //ForcePosition
    private IEnumerator WaitForcePosition()
    {
        ForcePosition();
        yield return new WaitForSeconds(5);
        ForcePosition();
        while (true)
        {
            yield return new WaitForSeconds(60);
            ForcePosition();
        }
    }

    // Update is called once per frame
    private IEnumerator UpdateDelay()
    {
        int cycleBeforeMoving = Random.Range(1, 3) * 2;

        _wanderPoint = transform.position;

        _agent.speed = _slowSpeed;

        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if(_canTakeDecision)
            {
                _visibleTargets = CharaAi.FindVisibleTargets(transform, targetMask, obstacleMask, _viewRadius);


                if (_visibleTargets.Count > 0)
                {
                    _closestPlayer = CharaAi.FindClosestTransform(_visibleTargets, transform.position);

                    //Si on a détecté une target
                    SetMovementMode(true);

                    _wanderPoint = CharaAi.FindFleePositionFromTarget(_closestPlayer.position, transform.position, 5);

                    SetDestination(_wanderPoint);
                }
                else
                {
                    //Si on n'est pas en train de s'enfuir
                    if (Vector3.Distance(transform.position, _wanderPoint) <= _agent.stoppingDistance + 0.3f)
                    {
                        if (!isRunningAway)
                        {
                            //Si on est safe
                            if (cycleBeforeMoving <= 0)
                            {
                                _wanderPoint = CharaAi.FindRandomWanderPoint(_wanderRadius, transform.position);

                                SetDestination(_wanderPoint);

                                cycleBeforeMoving = Random.Range(1, 3) * 2;
                            }
                            else
                            {
                                cycleBeforeMoving--;
                            }
                        }
                        else
                        {
                            SetMovementMode(false);
                        }
                    }
                }

                //Verifie si le rat se desactive
                CheckDeactivate();
            }
        }
    }
    //Destination
    private void SetDestination(Vector3 position)
    {
        _agent.SetDestination(position);
        SendUpdatedWanderPoint();
    }

    //WalkMode
    [PunRPC]
    private void RPC_SetMovementMode(bool setToRun)
    {
        _agent.speed = setToRun ? _fastSpeed : _slowSpeed;
    }
    private void SetMovementMode(bool setToRun)
    {
        //Debug.Log("SetMovementMode: sending new movement");

        isRunningAway = setToRun;
        _agent.speed = setToRun ? _fastSpeed : _slowSpeed;

        if (Mode.Instance.online) _photon.RPC("RPC_SetMovementMode", PhotonTargets.OthersBuffered, setToRun);
    }

    //Networkig
    public void KillSelf()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public void SendUpdatedWanderPoint()
    {
        //Debug.Log("SendUpdatedWanderPoint: rat new wander point has been sent at " + transform.position );

        if (Mode.Instance.online) _photon.RPC("RPC_SendUpdatedWanderPoint", PhotonTargets.OthersBuffered, _wanderPoint.x, _wanderPoint.y, _wanderPoint.z);
    }
    [PunRPC]
    private void RPC_SendUpdatedWanderPoint(float x, float y, float z)
    {
        _wanderPoint = new Vector3(x, y, z);
        _agent.SetDestination(_wanderPoint);
    }

    public void ForcePosition()
    {
        //Debug.Log("ForcePosition: rat");

        if (Mode.Instance.online) _photon.RPC("RPC_ForceRatPosition", PhotonTargets.OthersBuffered, _wanderPoint.x, _wanderPoint.y, _wanderPoint.z);
    }
    [PunRPC]
    private void RPC_ForceRatPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }
}
