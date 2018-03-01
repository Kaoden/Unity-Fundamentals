using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
 
public class GFTools
{
    // hot keys: _ normal, # ctrl, % shift, & alt
 
    [MenuItem("Tools/GF/Duplicate with prefab #%D")]
    private static void DuplicateWithPrefab()
    {
        InstantiateWithPrefab(false);
    }
 
    [MenuItem("Tools/GF/Replace with prefab #%R")]
    private static void ReplaceWithPrefab()
    {
        InstantiateWithPrefab(true);
    }
 
    private static void InstantiateWithPrefab(bool deleteDestination)
    {
        GameObject originalObject = null;
        Object[] prefabList = Selection.GetFiltered(typeof(UnityEngine.GameObject), SelectionMode.Assets);
 
        // try to find a prefab
        foreach (var o in prefabList)
        {
            PrefabType t = PrefabUtility.GetPrefabType(o);
            if (t == PrefabType.Prefab || t == PrefabType.ModelPrefab)
            {
                if (originalObject == null)
                {
                    originalObject = o as GameObject;
                }
                else
                {
                    Debug.Log("select only one prefab in project window");
                    return;
                }
            }
        }
 
        // if no prefab found, use active object
        if (originalObject == null)
        {
            originalObject = Selection.activeGameObject;
        }
 
        if (originalObject == null)
        {
            Debug.Log("select one prefab in project window or an object in the scene window");
            return;
        }
 
        // clear selection
        Selection.objects = new Object[0];
 
        if (deleteDestination)
            Debug.Log("Replacing with " + originalObject.name + ":");
        else
            Debug.Log("Duplicating " + originalObject.name + " at:");
 
        List<GameObject> newObjects = new List<GameObject>();
 
        for (int i=0; i<prefabList.Length; i++)
        {
            GameObject go = prefabList[i] as GameObject;
            if (!go || go == originalObject)
                continue;
 
            PrefabType t = PrefabUtility.GetPrefabType(go);
            if (t != PrefabType.Prefab && t != PrefabType.ModelPrefab)
            {
                Debug.Log("  " + (newObjects.Count + 1).ToString() + " " + go.name);
 
                GameObject newObject;
 
                newObject = (GameObject)PrefabUtility.InstantiatePrefab(originalObject);
                if(newObject == null)
                {
                    newObject = GameObject.Instantiate(originalObject);
                    if (newObject == null)
                    {
                        Debug.Log("failed to instantiate");
                        break;
                    }
                }
 
                Undo.RegisterCreatedObjectUndo(newObject, "created prefab");
                newObject.name = originalObject.name;
 
                newObject.transform.SetParent(go.transform.parent, true);
                newObject.transform.localPosition = go.transform.localPosition;
                newObject.transform.localRotation = go.transform.localRotation;
                newObject.transform.localScale = go.transform.localScale;
                newObject.transform.SetSiblingIndex(go.transform.GetSiblingIndex()+1);
 
                newObjects.Add(newObject);
 
                if (deleteDestination)
                    Undo.DestroyObjectImmediate(go);
            }
        }
 
        Selection.objects = newObjects.ToArray();
 
        EditorSceneManager.MarkAllScenesDirty();
    }
}