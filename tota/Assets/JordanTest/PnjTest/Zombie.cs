using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    private NavMeshAgent _agent;

    public float fieldOfViewAngle = 50f;
    public float wanderRadius = 7f;
    private Vector3 _wanderPoint;

    public List<Transform> visibleTargets;
    public Transform _player;

    public bool isInFov;
    private bool _canMove = true;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _wanderPoint = RandomWanderPoint();
        _agent.SetDestination(_wanderPoint);
        _agent.speed = 1;
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    // Update is called once per frame
    void Update()
    {
    }



    void WanderStop(Vector3 destination)//Stop when arrived at destination
    {
        if (Vector3.Distance(transform.position, destination) < 2f)
        {
            _wanderPoint = RandomWanderPoint();
            StartCoroutine(MovingDelay());
        }
    }

    Vector3 RandomWanderPoint()// new random point
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;
        NavMeshHit Hit;
        NavMesh.SamplePosition(randomPoint, out Hit, wanderRadius, -1);

        return new Vector3(Hit.position.x, transform.position.y, Hit.position.z);
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

    bool FindVisibleTargets()// search for visible targets
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, wanderRadius, targetMask);
        int count = targetsInViewRadius.Length;
        for (int i = 0; i < count; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < fieldOfViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
        _player = closestPlayer(visibleTargets);

        return visibleTargets.Count > 0;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)// Direction of FOV
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (FindVisibleTargets())
            {
                _wanderPoint = _player.position;
            }
            WanderStop(_wanderPoint);
            if (_canMove || FindVisibleTargets())
            {
                _agent.SetDestination(_wanderPoint);
            }
        }
    }

    IEnumerator MovingDelay()
    {
        _canMove = false;
        yield return new WaitForSeconds(4f);
        _canMove = true;
    }


}
