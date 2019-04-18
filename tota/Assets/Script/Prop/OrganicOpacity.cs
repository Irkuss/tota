using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganicOpacity : MonoBehaviour
{
    private Renderer _renderer;
    private Collider _collider;

    private int currentFloorLevel;

    // Unity Callback
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();

        if(_renderer != null && _collider != null)
        {
            FloorManager.onFloorLevelChanged += UpdateFloorLevel;
        }

        //Init le floor level (pas ouf mais n'arrive qu'une fois par organic)
        currentFloorLevel = GameObject.Find("eCentralManager").GetComponent<FloorManager>().GetFloorLevel();
    }

    private void OnDestroy()
    {
        if (_renderer != null && _collider != null)
        {
            //Unsubscribe to prevent memory leak
            FloorManager.onFloorLevelChanged -= UpdateFloorLevel;
        }
    }

    //Updating opacity

    private void UpdateFloorLevel(int newFloorLevel)
    {
        currentFloorLevel = newFloorLevel;

        UpdateRenderer();
    }
    
    private void UpdateRenderer()
    {
        //Debug.Log("OrganicOpacity: Updating Renderer");
        if (IsAboveFloorLevel(currentFloorLevel))
        {
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            //_renderer.enabled = false;
            _collider.enabled = false;
        }
        else
        {
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            //_renderer.enabled = true;
            _collider.enabled = true;
        }
    }

    private bool IsAboveFloorLevel(int newFloorLevel)
    {
        if (transform.position.y > newFloorLevel * FloorManager.ChunkHeight + FloorManager.ChunkHeight*0.95f)
        {
            return true;
        }
        return false;
    }

    
}
