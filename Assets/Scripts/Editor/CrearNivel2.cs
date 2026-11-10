#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

// Nivel 2: corta las hojas de arte, crea los tiles y las paletas, y pone las capas (Tilemaps) en la escena Nivel2.
// Menu: Esperanza > Crear paletas Nivel 2
// Paletas que genera (Assets/Paletas):
//   Paleta_Pisos_N2           pisos/plataformas con colision (segundoNivel_tiles, las piezas encerradas en rojo)
//   Paleta_Objetos_N2         escalera, plataforma temporal, pinchos y fuego (cada uno ya trae su comportamiento)
//   Paleta_Decoracion_Atras_N2    decoracion SIN colision que se dibuja detras de Mateo
//   Paleta_Decoracion_Frente_N2   la misma decoracion, pero se dibuja delante de Mateo
public static class CrearNivel2
{
    const string Dir = "Assets/Arte/Entorno/niveles/nivel 2/";
    const string RutaTiles = Dir + "segundoNivel_tiles.png";
    const string RutaDeco = Dir + "assets_decoracion_N2.png";
    const string RutaFuego = Dir + "fuego_spritesheet.png";
    const float PpuTiles = 48f;      // un bloque de piso (96 px) mide exactamente 2 unidades: encajan sin huecos en una cuadricula de 2x2
    const float PpuFuego = 150f;
    const string CarpetaTiles = "Assets/Tiles/Nivel2";
    const string CarpetaPrefabs = "Assets/Prefabs/Nivel2";

    // x0,y0,x1,y1 en pixeles (y desde arriba) de cada pieza, detectadas de la imagen
    static readonly int[][] RectsTiles = {
        new[]{0,14,14,112,114},
        new[]{1,129,14,225,114},
        new[]{2,236,14,331,114},
        new[]{3,340,14,433,114},
        new[]{4,448,26,550,112},
        new[]{5,826,26,950,111},
        new[]{6,561,35,675,111},
        new[]{7,1344,36,1508,89},
        new[]{8,687,38,814,111},
        new[]{9,1219,37,1323,94},
        new[]{10,1033,39,1199,100},
        new[]{11,1030,99,1199,181},
        new[]{12,1362,110,1498,176},
        new[]{13,1223,126,1294,170},
        new[]{14,14,128,329,234},
        new[]{15,338,128,431,234},
        new[]{16,448,128,550,218},
        new[]{17,561,132,675,244},
        new[]{18,687,132,814,244},
        new[]{19,826,132,950,244},
        new[]{20,1028,197,1487,311},
        new[]{21,9,239,118,371},
        new[]{22,116,239,226,371},
        new[]{23,224,239,334,371},
        new[]{24,333,239,439,371},
        new[]{25,448,261,643,426},
        new[]{26,656,261,758,426},
        new[]{27,785,266,943,311},
        new[]{28,1335,316,1425,415},
        new[]{29,1085,319,1185,421},
        new[]{30,1208,323,1300,423},
        new[]{31,785,345,941,424},
        new[]{32,9,372,118,464},
        new[]{33,117,372,226,464},
        new[]{34,229,372,335,472},
        new[]{35,334,372,442,472},
        new[]{36,1303,436,1405,600},
        new[]{37,442,433,648,581},
        new[]{38,1022,433,1119,596},
        new[]{39,650,434,764,581},
        new[]{40,812,435,949,657},
        new[]{41,1419,436,1525,598},
        new[]{42,1149,444,1265,597},
        new[]{43,230,470,348,570},
        new[]{44,9,476,115,565},
        new[]{45,115,490,232,570},
        new[]{46,8,584,114,749},
        new[]{47,113,584,223,749},
        new[]{48,222,584,331,750},
        new[]{49,329,584,442,749},
        new[]{50,441,584,550,749},
        new[]{51,549,584,656,748},
        new[]{52,655,584,764,748},
        new[]{53,1151,597,1207,728},
        new[]{54,1009,598,1068,728},
        new[]{55,1083,607,1138,728},
        new[]{56,1227,619,1349,728},
        new[]{57,1380,628,1517,740},
        new[]{58,870,727,925,871},
        new[]{59,1112,730,1289,853},
        new[]{60,957,732,1093,868},
        new[]{61,8,750,115,934},
        new[]{62,115,750,223,934},
        new[]{63,223,750,331,934},
        new[]{64,330,750,441,934},
        new[]{65,441,750,550,934},
        new[]{66,549,750,656,934},
        new[]{67,654,750,764,934},
        new[]{68,1338,768,1508,859},
        new[]{69,1081,838,1257,998},
        new[]{70,822,858,1077,997},
        new[]{71,1263,866,1515,998}
    };

