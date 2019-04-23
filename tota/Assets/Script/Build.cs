using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build : MonoBehaviour
{
    [SerializeField] private string _buildpath = null;

    public void BuildMode()
    {
        if (_buildpath == null || _buildpath == "") return;
        PermissionsManager.Instance.spirit.EnterBuildMode(_buildpath);
    }
}
