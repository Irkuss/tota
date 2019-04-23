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
        if (PhotonNetwork.isMasterClient)
        {
            _agent = GetComponent<NavMeshAgent>();
            _photon = GetComponent<PhotonView>();
            Wander(GetRandomWanderPoint());
            StartCoroutine(UpdateDelay());
        }
    }

    // Update is called once per frame
    private IEnumerator UpdateDelay()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);

            FindTargets();
            if (_visibleTargets.Count > 0)
            {
                _agent.speed += 0.5f;
                _wanderPoint = RunAway(_player.position);
                Wander(_wanderPoint);
            }
            else
            {
                _agent.speed = 1f;

            }
            Wander(_wanderPoint);
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

    public void Wander(Vector3 destination)//Stop when arrived at destination
    {
        if (Vector3.Distance(transform.position, destination) <= _agent.stoppingDistance + 2f)
        {
            Debug.Log("Rats: Arrived");
            if(PhotonNetwork.isMasterClient)
            {
                _wanderPoint = GetRandomWanderPoint();
                SendUpdatedWanderPoint();
            }
        }
        else
        {
            _agent.SetDestination(_wanderPoint);
            SendUpdatedWanderPoint();
        }
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
}