    static readonly int[][] RectsDeco = {
        new[]{0,351,41,497,257},
        new[]{1,1251,41,1372,267},
        new[]{2,891,44,994,255},
        new[]{3,18,79,215,257},
        new[]{4,1372,86,1516,267},
        new[]{5,986,89,1144,261},
        new[]{6,500,105,888,257},
        new[]{7,1149,105,1270,254},
        new[]{8,224,111,330,257},
        new[]{9,18,278,147,421},
        new[]{10,331,278,486,421},
        new[]{11,651,279,737,413},
        new[]{12,161,282,311,421},
        new[]{13,503,282,634,419},
        new[]{14,836,303,969,413},
        new[]{15,984,303,1061,414},
        new[]{16,1194,303,1300,421},
        new[]{17,745,313,828,416},
        new[]{18,1429,315,1521,413},
        new[]{19,1064,331,1189,415},
        new[]{20,1314,340,1413,409},
        new[]{21,623,431,676,613},
        new[]{22,538,434,591,616},
        new[]{23,1342,436,1516,602},
        new[]{24,19,440,154,598},
        new[]{25,172,440,296,589},
        new[]{26,316,440,399,612},
        new[]{27,434,442,515,609},
        new[]{28,836,449,941,602},
        new[]{29,959,449,1148,600},
        new[]{30,707,451,813,602},
        new[]{31,1155,461,1329,550},
        new[]{32,1173,597,1236,813},
        new[]{33,1277,599,1334,807},
        new[]{34,766,616,819,808},
        new[]{35,1357,622,1413,797},
        new[]{36,15,630,515,780},
        new[]{37,524,634,760,780},
        new[]{38,1086,641,1149,806},
        new[]{39,839,650,1081,699},
        new[]{40,1447,679,1503,813},
        new[]{41,841,727,1081,780},
        new[]{42,375,816,533,991},
        new[]{43,677,819,786,991},
        new[]{44,1138,826,1375,989},
        new[]{45,15,837,185,993},
        new[]{46,541,840,674,991},
        new[]{47,1383,840,1522,990},
        new[]{48,995,859,1140,991},
        new[]{49,808,861,971,991},
        new[]{50,195,873,366,992}
    };

