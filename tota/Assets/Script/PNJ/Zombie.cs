using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    //Public, modifiable
    public float fieldOfViewAngle = 140f;
    public float wanderRadius = 10f;

    //LayerMask
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstacleMask;

    //reference
    private NavMeshAgent _agent;
    private PhotonView _photon;

    //Destination (decided by masterClient)
    private Vector3 _wanderPoint;

    //Targets
    private List<Transform> _visibleTargets;
    [HideInInspector] public Transform player;

    private GameObject alert = null;
    //depre?
    private bool _canMove = true;

    // Start is called before the first frame update
    private void Start()
    {
        _visibleTargets = new List<Transform>();
        if(PhotonNetwork.isMasterClient)
        {
            //ref
            _agent = GetComponent<NavMeshAgent>();
            _photon = GetComponent<PhotonView>();
            //Start
            _wanderPoint = GetRandomWanderPoint();
            _agent.SetDestination(_wanderPoint);
            _agent.speed = 1;
            StartCoroutine("FindTargetsWithDelay", .6f);
        }
    }

    //Main loop
    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
            if (_visibleTargets.Count > 0)
            {
                Debug.Log("Zombie: In FOV");
                if (alert == null)
                {
                    AudioManager.instance.Play("Ping");
                    alert = new GameObject("Alert");
                }
                _wanderPoint = player.position;
                _agent.SetDestination(_wanderPoint);
                float distance = Vector3.Distance(player.position, transform.position);
                if (distance < 2f)
                {
                    player.GetComponent<CharaRpg>().TryDeathBite(30);
                }
            }
            else
            {
                Debug.Log("Zombie: Wander");
                Destroy(alert);

                //
                if (Vector3.Distance(transform.position, _wanderPoint) <= _agent.stoppingDistance + 1f)
                {
                    Debug.Log("Zombie: Arrived");
                    if(PhotonNetwork.isMasterClient)
                    {
                        _wanderPoint = GetRandomWanderPoint();
                        SendUpdatedWanderPoint();
                    }
                    yield return new WaitForSeconds(2f);
                    _agent.SetDestination(_wanderPoint);
                }
                //WanderStop(_wanderPoint);
            }
        }
    }

    //Helper functions

    private Vector3 GetRandomWanderPoint()//find a new random point
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
    private void FindVisibleTargets()// search for visible targets
    {
        _visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, wanderRadius, _targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < fieldOfViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, _obstacleMask))
                {
                    _visibleTargets.Add(target);
                }
            }
        }
        player = GetClosestPlayer();
    }
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)// Direction of FOV
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    
    //Depre?
    private IEnumerator MovingDelay()
    {
        _canMove = false;
        yield return new WaitForSeconds(2f);
        _canMove = true;
    }
    private void WanderStop(Vector3 destination)//Stop when arrived at destination
    {
        if (Vector3.Distance(transform.position, destination) <= _agent.stoppingDistance + 1f)
        {
            Debug.Log("Zombie: Arrived");
            _wanderPoint = GetRandomWanderPoint();
            StartCoroutine(MovingDelay());
        }

        if (_canMove)
            _agent.SetDestination(_wanderPoint);
    }

    //Attacked
    private int healthPoint = 100;

    //RPC
    public void GetAttackedWith(int damage)
    {
        _photon.RPC("RPC_GetAttackedWith", PhotonTargets.AllBuffered, damage);
    }
    [PunRPC] private void RPC_GetAttackedWith(int damage)
    {
        healthPoint -= damage;
        if(healthPoint <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void SendUpdatedWanderPoint()
    {
        _photon.RPC("RPC_SendUpdatedWanderPoint", PhotonTargets.OthersBuffered, _wanderPoint.x,_wanderPoint.y,_wanderPoint.z);
    }
    [PunRPC]private void RPC_SendUpdatedWanderPoint(float x, float y, float z)
    {
        _wanderPoint = new Vector3(x, y, z);
        _agent.SetDestination(_wanderPoint);
    }
}
