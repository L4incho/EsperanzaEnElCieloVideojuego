#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Crea el prefab "BarrilFuego" (barril en llamas como objeto normal, sin tile) y convierte los barriles pintados como tile en instancias del prefab.
// Menu: Esperanza > Crear objeto Barril en llamas
public static class CrearBarrilFuego
{
    const string Origen = "Assets/Prefabs/Nivel2/Fuego_N2.prefab";
    const string Destino = "Assets/Prefabs/Nivel2/BarrilFuego.prefab";

    [MenuItem("Esperanza/ZZ Crear objeto Barril en llamas")]
    public static void Crear()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Destino);
        if (prefab == null)
        {
            var raiz = PrefabUtility.LoadPrefabContents(Origen);
            foreach (var o in raiz.GetComponents<OcultarTile>()) Object.DestroyImmediate(o);
            raiz.name = "BarrilFuego";
            prefab = PrefabUtility.SaveAsPrefabAsset(raiz, Destino);
            PrefabUtility.UnloadPrefabContents(raiz);
            Debug.Log("[Barril] prefab creado: " + Destino);
        }

        var esc = SceneManager.GetActiveScene();
        int n = 0;
        foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            var lista = new System.Collections.Generic.List<Vector3Int>();
            foreach (var c in tm.cellBounds.allPositionsWithin)
            {
                var t = tm.GetTile<Tile>(c);
                if (t == null || t.gameObject == null || t.gameObject.GetComponent<FuegoBarril>() == null) continue;
                lista.Add(c);
            }
            foreach (var c in lista)
            {
                var m = tm.GetTransformMatrix(c);
                Vector3 basePos = tm.transform.TransformPoint(tm.CellToLocal(c) + Vector3.Scale(tm.tileAnchor, tm.cellSize));
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, esc);
                inst.transform.position = basePos + (Vector3)m.GetColumn(3);
                inst.transform.localScale = Vector3.Scale(inst.transform.localScale, m.lossyScale);
                inst.transform.rotation = m.rotation;
                inst.name = "BarrilFuego";
                Undo.RegisterCreatedObjectUndo(inst, "Barril");
                tm.SetTile(c, null);
                n++;
                Debug.Log("[Barril] tile " + tm.name + " " + c + " -> objeto en " + inst.transform.position.ToString("F2"));
            }
        }
        Debug.Log("[Barril] barriles convertidos a objeto: " + n);
        EditorSceneManager.MarkSceneDirty(esc);
        EditorSceneManager.SaveScene(esc);
    }
}
#endif
