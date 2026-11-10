#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

// Corta el spritesheet de microchips, crea el prefab, agrega el contador al HUD y pone microchips en el Tutorial.
public static class CrearMicrochips
{
    const string Ruta = "Assets/Arte/Objetos/microchip_spritesheet.png";
    const string RutaPrefab = "Assets/Prefabs/Microchip.prefab";
    const string RutaHud = "Assets/Prefabs/HUD_Vida.prefab";

    static Sprite[] Cortar()
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(Ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsToUnits = 550f;
        imp.filterMode = FilterMode.Point;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.maxTextureSize = 4096;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        float[] cx = { 292f, 725f, 1088f, 1434f, 1862f };
        var meta = new SpriteMetaData[cx.Length];
        for (int i = 0; i < cx.Length; i++)
            meta[i] = new SpriteMetaData { name = "microchip_" + i, rect = new Rect(cx[i] - 200f, 168f, 400f, 406f), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) };
#pragma warning disable 618
        imp.spritesheet = meta;
#pragma warning restore 618
        imp.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(Ruta).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    public static void AgregarContador(GameObject raiz, Sprite icono)
    {
        var hud = raiz.GetComponent<HudVida>();
        if (hud == null) return;
        var viejo = raiz.transform.Find("ContadorMicrochips");
        if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

        var c = Hijo(raiz.transform, "ContadorMicrochips");
        c.anchorMin = c.anchorMax = new Vector2(0, 1);
        c.pivot = new Vector2(0, 1);
        c.anchoredPosition = new Vector2(630, -40);
        c.sizeDelta = new Vector2(190, 70);
        var fondo = c.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.04f, 0.08f, 0.12f, 0.8f);
        fondo.raycastTarget = false;

        var ic = Hijo(c, "Icono");
        ic.anchorMin = ic.anchorMax = new Vector2(0, 0.5f);
        ic.pivot = new Vector2(0, 0.5f);
        ic.anchoredPosition = new Vector2(8, 0);
        ic.sizeDelta = new Vector2(56, 56);
        var img = ic.gameObject.AddComponent<Image>();
        img.sprite = icono; img.preserveAspect = true; img.raycastTarget = false;

        var tx = Hijo(c, "Texto");
        tx.anchorMin = Vector2.zero; tx.anchorMax = Vector2.one;
        tx.offsetMin = new Vector2(70, 0); tx.offsetMax = new Vector2(-8, 0);
        var t = tx.gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 40; t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = new Color(0.55f, 1f, 0.6f);
        t.raycastTarget = false;
        t.text = "x 0";
        hud.textoMicrochips = t;
    }

    [MenuItem("Esperanza/Crear microchips coleccionables")]
    public static void Crear()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        var spr = Cortar();
        if (spr.Length < 5) { Debug.LogError("[Microchips] No se pudo cortar el spritesheet"); return; }

        // Prefab
        var raiz = new GameObject("Microchip");
        var col = raiz.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.4f;
        var vis = new GameObject("Visual");
        vis.transform.SetParent(raiz.transform, false);
        var sr = vis.AddComponent<SpriteRenderer>();
        sr.sprite = spr[0]; sr.sortingOrder = 5;
        var mc = raiz.AddComponent<Microchip>();
        mc.cuadros = spr;
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab) != null) AssetDatabase.DeleteAsset(RutaPrefab);
        PrefabUtility.SaveAsPrefabAsset(raiz, RutaPrefab);
        Object.DestroyImmediate(raiz);

        // HUD
        var h = PrefabUtility.LoadPrefabContents(RutaHud);
        AgregarContador(h, spr[0]);
        PrefabUtility.SaveAsPrefabAsset(h, RutaHud);
        PrefabUtility.UnloadPrefabContents(h);

        // Tutorial
        var sc = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        foreach (var v in Object.FindObjectsByType<Microchip>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(v.gameObject);
        var mateo = Object.FindFirstObjectByType<PlayerController>();
        Physics2D.SyncTransforms();
        int capa = LayerMask.GetMask("Suelo");
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab);
        float x0 = mateo != null ? mateo.transform.position.x : 0f, y0 = mateo != null ? mateo.transform.position.y : 0f;
        int n = 0;
        foreach (float dx in new[] { 4f, 6.5f, 9f, 12f, 15f, 18f, 21f, 24f })
        {
            var h2 = Physics2D.Raycast(new Vector2(x0 + dx, y0 + 1.5f), Vector2.down, 6f, capa);
            if (h2.collider == null || h2.collider.isTrigger) continue;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, sc);
            inst.name = "Microchip_" + (++n).ToString("00");
            inst.transform.position = new Vector3(x0 + dx, h2.point.y + 0.9f, 0f);
        }
        Debug.Log("[Microchips] " + n + " microchips puestos en el Tutorial.");
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
    }

    static RectTransform Hijo(Transform padre, string nombre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        g.transform.SetParent(padre, false);
        return g.GetComponent<RectTransform>();
    }
}
#endif
