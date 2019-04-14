using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkChara : MonoBehaviour
{
    [HideInInspector] public SpiritHead spirit;
    [HideInInspector] public GameObject chara;
    public Text Name;

    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => SelectChara());
    }

    private void SelectChara()
    {       
        if (!spirit.ContainsChara(chara))
        {
            spirit.DeselectAllExcept(chara);
            spirit.MoveCamera(chara.transform.position);
        }
        else
        {
            spirit.DeselectChara(chara);
        }
    }
}
