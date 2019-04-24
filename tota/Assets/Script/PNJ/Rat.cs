using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rat : MonoBehaviour
{
    private NavMeshAgent _agent;
    private PhotonView _photon;

    public float wanderRadius;
    public float viewRadius;
    private Vector3 _wanderPoint;

    private List<Transform> _visibleTargets;
    [HideInInspector]  public Transform _player;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

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

    // Update is called once per frame
    private IEnumerator UpdateDelay()
    {
        int cycleBeforeMoving = 20;

        bool isRunningAway = false;
        float slowSpeed = 1f;
        float fastSpeed = 4f;

        _wanderPoint = transform.position;

        _agent.speed = slowSpeed;

        ForcePosition();
        int cycleBeforeForcingPosition = 100;

        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (cycleBeforeForcingPosition <= 0)
            {
                ForcePosition();
                cycleBeforeForcingPosition = 100;
            }
            else
            {
                cycleBeforeForcingPosition--;
            }

            if (!isRunningAway)
            {
                Debug.Log("RatUpdateDelay: We are not running");
                //Si on n'est pas en train de s'enfuir
                FindTargets();
                if (_visibleTargets.Count > 0)
                {
                    Debug.Log("RatUpdateDelay: started to flee!");
                    //Si on a détecté une target
                    isRunningAway = true;
                    _agent.speed = fastSpeed;
                    _wanderPoint = RunAway(_player.position);
                    _agent.SetDestination(_wanderPoint);
                    Debug.Log("RatUpdateDelay: currently moving from to " + _wanderPoint);
                    SendUpdatedWanderPoint();
                }
                else
                {
                    Debug.Log("RatUpdateDelay: we are safe");
                    //Si on est safe
                    if (Vector3.Distance(transform.position, _wanderPoint) <= _agent.stoppingDistance + 0.3f)
                    {

                        if (cycleBeforeMoving <= 0)
                        {
                            Debug.Log("RatUpdateDelay: starting to move");
                            _wanderPoint = GetRandomWanderPoint();
                            _agent.SetDestination(_wanderPoint);
                            SendUpdatedWanderPoint();
                            Debug.Log("RatUpdateDelay: currently moving from to " + _wanderPoint);
                            cycleBeforeMoving = Random.Range(2, 6) * 2;
                        }
                        else
                        {
                            cycleBeforeMoving--;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("RatUpdateDelay: We are running!");
                //Si on est en train de s'enfuir
                if (Vector3.Distance(transform.position, _wanderPoint) <= _agent.stoppingDistance + 0.3f)
                {
                    isRunningAway = false;
                    _agent.speed = slowSpeed;
                }
            }
        }
    }

    private Vector3 RunAway(Vector3 playerPos)
    {
        NavMeshHit Hit;
        NavMesh.SamplePosition(-playerPos * 1.3f, out Hit, wanderRadius, -1);

        return new Vector3(Hit.position.x, transform.position.y, Hit.position.z);
    }
 
    public Vector3 GetRandomWanderPoint()// new random point
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;
        NavMeshHit Hit;
        NavMesh.SamplePosition(randomPoint, out Hit, wanderRadius, -1);

        return new Vector3(Hit.position.x, transform.position.y, Hit.position.z);
    }

    private Transform GetClosestPlayer()// returns closest player
    {
        Transform target = null;
        float min = 1 / 0f; //infinity
        foreach (Transform player in _visibleTargets)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance < min)
            {
                min = distance;
                target = player;
            }
        }

        return target;
    }

    private void FindTargets()// search for visible targets
    {
        _visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        int count = targetsInViewRadius.Length;
        for (int i = 0; i < count; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position);
            if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
            {
                _visibleTargets.Add(target);
            }
        }
        _player = GetClosestPlayer();
    }





    //Networkig
    public void KillSelf()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public void SendUpdatedWanderPoint()
    {
        _photon.RPC("RPC_SendUpdatedWanderPoint", PhotonTargets.OthersBuffered, _wanderPoint.x, _wanderPoint.y, _wanderPoint.z);
    }
    [PunRPC]
    private void RPC_SendUpdatedWanderPoint(float x, float y, float z)
    {
        _wanderPoint = new Vector3(x, y, z);
        _agent.SetDestination(_wanderPoint);
    }

    public void ForcePosition()
    {
        _photon.RPC("RPC_ForceRatPosition", PhotonTargets.OthersBuffered, _wanderPoint.x, _wanderPoint.y, _wanderPoint.z);
    }
    [PunRPC]
    private void RPC_ForceRatPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }
}
