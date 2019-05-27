using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class B_InterNeutral : MonoBehaviour
{
    public int actionCount = 0;
    public string propFolder = "";
    public GameObject originalPrefab = null;

    private static string propSoPathFromAssets = "/General/Prop/Prop";

    //Component to add to neutral interactable
    public void BuildInterNeutral()
    {
        Debug.Log("BuildInterNeutral: Building PropHandler Components");
        //Verification des input

        if (propFolder != "") 
        {
            //Rajoute un '/' devant le chemin du folder s'il est oublié
            if(propFolder[0] != '/')
            {
                propFolder = "/" + propFolder;
                Debug.Log("BuildInterNeutral: Unexpected missing '/', added '/' to propFolder");
            }
        }
        else Debug.LogWarning("BuildInterNeutral: Unexpected missing propFolder, this should be normal only when testing!");

        //Reset les positions du parent et sa propre postion
        ResetTransform(transform.parent.transform);
        ResetTransform(transform);
        Debug.Log("BuildInterNeutral: Successfuly updated transform of parent/child");

        //Ajoute les components
        BoxCollider boxColl = gameObject.AddComponent<BoxCollider>();
        transform.position = new Vector3(-boxColl.center.x, 0, -boxColl.center.z);

        Interactable inter = gameObject.AddComponent<PropHandler>();
        inter.ForceArrays(actionCount);
        Debug.Log("BuildInterNeutral: Successfuly added Components");

        //Ajoute le prop équivalent
        string nickName = transform.parent.name;
        Prop createdPropSo = CreateProp(nickName, propFolder);
        createdPropSo.nickName = nickName;

        string fullPath = AssetDatabase.GetAssetPath(originalPrefab);
        fullPath = fullPath.Substring(17);
        fullPath = fullPath.Substring(0, fullPath.IndexOf('.'));
        createdPropSo.path = fullPath;

        Debug.Log("BuildInterNeutral: Successfuly added Prop to " + AssetDatabase.GetAssetPath(originalPrefab));
        DestroyImmediate(this);
    }

    private static void ResetTransform(Transform tr)
    {
        tr.localPosition = Vector3.zero;
        tr.localEulerAngles = Vector3.zero;
        tr.localScale = Vector3.one;
    }

    public static Prop CreateProp(string nickName, string subPathFolder)
    {
        Prop asset = ScriptableObject.CreateInstance<Prop>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        path += propSoPathFromAssets;

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + subPathFolder+ "/" + nickName + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }

}
