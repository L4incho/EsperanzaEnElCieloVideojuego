#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Herramienta de editor: corta los spritesheets del tanque de acido, crea los tiles animados,
// una Tile Palette y un Tilemap mortal en la escena Tutorial.
// Menu: Esperanza > Crear acido (tiles, paleta y zona mortal)
public static class CrearAcido
{
    const string Carpeta = "Assets/Arte/Entorno/niveles/nivel 1 (tutorial)/animaciones/";
    const string CarpetaTiles = "Assets/Tiles/Acido";
    const string RutaPaleta = "Assets/Paletas/Paleta_Acido.prefab";
    const string RutaEscena = "Assets/Scenes/Tutorial.unity";
    const int Cuadros = 7;
    const int Celda = 64;       // cada cuadro mide 64x64 px
    const float Fps = 8f;

    // nombre, archivo, x/ancho del contenido real dentro de la celda (los extremos son mas angostos que 64)
    // pivote: el extremo izquierdo se pega al borde derecho de la celda, el derecho al borde izquierdo
    struct Pieza { public string id, archivo; public int x, ancho; public float pivotePx; }

    static readonly Pieza[] Piezas =
    {
        new Pieza{ id="izquierda", archivo="acido_izquierda_spritesheet.png", x=9,  ancho=45, pivotePx=13 },
        new Pieza{ id="medio",     archivo="acido_medioapritesheet.png",       x=0,  ancho=64, pivotePx=32 },
        new Pieza{ id="derecha",   archivo="acido_derecha_spritesheet.png",    x=10, ancho=45, pivotePx=32 },
    };

    [MenuItem("Esperanza/Crear acido (tiles, paleta y zona mortal)")]
    public static void Crear()
    {
        var tiles = new List<TileBase>();
        foreach (var p in Piezas) tiles.Add(CrearTile(p));

        CrearPaleta(tiles);
        CrearZonaEnEscena(tiles);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Acido listo: paleta " + RutaPaleta + " y Tilemap 'Acido' en la escena Tutorial.");
    }

    static TileBase CrearTile(Pieza p)
    {
        string ruta = Carpeta + p.archivo;
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (imp == null) { Debug.LogError("No se encontro " + ruta); return null; }

        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = 32;               // igual que el laboratorio: 64 px = 2 unidades
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.SaveAndReimport();

        var fabrica = new SpriteDataProviderFactories();
        fabrica.Init();
        var dp = fabrica.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        int altoTextura = Cuadros * Celda;
        var rects = new List<SpriteRect>();
        for (int i = 0; i < Cuadros; i++)
        {
            rects.Add(new SpriteRect
            {
                name = "acido_" + p.id + "_" + i,
                rect = new Rect(p.x, altoTextura - Celda * (i + 1), p.ancho, Celda),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(p.pivotePx / p.ancho, 0.5f),
                spriteID = GUID.Generate()
            });
        }
        dp.SetSpriteRects(rects.ToArray());
        dp.Apply();
        imp.SaveAndReimport();

        var sprites = new List<Sprite>();
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ruta))
            if (o is Sprite s) sprites.Add(s);
        sprites.Sort((a, b) => Num(a.name).CompareTo(Num(b.name)));

        if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");
        if (!AssetDatabase.IsValidFolder(CarpetaTiles)) AssetDatabase.CreateFolder("Assets/Tiles", "Acido");
        string rutaTile = CarpetaTiles + "/Acido_" + p.id + ".asset";
        var tile = AssetDatabase.LoadAssetAtPath<AnimatedTile>(rutaTile);
        bool nuevo = tile == null;
        if (nuevo) tile = ScriptableObject.CreateInstance<AnimatedTile>();
        tile.m_AnimatedSprites = sprites.ToArray();
        tile.m_MinSpeed = Fps;
        tile.m_MaxSpeed = Fps;
        tile.m_AnimationStartTime = 0f;
        tile.m_TileColliderType = Tile.ColliderType.Grid;   // la celda completa es zona mortal
        if (nuevo) AssetDatabase.CreateAsset(tile, rutaTile); else EditorUtility.SetDirty(tile);
        return tile;
    }

    static void CrearPaleta(List<TileBase> tiles)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Paletas")) AssetDatabase.CreateFolder("Assets", "Paletas");
        var raiz = new GameObject("Paleta_Acido");
        var grid = raiz.AddComponent<Grid>();
        grid.cellSize = new Vector3(2, 2, 0);           // cada tile mide 2x2 unidades
        var capa = new GameObject("Layer1");
        capa.transform.SetParent(raiz.transform);
        var tm = capa.AddComponent<Tilemap>();
        capa.AddComponent<TilemapRenderer>();
        for (int i = 0; i < tiles.Count; i++) tm.SetTile(new Vector3Int(i * 2, 0, 0), tiles[i]);

        if (AssetDatabase.LoadMainAssetAtPath(RutaPaleta) != null) AssetDatabase.DeleteAsset(RutaPaleta);
        PrefabUtility.SaveAsPrefabAsset(raiz, RutaPaleta);
        Object.DestroyImmediate(raiz);
        var ajustes = ScriptableObject.CreateInstance<GridPalette>();
        ajustes.name = "Palette Settings";
        AssetDatabase.AddObjectToAsset(ajustes, RutaPaleta);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(RutaPaleta);
    }

    static void CrearZonaEnEscena(List<TileBase> tiles)
    {
        var escena = EditorSceneManager.GetActiveScene();
        if (escena.path != RutaEscena) escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        var viejo = GameObject.Find("GridAcido");
        if (viejo != null) Object.DestroyImmediate(viejo);

        var go = new GameObject("GridAcido");
        var grid = go.AddComponent<Grid>();
        grid.cellSize = new Vector3(2, 2, 0);
        var hijo = new GameObject("Acido");
        hijo.transform.SetParent(go.transform);
        var tm = hijo.AddComponent<Tilemap>();
        var r = hijo.AddComponent<TilemapRenderer>();
        r.sortingOrder = 11;                              // se dibuja delante de Mateo: se hunde en el acido
        var col = hijo.AddComponent<TilemapCollider2D>();
        col.isTrigger = true;
        hijo.AddComponent<ZonaMortal>();

        // Tanque de ejemplo (izquierda, 2 medios, derecha) lejos del juego, para que veas como queda; se puede borrar
        // (se deja vacio: el usuario lo pinta con la paleta)
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }

    static int Num(string n)
    {
        int i = n.LastIndexOf('_');
        return (i >= 0 && int.TryParse(n.Substring(i + 1), out int v)) ? v : 0;
    }
}
#endif
