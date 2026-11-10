#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

// Importa las hojas de 8 cuadros de las gemas (HD) y las asigna a los prefabs GemaVerde / GemaRoja / GemaAzul.
public static class CrearGemasPx
{
    static readonly string[] Colores = { "verde", "roja", "azul" };

    static Sprite[] Cortar(string ruta, string nombre)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = 100;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.maxTextureSize = 4096;
        var datos = new SpriteMetaData[8];
        for (int i = 0; i < 8; i++)
            datos[i] = new SpriteMetaData { name = nombre + "_" + i, rect = new Rect(i * 32, 0, 32, 32), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) };
#pragma warning disable 618
        imp.spritesheet = datos;
#pragma warning restore 618
        imp.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1))).ToArray();
    }

    [MenuItem("Esperanza/Gemas pixel art (8 cuadros)")]
    public static void Crear()
    {
        foreach (var c in Colores)
        {
            string png = "Assets/Arte/Objetos/gema_" + c + "_px.png";
            if (AssetDatabase.LoadMainAssetAtPath(png) == null) { Debug.LogError("[GemasPx] Falta " + png); return; }
            var cuadros = Cortar(png, "gema_" + c + "_px");
            string pref = "Assets/Prefabs/Gema" + (c == "verde" ? "Verde" : c == "roja" ? "Roja" : "Azul") + ".prefab";
            var raiz = PrefabUtility.LoadPrefabContents(pref);
            var g = raiz.GetComponent<GemaVerde>();
            g.cuadros = cuadros;
            g.cuadrosPorSegundo = 10f;
            var sr = raiz.transform.GetChild(0).GetComponent<SpriteRenderer>();
            sr.sprite = cuadros[0];
            PrefabUtility.SaveAsPrefabAsset(raiz, pref);
            PrefabUtility.UnloadPrefabContents(raiz);
            Debug.Log("[GemasPx] " + pref + " con " + cuadros.Length + " cuadros");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[GemasPx] Listo");
    }
}
#endif
