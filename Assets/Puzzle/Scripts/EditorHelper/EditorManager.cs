using UnityEngine;
using UnityEditor;

public class EditorManager
{
#if UNITY_EDITOR
    [MenuItem("Editor/SetShadowShapes")]
    public static void SetShadowShapes()
    {
        GameObject[] shadows = GameObject.FindGameObjectsWithTag("Shadow");
        int shadowCounter = 0;
        for (int i = 0; i < shadows.Length; i++)
        {
            shadows[i].GetComponent<MeshFilter>().mesh = shadows[i].transform.parent.GetComponent<MeshFilter>().sharedMesh;
            shadowCounter++;
        }
        Debug.Log(shadowCounter.ToString() +" shadows has been updated. Save the prefab.");
    }
    #endif
}