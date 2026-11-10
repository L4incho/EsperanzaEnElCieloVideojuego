#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;

// Menu: Esperanza > Aplicar botones del menu
// 1) Corta botones_menu.png en 18 sprites (6 botones x 3 estados: Normal, Resaltado, Presionado).
// 2) En las escenas MenuPrincipal y SeleccionModo cambia los botones por los nuevos sprites con efecto
//    (Sprite Swap), quita los textos (los sprites ya traen la palabra) y activa el control con teclado.
public static class AplicarBotonesMenu
{
    const string Carpeta = "Assets/Arte/Esperanza en el cielo/UI del juego/menu/TODOS_los_botones/";
    const string HojaMenu = Carpeta + "botones_menu.png";
    const string HojaJV = Carpeta + "botones_jugar_volver&jugar.png";

    static readonly string[] Nombres = { "JUGAR", "CONTINUAR", "NIVELES", "OPCIONES", "CREDITOS", "SALIR" };
    static readonly string[] Estados = { "Normal", "Resaltado", "Presionado" };
    // Filas (pixel superior, pixel inferior) medidas desde arriba en la imagen de 1536x1024
    static readonly int[,] Filas = { { 96, 243 }, { 251, 394 }, { 402, 541 }, { 545, 678 }, { 682, 816 }, { 822, 961 } };
    static readonly int[] CentrosX = { 314, 781, 1240 };
    const int AnchoSprite = 404;

    struct Config { public string hoja, normal, resaltado, presionado; public float ancho; }

    [MenuItem("Esperanza/Aplicar botones del menu")]
    public static void Aplicar()
    {
        CortarHojaMenu();

        var cfg = new Dictionary<string, Config>
        {
            { "BotonJugar", Menu("JUGAR") },
            { "BotonConfiguracion", Menu("OPCIONES") },
            { "BotonSalir", Menu("SALIR") },
            { "BotonHistoria", Menu("JUGAR") },
            { "BotonNiveles", Menu("NIVELES") },
            { "BotonVolver", new Config { hoja = HojaJV, normal = "botones_jugar_volver&jugar_1", resaltado = "botones_jugar_volver&jugar_3", presionado = "botones_jugar_volver&jugar_5", ancho = 360f } },
        };

        EditorSceneManager.SaveOpenScenes();
        string escenaOriginal = EditorSceneManager.GetActiveScene().path;

        foreach (string ruta in new[] { "Assets/Scenes/MenuPrincipal.unity", "Assets/Scenes/SeleccionModo.unity" })
        {
            var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            var botones = new List<Button>();
            foreach (var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if (!cfg.TryGetValue(b.gameObject.name, out var c)) continue;
                ConfigurarBoton(b, c);
                botones.Add(b);
            }
            // De arriba hacia abajo
            botones = botones.OrderByDescending(b => ((RectTransform)b.transform).anchoredPosition.y).ToList();
            for (int i = 0; i < botones.Count; i++)
            {
                var nav = new Navigation { mode = Navigation.Mode.Explicit };
                nav.selectOnUp = botones[(i - 1 + botones.Count) % botones.Count];
                nav.selectOnDown = botones[(i + 1) % botones.Count];
                botones[i].navigation = nav;
                EditorUtility.SetDirty(botones[i]);
            }

            var canvas = Object.FindFirstObjectByType<Canvas>();
            var mt = canvas.GetComponent<MenuTeclado>();
            if (mt == null) mt = canvas.gameObject.AddComponent<MenuTeclado>();
            mt.botones = botones.ToArray();
            mt.escapeVuelveAlMenu = ruta.EndsWith("SeleccionModo.unity");
            EditorUtility.SetDirty(mt);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("Botones aplicados en " + ruta + " (" + botones.Count + " botones).");
        }

        if (!string.IsNullOrEmpty(escenaOriginal)) EditorSceneManager.OpenScene(escenaOriginal, OpenSceneMode.Single);
    }

    static Config Menu(string nombre)
    {
        return new Config
        {
            hoja = HojaMenu,
            normal = "btn_" + nombre + "_Normal",
            resaltado = "btn_" + nombre + "_Resaltado",
            presionado = "btn_" + nombre + "_Presionado",
            ancho = 310f
        };
    }

    static Sprite Buscar(string hoja, string nombre)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(hoja))
            if (o is Sprite s && s.name == nombre) return s;
        Debug.LogError("No encontre el sprite " + nombre + " en " + hoja);
        return null;
    }

    static void ConfigurarBoton(Button b, Config c)
    {
        var normal = Buscar(c.hoja, c.normal);
        var resaltado = Buscar(c.hoja, c.resaltado);
        var presionado = Buscar(c.hoja, c.presionado);
        if (normal == null || resaltado == null || presionado == null) return;

        var img = b.GetComponent<Image>();
        img.sprite = normal;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = Color.white;
        b.targetGraphic = img;

        b.transition = Selectable.Transition.SpriteSwap;
        b.spriteState = new SpriteState
        {
            highlightedSprite = resaltado,
            selectedSprite = resaltado,
            pressedSprite = presionado,
            disabledSprite = presionado
        };

        var rt = (RectTransform)b.transform;
        float alto = c.ancho * normal.rect.height / normal.rect.width;
        rt.sizeDelta = new Vector2(c.ancho, alto);

        // Quita los textos: los sprites ya tienen la palabra
        for (int i = b.transform.childCount - 1; i >= 0; i--)
        {
            var hijo = b.transform.GetChild(i);
            if (hijo.GetComponent<TMPro.TMP_Text>() != null || hijo.GetComponent<Text>() != null)
                Object.DestroyImmediate(hijo.gameObject);
        }

        if (b.GetComponent<BotonResaltar>() == null) b.gameObject.AddComponent<BotonResaltar>();
        EditorUtility.SetDirty(b);
        EditorUtility.SetDirty(img);
    }

    static void CortarHojaMenu()
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(HojaMenu);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();

        var f = new SpriteDataProviderFactories();
        f.Init();
        var dp = f.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();

        const int altoImg = 1024;
        var rects = new List<SpriteRect>();
        for (int fila = 0; fila < Nombres.Length; fila++)
        {
            int arriba = Filas[fila, 0], abajo = Filas[fila, 1];
            int h = abajo - arriba + 1;
            int y = altoImg - (abajo + 1);
            for (int col = 0; col < 3; col++)
            {
                rects.Add(new SpriteRect
                {
                    name = "btn_" + Nombres[fila] + "_" + Estados[col],
                    rect = new Rect(CentrosX[col] - AnchoSprite / 2, y, AnchoSprite, h),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate()
                });
            }
        }
        dp.SetSpriteRects(rects.ToArray());
        var ids = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();
        AssetDatabase.Refresh();
    }
}
#endif
