#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Herramienta de editor: genera una Tile Palette con todos los sprites de assets_decoracion (nivel 1).
// Menu: Esperanza > Crear paleta Decoracion Nivel 1
public static class CrearPaletaDecoracion
{
    const string RutaHoja = "Assets/Arte/Entorno/niveles/nivel 1 (tutorial)/assets_decoracion.png";
    const string CarpetaTiles = "Assets/Tiles/Decoracion_N1";
    const string RutaPaleta = "Assets/Paletas/Paleta_Decoracion_N1.prefab";
    const int Columnas = 6;      // tiles por fila en la paleta

    [MenuItem("Esperanza/Crear paleta Decoracion Nivel 1")]
    public static void Crear()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(RutaHoja).OfType<Sprite>()
            .OrderBy(s => Numero(s.name)).ToArray();
        if (sprites.Length == 0)
        {
            Debug.LogError("No se encontraron sprites en " + RutaHoja + " (revisa que este cortada en Multiple).");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Paletas")) AssetDatabase.CreateFolder("Assets", "Paletas");
        if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");
        if (!AssetDatabase.IsValidFolder(CarpetaTiles)) AssetDatabase.CreateFolder("Assets/Tiles", "Decoracion_N1");

        var raiz = new GameObject("Paleta_Decoracion_N1");
        var grid = raiz.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        var capa = new GameObject("Layer1");
        capa.transform.SetParent(raiz.transform);
        var tilemap = capa.AddComponent<Tilemap>();
        capa.AddComponent<TilemapRenderer>();

        // Separacion = lo que mide el sprite mas grande (en celdas) + 1, para que no se encimen en la paleta
        float maxLado = 0;
        foreach (var sp in sprites) maxLado = Mathf.Max(maxLado, sp.rect.width / sp.pixelsPerUnit, sp.rect.height / sp.pixelsPerUnit);
        int Separacion = Mathf.CeilToInt(maxLado) + 1;

        for (int i = 0; i < sprites.Length; i++)
        {
            string ruta = CarpetaTiles + "/" + sprites[i].name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
            bool nuevo = tile == null;
            if (nuevo) tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprites[i];
            tile.colliderType = Tile.ColliderType.None; // decoracion: sin colision
            if (nuevo) AssetDatabase.CreateAsset(tile, ruta); else EditorUtility.SetDirty(tile);

            var pos = new Vector3Int((i % Columnas) * Separacion, -(i / Columnas) * Separacion, 0);
            tilemap.SetTile(pos, tile);
        }

        if (AssetDatabase.LoadMainAssetAtPath(RutaPaleta) != null) AssetDatabase.DeleteAsset(RutaPaleta);
        PrefabUtility.SaveAsPrefabAsset(raiz, RutaPaleta);
        Object.DestroyImmediate(raiz);

        var ajustes = ScriptableObject.CreateInstance<GridPalette>();
        ajustes.name = "Palette Settings";
        AssetDatabase.AddObjectToAsset(ajustes, RutaPaleta);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(RutaPaleta);
        Debug.Log("Paleta creada: " + RutaPaleta + " con " + sprites.Length + " tiles.");
    }

    static int Numero(string nombre)
    {
        int i = nombre.LastIndexOf('_');
        return (i >= 0 && int.TryParse(nombre.Substring(i + 1), out int n)) ? n : 0;
    }
}
#endif
