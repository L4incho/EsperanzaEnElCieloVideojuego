#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Importa el sprite del barril apagado y se lo asigna a los prefabs Fuego_N2 y BarrilFuego.
// Menu: Esperanza > ZZ Aplicar barril apagado
public static class AplicarBarrilApagado
{
    const string Hoja = "Assets/Arte/Entorno/niveles/nivel 2/fuego_spritesheet.png";
    const string Nuevo = "Assets/Arte/Entorno/niveles/nivel 2/n2_barril_apagado.png";

    [MenuItem("Esperanza/ZZ Aplicar barril apagado")]
    public static void Aplicar()
    {
        AssetDatabase.ImportAsset(Nuevo, ImportAssetOptions.ForceUpdate);
        var hoja = (TextureImporter)AssetImporter.GetAtPath(Hoja);
        var ti = (TextureImporter)AssetImporter.GetAtPath(Nuevo);
        if (ti == null) { Debug.LogError("[BarrilApagado] no esta el PNG " + Nuevo); return; }
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = hoja.spritePixelsPerUnit;
        ti.filterMode = hoja.filterMode;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.textureCompression = hoja.textureCompression;
        ti.maxTextureSize = hoja.maxTextureSize;
        var s = new TextureImporterSettings(); ti.ReadTextureSettings(s);
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        s.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(s);
        ti.spritePivot = new Vector2(0.5f, 0.2234957f);   // mismo pivote que el ultimo cuadro del fuego: el barril queda alineado
        ti.SaveAndReimport();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Nuevo);
        if (sprite == null) { Debug.LogError("[BarrilApagado] no se pudo cargar el sprite"); return; }
        foreach (var ruta in new[] { "Assets/Prefabs/Nivel2/Fuego_N2.prefab", "Assets/Prefabs/Nivel2/BarrilFuego.prefab" })
        {
            var raiz = PrefabUtility.LoadPrefabContents(ruta);
            var fb = raiz.GetComponent<FuegoBarril>();
            if (fb != null) { fb.apagado = sprite; fb.tiempoApagado = 2.5f; fb.tiempoEncendido = 2f; fb.golpesDano = 1; }
            PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            PrefabUtility.UnloadPrefabContents(raiz);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[BarrilApagado] listo: sprite " + sprite.name + " " + sprite.rect + " pivot " + sprite.pivot + " ppu " + sprite.pixelsPerUnit + " (hoja ppu " + hoja.spritePixelsPerUnit + ")");
    }
}
#endif
