#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menu: Esperanza > Crear escena Seleccion de Niveles
// Crea la escena "SeleccionNiveles" con el fondo y los 4 paneles de Ui del juego/menu/pestana_niveles,
// la agrega al Build Settings y conecta el boton "Niveles" de SeleccionModo.
public static class CrearSeleccionNiveles
{
    const string Carpeta = "Assets/Arte/Esperanza en el cielo/UI del juego/menu/pestaña_niveles/";
    const string Fondo = Carpeta + "fondo_niveles.png";
    const string Paneles = Carpeta + "paneles_niveles.png";
    const string HojaJV = "Assets/Arte/Esperanza en el cielo/UI del juego/menu/TODOS_los_botones/botones_jugar_volver&jugar.png";
    const string RutaEscena = "Assets/Scenes/SeleccionNiveles.unity";

    static readonly string[] EscenasNivel = { "Tutorial", "Nivel2", "Nivel3", "Nivel4Jefe" };

    [MenuItem("Esperanza/Crear escena Seleccion de Niveles")]
    public static void Crear()
    {
        PrepararSprites();
        EditorSceneManager.SaveOpenScenes();
        string escenaOriginal = EditorSceneManager.GetActiveScene().path;

        var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);
        var cam = Camera.main;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.05f, 0.12f);

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        var nav = canvasGO.AddComponent<MenuNavegacion>();

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Fondo
        var fondo = CrearImagen("FondoNiveles", canvasGO.transform, Cargar(Fondo, "fondo_niveles"));
        Estirar((RectTransform)fondo.transform);

        // Paneles 2x2
        float anchoPanel = 600f;
        Vector2[] pos = { new Vector2(-320, 165), new Vector2(320, 165), new Vector2(-320, -190), new Vector2(320, -190) };
        string[] nombres = { "Panel_Nivel1", "Panel_Nivel2", "Panel_Nivel3", "Panel_Nivel4" };
        var botones = new List<Button>();
        var candados = new GameObject[4];
        var selector = canvasGO.AddComponent<SelectorNiveles>();
        for (int i = 0; i < 4; i++)
        {
            var spr = Cargar(Paneles, "panel_nivel_" + (i + 1));
            var img = CrearImagen(nombres[i], canvasGO.transform, spr);
            var rt = (RectTransform)img.transform;
            rt.sizeDelta = new Vector2(anchoPanel, anchoPanel * spr.rect.height / spr.rect.width);
            rt.anchoredPosition = pos[i];
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            var cb = btn.colors;
            cb.normalColor = new Color(0.78f, 0.78f, 0.78f);
            cb.highlightedColor = Color.white;
            cb.selectedColor = Color.white;
            cb.pressedColor = new Color(0.6f, 0.6f, 0.6f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            img.gameObject.AddComponent<BotonResaltar>();
            img.gameObject.AddComponent<EfectoSeleccion>();
            UnityEventTools.AddIntPersistentListener(btn.onClick, selector.Seleccionar, i);
            botones.Add(btn);

            // Capa "Proximamente"
            var capa = CrearImagen("Candado", img.transform, null);
            capa.color = new Color(0f, 0f, 0f, 0.62f);
            capa.raycastTarget = false;
            Estirar((RectTransform)capa.transform);
            var txt = CrearTexto("Texto", capa.transform, "PRÓXIMAMENTE", 44, Color.white);
            Estirar((RectTransform)txt.transform);
            capa.gameObject.SetActive(false); // se activa sola al jugar si el nivel no existe
            candados[i] = capa.gameObject;
        }

        // Boton Volver (mismos sprites que el resto de menus)
        var volver = CrearImagen("BotonVolver", canvasGO.transform, Cargar(HojaJV, "botones_jugar_volver&jugar_1"));
        var vrt = (RectTransform)volver.transform;
        vrt.sizeDelta = new Vector2(330, 330f * volver.sprite.rect.height / volver.sprite.rect.width);
        vrt.anchoredPosition = new Vector2(0, -432);
        volver.preserveAspect = true;
        var vb = volver.gameObject.AddComponent<Button>();
        vb.targetGraphic = volver;
        vb.transition = Selectable.Transition.SpriteSwap;
        var rs = Cargar(HojaJV, "botones_jugar_volver&jugar_3");
        vb.spriteState = new SpriteState { highlightedSprite = rs, selectedSprite = rs, pressedSprite = Cargar(HojaJV, "botones_jugar_volver&jugar_5"), disabledSprite = rs };
        volver.gameObject.AddComponent<BotonResaltar>();
        UnityEventTools.AddPersistentListener(vb.onClick, nav.VolverSeleccion);

        selector.botones = botones.ToArray();
        selector.escenas = EscenasNivel;
        selector.candados = candados;

        // Navegacion con teclado: cuadricula 2x2 + Volver
        var todos = new List<Button>(botones) { vb };
        Navigation N(Selectable up, Selectable down, Selectable left, Selectable right) =>
            new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = up, selectOnDown = down, selectOnLeft = left, selectOnRight = right };
        botones[0].navigation = N(vb, botones[2], botones[1], botones[1]);
        botones[1].navigation = N(vb, botones[3], botones[0], botones[0]);
        botones[2].navigation = N(botones[0], vb, botones[3], botones[3]);
        botones[3].navigation = N(botones[1], vb, botones[2], botones[2]);
        vb.navigation = N(botones[2], botones[0], botones[2], botones[3]);

        var mt = canvasGO.AddComponent<MenuTeclado>();
        mt.botones = todos.ToArray();
        mt.escenaEscape = "SeleccionModo";

        EditorSceneManager.SaveScene(escena, RutaEscena);
        AgregarABuild(RutaEscena);

        // Conecta el boton Niveles de SeleccionModo
        var sm = EditorSceneManager.OpenScene("Assets/Scenes/SeleccionModo.unity", OpenSceneMode.Single);
        foreach (var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (b.gameObject.name != "BotonNiveles") continue;
            var mn = Object.FindFirstObjectByType<MenuNavegacion>();
            bool ya = false;
            for (int k = 0; k < b.onClick.GetPersistentEventCount(); k++)
                if (b.onClick.GetPersistentMethodName(k) == "AbrirNiveles") ya = true;
            if (!ya && mn != null) UnityEventTools.AddPersistentListener(b.onClick, mn.AbrirNiveles);
            EditorUtility.SetDirty(b);
        }
        EditorSceneManager.MarkSceneDirty(sm);
        EditorSceneManager.SaveScene(sm);

        if (!string.IsNullOrEmpty(escenaOriginal)) EditorSceneManager.OpenScene(escenaOriginal, OpenSceneMode.Single);
        Debug.Log("Escena SeleccionNiveles creada y agregada al Build Settings.");
    }

    static void PrepararSprites()
    {
        // Fondo: una sola imagen
        var imp = (TextureImporter)AssetImporter.GetAtPath(Fondo);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();

        // Paneles: 4 recortes (x, y desde abajo, ancho, alto) en la imagen de 1672x941
        imp = (TextureImporter)AssetImporter.GetAtPath(Paneles);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();
        var f = new SpriteDataProviderFactories();
        f.Init();
        var dp = f.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        var rects = new List<SpriteRect>();
        void Add(string n, int x, int y, int w, int h) => rects.Add(new SpriteRect { name = n, rect = new Rect(x, y, w, h), alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f), spriteID = GUID.Generate() });
        Add("panel_nivel_1", 12, 478, 813, 452);
        Add("panel_nivel_2", 840, 478, 819, 452);
        Add("panel_nivel_3", 12, 15, 813, 456);
        Add("panel_nivel_4", 840, 15, 819, 457);
        dp.SetSpriteRects(rects.ToArray());
        dp.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    static Sprite Cargar(string ruta, string nombre)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ruta))
            if (o is Sprite s && (s.name == nombre || nombre == "fondo_niveles")) return s;
        Debug.LogError("No encontre el sprite " + nombre + " en " + ruta);
        return null;
    }

    static Image CrearImagen(string nombre, Transform padre, Sprite sprite)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = false;
        return img;
    }

    static TMP_Text CrearTexto(string nombre, Transform padre, string texto, float tam, Color color)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = texto;
        t.fontSize = tam;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        return t;
    }

    static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void AgregarABuild(string ruta)
    {
        var lista = EditorBuildSettings.scenes.ToList();
        if (lista.Any(s => s.path == ruta)) return;
        int idx = lista.FindIndex(s => s.path.EndsWith("SeleccionModo.unity"));
        var nueva = new EditorBuildSettingsScene(ruta, true);
        if (idx >= 0) lista.Insert(idx + 1, nueva); else lista.Add(nueva);
        EditorBuildSettings.scenes = lista.ToArray();
    }
}
#endif
