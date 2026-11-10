#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// Crea (una sola vez) el prefab Assets/Prefabs/Escalera.prefab.
// Tambien disponible en el menu: Esperanza > Crear prefab Escalera
[InitializeOnLoad]
public static class CrearPrefabEscalera
{
    const string Ruta = "Assets/Prefabs/Escalera.prefab";

    static CrearPrefabEscalera()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(Ruta)) Crear();
        };
    }

    [MenuItem("Esperanza/Crear prefab Escalera")]
    public static void Crear()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        var go = new GameObject("Escalera");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5; // detras de Mateo (10)
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 3f);
        go.AddComponent<Escalera>();

        PrefabUtility.SaveAsPrefabAsset(go, Ruta);
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        Debug.Log("Prefab creado: " + Ruta);
    }
}
#endif
