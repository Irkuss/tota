using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMouvement : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public float wanderRadius = 7f;
    private Vector3 wanderPoint;
    private NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderPoint = RandomWanderPoint();
    }

    // Update is called once per frame
    void Update()
    {
        Wander();
    }

    void Wander()
    {
        if (Vector3.Distance(transform.position, wanderPoint) < 2f)
        {
            wanderPoint = RandomWanderPoint();
        }
        else
        {
            agent.SetDestination(wanderPoint);
        }
    }

    Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;
        NavMeshHit Hit;
        NavMesh.SamplePosition(randomPoint, out Hit, wanderRadius, -1);

        return new Vector3(Hit.position.x, transform.position.y, Hit.position.z);
    }
}
