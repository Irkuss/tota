using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine.AI;

public class B_InterNeutral : MonoBehaviour
{
    [Header("Prop context")]
    public string propFolder = "";
    public string subPropFolder = "cars";
    [Header("NavMeshModifier")]
    public bool canChangeLocation = true;
    public bool isAnObstacle = true;
    [Header("Auto-fill Interactable")]
    public int actionCount = 2;
    [Header("Auto-fill Furniture")]
    public LootTable lootTable = null;

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
        if (subPropFolder != "")
        {
            //Rajoute un '/' devant le chemin du folder s'il est oublié
            if (subPropFolder[0] != '/')
            {
                subPropFolder = "/" + subPropFolder;
                Debug.Log("BuildInterNeutral: Unexpected missing '/', added '/' to subPropFolder");
            }
        }
        //==========Ajoute le prop équivalent==========
        string nickName = transform.parent.name;
        Prop createdPropSo = CreateProp(nickName, propFolder);
        createdPropSo.nickName = nickName;

        createdPropSo.path = "Prop" + subPropFolder + "/" + nickName;

        Debug.Log("BuildInterNeutral: Successfuly added Prop to " + createdPropSo.path);

        //==========Reset les positions du parent et sa propre postion==========
        ResetTransform(transform.parent.transform);
        ResetTransform(transform);
        Debug.Log("BuildInterNeutral: Successfuly updated transform of parent/child");

        //==========Ajoute les components==========
        //Collider et transform
        BoxCollider boxColl = gameObject.AddComponent<BoxCollider>();
        transform.position = new Vector3(-boxColl.center.x, 0, -boxColl.center.z);

        //Interactable et furniture
        Interactable inter;
        if(lootTable == null)
        {
            inter = gameObject.AddComponent<PropHandler>();
        }
        else
        {
            Debug.LogWarning("BuildInterNeutral: creating a furniture without CharaInventory, don't forget to add it!");
            inter = gameObject.AddComponent<PropFurniture>();
            gameObject.AddComponent<Outline>();
            (inter as PropFurniture).lootTable = lootTable;
        }
        inter.ForceArrays(actionCount);
        //NavmeshModifier
        if (canChangeLocation)
        {
            NavMeshModifier navMod = gameObject.AddComponent<NavMeshModifier>();
            navMod.ignoreFromBuild = true;
        }
        if(isAnObstacle)
        {
            NavMeshObstacle obs = gameObject.AddComponent<NavMeshObstacle>();
            obs.carving = true;
        }
        

        Debug.Log("BuildInterNeutral: Successfuly added Components");
        
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
        Debug.Log("CreateProp: at " + path + subPathFolder + "/" + nickName + ".asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        //Selection.activeObject = asset;

        return asset;
    }

}
#endif