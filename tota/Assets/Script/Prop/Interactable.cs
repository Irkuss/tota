using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    //Interactable class, has to be derived from to be useful (ex: PropHandl

    //Characteristics of Interactable
    [Header("Interaction center")]
    [SerializeField] private Transform _interTransform = null;
    public Transform InterTransform { get => _interTransform; }

    [Header("Radius of interaction, from interaction center")]
    [SerializeField] private float _radius = 5;
    public float Radius { get => _radius; }

    //Interact, has to be overwritten
    public virtual void Interact(CharaHead chara)
    {
        Debug.Log("Interactable: Interacting as " + chara.name);
    }
}
