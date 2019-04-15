using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prop/Prop")]
public class Prop : ScriptableObject
{
    public string nickName = "Prop Name";
    public string path = "Prop/rest of the path";
    public PropTable.PropType propType = PropTable.PropType.Undecided;
}
