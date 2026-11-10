#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Nivel 2: pone el disparador de la puerta (tecla W, igual que en el Tutorial) sobre la puerta del extremo derecho y lleva al Nivel 3.
public static class CrearPuertaNivel3
{
    const string NombreSprite = "laboratorio_spritesheet_12";
    public static float PX = 20.2f, PY = 22.4f, AN = 1.2f, AL = 1.6f;

    [MenuItem("Esperanza/ZZ Crear puerta al Nivel 3")]
    public static void Crear()
    {
        var sc = EditorSceneManager.GetActiveScene();
        var halladas = new List<Bounds>();
        foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                var spr = tm.GetSprite(pos);
                if (spr == null || spr.name != NombreSprite) continue;
                var mat = tm.GetTransformMatrix(pos);
                Vector3 esc = Vector3.Scale(tm.transform.lossyScale, mat.lossyScale);
                Vector3 centro = tm.transform.TransformPoint(tm.CellToLocal(pos) + Vector3.Scale(tm.tileAnchor, tm.cellSize)) + (Vector3)mat.GetColumn(3);
                Vector3 e = Vector3.Scale(spr.bounds.size, esc), off = Vector3.Scale(spr.bounds.center, esc);
                halladas.Add(new Bounds(centro + off, new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), 1f)));
                Debug.Log("[PuertaN3] tile " + tm.name + " " + pos + " centro " + (centro + off).ToString("F2") + " tamano " + e.ToString("F2"));
            }
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (sr.sprite != null && sr.sprite.name == NombreSprite) { halladas.Add(sr.bounds); Debug.Log("[PuertaN3] SpriteRenderer " + sr.name + " " + sr.bounds.center.ToString("F2")); }
        // En el Nivel 2 la puerta es el panel amarillo de la parte alta derecha (posicion estimada; se ajusta con X/Y/ancho/alto)
        var puerta = new Bounds(new Vector3(PX, PY, 0f), new Vector3(AN, AL, 1f));
        var viejo = GameObject.Find("Puerta_Nivel3");
        if (viejo != null) Object.DestroyImmediate(viejo);
        var go = new GameObject("Puerta_Nivel3");
        go.transform.position = new Vector3(puerta.center.x, puerta.center.y, 0f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(Mathf.Min(puerta.size.x, 1.6f), puerta.size.y);
        var p = go.AddComponent<Puerta>();
        p.escenaDestino = "Nivel3";
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log("[PuertaN3] Puerta_Nivel3 creada en " + go.transform.position.ToString("F2") + " tamano " + col.size.ToString("F2") + " -> Nivel3");
        Selection.activeGameObject = go;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Frame(new Bounds(go.transform.position, new Vector3(9f, 6f, 1f)), true);
    }
}
#endif
