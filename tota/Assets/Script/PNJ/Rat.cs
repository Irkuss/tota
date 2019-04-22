using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rat : MonoBehaviour
{
    private NavMeshAgent _agent;

    public float wanderRadius;
    public float viewRadius;
    private Vector3 _wanderPoint;

    public List<Transform> visibleTargets;
    public Transform _player;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        Wander(RandomWanderPoint());
    }

    // Update is called once per frame
    void Update()
    {
        if (FindTargets())
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

    Vector3 RunAway(Vector3 playerPos)
    {
        NavMeshHit Hit;
        NavMesh.SamplePosition(-playerPos * 1.3f, out Hit, wanderRadius, -1);

        return new Vector3(Hit.position.x, transform.position.y, Hit.position.z);
    }

    public Vector3 RandomWanderPoint()// new random point
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
            _wanderPoint = RandomWanderPoint();
            //StartCoroutine(MovingDelay());
        }
        else
        {
            _agent.SetDestination(_wanderPoint);
        }
        /*if (_canMove)
            _agent.SetDestination(_wanderPoint);*/
    }

    private Transform closestPlayer(List<Transform> players)// returns closest player
    {
        Transform target = null;
        float min = 1 / 0f; //infinity
        foreach (Transform player in players)
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

    bool FindTargets()// search for visible targets
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        int count = targetsInViewRadius.Length;
        for (int i = 0; i < count; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position);
            if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
            {
                visibleTargets.Add(target);
            }
            
        }
        _player = closestPlayer(visibleTargets);

        return visibleTargets.Count > 0;
    }
}
