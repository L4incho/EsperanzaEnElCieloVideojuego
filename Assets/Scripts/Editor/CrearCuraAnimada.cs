#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

// Importa las hojas de 8 cuadros de la venda y el botiquin y las asigna a sus prefabs.
public static class CrearCuraAnimada
{
    static Sprite[] Cortar(string ruta, string nombre, float ppu, float pivotY)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        var datos = new SpriteMetaData[8];
        for (int i = 0; i < 8; i++)
            datos[i] = new SpriteMetaData { name = nombre + "_" + i, rect = new Rect(i * 64, 0, 64, 64), alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(0.5f, pivotY) };
#pragma warning disable 618
        imp.spritesheet = datos;
#pragma warning restore 618
        imp.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1))).ToArray();
    }

    static void Aplicar(string nombre, string png, float ppu, float pivotY)
    {
        var cuadros = Cortar(png, nombre.ToLower() + "_px", ppu, pivotY);
        string pref = "Assets/Prefabs/" + nombre + ".prefab";
        var raiz = PrefabUtility.LoadPrefabContents(pref);
        var rc = raiz.GetComponent<RecogibleCuracion>();
        rc.cuadros = cuadros;
        rc.cuadrosPorSegundo = 8f;
        rc.amplitud = 0f;   // el salto ya esta dentro de los cuadros
        raiz.GetComponent<SpriteRenderer>().sprite = cuadros[0];
        PrefabUtility.SaveAsPrefabAsset(raiz, pref);
        PrefabUtility.UnloadPrefabContents(raiz);
        Debug.Log("[CuraAnim] " + nombre + " con " + cuadros.Length + " cuadros");
    }

    [MenuItem("Esperanza/Venda y botiquin animados (8 cuadros)")]
    public static void Crear()
    {
        Aplicar("Venda", "Assets/Arte/Objetos/venda_px.png", 51f, 0.375f);
        Aplicar("Botiquin", "Assets/Arte/Objetos/botiquin_px.png", 59f, 0.359f);
        AssetDatabase.SaveAssets();
        Debug.Log("[CuraAnim] Listo");
    }
}
#endif
