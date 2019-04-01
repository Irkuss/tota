using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorOpacity : MonoBehaviour
{
    public int currentFloor = 0;

    private Renderer _renderer;
    private Collider _collider;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();

        if (_renderer != null && _collider != null)
        {
            //subscribe to onFloorLevelChanged
            FloorManager.onFloorLevelChanged += UpdateRenderer;
        }
    }

    private void UpdateRenderer(int newFloorLevel) //Subscribed to onFloorLevelChanged
    {
        if (currentFloor > newFloorLevel)
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

    private void OnDestroy()
    {
        if (_renderer != null && _collider != null)
        {
            //Unsubscribe to prevent momory leak
            FloorManager.onFloorLevelChanged -= UpdateRenderer;
        }
    }
}
