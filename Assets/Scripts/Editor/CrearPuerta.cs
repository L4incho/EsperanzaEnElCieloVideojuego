#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

// Pone el disparador de la puerta sobre el tile de puerta que dibujaste (laboratorio_spritesheet_12, el de la esquina superior izquierda).
public static class CrearPuerta
{
    const string NombreSprite = "laboratorio_spritesheet_12";

    [MenuItem("Esperanza/Crear puerta al Nivel 2 (tecla W)")]
    public static void Crear()
    {
        var sc = EditorSceneManager.GetActiveScene();
        var halladas = new List<(Tilemap mapa, Vector3Int celda, Bounds caja)>();
        foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                var spr = tm.GetSprite(pos);
                if (spr == null || spr.name != NombreSprite) continue;
                var mat = tm.GetTransformMatrix(pos);
                Vector3 centro = tm.GetCellCenterWorld(pos);
                Vector3 e = Vector3.Scale(spr.bounds.size, Vector3.Scale(tm.transform.lossyScale, mat.lossyScale));
                Vector3 off = Vector3.Scale(spr.bounds.center, Vector3.Scale(tm.transform.lossyScale, mat.lossyScale));
                halladas.Add((tm, pos, new Bounds(centro + off, new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), 1f))));
            }
        }
        if (halladas.Count == 0) { Debug.LogError("[Puerta] No encontre ningun tile " + NombreSprite + " en la escena (guarda la escena y revisa que el tile de la puerta este pintado)"); return; }
        foreach (var h in halladas) Debug.Log("[Puerta] tile " + NombreSprite + " en " + h.mapa.name + " celda " + h.celda + " centro " + h.caja.center + " tamano " + h.caja.size);

        // la de mas arriba y mas a la izquierda
        var puerta = halladas.OrderByDescending(h => h.caja.center.y).ThenBy(h => h.caja.center.x).First();

        var viejo = GameObject.Find("Puerta_Nivel2");
        if (viejo != null) Object.DestroyImmediate(viejo);
        var go = new GameObject("Puerta_Nivel2");
        go.transform.position = new Vector3(puerta.caja.center.x, puerta.caja.center.y, 0f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(Mathf.Min(puerta.caja.size.x, 1.6f), puerta.caja.size.y);
        var p = go.AddComponent<Puerta>();
        p.escenaDestino = "Nivel2";

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log("[Puerta] Puerta_Nivel2 creada en " + go.transform.position + " (tamano " + col.size + ") y escena guardada.");
        Selection.activeGameObject = go;
    }
}
#endif
