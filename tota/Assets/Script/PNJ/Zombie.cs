using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

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
    private ThirdPersonCharacter _character;

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
        _character = GetComponent<ThirdPersonCharacter>();
        //ref
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _photon = GetComponent<PhotonView>();
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(FindTargetsWithDelay());
        }
    }

    //Activor and deactivator
    private bool _canTakeDecision = false;
    private List<CharaHead> _activatorCharas = new List<CharaHead>();

    public void ForceActivate(CharaHead activatorChara)
    {
        if (!_canTakeDecision) //Debug.Log("ForceActivate: A zombie has been activated");
        //Called by CharaHead in CheckForAi

        if(!_activatorCharas.Contains(activatorChara))
        {
            //ajoute le chara en tant qu'activator
            _activatorCharas.Add(activatorChara);
            _canTakeDecision = true;
        }
    }
    private void CheckDeactivate()
    {
        List<CharaHead> activatorToRemove = new List<CharaHead>();
        //Verifie la distance de chaque activateur
        foreach(CharaHead activatorChara in _activatorCharas)
        {
            if(Vector3.Distance(transform.position, activatorChara.transform.position) > CharaHead.c_radiusToActivate)
            {
                activatorToRemove.Add(activatorChara);
            }
        }
        //Supprimes les charas eloignées des activateurs
        foreach(CharaHead deactivator in activatorToRemove)
        {
            _activatorCharas.Remove(deactivator);
        }
        //End condition
        if(_activatorCharas.Count == 0)
        {
            Debug.Log("CheckDeactivate: A zombie has been deactivated");
            _canTakeDecision = false;
        }
    }

    //Main loop
    private IEnumerator FindTargetsWithDelay()
    {
        int cycleBeforeMoving = Random.Range(2,4) * 2;
        float delay = 0.5f;
        _agent.speed = 0.5f;
        _wanderPoint = transform.position;

        ForcePosition();
        int cycleBeforeForcingPosition = 100;

        while (true)
        {
            yield return new WaitForSeconds(delay);
            if(_canTakeDecision)
            {
                //Debug.Log("FindTargetsWithDelay: taking decision");
                if (cycleBeforeForcingPosition <= 0)
                {
                    ForcePosition();
                    cycleBeforeForcingPosition = 100;
                }
                else
                {
                    cycleBeforeForcingPosition--;
                }

                FindVisibleTargets();
                if (_visibleTargets.Count > 0)
                {
                    //Zombie is chasing a chara
                    if (alert == null)
                    {
                        AudioManager.instance.Play("Ping");
                        alert = new GameObject("Alert");
                    }
                    _wanderPoint = player.position;

                    _agent.SetDestination(_wanderPoint);
                    SendUpdatedWanderPoint();

                    if (Vector3.Distance(player.position, transform.position) < 2f)
                    {
                        player.GetComponent<CharaRpg>().TryDeathBite(30);
                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    //Zombie is wandering
                    Destroy(alert);
                    if (Vector3.Distance(transform.position, _wanderPoint) <= _agent.stoppingDistance + 0.3f)
                    {
                        //Zombie is close to destination
                        if (cycleBeforeMoving <= 0)
                        {
                            //Find new wander spot
                            _wanderPoint = GetRandomWanderPoint();
                            SendUpdatedWanderPoint();
                            _agent.SetDestination(_wanderPoint);

                            cycleBeforeMoving = Random.Range(4, 10) * 2;
                        }
                        else
                        {
                            //Wait before finding new wander spot
                            cycleBeforeMoving--;
                        }
                    }
                }
                //Verifie si le zombie se desactive
                CheckDeactivate();
            }
        }
    }

    private void Update()
    {
        if(_agent.remainingDistance > _agent.stoppingDistance)
        {
            _character.Move(_agent.desiredVelocity, false, false);
        }
        else
        {
            _character.Move(Vector3.zero, false, false);
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
    
    //Attacked
    private int healthPoint = 100;

    //RPC
    public void GetAttackedWith(int damage)
    {
        if(Mode.Instance.online) _photon.RPC("RPC_GetAttackedWith", PhotonTargets.AllBuffered, damage);
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
        if (Mode.Instance.online) _photon.RPC("RPC_SendUpdatedWanderPoint", PhotonTargets.OthersBuffered, _wanderPoint.x,_wanderPoint.y,_wanderPoint.z);
    }
    [PunRPC]private void RPC_SendUpdatedWanderPoint(float x, float y, float z)
    {
        _wanderPoint = new Vector3(x, y, z);
        _agent.SetDestination(_wanderPoint);
    }

    public void ForcePosition()
    {
        if (Mode.Instance.online) _photon.RPC("RPC_ForceZombiePosition", PhotonTargets.OthersBuffered, _wanderPoint.x, _wanderPoint.y, _wanderPoint.z);
    }
    [PunRPC] private void RPC_ForceZombiePosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

}
