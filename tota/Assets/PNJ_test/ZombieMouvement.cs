using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMouvement : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public float wanderRadius = 7f;
    private Vector3 _wanderPoint;
    private NavMeshAgent _agent;
    public List<Transform> players;
    private Transform _player;
    public float fieldOfViewAngle = 50f;
    public bool isInFov;
    public Vector3 personalLastSighting;
    private bool _canMove = true;
    private bool _detection = true;
    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _wanderPoint = RandomWanderPoint(); _agent.SetDestination(_wanderPoint);
        _agent.speed = 1;
    }

    // Update is called once per frame
    void Update()
    {        
        if(_canMove)
        {
            Wander();
        }
        
        if(_detection)
        {
            _player = closerPlayer(players);
            isInFov = inFov(transform, _player, fieldOfViewAngle, wanderRadius);
            StartCoroutine(CoroutineFov());
        }
        
    }

    void Wander()
    {
        Debug.Log(Vector3.Distance(transform.position, _wanderPoint));
        if (isInFov)
        {
            _agent.speed = 2;
            _wanderPoint = _player.position;
            _agent.SetDestination(_wanderPoint);
        }
        else
        {
            _agent.speed = 1;
            if (Vector3.Distance(transform.position, _wanderPoint) < 2f)
            {
                _wanderPoint = RandomWanderPoint();
                Debug.Log(_wanderPoint);
                StartCoroutine(CoroutinePlayer());                
            }
        }
    }

    Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;
        NavMeshHit Hit;
        NavMesh.SamplePosition(randomPoint, out Hit, wanderRadius, -1);

        return new Vector3(Hit.position.x, transform.position.y, Hit.position.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle, transform.up) * transform.forward * wanderRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle, transform.up) * transform.forward * wanderRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
        if (_player != null)
        {
            if (isInFov)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, (_player.position - transform.position).normalized * wanderRadius);
        }
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * wanderRadius);
    }

    private Transform closerPlayer(List<Transform> players)
    {
        Transform target = null;
        float min = 1 / 0f; //infinity
        foreach (Transform player in players){
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance < min)
            {
                min = distance;
                target = player;
            }
        }

        return target;
    }

    public static bool inFov(Transform objet, Transform target, float maxAngle, float maxRadius)
    {
        //if (Vector3.Distance(objet.transform.position, target.transform.position) <= 0)
        /*Collider[] overlaps = new Collider[10];
        int count = Physics.OverlapSphereNonAlloc(objet.position, maxRadius, overlaps);
        for(int i = 0; i < count + 1; i++)
        {
            if (overlaps[i] != null)
            {
                if (overlaps[i].transform == target)
                {*/
                    Vector3 directionBetween = (target.position - objet.position).normalized;
                    directionBetween.y *= 0;

                    float angle = Vector3.Angle(objet.forward, directionBetween);

                    if (angle <= maxAngle)
                    {
                        Ray ray = new Ray(objet.position, target.position - objet.position);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, maxRadius))
                        {
                            if (hit.transform == target)
                                Debug.Log("OKKKKKKKKKKK"); return true;
                        }
                    }



        Debug.Log("NOOOOOOOPE");
        return false;
    }

    IEnumerator CoroutinePlayer()
    {
        _canMove = false;
        yield return new WaitForSeconds(3f);
        _canMove = true;
        _agent.SetDestination(_wanderPoint);
    }

    IEnumerator CoroutineFov()
    {
        _detection = false;
        yield return new WaitForSeconds(1f);
        _detection = true;
        
    }
}
