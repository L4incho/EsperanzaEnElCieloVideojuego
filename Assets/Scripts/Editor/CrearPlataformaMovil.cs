#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Nivel 2: convierte la plataforma suelta (tile en la celda 0,-2 de "Suelo") en una plataforma que va de derecha a izquierda y regresa.
// Menu: Esperanza > Plataforma movil Nivel 2
public static class CrearPlataformaMovilN2
{
    const float XDerecha = 10.9f, XIzquierda = -4.2f;   // centro de la plataforma en cada extremo del recorrido

    [MenuItem("Esperanza/Plataforma movil Nivel 2")]
    public static void Crear()
    {
        var esc = SceneManager.GetActiveScene();
        var suelo = GameObject.Find("Suelo");
        if (suelo == null) { Debug.LogError("[PlatMovil] no encontre la capa Suelo"); return; }
        var tm = suelo.GetComponent<Tilemap>();
        var celda = new Vector3Int(0, -2, 0);

        var existente = GameObject.Find("PlataformaMovil_N2");
        if (existente == null)
        {
            if (!tm.HasTile(celda)) { Debug.LogError("[PlatMovil] no hay tile en la celda " + celda); return; }
            var sp = tm.GetSprite(celda);
            var m = tm.GetTransformMatrix(celda);
            var trend = suelo.GetComponent<TilemapRenderer>();
            Vector3 pos = tm.transform.TransformPoint(tm.CellToLocal(celda) + Vector3.Scale(tm.tileAnchor, tm.cellSize)) + new Vector3(m.m03, m.m13, 0f);

            var go = new GameObject("PlataformaMovil_N2");
            go.layer = suelo.layer;
            go.transform.position = pos;
            go.transform.localScale = m.lossyScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            if (trend != null) { sr.sortingLayerID = trend.sortingLayerID; sr.sortingOrder = trend.sortingOrder; }

            // colision = la forma fisica del sprite (igual que cuando era tile)
            int n = sp.GetPhysicsShapeCount();
            if (n > 0)
            {
                var pc = go.AddComponent<PolygonCollider2D>();
                pc.pathCount = n;
                var pts = new System.Collections.Generic.List<Vector2>();
                for (int i = 0; i < n; i++) { sp.GetPhysicsShape(i, pts); pc.SetPath(i, pts.ToArray()); }
            }
            else
            {
                var bc = go.AddComponent<BoxCollider2D>();
                bc.size = sp.bounds.size; bc.offset = sp.bounds.center;
            }
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            existente = go;
            tm.SetTile(celda, null);   // ya no es tile
        }
        var pm = existente.GetComponent<PlataformaMovil>() ?? existente.AddComponent<PlataformaMovil>();
        float y = existente.transform.position.y;
        // arranca en el extremo derecho y va hacia la izquierda
        pm.puntoA = new Vector2(XDerecha, y);
        pm.puntoB = new Vector2(XIzquierda, y);
        pm.velocidad = 2.2f;
        pm.pausaEnExtremos = 0.6f;
        existente.transform.position = new Vector3(XDerecha, y, existente.transform.position.z);
        EditorUtility.SetDirty(existente);
        EditorSceneManager.MarkSceneDirty(esc);
        EditorSceneManager.SaveScene(esc);
        Debug.Log("[PlatMovil] lista en y=" + y.ToString("F2") + ", recorrido x " + XDerecha + " -> " + XIzquierda);
    }
}

// Prueba en Play: la plataforma se mueve y se lleva a Mateo. Escribe prueba_plataforma.txt
[InitializeOnLoad]
public static class PruebaPlataformaArranque
{
    const string Clave = "plat_prueba";
    static PruebaPlataformaArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            {
                EditorPrefs.SetBool(Clave, false);
                new GameObject("PruebaPlataforma").AddComponent<PruebaPlataformaRunner>();
            }
        };
    }
    [MenuItem("Esperanza/PRUEBA plataforma movil (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}

#endif
