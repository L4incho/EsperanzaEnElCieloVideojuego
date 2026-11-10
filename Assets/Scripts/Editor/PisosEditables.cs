#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Pisos del Nivel 2 con transformacion editable: se quita el bloqueo de cada tile para poder mover/escalar/rotar
// cada pieza pintada con la herramienta Seleccionar de la Tile Palette (Inspector: Offset, Rotation, Scale, Move/Rotate/Scale/Transform).
// Menu: Esperanza > Pisos Nivel 2 editables
public static class PisosEditables
{
    [MenuItem("Esperanza/Pisos Nivel 2 editables")]
    public static void Aplicar()
    {
        int nTiles = 0;
        foreach (var g in AssetDatabase.FindAssets("t:Tile", new[] { "Assets/Tiles/Nivel2" }))
        {
            var t = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(g));
            // pisos: los que tienen colision de sprite y no instancian objeto
            if (t == null || t.colliderType != Tile.ColliderType.Sprite || t.gameObject != null) continue;
            t.flags = TileFlags.None;
            EditorUtility.SetDirty(t);
            nTiles++;
        }
        AssetDatabase.SaveAssets();

        int nCeldas = 0;
        var suelo = GameObject.Find("Suelo");
        if (suelo != null)
        {
            var tm = suelo.GetComponent<Tilemap>();
            foreach (var p in tm.cellBounds.allPositionsWithin)
                if (tm.HasTile(p)) { tm.SetTileFlags(p, TileFlags.None); nCeldas++; }
        }
        var esc = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(esc);
        EditorSceneManager.SaveScene(esc);
        Debug.Log("[Pisos] " + nTiles + " tiles de piso desbloqueados; " + nCeldas + " celdas ya pintadas en la escena");
    }
}
#endif
