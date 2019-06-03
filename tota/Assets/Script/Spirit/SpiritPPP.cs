using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SpiritPPP : MonoBehaviour
{
    private SpiritHead _head = null;
    private SpiritZoom _zoom = null;

    private readonly string tagPPP = "ppp";
    private PostProcessVolume volume = null;

    private ColorGrading _colorgrading = null;
    private DepthOfField _depthOfField = null;

    private readonly float _interpolationTime = 0.7f;

    private Vector3 _currentTarget = Vector3.zero;
    private IEnumerator _cor_focusLerp = null;

    private float lastYSpiritPosition;

    private void Start()
    {
        _head = GetComponent<SpiritHead>();
        _zoom = GetComponent<SpiritZoom>();

        lastYSpiritPosition = transform.position.y;

        GameObject ppp = GameObject.FindGameObjectWithTag(tagPPP);

        if(ppp != null)
        {
            volume = ppp.GetComponent<PostProcessVolume>();

            if(volume == null)
            {
                Debug.LogWarning("SpiritPPP: did not fing postProcessVolume (gameObject with tag '" + tagPPP + "' is missing a PostProcessVolume Component)");
            }
            else
            {
                volume.profile.TryGetSettings<ColorGrading>(out _colorgrading);
                volume.profile.TryGetSettings<DepthOfField>(out _depthOfField);
            }
        }
        else
        {
            Debug.LogWarning("SpiritPPP: did not fing postProcessVolume (gameObject with tag '" + tagPPP + "')");
        }
    }

    private int coloTest = 0;



    //Auto DoF
    private bool interpolate = false;
    private Vector3 lastFocusPoint = Vector3.zero;
    
    private void FixedUpdate()
    {
        Focus();
    }

    private void Focus()
    {
        if(CustomRaycast(out RaycastHit hit))
        {
            Debug.DrawLine(_zoom.DesiredPosition, hit.point);

            Vector3 hitCustomPosition = new Vector3(hit.point.x, hit.transform.position.y, hit.point.z);
            
            if(hitCustomPosition.y != _currentTarget.y || lastYSpiritPosition != _zoom.DesiredPosition.y)
            {
                if (interpolate && _currentTarget != Vector3.zero)
                {
                    if (_cor_focusLerp != null) StopCoroutine(_cor_focusLerp);

                    _cor_focusLerp = InterpolateFocus(hitCustomPosition);

                    StartCoroutine(_cor_focusLerp);
                }
                else
                {
                    _currentTarget = hitCustomPosition;

                    //Set le Dof
                    _depthOfField.focusDistance.value = Vector3.Distance(_currentTarget, _zoom.DesiredPosition);
                    //Debug.Log("Focus: set focus distance to " + _depthOfField.focusDistance.value);
                }
            }

            lastYSpiritPosition = _zoom.DesiredPosition.y;
        }
    }

    private bool CustomRaycast(out RaycastHit hit)
    {
        Vector3 direction = Vector3.down;
        Vector3 origin = _zoom.DesiredPosition;

        bool hitSomethingValid = false;

        int maxStep = 20;
        int step = 0;

        while (!hitSomethingValid && step < maxStep)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit possibleHit))
            {
                if (_head.IsHitValid(possibleHit, true))
                {
                    hit = possibleHit;
                    return true;
                }
                origin = possibleHit.point + direction * 0.02f;

                step++;
            }
            else
            {
                break;
            }
        }
        if (step == maxStep) Debug.LogWarning("CustomRaycast: Unexpected reached maxStep ! ");
        Debug.LogWarning("CustomRaycast: Unexpected spiritRaycast hit nothing");
        hit = new RaycastHit();
        return false;
    }

    private IEnumerator InterpolateFocus(Vector3 targetPosition)
    {
        while (!DoorHandler.FloatEqual(_currentTarget.y, targetPosition.y))
        {
            Debug.Log("InterpolateFocus: interpolating dof");
            yield return new WaitForEndOfFrame();

            _currentTarget = Vector3.Lerp(_currentTarget, targetPosition, 0.7f);

            //Set le Dof
            _depthOfField.focusDistance.value = Vector3.Distance(_currentTarget, _zoom.DesiredPosition);
        }
        _currentTarget = targetPosition;

        //Set le Dof
        _depthOfField.focusDistance.value = Vector3.Distance(_currentTarget, _zoom.DesiredPosition);
    }






}
