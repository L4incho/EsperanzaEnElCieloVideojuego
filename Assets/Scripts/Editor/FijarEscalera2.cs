#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FijarEscalera2
{
    [MenuItem("Esperanza/Fijar segunda escalera (Y 1.70)")]
    public static void Fijar()
    {
        var esc = GameObject.Find("SegundaEscalera");
        if (esc == null) { Debug.LogError("[Fijar] No existe SegundaEscalera"); return; }
        Undo.RecordObject(esc.transform, "Fijar escalera");
        esc.transform.position = new Vector3(13.59f, 1.70f, 0f);
        EditorSceneManager.MarkSceneDirty(esc.scene);
        Debug.Log("[Fijar] SegundaEscalera en (13.59, 1.70). Guarda con Ctrl+S.");
    }
}
#endif