    // piezas de segundoNivel_tiles
    static readonly int[] Pisos = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 25, 26, 27, 31, 36, 57, 68 };
    const int Escalera = 41, Temporal = 28;
    static readonly int[] Pinchos = { 45, 43 };

    // zona solida de cada piso como fraccion del sprite (x0, y0, x1, y1; y medido desde ARRIBA). Lo que no esta aqui es el rectangulo completo.
    static readonly Dictionary<int, float[]> ZonaSolida = new Dictionary<int, float[]>
    {
        {5, new[]{0f,0.60f,1f,1f}}, {8, new[]{0f,0.60f,1f,1f}},                         // base con postes: solo la base
        {17, new[]{0f,0f,1f,0.52f}}, {18, new[]{0f,0f,1f,0.52f}}, {19, new[]{0f,0f,1f,0.52f}}, // losa con patas
        {7, new[]{0f,0f,1f,0.70f}}, {9, new[]{0f,0f,1f,0.70f}}, {10, new[]{0f,0f,1f,0.70f}},   // tuberias curvas
        {11, new[]{0f,0.30f,1f,1f}}, {31, new[]{0f,0.40f,1f,1f}},                       // tuberias con valvula
        {12, new[]{0f,0f,1f,0.60f}}, {20, new[]{0f,0f,1f,0.42f}},                       // rejilla y tuberia con lianas
        {57, new[]{0f,0f,1f,0.28f}}, {68, new[]{0f,0f,1f,0.28f}},                       // puentes de metal
    };

    static readonly Dictionary<int, Vector2> EscalaPiso = new Dictionary<int, Vector2>
    {
        {0, new Vector2(0.9796f, 0.96f)},
        {1, new Vector2(1.0f, 0.96f)},
        {2, new Vector2(1.0105f, 0.96f)},
        {3, new Vector2(1.0323f, 0.96f)},
        {15, new Vector2(1.0323f, 1.0f)},
        {20, new Vector2(1.0458f, 1.0f)},
        {25, new Vector2(0.9846f, 1.0f)},
    };

    // ---------------------------------------------------------------- menu

    [MenuItem("Esperanza/Crear paletas Nivel 2")]
    public static void Crear()
    {
        AsegurarCarpetas();
        CortarHojas();
        var sT = Sprites(RutaTiles);
        var sD = Sprites(RutaDeco);
        var sF = Sprites(RutaFuego);

        // ---- tiles
        var pisos = new List<TileBase>();
        foreach (int k in Pisos) pisos.Add(CrearTile(sT["n2_t_" + k.ToString("00")], Tile.ColliderType.Sprite, null, null, EscalaPiso.ContainsKey(k) ? EscalaPiso[k] : Vector2.one));

        var decoracion = new List<TileBase>();
        var usados = new HashSet<int>(Pisos) { Escalera, Temporal, Pinchos[0], Pinchos[1] };
        for (int k = 0; k < RectsTiles.Length; k++)
            if (!usados.Contains(k)) decoracion.Add(CrearTile(sT["n2_t_" + k.ToString("00")], Tile.ColliderType.None, null));
        for (int k = 0; k < RectsDeco.Length; k++)
            decoracion.Add(CrearTile(sD["n2_d_" + k.ToString("00")], Tile.ColliderType.None, null));

        var objetos = new List<TileBase>();
        objetos.Add(CrearTile(sT["n2_t_" + Escalera], Tile.ColliderType.None, CrearPrefabEscalera(sT["n2_t_" + Escalera]), "Escalera"));
        objetos.Add(CrearTile(sT["n2_t_" + Temporal], Tile.ColliderType.None, CrearPrefabTemporal(sT["n2_t_" + Temporal]), "PlataformaTemporal"));
        objetos.Add(CrearTile(sT["n2_t_" + Pinchos[0]], Tile.ColliderType.None, CrearPrefabPinchos(sT["n2_t_" + Pinchos[0]], "Pinchos_A", 0.05f, 0.95f), "Pinchos_A"));
        objetos.Add(CrearTile(sT["n2_t_" + Pinchos[1]], Tile.ColliderType.None, CrearPrefabPinchos(sT["n2_t_" + Pinchos[1]], "Pinchos_B", 0.05f, 0.70f), "Pinchos_B"));
        var cuadrosFuego = Enumerable.Range(0, 5).Select(i => sF["n2_fuego_" + i.ToString("00")]).ToArray();
        objetos.Add(CrearTile(cuadrosFuego[2], Tile.ColliderType.None, CrearPrefabFuego(cuadrosFuego), "Fuego"));

        // ---- paletas
        PaletaUtil.Construir("Assets/Paletas/Paleta_Pisos_N2.prefab", "Paleta_Pisos_N2", pisos, 36, true);
        PaletaUtil.Construir("Assets/Paletas/Paleta_Objetos_N2.prefab", "Paleta_Objetos_N2", objetos);
        PaletaUtil.Construir("Assets/Paletas/Paleta_Decoracion_Atras_N2.prefab", "Paleta_Decoracion_Atras_N2", decoracion);
        PaletaUtil.Construir("Assets/Paletas/Paleta_Decoracion_Frente_N2.prefab", "Paleta_Decoracion_Frente_N2", decoracion);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PonerCapasEnEscena();
        Debug.Log("[Nivel2] Listo: " + pisos.Count + " pisos, " + objetos.Count + " objetos, " + decoracion.Count + " decoraciones. Paletas en Assets/Paletas.");
    }

    // ---------------------------------------------------------------- cortar hojas

    static void AsegurarCarpetas()
    {
        foreach (var c in new[] { "Assets/Paletas", "Assets/Tiles", CarpetaTiles, "Assets/Prefabs", CarpetaPrefabs })
        {
            if (AssetDatabase.IsValidFolder(c)) continue;
            int i = c.LastIndexOf('/');
            AssetDatabase.CreateFolder(c.Substring(0, i), c.Substring(i + 1));
        }
    }

    static void CortarHojas()
    {
        // tiles: con zona de colision propia en los pisos
        Cortar(RutaTiles, RectsTiles, "n2_t_", PpuTiles, 1024, null, true);
        Cortar(RutaDeco, RectsDeco, "n2_d_", PpuTiles, 1024, null, false);

        // fuego: 5 cuadros, todos con el pivote en la base del barril (el barril apoya en el fondo de su celda)
        int[][] bandas = { new[] { 58, 416 }, new[] { 466, 869 }, new[] { 903, 1332 }, new[] { 1365, 1768 }, new[] { 1820, 2164 } };
        var f = new int[5][];
        for (int i = 0; i < 5; i++) f[i] = new[] { i, 270, bandas[i][0] - 2, 452, bandas[i][1] + 3 };
        Cortar(RutaFuego, f, "n2_fuego_", PpuFuego, 2170, null, false, true);
    }

    static void Cortar(string ruta, int[][] rects, string prefijo, float ppu, int altoImg, object _, bool contornosPisos, bool pivoteBase = false)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (imp == null) { Debug.LogError("[Nivel2] No existe " + ruta); return; }
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.maxTextureSize = 4096;
        imp.SaveAndReimport();

        var fabrica = new SpriteDataProviderFactories();
        fabrica.Init();
        var dp = fabrica.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        var lista = new List<SpriteRect>();
        foreach (var r in rects)
        {
            int x0 = r[1], y0 = r[2], x1 = r[3], y1 = r[4];
            var sr = new SpriteRect
            {
                name = prefijo + r[0].ToString("00"),
                rect = new Rect(x0, altoImg - y1, x1 - x0, y1 - y0),
                alignment = SpriteAlignment.Custom,
                pivot = pivoteBase ? new Vector2(0.5f, (3f + ppu * 0.5f) / (y1 - y0)) : ((prefijo == "n2_t_" && System.Array.IndexOf(Pisos, r[0]) >= 0) ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0.5f)),
                spriteID = GUID.Generate()
            };
            lista.Add(sr);
        }
        dp.SetSpriteRects(lista.ToArray());

        if (contornosPisos)
        {
            var po = dp.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
            foreach (var sr in lista)
            {
                int k = int.Parse(sr.name.Substring(prefijo.Length));
                var pts = ContornoPiso(k, sr.rect.width, sr.rect.height);
                if (pts != null) po.SetOutlines(sr.spriteID, new List<Vector2[]> { pts });
            }
        }
        dp.Apply();
        imp.SaveAndReimport();
    }

    // contorno (en pixeles, relativo al centro del sprite) del area solida de un piso
    static Vector2[] ContornoPiso(int k, float w, float h)
    {
        if (System.Array.IndexOf(Pisos, k) < 0) return null;
        float hw = w / 2f, hh = h / 2f;
        if (k == 4) // rampa: baja hacia la derecha
            return new[] { new Vector2(-hw, -hh), new Vector2(hw, -hh), new Vector2(hw, -hh + h * 0.2f), new Vector2(-hw, hh) };
        float[] z;
        if (!ZonaSolida.TryGetValue(k, out z)) z = new[] { 0f, 0f, 1f, 1f };
        float xa = -hw + z[0] * w, xb = -hw + z[2] * w, ya = hh - z[3] * h, yb = hh - z[1] * h;
        // un poco hacia adentro para no pegarse al brillo del borde
        return new[] { new Vector2(xa, ya), new Vector2(xb, ya), new Vector2(xb, yb), new Vector2(xa, yb) };
    }

    static Dictionary<string, Sprite> Sprites(string ruta)
    {
        var d = new Dictionary<string, Sprite>();
        foreach (var s in AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>()) d[s.name] = s;
        return d;
    }

    // ---------------------------------------------------------------- tiles

    static Tile CrearTile(Sprite sp, Tile.ColliderType colision, GameObject objeto, string nombre = null, Vector2? escala = null)
    {
        string ruta = CarpetaTiles + "/" + (nombre ?? sp.name) + ".asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
        bool nuevo = tile == null;
        if (nuevo) tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sp;
        tile.colliderType = colision;
        tile.gameObject = objeto;
        var e = escala ?? Vector2.one;
        tile.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(e.x, e.y, 1f));
        tile.flags = TileFlags.LockTransform;
        if (nuevo) AssetDatabase.CreateAsset(tile, ruta); else EditorUtility.SetDirty(tile);
        return tile;
    }

    static int CapaSuelo { get { int l = LayerMask.NameToLayer("Suelo"); return l < 0 ? 3 : l; } }

    static GameObject Guardar(GameObject go, string nombre)
    {
        string ruta = CarpetaPrefabs + "/" + nombre + ".prefab";
        var p = PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
        return p;
    }

    static Vector2 Tam(Sprite s) { return s.rect.size / s.pixelsPerUnit; }

    // Escalera: Mateo la sube con W y la baja con S (igual que la del Tutorial). Se pueden apilar varias hacia arriba.
    static GameObject CrearPrefabEscalera(Sprite s)
    {
        var go = new GameObject("Escalera_N2");
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        var t = Tam(s);
        col.size = new Vector2(Mathf.Min(1.0f, t.x), t.y);
        var e = go.AddComponent<Escalera>();
        e.ajustarAreaAlSprite = false;
        return Guardar(go, "Escalera_N2");
    }

    // Plataforma temporal: al pisarla tiembla, desaparece y vuelve a aparecer.
    static GameObject CrearPrefabTemporal(Sprite s)
    {
        var go = new GameObject("PlataformaTemporal_N2");
        go.layer = CapaSuelo;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingOrder = 6;
        var t = Tam(s);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(t.x * 0.92f, 0.5f);
        col.offset = new Vector2(0f, t.y / 2f - 0.25f);
        var p = go.AddComponent<PlataformaTemporal>();
        p.tiempoAntesDeCaer = 0.6f;
        p.tiempoDesaparecida = 3f;
        go.AddComponent<OcultarTile>();
        return Guardar(go, "PlataformaTemporal_N2");
    }

    // Pinchos: base solida abajo y zona de puntas arriba que quita 1/3 de la vida.
    static GameObject CrearPrefabPinchos(Sprite s, string nombre, float fx0, float fx1)
    {
        var go = new GameObject(nombre + "_N2");
        go.layer = CapaSuelo;
        var t = Tam(s);
        float baseAlto = t.y * 0.35f;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(t.x * 0.96f, baseAlto);
        col.offset = new Vector2(0f, -t.y / 2f + baseAlto / 2f);
        var d = go.AddComponent<PinchosDanio>();
        float ancho = (fx1 - fx0) * t.x;
        float cx = (fx0 + fx1) / 2f * t.x - t.x / 2f;
        float alto = t.y * 0.55f;
        d.zonaTamano = new Vector2(ancho, alto);
        d.zonaCentro = new Vector2(cx, -t.y / 2f + baseAlto + alto / 2f - 0.05f);
        return Guardar(go, nombre + "_N2");
    }

    // Fuego: barril solido (se puede tocar de lado) con llama animada que quema solo cuando esta alta.
    static GameObject CrearPrefabFuego(Sprite[] cuadros)
    {
        var go = new GameObject("Fuego_N2");
        go.layer = CapaSuelo;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cuadros[2];
        sr.sortingOrder = 6;
        var f = go.AddComponent<FuegoBarril>();
        f.cuadros = cuadros;
        f.cuadrosPorSegundo = 6f;
        f.cuadrosPeligrosos = new[] { 1, 2, 3 };
        // la celda mide 1 unidad: el barril apoya en el fondo de la celda (pivote 0.5 unidades sobre la base)
        float barrilAlto = 247f / PpuFuego, barrilAncho = 174f / PpuFuego;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(barrilAncho * 0.95f, barrilAlto);
        col.offset = new Vector2(0f, -0.5f + barrilAlto / 2f);
        f.zonaTamano = new Vector2(barrilAncho * 0.7f, 1.0f);
        f.zonaCentro = new Vector2(0f, -0.5f + barrilAlto + 0.45f);
        go.AddComponent<OcultarTile>();
        return Guardar(go, "Fuego_N2");
    }

    // ---------------------------------------------------------------- escena

    static void PonerCapasEnEscena()
    {
        EditorSceneManager.SaveOpenScenes();
        string original = EditorSceneManager.GetActiveScene().path;
        var escena = EditorSceneManager.OpenScene("Assets/Scenes/Nivel2.unity", OpenSceneMode.Single);

        var grid = GameObject.Find("Grid");
        if (grid == null)
        {
            grid = new GameObject("Grid");
            var g = grid.AddComponent<Grid>();
            g.cellSize = new Vector3(1, 1, 0);
        }

        // los pisos van en su propia cuadricula de 2x2 (un bloque = 1 celda) para que los bloques queden pegados, sin huecos
        var gridPisos = GameObject.Find("GridPisos");
        if (gridPisos == null)
        {
            gridPisos = new GameObject("GridPisos");
            var gp = gridPisos.AddComponent<Grid>();
            gp.cellSize = new Vector3(2, 2, 0);
        }
        var viejo = grid.transform.Find("Suelo");
        if (viejo != null)
        {
            // si ya habia pisos pintados en la capa vieja (cuadricula de 1x1) no se borran: se renombra para que el usuario decida
            var tmv = viejo.GetComponent<Tilemap>();
            if (tmv != null && tmv.GetUsedTilesCount() > 0) viejo.name = "Suelo_VIEJO_borrar";
            else Object.DestroyImmediate(viejo.gameObject);
        }
        var suelo = Capa(gridPisos, "Suelo", 0, true);
        suelo.GetComponent<Tilemap>().tileAnchor = new Vector3(0.5f, 1f, 0f);
        Capa(grid, "Objetos", 6, false);
        Capa(grid, "DecoracionDetrasMateo", 1, false);
        Capa(grid, "DecoracionFrenteMateo", 11, false);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        if (!string.IsNullOrEmpty(original) && original != escena.path) EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
    }

    static GameObject Capa(GameObject grid, string nombre, int orden, bool conColision)
    {
        var t = grid.transform.Find(nombre);
        GameObject go;
        if (t != null) go = t.gameObject;
        else
        {
            go = new GameObject(nombre);
            go.transform.SetParent(grid.transform, false);
            go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>();
        }
        go.GetComponent<TilemapRenderer>().sortingOrder = orden;
        if (conColision)
        {
            go.layer = CapaSuelo;
            Rigidbody2D rb; if (!go.TryGetComponent(out rb)) rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            TilemapCollider2D tc; if (!go.TryGetComponent(out tc)) tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            if (!go.TryGetComponent<CompositeCollider2D>(out _)) go.AddComponent<CompositeCollider2D>();
        }
        return go;
    }
}

