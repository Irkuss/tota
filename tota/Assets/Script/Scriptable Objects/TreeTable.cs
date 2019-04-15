using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prop/TreeTable")]
public class TreeTable : ScriptableObject
{
    public string pathTreeFolder;
    public string[] treeNicknames;

    public string GetRandomPath()
    {
        return pathTreeFolder + treeNicknames[Random.Range(0, treeNicknames.Length)];
    }
}
