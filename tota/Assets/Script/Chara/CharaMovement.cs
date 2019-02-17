﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharaMovement : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;

    public void MoveTo(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }
}