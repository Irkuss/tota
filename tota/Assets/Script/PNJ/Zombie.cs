using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Zombie : MonoBehaviour
{
    //Defining attribute
    public float fieldOfViewAngle = 140f;
    public float wanderRadius = 10f;

    //reference
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstacleMask;

    private NavMeshAgent _agent;
    private PhotonView _photon;
    private ThirdPersonCharacter _character;

    //Destination (decided by masterClient)
    private Vector3 _wanderPoint;
    
    private List<Transform> _visibleTargets;
    private Transform _closestPlayer;
    public Transform Player => _closestPlayer;

    private GameObject alert = null;

    // Start is called before the first frame update
    private void Start()
    {
        _visibleTargets = new List<Transform>();
        _character = GetComponent<ThirdPersonCharacter>();

        //ref
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _agent.speed = 0.5f;

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
        //if (!_canTakeDecision) Debug.Log("ForceActivate: A zombie has been activated");
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
            if(activatorChara == null || Vector3.Distance(transform.position, activatorChara.transform.position) > CharaHead.c_radiusToActivate)
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
            //Debug.Log("CheckDeactivate: A zombie has been deactivated");
            _canTakeDecision = false;
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

    //Main loop
    private IEnumerator FindTargetsWithDelay()
    {
        int cycleBeforeMoving = Random.Range(1,3) * 2;

        _wanderPoint = transform.position;

        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if(_canTakeDecision)
            {
                _visibleTargets = CharaAi.FindVisibleTargets(transform, _targetMask, _obstacleMask, wanderRadius, fieldOfViewAngle);

                if (_visibleTargets.Count > 0)
                {
                    _closestPlayer = CharaAi.FindClosestTransform(_visibleTargets, transform.position);

                    //Zombie is chasing a chara

                    if (alert == null)
                    {
                        AudioManager.instance.PlayAtPosition("Ping", transform.position);
                        alert = new GameObject("Alert");
                    }
                    _wanderPoint = _closestPlayer.position;

                    SetDestination(_wanderPoint);

                    if (Vector3.Distance(_closestPlayer.position, transform.position) < 2f)
                    {
                        _closestPlayer.GetComponent<CharaRpg>().TryDeathBite(70);
                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    //Zombie is wandering
                    //Destroy(alert);
                    if (Vector3.Distance(transform.position, _wanderPoint) <= _agent.stoppingDistance + 0.3f)
                    {
                        //Zombie is close to destination
                        if (cycleBeforeMoving <= 0)
                        {
                            //Find new wander spot
                            _wanderPoint = CharaAi.FindRandomWanderPoint(wanderRadius, transform.position);

                            SetDestination(_wanderPoint);

                            cycleBeforeMoving = Random.Range(4, 8) * 2;
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

    //Animation Update
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

    //Destination
    private void SetDestination(Vector3 position)
    {
        _agent.SetDestination(position);
        SendUpdatedWanderPoint();
    }


    //Helper Methods
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

        //_wanderPoint = CharaMovement.CorrectDestination(_wanderPoint);

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