// Arma una Tile Palette con los tiles bien juntos (para que se vean grandes y no haya que acercar tanto el zoom).
public static class PaletaUtil
{
    static int Impar(float v) { int n = Mathf.Max(1, Mathf.CeilToInt(v - 0.001f)); return n % 2 == 0 ? n + 1 : n; }

    static Vector2 TamTile(TileBase t)
    {
        Sprite s = null;
        var tile = t as Tile;
        if (tile != null) s = tile.sprite;
        var an = t as AnimatedTile;
        if (an != null && an.m_AnimatedSprites != null && an.m_AnimatedSprites.Length > 0) s = an.m_AnimatedSprites[0];
        if (s == null) return Vector2.one;
        return s.rect.size / s.pixelsPerUnit;
    }

    public static void Construir(string ruta, string nombre, List<TileBase> tiles, int anchoMaximo = 36, bool anclaArriba = false)
    {
        var raiz = new GameObject(nombre);
        var grid = raiz.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        var capa = new GameObject("Layer1");
        capa.transform.SetParent(raiz.transform);
        var tm = capa.AddComponent<Tilemap>();
        if (anclaArriba) tm.tileAnchor = new Vector3(0.5f, 1f, 0f);
        capa.AddComponent<TilemapRenderer>();

        // cada pieza reserva un cuadro de celdas (impar, para que quede centrada); se ordenan por altura y se acomodan en filas
        var items = tiles.Select(t => { var s = TamTile(t); return new { t, w = Impar(s.x), h = Impar(s.y) }; })
                         .OrderByDescending(i => i.h).ThenByDescending(i => i.w).ToList();
        int x = 0, yTop = 0, altoFila = 0;
        foreach (var it in items)
        {
            if (x + it.w > anchoMaximo && x > 0) { yTop -= altoFila; x = 0; altoFila = 0; }
            if (it.h > altoFila) altoFila = it.h;
            tm.SetTile(new Vector3Int(x + (it.w - 1) / 2, anclaArriba ? yTop - 1 : yTop - 1 - (it.h - 1) / 2, 0), it.t);
            x += it.w;
        }

        if (AssetDatabase.LoadMainAssetAtPath(ruta) != null) AssetDatabase.DeleteAsset(ruta);
        PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
        Object.DestroyImmediate(raiz);
        var ajustes = ScriptableObject.CreateInstance<GridPalette>();
        ajustes.name = "Palette Settings";
        ajustes.cellSizing = GridPalette.CellSizing.Manual; // celda fija de 1 unidad: las piezas se ven grandes y sin acercar tanto el zoom
        AssetDatabase.AddObjectToAsset(ajustes, ruta);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ruta);
    }

    // Vuelve a acomodar la paleta de decoracion del Tutorial con los mismos tiles pero bien juntos.
    [MenuItem("Esperanza/Reorganizar paleta Decoracion Nivel 1 (mas grande)")]
    public static void ReorganizarNivel1()
    {
        var guids = AssetDatabase.FindAssets("t:Tile", new[] { "Assets/Tiles/Decoracion_N1" });
        var tiles = guids.Select(g => AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(g)))
                         .Where(t => t != null)
                         .OrderBy(t => { int i = t.name.LastIndexOf('_'); int n; return (i >= 0 && int.TryParse(t.name.Substring(i + 1), out n)) ? n : 0; })
                         .ToList();
        if (tiles.Count == 0) { Debug.LogError("[Paleta] No encontre tiles en Assets/Tiles/Decoracion_N1"); return; }
        Construir("Assets/Paletas/Paleta_Decoracion_N1.prefab", "Paleta_Decoracion_N1", tiles);
        Debug.Log("[Paleta] Paleta del Tutorial reorganizada: " + tiles.Count + " tiles.");
    }
}
#endif
