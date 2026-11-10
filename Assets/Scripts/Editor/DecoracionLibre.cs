#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Decoracion del Nivel 2 como TILES con transformacion editable (igual que la del Tutorial):
// se les quita el bloqueo de transformacion para que, con la herramienta Seleccionar de la Tile Palette,
// se pueda cambiar la posicion, escala y rotacion de cada pieza pintada desde el Inspector.
// Menu: Esperanza > Decoracion Nivel 2 editable
public static class DecoracionLibre
{
    const string CarpTiles = "Assets/Tiles/Nivel2";
    const string CarpObj = "Assets/Prefabs/Nivel2/Decoracion";   // de un intento anterior (objetos): se borra

    [MenuItem("Esperanza/Decoracion Nivel 2 editable")]
    public static void Crear()
    {
        // 1) nombres de las piezas de decoracion (los prefabs del intento anterior las listan)
        var nombres = AssetDatabase.FindAssets("_Atras t:Prefab", new[] { CarpObj })
            .Select(g => System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)))
            .Select(n => n.Substring(5, n.Length - 5 - 6)).OrderBy(n => n).ToList();   // quita "Deco_" y "_Atras"
        var tiles = new List<TileBase>();
        foreach (var n in nombres)
        {
            var t = AssetDatabase.LoadAssetAtPath<Tile>(CarpTiles + "/" + n + ".asset");
            if (t == null) { Debug.LogWarning("[Deco] no hay tile " + n); continue; }
            t.flags = TileFlags.None;   // sin bloqueo: posicion/escala/rotacion editables
            EditorUtility.SetDirty(t);
            tiles.Add(t);
        }
        AssetDatabase.SaveAssets();
        if (tiles.Count > 0)
        {
            PaletaUtil.Construir("Assets/Paletas/Paleta_Decoracion_Atras_N2.prefab", "Paleta_Decoracion_Atras_N2", tiles);
            PaletaUtil.Construir("Assets/Paletas/Paleta_Decoracion_Frente_N2.prefab", "Paleta_Decoracion_Frente_N2", tiles);
        }
        if (AssetDatabase.IsValidFolder(CarpObj) && tiles.Count > 0) AssetDatabase.DeleteAsset(CarpObj);
        AssetDatabase.Refresh();

        // 2) escena: capas de decoracion como Tilemap
        var esc = SceneManager.GetActiveScene();
        var grid = GameObject.Find("GridDecoracion");
        if (grid == null) { grid = new GameObject("GridDecoracion"); grid.AddComponent<Grid>().cellSize = new Vector3(1, 1, 0); }
        string[] viejos = { "DecoracionAtras", "DecoracionFrente" }, nuevos = { "DecoracionDetrasMateo", "DecoracionFrenteMateo" };
        int[] orden = { 1, 11 };
        for (int k = 0; k < 2; k++)
        {
            var t = grid.transform.Find(nuevos[k]) ?? grid.transform.Find(viejos[k]);
            if (t == null) { var n = new GameObject(nuevos[k]); n.transform.SetParent(grid.transform, false); t = n.transform; }
            t.name = nuevos[k];
            for (int i = t.childCount - 1; i >= 0; i--) Object.DestroyImmediate(t.GetChild(i).gameObject);   // objetos de prueba
            var tm = t.GetComponent<Tilemap>() ?? t.gameObject.AddComponent<Tilemap>();
            var tr = t.GetComponent<TilemapRenderer>() ?? t.gameObject.AddComponent<TilemapRenderer>();
            tr.sortingOrder = orden[k];
            // lo ya pintado: quitar el bloqueo de transformacion
            foreach (var p in tm.cellBounds.allPositionsWithin) if (tm.HasTile(p)) tm.SetTileFlags(p, TileFlags.None);
        }
        EditorSceneManager.MarkSceneDirty(esc);
        EditorSceneManager.SaveScene(esc);
        Debug.Log("[Deco] " + tiles.Count + " tiles de decoracion editables; paletas reconstruidas; capas en la escena listas");
    }
}
#endif
