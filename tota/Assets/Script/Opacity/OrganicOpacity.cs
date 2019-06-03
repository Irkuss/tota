using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganicOpacity : GeneralOpacity
{
    [Header("Is this object moving or not?")]
    public bool isMoving = false;
    [Header("Particle to hide opacity")]
    public GameObject particleGameObject = null;
    private bool _wasAboveFloorLevel;

    private Renderer[] _renderers;
    private Renderer _renderer;
    private Collider _collider;

    private IEnumerator _cor_updateRender;
    private float _previousY;

    private int _spiritFloorLevel;

    // Unity Callback
    private void Start()
    {
        BeginOpacity();
    }

    protected void BeginOpacity()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();

        if (particleGameObject != null) particleGameObject.SetActive(ShouldParticleGameObjectBeActive());

        //if (_renderers.Length > 0 && _collider != null) FloorManager.onFloorLevelChanged += UpdateFloorLevel;
        FloorManager.onFloorLevelChanged += UpdateFloorLevel;

        //Init le floor level (pas ouf mais n'arrive qu'une fois par organic)
        _spiritFloorLevel = GameObject.Find("eCentralManager").GetComponent<FloorManager>().FloorLevel;
        _wasAboveFloorLevel = IsAboveFloorLevel(_spiritFloorLevel);
        UpdateRenderer();
        if (isMoving)
        {
            _cor_updateRender = Cor_UpdateRenderer();

            _previousY = transform.position.y;
            StartCoroutine(_cor_updateRender);
        }
    }

    private void OnDestroy()
    {
        //Debug.Log("OrganicOpacity: destroying organic opacity with name " + name);
        //if (_renderers.Length > 0 && _collider != null) FloorManager.onFloorLevelChanged -= UpdateFloorLevel; //Unsubscribe to prevent memory leak
        FloorManager.onFloorLevelChanged -= UpdateFloorLevel;
        if (_cor_updateRender != null)
        {
            StopCoroutine(_cor_updateRender);
        }
    }

    //Updating opacity (by callback)
    private void UpdateFloorLevel(int newFloorLevel)
    {
        _wasAboveFloorLevel = IsAboveFloorLevel(_spiritFloorLevel);
        _spiritFloorLevel = newFloorLevel;

        UpdateRenderer();
    }
    //Updating opacity (constantly)
    private IEnumerator Cor_UpdateRenderer()
    {
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
        //Debug.Log("OrganicOpacity: Updating Renderer");
        bool currentAbove = IsAboveFloorLevel(_spiritFloorLevel);
        if (currentAbove && !_wasAboveFloorLevel)
        {
            //Debug.Log("OrganicOpacity: Setting to invisible");
            foreach (Renderer rend in _renderers)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            if(_renderer!=null) _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            if (particleGameObject != null) particleGameObject.SetActive(false);
            //_collider.enabled = false;
        }
        else if(!currentAbove && _wasAboveFloorLevel)
        {
            //Debug.Log("OrganicOpacity: Setting to visible");
            foreach (Renderer rend in _renderers)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            if (_renderer != null) _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            if(particleGameObject != null) particleGameObject.SetActive(ShouldParticleGameObjectBeActive());
            //_collider.enabled = true;
        }
        _wasAboveFloorLevel = currentAbove;
    }

    private bool IsAboveFloorLevel(int newFloorLevel)
    {
        if (transform.position.y > newFloorLevel * FloorManager.c_chunkHeight + FloorManager.c_chunkHeight*0.95f)
        {
            return true;
        }
        return false;
    }


    public override bool IsBelowFloor(int floor)
    {
        return !IsAboveFloorLevel(floor);
    }

    //Particle
    protected virtual bool ShouldParticleGameObjectBeActive()
    {
        return false;
    }
}
