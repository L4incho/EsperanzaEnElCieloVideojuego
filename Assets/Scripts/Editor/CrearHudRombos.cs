#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

// Cambia el HUD (prefab HUD_Vida) a barras de rombos: vida (verde), disparo J (azul claro) y habilidad K (color de la gema).
public static class CrearHudRombos
{
    const string Carpeta = "Assets/Arte/HUD";
    const string RutaPrefab = "Assets/Prefabs/HUD_Vida.prefab";

    static Sprite Importar(string nombre)
    {
        string ruta = Carpeta + "/" + nombre + ".png";
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (imp == null) { Debug.LogError("[HUD] Falta " + ruta); return null; }
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 100;
        imp.filterMode = FilterMode.Point;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault();
    }

    static RectTransform Hijo(Transform padre, string nombre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        g.transform.SetParent(padre, false);
        return g.GetComponent<RectTransform>();
    }

    static void EsquinaSup(RectTransform r, Vector2 pos, Vector2 tam)
    {
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);
        r.anchoredPosition = pos;
        r.sizeDelta = tam;
    }

    // Barra pequena con letra: devuelve la raiz; "segmentos" es donde se dibujan los rombos
    static GameObject Fila(Transform raiz, string nombre, string letra, Vector2 pos, out RectTransform segmentos)
    {
        var viejo = raiz.Find(nombre);
        if (viejo != null) Object.DestroyImmediate(viejo.gameObject);
        var fila = Hijo(raiz, nombre);
        EsquinaSup(fila, pos, new Vector2(330, 24));

        var caja = Hijo(fila, "Letra");
        EsquinaSup(caja, Vector2.zero, new Vector2(30, 22));
        var fondo = caja.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.10f, 0.12f, 0.20f, 0.95f);
        fondo.raycastTarget = false;
        var borde = caja.gameObject.AddComponent<Outline>();
        borde.effectColor = new Color(0.47f, 0.55f, 0.78f, 1f);
        borde.effectDistance = new Vector2(1.5f, -1.5f);
        var tx = Hijo(caja, "Texto");
        tx.anchorMin = Vector2.zero; tx.anchorMax = Vector2.one; tx.offsetMin = tx.offsetMax = Vector2.zero;
        var t = tx.gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 16; t.fontStyle = FontStyle.Bold; t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white; t.raycastTarget = false; t.text = letra;

        segmentos = Hijo(fila, "Rombos");
        EsquinaSup(segmentos, new Vector2(38, -0.5f), new Vector2(8 * 30.4f + 36.3f, 21.2f));
        return fila.gameObject;
    }

    [MenuItem("Esperanza/HUD con rombos (vida, J y K)")]
    public static void Crear()
    {
        AssetDatabase.Refresh();
        if (!AssetDatabase.IsValidFolder(Carpeta)) { Debug.LogError("[HUD] Falta la carpeta " + Carpeta); return; }
        var vacio = Importar("hud_rombo_vacio"); var blanco = Importar("hud_rombo_blanco");
        var vida = Importar("hud_rombo_vida"); var arma = Importar("hud_rombo_arma");
        var roja = Importar("hud_rombo_roja"); var azul = Importar("hud_rombo_azul");
        Importar("hud_rombo_verde");
        if (vacio == null || blanco == null || vida == null || arma == null || roja == null || azul == null) return;

        var raiz = PrefabUtility.LoadPrefabContents(RutaPrefab);
        var hud = raiz.GetComponent<HudVida>();
        var cont = raiz.transform.Find("Contenedor");
        if (hud == null || cont == null) { Debug.LogError("[HUD] El prefab no tiene HudVida/Contenedor"); PrefabUtility.UnloadPrefabContents(raiz); return; }

        // quitar barras antiguas
        foreach (var n in new[] { "FondoBarra", "Barra", "Rombos" })
        {
            var o = cont.Find(n); if (o != null) Object.DestroyImmediate(o.gameObject);
        }
        // rombos de vida: detras del marco (el marco tiene el hueco transparente)
        float k = 580f / 2070f;
        var rv = Hijo(cont, "Rombos");
        EsquinaSup(rv, new Vector2((575 - 94) * k, -(285 - 59) * k), new Vector2((8 * 155 + 185) * k, 108 * k));
        rv.SetSiblingIndex(0);

        RectTransform segJ, segK;
        var filaJ = Fila(raiz.transform, "BarraDisparo", "J", new Vector2(127, -175), out segJ);
        var filaK = Fila(raiz.transform, "BarraHabilidad", "K", new Vector2(127, -208), out segK);
        filaK.SetActive(false);

        hud.spVacio = vacio; hud.spBlanco = blanco; hud.spVida = vida; hud.spDisparo = arma; hud.spRoja = roja; hud.spAzul = azul;
        hud.contVida = rv; hud.contDisparo = segJ; hud.contHabilidad = segK; hud.contenedorHabilidad = filaK;
        hud.tamVida = new Vector2(185 * k, 108 * k); hud.pasoVida = 155 * k;
        hud.tamMini = new Vector2(36.3f, 21.2f); hud.pasoMini = 30.4f;
        hud.barra = null; hud.barraDisparo = null; hud.textoDisparo = null; hud.barraHabilidad = null; hud.textoHabilidad = null;

        PrefabUtility.SaveAsPrefabAsset(raiz, RutaPrefab);
        PrefabUtility.UnloadPrefabContents(raiz);
        AssetDatabase.SaveAssets();
        Debug.Log("[HUD] Listo: HUD con rombos en el prefab HUD_Vida (se actualiza en todos los niveles).");
    }
}
#endif
