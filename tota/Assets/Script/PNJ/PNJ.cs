using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PNJ: MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _transform;

    public float _wanderRadius;
    private Vector3 _wanderPoint;

    private bool _canMove = true;

    public PNJ(Transform transform, NavMeshAgent agent, float wanderRadius)
    {
        _transform = transform;
        _agent = agent;
        _wanderRadius = wanderRadius;
    }

    public Vector3 RandomWanderPoint()// new random point
    {
        Vector3 randomPoint = (Random.insideUnitSphere * _wanderRadius) + _transform.position;
        NavMeshHit Hit;
        NavMesh.SamplePosition(randomPoint, out Hit, _wanderRadius, -1);

        return new Vector3(Hit.position.x, _transform.position.y, Hit.position.z);
    }

    public void WanderStop(Vector3 destination)//Stop when arrived at destination
    {
        if (Vector3.Distance(_transform.position, destination) <= _agent.stoppingDistance + 1f)
        {
            Debug.Log("Zombie: Arrived");
            _wanderPoint = RandomWanderPoint();
            StartCoroutine(MovingDelay());
        }

        if (_canMove)
            _agent.SetDestination(_wanderPoint);
    }

    IEnumerator MovingDelay()
    {
        _canMove = false;
        yield return new WaitForSeconds(2f);
        _canMove = true;
    }
}
