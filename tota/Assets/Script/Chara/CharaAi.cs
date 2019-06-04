using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharaAi : AiDeactivator
{
    //Recipe and Item
    [SerializeField] private Item[] _itemToSearch = null;
    [SerializeField] private RecipeTable _bedRecipe = null;
    public static Item[] ItemToSearch = null;

    //Layer
    [SerializeField] private LayerMask zombieMask;
    [SerializeField] private LayerMask charaMask;
    [SerializeField] private LayerMask obstacleMask;

    //Fix radius
    private float _wanderRadius = 20;
    private float _viewRadius = 14;
    private float _searchRadius = 20;

    private float _interRadius; //AUTO SET, DONT SET (used to stop ai when a chara with a team is close)

    //Reference
    private CharaHead _head = null;
    private CharaRpg _rpg = null;
    private CharaMovement _movement = null;
    private CharaInventory _inventory = null;

    //Freshrate atribute
    private int _aiUpdateRefreshRate = 10;
    private int _refreshRateProgress = 10;

    private List<Transform> _visibleZombie;
    private Transform _closestZombie;

    private List<Transform> _visibleChara;
    private Transform _closestChara;

    private Transform _targetToFace = null;

    private int _cycleBeforeTakingAction;
    private bool _isFleeing = false;

    private List<PropFurniture> alreadyVisitedFurniture = null;


    private void Start()
    {
        ItemToSearch = _itemToSearch;

        _head = GetComponent<CharaHead>();
        _rpg = GetComponent<CharaRpg>();
        _movement = GetComponent<CharaMovement>();
        _inventory = GetComponent<CharaInventory>();
        
        _interRadius = GetComponent<CharaInteract>().Radius + 0.5f;

        _cycleBeforeTakingAction = Random.Range(1, 3) * 2;

        alreadyVisitedFurniture = new List<PropFurniture>();
    }

    // UpdateAi is called once per frame
    public void UpdateAi()
    {
        if(_refreshRateProgress == _aiUpdateRefreshRate)
        {
            if(_canTakeDecision)
            {

                if(!_rpg.ShouldBeDown())
                {
                    MainUpdateAi();
                }

                CheckDeactivate();
            }
            
            _refreshRateProgress = 0;
        }
        else
        {
            _refreshRateProgress++;
        }

        UpdateRotation();
    }

    private void MainUpdateAi()
    {
        _visibleZombie = CharaAi.FindVisibleTargets(transform, zombieMask, obstacleMask, _viewRadius);

        bool lockAction = false;

        if(_visibleZombie.Count > 0)
        {
            _closestZombie = CharaAi.FindClosestTransform(_visibleZombie, transform.position);

            Vector3 fleePosition = CharaAi.FindFleePositionFromTarget(_closestZombie.position, transform.position, 8);

            _head.SetDestination(fleePosition, true);

            _isFleeing = true;

            _targetToFace = null;
        }
        else
        {
            //Gestion générale (tant qu'on ne fuie pas)
            if (!_isFleeing)
            {
                _visibleChara = CharaAi.FindVisibleTargets(transform, charaMask, obstacleMask, _viewRadius);

                //Gestion des charas
                if (_visibleChara.Count > 0)
                {
                    _closestChara = CharaAi.FindClosestTransform(_visibleChara, transform.position);

                    if (Vector3.Distance(transform.position, _closestChara.position) < _interRadius && _closestChara.GetComponent<CharaPermissions>().Team != null)
                    {
                        //Si un chara avec une team est trop proche de nous, s'arrete et le regarde (generalement pour interragir)
                        _targetToFace = _closestChara;

                        _movement.StopAgent(true);

                        lockAction = true;
                    }
                    else 
                    {
                        if (_movement.AgentIsIdling)
                        {
                            _targetToFace = _closestChara;
                        }

                        lockAction = false;
                    }
                }
                else
                {
                    //Si on bouge, on ne cible personne a regarder
                    _targetToFace = null;
                }
            }

            //Gestion de la fin de déplacement
            if (_movement.ReachedDestination())
            {
                if(_isFleeing)
                {
                    _movement.StopAgent(true);
                }

                if(!lockAction && _head.IsFree)
                {
                    //Si on est safe
                    if (_cycleBeforeTakingAction <= 0)
                    {
                        bool isFreeToSearch = true;
                        bool isFreeToWander = true;

                        if (_rpg.HasWoundOfType(WoundInfo.WoundType.Bleeding))
                        {
                            //Si saigne, essaye de se mettre un bandage
                            if (_inventory.Contains(_itemToSearch[0]))
                            {
                                _head.UseItem(_itemToSearch[0]);//Tellement caca

                                isFreeToWander = false;
                                isFreeToSearch = false;
                            }
                        }
                        else if (_rpg.Hunger > 15)
                        {
                            //Si a faim essaye de manger
                            if(_inventory.Contains(_itemToSearch[1]))
                            {
                                _head.UseItem(_itemToSearch[1]);//Tellement caca

                                isFreeToWander = false;
                                isFreeToSearch = false;
                            }
                        }

                        if(isFreeToSearch && LacksItemToSearch())
                        {
                            //S'il manque des items
                            List<PropFurniture> allCloseFurniture = FindPropHandlerOfType<PropFurniture>(_searchRadius);

                            allCloseFurniture = RemoveAlreadyVisitedPropHandler(allCloseFurniture);

                            PropFurniture closestFurniture = GetClosestPropHandlerOfType(allCloseFurniture);

                            if (closestFurniture != null && closestFurniture.CheckAvailability(_head, -1))
                            {
                                //Debug.Log("MainUpdateAi: found a furniture, setting focus to it");
                                alreadyVisitedFurniture.Add(closestFurniture);
                                
                                _head.SetFocus(closestFurniture, -1);//Interaction AI

                                isFreeToWander = false;
                            }
                        }

                        if(isFreeToWander)
                        {
                            Vector3 wanderPosition = CharaAi.FindRandomWanderPoint(_wanderRadius, transform.position);
                            _head.SetDestination(wanderPosition, false);
                        }


                        _targetToFace = null;

                        _cycleBeforeTakingAction = Random.Range(1, 5) * 2;
                    }
                    else
                    {
                        _cycleBeforeTakingAction--;
                    }
                }
            }
        }
    }

    private void UpdateRotation()
    {
        if(_targetToFace != null && !_rpg.ShouldBeDown())
        {
            Vector3 direction = (_targetToFace.position - transform.position).normalized;
            //Debug.Log("UpdateRotation: direction is (" + _targetToFace.position + " - " + transform.position + ").normalized = " + direction);
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }


    //Opening Furniture
    private bool LacksItemToSearch()
    {
        foreach(Item item in _itemToSearch)
        {
            if (!_inventory.Contains(item))
            {
                return true;
            }
        }
        return false;
    }
    private List<T> FindPropHandlerOfType<T>(float searchRadius) where T : PropHandler
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, searchRadius);

        List<T> propHandlers = new List<T>();

        foreach (Collider coll in colls)
        {
            T propHandler = coll.GetComponent<T>();

            if (propHandler != null)
            {
                propHandlers.Add(propHandler);
            }
        }

        return propHandlers;
    }
    private List<PropFurniture> RemoveAlreadyVisitedPropHandler(List<PropFurniture> propHandlers)
    {
        List<PropFurniture> newPropHandler = new List<PropFurniture>();

        foreach(PropFurniture prop in propHandlers)
        {
            if(!alreadyVisitedFurniture.Contains(prop))
            {
                newPropHandler.Add(prop);
            }
        }

        return newPropHandler;
    }
    private T GetClosestPropHandlerOfType<T>(List<T> propHandlers) where T : PropHandler
    {
        T minPropHandler = null;
        float minDistance = 0;

        foreach(T propHandler in propHandlers)
        {
            float distance = Vector3.Distance(transform.position, propHandler.transform.position);

            if (minPropHandler == null || distance < minDistance)
            {
                minPropHandler = propHandler;
                minDistance = distance;
            }
        }

        return minPropHandler;
    }

    //Static ai methods (used by CharaAi, Rat, Zombie)

    public static Vector3 FindRandomWanderPoint(float wanderRadius, Vector3 basePosition)//find a new random point
    {
        bool foundRandomWander = false;

        Vector3 wanderPoint = Vector3.zero;
        NavMeshHit hit;

        while (!foundRandomWander)
        {
            wanderPoint = (Random.insideUnitSphere * wanderRadius) + basePosition;

            if (NavMesh.SamplePosition(wanderPoint, out hit, 2, -1))
            {
                wanderPoint = hit.position;

                foundRandomWander = true;
            }
        }
        if (wanderPoint == Vector3.zero) Debug.LogWarning("GetRandomWanderPoint: Failed to find wander position");
        return wanderPoint;
    }

    public static Vector3 FindFleePositionFromTarget(Vector3 targetPosition, Vector3 basePosition, float maxFleeDistance)
    {
        Vector3 dirTargetToBase = (basePosition - targetPosition).normalized;

        bool foundRandomWander = false;

        float currentFleeDistance = maxFleeDistance;

        Vector3 fleePoint = Vector3.zero;
        NavMeshHit hit;

        while (!foundRandomWander)
        {
            fleePoint = basePosition + currentFleeDistance * dirTargetToBase;

            if (NavMesh.SamplePosition(fleePoint, out hit, 4, -1))
            {
                fleePoint = hit.position;

                foundRandomWander = true;
            }
            else
            {
                currentFleeDistance += -0.1f;
                if (currentFleeDistance < 0)
                {
                    break;
                }
            }
        }
        if (fleePoint == Vector3.zero) Debug.LogWarning("GetRandomWanderPoint: Failed to find wander position");

        return fleePoint;
    }
    
    public static List<Transform> FindVisibleTargets(
        Transform baseTransform, 
        LayerMask targetMask, 
        LayerMask obstacleMask, 
        float viewRadius,
        float fieldOfViewAngle = 360f)// search for visible targets
    {
        List<Transform> visibleTargets = new List<Transform>();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(baseTransform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;

            if(target != baseTransform)
            {
                Vector3 dirToTarget = (target.position - baseTransform.position).normalized;

                if (Vector3.Angle(baseTransform.forward, dirToTarget) < fieldOfViewAngle / 2)
                {
                    float dstToTarget = Vector3.Distance(baseTransform.position, target.position);
                    if (!Physics.Raycast(baseTransform.position, dirToTarget, dstToTarget, obstacleMask))
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }

        return visibleTargets;
    }

    public static Transform FindClosestTransform(List<Transform> transforms, Vector3 basePosition)
    {
        Transform closestTransform = null;
        float closestTransformDistance = -1;
        foreach (Transform transformElement in transforms)
        {
            float distance = Vector3.Distance(transformElement.position, basePosition);

            if (distance < closestTransformDistance || closestTransformDistance < 0)
            {
                closestTransformDistance = distance;
                closestTransform = transformElement;
            }
        }

        return closestTransform;
    }
}
