#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

// Crea las gemas roja y azul (prefabs), conecta los trajes en Mateo, agrega la barra "K" al HUD y las coloca en el Tutorial.
public static class CrearGemas
{
    const string PrefabVerde = "Assets/Prefabs/GemaVerde.prefab";
    const string PrefabRoja = "Assets/Prefabs/GemaRoja.prefab";
    const string PrefabAzul = "Assets/Prefabs/GemaAzul.prefab";
    const string SprRoja = "Assets/Arte/Objetos/gema_roja.png";
    const string SprAzul = "Assets/Arte/Objetos/gema_azul.png";
    const string CtrlRojo = "Assets/Arte/Personajes/mateo/Animations/MateoRojo.overrideController";
    const string CtrlAzul = "Assets/Arte/Personajes/mateo/Animations/MateoAzul.overrideController";
    const string RutaHud = "Assets/Prefabs/HUD_Vida.prefab";

    static Sprite Spr(string ruta)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault();
    }

    static void HacerPrefab(string origen, string destino, string sprite, TipoGema tipo)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(destino) != null) AssetDatabase.DeleteAsset(destino);
        AssetDatabase.CopyAsset(origen, destino);
        var raiz = PrefabUtility.LoadPrefabContents(destino);
        raiz.name = tipo == TipoGema.Roja ? "GemaRoja" : "GemaAzul";
        var anim = raiz.GetComponent<Animator>();
        if (anim != null) Object.DestroyImmediate(anim);   // la animacion del Animator es la de la gema verde
        var g = raiz.GetComponent<GemaVerde>();
        g.tipo = tipo;
        var vis = raiz.GetComponentInChildren<SpriteRenderer>();
        vis.sprite = Spr(sprite);
        PrefabUtility.SaveAsPrefabAsset(raiz, destino);
        PrefabUtility.UnloadPrefabContents(raiz);
    }

    public static void AgregarBarraK(GameObject raiz)
    {
        var hud = raiz.GetComponent<HudVida>();
        if (hud == null) return;
        var viejo = raiz.transform.Find("BarraHabilidad");
        if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

        var disp = Hijo(raiz.transform, "BarraHabilidad");
        disp.anchorMin = disp.anchorMax = new Vector2(0, 1);
        disp.pivot = new Vector2(0, 1);
        disp.anchoredPosition = new Vector2(150, -215);
        disp.sizeDelta = new Vector2(300, 30);
        var fondo = disp.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.04f, 0.06f, 0.12f, 0.85f);
        fondo.raycastTarget = false;
        var relleno = Hijo(disp, "Relleno");
        relleno.anchorMin = Vector2.zero; relleno.anchorMax = Vector2.one;
        relleno.offsetMin = new Vector2(3, 3); relleno.offsetMax = new Vector2(-3, -3);
        var img = relleno.gameObject.AddComponent<Image>();
        img.raycastTarget = false;
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
        t.text = "K";
        hud.contenedorHabilidad = disp.gameObject;
        hud.barraHabilidad = img;
        hud.textoHabilidad = t;
        disp.gameObject.SetActive(false);
    }

    [MenuItem("Esperanza/Crear gemas roja y azul (K)")]
    public static void Crear()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabVerde) == null) { Debug.LogError("[Gemas] No existe " + PrefabVerde); return; }
        if (Spr(SprRoja) == null || Spr(SprAzul) == null) { Debug.LogError("[Gemas] Faltan sprites de las gemas"); return; }
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        HacerPrefab(PrefabVerde, PrefabRoja, SprRoja, TipoGema.Roja);
        HacerPrefab(PrefabVerde, PrefabAzul, SprAzul, TipoGema.Azul);

        // Barra K en el prefab del HUD (se actualiza en todos los niveles porque es el mismo prefab)
        var hudRaiz = PrefabUtility.LoadPrefabContents(RutaHud);
        AgregarBarraK(hudRaiz);
        PrefabUtility.SaveAsPrefabAsset(hudRaiz, RutaHud);
        PrefabUtility.UnloadPrefabContents(hudRaiz);

        // Tutorial
        var sc = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        var mateo = Object.FindFirstObjectByType<PlayerController>();
        if (mateo != null)
        {
            mateo.animatorTrajeRojo = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CtrlRojo);
            mateo.animatorTrajeAzul = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CtrlAzul);
            EditorUtility.SetDirty(mateo);
        }
        else Debug.LogError("[Gemas] No hay Mateo en el Tutorial");

        foreach (var g in Object.FindObjectsByType<GemaVerde>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (g.tipo != TipoGema.Verde) Object.DestroyImmediate(g.gameObject);

        var venda = GameObject.Find("Venda");
        var boti = GameObject.Find("Botiquin");
        Vector3 baseP = boti != null ? boti.transform.position : (venda != null ? venda.transform.position : (mateo != null ? mateo.transform.position : Vector3.zero));
        Physics2D.SyncTransforms();
        int capa = LayerMask.GetMask("Suelo");
        float[] dx = { 3.5f, 6f };
        TipoGema[] tipos = { TipoGema.Roja, TipoGema.Azul };
        string[] rutas = { PrefabRoja, PrefabAzul };
        for (int i = 0; i < 2; i++)
        {
            Vector3 p = new Vector3(baseP.x + dx[i], baseP.y, 0f);
            var h = Physics2D.Raycast(new Vector2(p.x, baseP.y + 1.5f), Vector2.down, 6f, capa);
            if (h.collider != null) p.y = h.point.y + 0.9f;
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(rutas[i]);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, sc);
            inst.name = tipos[i] == TipoGema.Roja ? "GemaRoja_01" : "GemaAzul_01";
            inst.transform.position = p;
            Debug.Log("[Gemas] " + inst.name + " en " + p);
        }
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        // Los trajes de Mateo tambien en los demas niveles
        foreach (var n in new[] { "Nivel2", "Nivel3", "Nivel4Jefe" })
        {
            string ruta = "Assets/Scenes/" + n + ".unity";
            if (!System.IO.File.Exists(ruta)) continue;
            var s2 = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            var m2 = Object.FindFirstObjectByType<PlayerController>();
            if (m2 == null) continue;
            m2.animatorTrajeRojo = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CtrlRojo);
            m2.animatorTrajeAzul = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CtrlAzul);
            if (m2.animatorTrajeVerde == null)
                m2.animatorTrajeVerde = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Arte/Personajes/mateo/Animations/MateoVerde.overrideController");
            EditorUtility.SetDirty(m2);
            EditorSceneManager.MarkSceneDirty(s2);
            EditorSceneManager.SaveScene(s2);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        Debug.Log("[Gemas] Listo: gemas roja y azul creadas, trajes conectados y escena guardada.");
    }

    static RectTransform Hijo(Transform padre, string nombre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        g.transform.SetParent(padre, false);
        return g.GetComponent<RectTransform>();
    }
}
#endif
