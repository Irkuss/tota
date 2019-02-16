using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaOutline : MonoBehaviour
{
    [SerializeField]
    private Outline outline;
    //[SerializeField]
    private Outline.Mode selectedMod = Outline.Mode.OutlineAndSilhouette;
    //[SerializeField]
    private Outline.Mode deselectedMod = Outline.Mode.SilhouetteOnly;


    private void Start()
    {
        outline.OutlineMode = deselectedMod;
    }

    //Public Methods

    public void SetOutlineToSelected()
    {
        outline.OutlineMode = selectedMod;
    }

    public void SetOutlineToNotSelected()
    {
        outline.OutlineMode = deselectedMod;
    }
}
