using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="buildTable")]
public class buildTable : ScriptableObject
{
    //ScriptableObject contenant les chemins possibles d'un type de bâtiment.
    //Les chemins sont à partir de dossier "Asset/Resources/".
    //Si tous les bâtiments sont dans un folder commun, on peut mettre le chemin de ce folder dans pathFolder
    //Sinon, on met "" dans pathFolder.

    //(Utilisé dans Generator.cs)

    public string pathFolder;
    public string[] pathArray;

    public string GetRandomPath()
    {
        return pathFolder + pathArray[Random.Range(0,pathArray.Length)];
    }
}
