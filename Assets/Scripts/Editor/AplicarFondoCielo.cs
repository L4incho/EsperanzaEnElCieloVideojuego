#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Nivel 2: pone el fondo nuevo (continuacion en atardecer) en el objeto "Fondo_Nivel2 (1)".
// Menu: Esperanza > Aplicar fondo cielo Nivel 2
public static class AplicarFondoCielo
{
    const string Ruta = "Assets/Arte/Entorno/niveles/fondos de nivel/fondo_nivel2_cielo.png";
    const string Original = "Assets/Arte/Entorno/niveles/fondos de nivel/fondo_nivel2.png";

    [MenuItem("Esperanza/Aplicar fondo cielo Nivel 2")]
    public static void Aplicar()
    {
        AssetDatabase.ImportAsset(Ruta, ImportAssetOptions.ForceUpdate);
        var imp = AssetImporter.GetAtPath(Ruta) as TextureImporter;
        var imO = AssetImporter.GetAtPath(Original) as TextureImporter;
        if (imp == null) { Debug.LogError("[FondoCielo] no encuentro " + Ruta); return; }
        // mismos ajustes que el fondo original
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = imO != null ? imO.spritePixelsPerUnit : 100f;
        imp.filterMode = imO != null ? imO.filterMode : FilterMode.Point;
        imp.mipmapEnabled = false;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = false;
        imp.SaveAndReimport();

        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(Ruta);
        var go = GameObject.Find("Fondo_Nivel2 (1)");
        if (sp == null || go == null) { Debug.LogError("[FondoCielo] sprite=" + sp + " objeto=" + go); return; }
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sp;
        EditorUtility.SetDirty(sr);
        var esc = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(esc);
        EditorSceneManager.SaveScene(esc);
        Debug.Log("[FondoCielo] aplicado: " + sp.name + " " + sp.rect.size + " en " + go.name + " (pos " + go.transform.position + ")");
    }
}
#endif
