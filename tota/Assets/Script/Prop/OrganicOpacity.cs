using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganicOpacity : MonoBehaviour
{
    private Renderer[] _renderers;
    private Collider _collider;

    private int currentFloorLevel;

    private IEnumerator _cor_updateRender;
    private float _previousY;

    // Unity Callback
    private void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _collider = GetComponent<Collider>();

        if(_renderers.Length > 0 && _collider != null)
        {
            FloorManager.onFloorLevelChanged += UpdateFloorLevel;
        }

        //Init le floor level (pas ouf mais n'arrive qu'une fois par organic)
        currentFloorLevel = GameObject.Find("eCentralManager").GetComponent<FloorManager>().GetFloorLevel();

        _cor_updateRender = Cor_UpdateRenderer();

        _previousY = transform.position.y;
        StartCoroutine(_cor_updateRender);
    }

    private void OnDestroy()
    {
        if (_renderers.Length > 0 && _collider != null)
        {
            //Unsubscribe to prevent memory leak
            FloorManager.onFloorLevelChanged -= UpdateFloorLevel;
        }

        StopCoroutine(_cor_updateRender);
    }

    //Updating opacity (by callback)
    private void UpdateFloorLevel(int newFloorLevel)
    {
        currentFloorLevel = newFloorLevel;

        UpdateRenderer();
    }
    //Updating opacity (constantly)
    private IEnumerator Cor_UpdateRenderer()
    {
        UpdateRenderer();
        while (true)
        {
            if(transform.position.y != _previousY)
            {
                UpdateRenderer();
                _previousY = transform.position.y;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    //Update opcaity handler
    private void UpdateRenderer()
    {
        Debug.Log("OrganicOpacity: Updating Renderer");
        if (IsAboveFloorLevel(currentFloorLevel))
        {
            Debug.Log("OrganicOpacity: Setting to invisible");
            foreach (Renderer rend in _renderers)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            Renderer selfRender = GetComponent<Renderer>();
            if(selfRender!=null) selfRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            _collider.enabled = false;
        }
        else
        {
            Debug.Log("OrganicOpacity: Setting to visible");
            foreach (Renderer rend in _renderers)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            Renderer selfRender = GetComponent<Renderer>();
            if (selfRender != null) selfRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            _collider.enabled = true;
        }
    }

    private bool IsAboveFloorLevel(int newFloorLevel)
    {
        if (transform.position.y > newFloorLevel * FloorManager.c_chunkHeight + FloorManager.c_chunkHeight*0.95f)
        {
            return true;
        }
        return false;
    }

    
}
