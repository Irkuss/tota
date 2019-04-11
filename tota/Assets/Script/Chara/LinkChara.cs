using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkChara : MonoBehaviour
{
    [HideInInspector] public SpiritHead spirit;
    [HideInInspector] public GameObject chara;

    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => SelectChara());
    }

    private void SelectChara()
    {
        spirit.DeselectAllExcept(chara);
        spirit.MoveCamera(chara.transform.position);
    }
}
