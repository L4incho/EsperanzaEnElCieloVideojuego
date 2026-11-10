#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Nivel 2: la plataforma suelta de la celda (-3,0) de "Suelo" pasa a ser una plataforma que sube hasta el nivel de las plataformas temporales y baja a su lugar.
// Menu: Esperanza > Plataforma movil vertical Nivel 2
public static class CrearPlataformaVerticalN2
{
    static float CimaTile(Tilemap tm, Vector3Int c, out Vector3 pivote)
    {
        var sp = tm.GetSprite(c); var m = tm.GetTransformMatrix(c);
        pivote = tm.transform.TransformPoint(tm.CellToLocal(c) + Vector3.Scale(tm.tileAnchor, tm.cellSize)) + new Vector3(m.m03, m.m13, 0f);
        return pivote.y + sp.bounds.max.y * m.lossyScale.y;
    }

    [MenuItem("Esperanza/Plataforma movil vertical Nivel 2")]
    public static void Crear()
    {
        var esc = SceneManager.GetActiveScene();
        var suelo = GameObject.Find("Suelo");
        var tm = suelo.GetComponent<Tilemap>();
        var go0 = GameObject.Find("PlataformaVertical_N2");
        // la plataforma marcada: la tile de Suelo mas cercana a este punto del mundo (x=-4.9, y=-1.8)
        var objetivo = new Vector2(-4.9f, -1.8f);
        var celda = new Vector3Int(0, 0, 0); float mejor = 1e9f;
        foreach (var p in tm.cellBounds.allPositionsWithin)
        {
            if (!tm.HasTile(p)) continue;
            Vector3 pv; float cm = CimaTile(tm, p, out pv);
            var spp = tm.GetSprite(p); if (spp == null) continue;
            var centro = new Vector2(pv.x + spp.bounds.center.x, pv.y + spp.bounds.center.y);
            float d = Vector2.Distance(centro, objetivo);
            if (d < mejor) { mejor = d; celda = p; }
        }
        if (go0 == null) Debug.Log("[PlatVert] tile elegida " + celda + " a " + mejor.ToString("F2") + " m del objetivo");
        var go = GameObject.Find("PlataformaVertical_N2");
        if (go == null)
        {
            if (!tm.HasTile(celda)) { Debug.LogError("[PlatVert] no hay tile en " + celda); return; }
            // nivel de las plataformas temporales = la cima de su sprite
            float cimaTemp = float.NaN; Vector3Int cTemp = default;
            foreach (var p in tm.cellBounds.allPositionsWithin)
            {
                var t = tm.GetTile<Tile>(p);
                if (t != null && t.gameObject != null && t.gameObject.name.Contains("Temporal")) { cTemp = p; cimaTemp = CimaTile(tm, p, out _); break; }
            }
            if (float.IsNaN(cimaTemp)) { Debug.LogError("[PlatVert] no encontre plataformas temporales"); return; }
            Vector3 piv; float cima = CimaTile(tm, celda, out piv);
            var sp = tm.GetSprite(celda); var m = tm.GetTransformMatrix(celda);
            var trend = suelo.GetComponent<TilemapRenderer>();

            go = new GameObject("PlataformaVertical_N2");
            go.layer = suelo.layer;
            go.transform.position = piv;
            go.transform.localScale = m.lossyScale;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sp;
            if (trend != null) { sr.sortingLayerID = trend.sortingLayerID; sr.sortingOrder = trend.sortingOrder; }
            int n = sp.GetPhysicsShapeCount();
            if (n > 0)
            {
                var pc = go.AddComponent<PolygonCollider2D>(); pc.pathCount = n;
                var pts = new System.Collections.Generic.List<Vector2>();
                for (int i = 0; i < n; i++) { sp.GetPhysicsShape(i, pts); pc.SetPath(i, pts.ToArray()); }
            }
            else { var bc = go.AddComponent<BoxCollider2D>(); bc.size = sp.bounds.size; bc.offset = sp.bounds.center; }
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            float sube = cimaTemp - cima;
            go.AddComponent<PlataformaMovil>();
            var pm = go.GetComponent<PlataformaMovil>();
            pm.puntoA = new Vector2(piv.x, piv.y);            // abajo (donde esta ahora)
            pm.puntoB = new Vector2(piv.x, piv.y + sube);     // arriba (nivel de las temporales)
            tm.SetTile(celda, null);
            Debug.Log("[PlatVert] sube " + sube.ToString("F2") + " m: de y=" + piv.y.ToString("F2") + " a y=" + (piv.y + sube).ToString("F2") + " (temporal en celda " + cTemp + ")");
        }
        var pm2 = go.GetComponent<PlataformaMovil>();
        pm2.velocidad = 2f; pm2.pausaEnExtremos = 0.8f; pm2.soloConPasajero = true;
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(esc);
        EditorSceneManager.SaveScene(esc);
    }
}

[InitializeOnLoad]
public static class PruebaPlataformaVerticalArranque
{
    const string Clave = "platv_prueba";
    static PruebaPlataformaVerticalArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            {
                EditorPrefs.SetBool(Clave, false);
                new GameObject("PruebaPlatV").AddComponent<PruebaPlataformaVerticalRunner>();
            }
        };
    }
    [MenuItem("Esperanza/PRUEBA plataforma vertical (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
