using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private float _radius = 5;
    public float Radius { get => _radius; }

    [SerializeField] private Transform _interTransform = null;
    public Transform InterTransform { get => _interTransform; }

    public virtual void Interact(CharaHead chara)
    {
        Debug.Log("Interactable: Interacting as " + chara.name);
        //PickUp() provenant de itemPickUp.cs
    }
}
