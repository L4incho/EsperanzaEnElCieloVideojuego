#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public static class CrearVidaYEnemigo
{
    const string RutaHud = "Assets/Arte/Esperanza en el cielo/UI del juego/dentro_del_juego/HUD_Mateo_marco_barras_vida.png";
    const string RutaDummy = "Assets/Arte/Personajes/dummy/dummy_nuevo.png";
    const string RutaPrefab = "Assets/Prefabs/HUD_Vida.prefab";
    static readonly string[] Escenas = { "Tutorial", "Nivel2", "Nivel3", "Nivel4Jefe" };

    static Sprite Sub(string ruta, string nombre)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault(s => nombre == null || s.name == nombre);
    }

    [MenuItem("Esperanza/Crear HUD de vida y poner en todos los niveles")]
    public static void CrearHud()
    {
        var sprHud = Sub(RutaHud, null);
        if (sprHud == null) { Debug.LogError("[Vida] No encuentro el sprite del HUD"); return; }

        // ---- Prefab del HUD ----
        var raiz = new GameObject("HUD_Vida", typeof(RectTransform));
        var canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var esc = raiz.AddComponent<CanvasScaler>();
        esc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        esc.referenceResolution = new Vector2(1920, 1080);
        esc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        esc.matchWidthOrHeight = 0.5f;

        var cont = Hijo(raiz.transform, "Contenedor");
        cont.anchorMin = cont.anchorMax = new Vector2(0, 1);
        cont.pivot = new Vector2(0, 1);
        cont.anchoredPosition = new Vector2(30, -25);
        cont.sizeDelta = new Vector2(580, 140.4f);   // proporcion del arte: 2072 x 497

        // hueco interior del marco (en fracciones del sprite)
        Vector2 a = new Vector2(0.300f, 0.330f), b = new Vector2(0.925f, 0.520f);

        var fondo = Hijo(cont, "FondoBarra");
        Rellenar(fondo, a, b);
        var imgFondo = fondo.gameObject.AddComponent<Image>();
        imgFondo.color = new Color(0.05f, 0.07f, 0.12f, 0.9f);
        imgFondo.raycastTarget = false;

        var barra = Hijo(cont, "Barra");
        Rellenar(barra, a, b);
        var imgBarra = barra.gameObject.AddComponent<Image>();
        imgBarra.raycastTarget = false;

        var marco = Hijo(cont, "Marco");
        marco.anchorMin = Vector2.zero; marco.anchorMax = Vector2.one;
        marco.offsetMin = marco.offsetMax = Vector2.zero;
        var imgMarco = marco.gameObject.AddComponent<Image>();
        imgMarco.sprite = sprHud;
        imgMarco.raycastTarget = false;

        var hud = raiz.AddComponent<HudVida>();
        hud.barra = imgBarra;

        // Barra de espera del disparo (tecla J), debajo del marco
        var disp = Hijo(raiz.transform, "BarraDisparo");
        disp.anchorMin = disp.anchorMax = new Vector2(0, 1);
        disp.pivot = new Vector2(0, 1);
        disp.anchoredPosition = new Vector2(150, -175);
        disp.sizeDelta = new Vector2(300, 30);
        var fondoD = disp.gameObject.AddComponent<Image>();
        fondoD.color = new Color(0.04f, 0.06f, 0.12f, 0.85f);
        fondoD.raycastTarget = false;
        var relleno = Hijo(disp, "Relleno");
        relleno.anchorMin = Vector2.zero; relleno.anchorMax = Vector2.one;
        relleno.offsetMin = new Vector2(3, 3); relleno.offsetMax = new Vector2(-3, -3);
        var imgRelleno = relleno.gameObject.AddComponent<Image>();
        imgRelleno.raycastTarget = false;
        var txt = Hijo(disp, "Texto");
        txt.anchorMin = Vector2.zero; txt.anchorMax = Vector2.one;
        txt.offsetMin = txt.offsetMax = Vector2.zero;
        var t = txt.gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 20;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        t.text = "J  LISTO";
        hud.barraDisparo = imgRelleno;
        hud.textoDisparo = t;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        CrearGemas.AgregarBarraK(raiz);
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, RutaPrefab);
        Object.DestroyImmediate(raiz);

        // ---- Ponerlo en cada nivel ----
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        foreach (var n in Escenas)
        {
            string ruta = "Assets/Scenes/" + n + ".unity";
            if (!System.IO.File.Exists(ruta)) { Debug.LogWarning("[Vida] No existe " + ruta); continue; }
            var sc = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            foreach (var viejo in Object.FindObjectsByType<HudVida>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(viejo.gameObject);
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(p, sc);
            inst.name = "HUD_Vida";
            EditorSceneManager.MarkSceneDirty(sc);
            EditorSceneManager.SaveScene(sc);
            Debug.Log("[Vida] HUD puesto en " + n);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
    }

    [MenuItem("Esperanza/Crear dummy infectado en el Tutorial")]
    public static void CrearDummy()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Tutorial")
            sc = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);

        foreach (var v in Object.FindObjectsByType<EnemigoInfectado>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(v.gameObject);

        Sprite s0 = Sub(RutaDummy, "dummy_nuevo_0"), s2 = Sub(RutaDummy, "dummy_nuevo_2"), s3 = Sub(RutaDummy, "dummy_nuevo_3"),
               s6 = Sub(RutaDummy, "dummy_nuevo_6"), s7 = Sub(RutaDummy, "dummy_nuevo_7");
        if (s7 == null || s0 == null) { Debug.LogError("[Dummy] Faltan sprites del dummy"); return; }

        var mateo = Object.FindFirstObjectByType<PlayerController>();
        int capaSuelo = LayerMask.GetMask("Suelo");
        float escala = 2.6f;

        // buscar suelo firme delante de Mateo
        Physics2D.SyncTransforms();
        Vector2 pos = Vector2.zero; bool ok = false;
        float x0 = mateo != null ? mateo.transform.position.x : 0f;
        float y0 = mateo != null ? mateo.transform.position.y : 0f;
        foreach (float dx in new[] { 9f, 8f, 10f, 7f, 11f, 6f, 12f })
        {
            var h = Physics2D.Raycast(new Vector2(x0 + dx, y0 + 1.5f), Vector2.down, 20f, capaSuelo);
            if (h.collider != null && !h.collider.isTrigger)
            {
                pos = new Vector2(x0 + dx, h.point.y + 0.385f * escala + 0.02f);
                ok = true;
                Debug.Log("[Dummy] suelo en x=" + (x0 + dx) + " y=" + h.point.y + " (" + h.collider.name + ")");
                break;
            }
        }
        if (!ok) { Debug.LogError("[Dummy] No encontre suelo cerca de Mateo; colocalo a mano"); pos = new Vector2(x0 + 9f, y0); }

        var go = new GameObject("Dummy_Infectado");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * escala;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s7;
        var sm = mateo != null ? mateo.GetComponent<SpriteRenderer>() : null;
        if (sm != null) { sr.sortingLayerID = sm.sortingLayerID; sr.sortingOrder = sm.sortingOrder; }
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.40f, 0.76f);
        var en = go.AddComponent<EnemigoInfectado>();
        en.quieto = s7;
        en.caminar = new[] { s0, s3, s0, s6 }.Where(s => s != null).ToArray();
        en.ataque = s2 != null ? s2 : s7;
        en.capaSuelo = capaSuelo;

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log("[Dummy] Dummy_Infectado creado en " + pos + " y escena guardada.");
    }

    static RectTransform Hijo(Transform padre, string nombre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        g.transform.SetParent(padre, false);
        return g.GetComponent<RectTransform>();
    }

    static void Rellenar(RectTransform r, Vector2 a, Vector2 b)
    {
        r.anchorMin = a; r.anchorMax = b;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
#endif
