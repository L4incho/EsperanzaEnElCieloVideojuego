#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Menu: Esperanza > Crear escalera central
// Busca en el centro del Tilemap "Suelo" el piso de abajo y el segundo piso, abre un hueco de 2 casillas
// en el segundo piso y coloca una escalera funcional que conecta los dos.
public static class CrearEscaleraCentral
{
    const string RutaHoja = "Assets/Arte/Entorno/niveles/nivel 1 (tutorial)/laboratorio_spritesheet.png";
    const string NombreObjeto = "Escalera_Central";

    [MenuItem("Esperanza/Crear escalera central")]
    public static void Crear()
    {
        if (GameObject.Find(NombreObjeto) != null)
        {
            Debug.LogWarning("Ya existe " + NombreObjeto + " en la escena. Borrala si quieres crearla de nuevo.");
            return;
        }

        // El Tilemap del suelo es el que tiene colision (TilemapCollider2D)
        Tilemap tm = null;
        foreach (var t in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if (t.GetComponent<TilemapCollider2D>() != null) { tm = t; break; }
        if (tm == null) { Debug.LogError("No encontre un Tilemap con TilemapCollider2D en la escena."); return; }

        BoundsInt b = tm.cellBounds;
        Vector3 cs = tm.cellSize;
        Vector3 anc = Vector3.Scale(tm.tileAnchor, cs);

        // Rectangulo en el mundo de cada tile (segun el tamano real de su sprite)
        var tiles = new List<KeyValuePair<Vector3Int, Rect>>();
        foreach (var p in b.allPositionsWithin)
        {
            if (!tm.HasTile(p)) continue;
            var sp = tm.GetSprite(p);
            if (sp == null) continue;
            Vector3 piv = tm.CellToWorld(p) + anc;
            var bb = sp.bounds;
            tiles.Add(new KeyValuePair<Vector3Int, Rect>(p, new Rect(piv.x + bb.min.x, piv.y + bb.min.y, bb.size.x, bb.size.y)));
        }
        if (tiles.Count == 0) { Debug.LogError("El Tilemap no tiene tiles."); return; }

        float xMin = float.MaxValue, xMax = float.MinValue;
        foreach (var t in tiles) { xMin = Mathf.Min(xMin, t.Value.xMin); xMax = Mathf.Max(xMax, t.Value.xMax); }
        float xMed = (xMin + xMax) / 2f;

        // Desde el centro hacia afuera, busca una x con al menos dos pisos separados
        float xCentro = float.NaN, topePiso1 = 0, bajoPiso2 = 0, topePiso2 = 0;
        List<Vector3Int> celdasPiso2 = null;
        for (float d = 0; d <= (xMax - xMin) / 2f && celdasPiso2 == null; d += 0.5f)
        {
            foreach (float xs in new[] { xMed + d, xMed - d })
            {
                var cols = new List<KeyValuePair<Vector3Int, Rect>>();
                foreach (var t in tiles) if (t.Value.xMin <= xs && xs < t.Value.xMax) cols.Add(t);
                cols.Sort((p, q) => p.Value.yMin.CompareTo(q.Value.yMin));
                // agrupa por intervalos de y que se tocan
                var gr = new List<Vector2>(); // (yMin,yMax)
                foreach (var t in cols)
                {
                    if (gr.Count > 0 && t.Value.yMin <= gr[gr.Count - 1].y + 0.01f)
                        gr[gr.Count - 1] = new Vector2(gr[gr.Count - 1].x, Mathf.Max(gr[gr.Count - 1].y, t.Value.yMax));
                    else gr.Add(new Vector2(t.Value.yMin, t.Value.yMax));
                }
                if (gr.Count < 2) continue;
                var ultimo = gr[gr.Count - 1];
                var tilesPiso2 = new List<Vector3Int>();
                Rect r0 = default; bool primero = true;
                foreach (var t in cols) if (t.Value.yMax > ultimo.x + 0.01f) { tilesPiso2.Add(t.Key); if (primero) { r0 = t.Value; primero = false; } }
                if (tilesPiso2.Count == 0) continue;
                xCentro = r0.center.x;
                topePiso1 = gr[0].y; bajoPiso2 = ultimo.x; topePiso2 = ultimo.y;
                celdasPiso2 = tilesPiso2;
                break;
            }
        }
        if (celdasPiso2 == null)
        {
            Debug.LogError("No encontre dos pisos en el Tilemap. Tiles=" + tiles.Count + " x entre " + xMin + " y " + xMax);
            return;
        }

        float yBase = topePiso1;
        float yTope = topePiso2;
        float alto = yTope - yBase;
        float altoLibre = bajoPiso2 - yBase;
        Debug.Log("Escalera: x=" + xCentro + " piso1 top=" + topePiso1 + " piso2 y=" + bajoPiso2 + ".." + topePiso2 + " (espacio libre " + altoLibre + ")");
        if (altoLibre < 2.2f)
            Debug.LogWarning("El espacio entre pisos es de solo " + altoLibre + " unidades; Mateo mide ~2.1. Puede quedar justo.");

        // Abre el hueco: quita el/los tiles del segundo piso en esa x
        Undo.RegisterCompleteObjectUndo(tm, "Hueco para escalera");
        foreach (var c in celdasPiso2) tm.SetTile(c, null);

        // Objeto raiz con el area de la escalera
        var raiz = new GameObject(NombreObjeto);
        Undo.RegisterCreatedObjectUndo(raiz, "Crear escalera");
        raiz.transform.position = new Vector3(xCentro, yBase, 0f);
        var area = raiz.AddComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.size = new Vector2(2f, alto);
        area.offset = new Vector2(0f, alto / 2f);
        raiz.AddComponent<Escalera>().ajustarAreaAlSprite = false;

        // Visual: tope (laboratorio_spritesheet_53) y tramos repetidos (laboratorio_spritesheet_54)
        Sprite spTope = null, spTramo = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(RutaHoja))
        {
            if (o is Sprite sp)
            {
                if (sp.name == "laboratorio_spritesheet_53") spTope = sp;
                if (sp.name == "laboratorio_spritesheet_54") spTramo = sp;
            }
        }
        var visual = new GameObject("Visual");
        visual.transform.SetParent(raiz.transform, false);
        if (spTope != null) CrearPieza(visual.transform, spTope, new Vector3(0f, alto - 1f, 0f));
        if (spTramo != null)
        {
            int n = Mathf.CeilToInt((alto - 2f) / 2f);
            for (int i = 0; i < n; i++)
                CrearPieza(visual.transform, spTramo, new Vector3(0f, alto - 3f - 2f * i, 0f));
        }

        // Plataforma de un solo sentido sobre el hueco: Mateo la cruza hacia arriba pero puede caminar encima
        var plat = new GameObject("PlataformaSuperior");
        plat.transform.SetParent(raiz.transform, false);
        int capaSuelo = LayerMask.NameToLayer("Suelo");
        if (capaSuelo >= 0) plat.layer = capaSuelo;
        var pc = plat.AddComponent<BoxCollider2D>();
        pc.size = new Vector2(2f, 0.2f);
        pc.offset = new Vector2(0f, alto - 0.1f);
        pc.usedByEffector = true;
        plat.AddComponent<PlataformaEscalera>().areaEscalera = area;
        var ef = plat.AddComponent<PlatformEffector2D>();
        ef.useOneWay = true;
        ef.surfaceArc = 170f;

        Selection.activeGameObject = raiz;
        EditorSceneManager.MarkSceneDirty(raiz.scene);
        Debug.Log("Escalera creada en x=" + xCentro + ", de y=" + yBase + " a y=" + yTope + ". Guarda la escena (Ctrl+S).");
    }

    static void CrearPieza(Transform padre, Sprite sprite, Vector3 posLocal)
    {
        var go = new GameObject(sprite.name);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = posLocal;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -5; // detras de los tiles (0) y de Mateo (10), delante del fondo (-10)
    }

    // Devuelve los tramos continuos de tiles (yMin, yMax) de una columna, de abajo hacia arriba
    static List<Vector2Int> GruposColumna(Tilemap tm, int x, BoundsInt b)
    {
        var res = new List<Vector2Int>();
        int inicio = int.MinValue;
        for (int y = b.yMin; y <= b.yMax; y++)
        {
            bool hay = y < b.yMax && tm.HasTile(new Vector3Int(x, y, 0));
            if (hay && inicio == int.MinValue) inicio = y;
            if (!hay && inicio != int.MinValue) { res.Add(new Vector2Int(inicio, y - 1)); inicio = int.MinValue; }
        }
        return res;
    }
}
#endif
