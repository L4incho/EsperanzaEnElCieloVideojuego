#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Menu: Esperanza > Crear niveles 2, 3 y jefe
// Corta los spritesheets de cada nivel, crea sus tiles y paletas, y construye las escenas
// Nivel2, Nivel3 y Nivel4Jefe a partir del Tutorial (mismo Mateo, camara y limites).
public static class CrearNiveles
{
    const string Base = "Assets/Arte/Entorno/niveles/";
    const float PPU = 50f;       // pixeles por unidad de los tiles (un tile ~2x2 unidades)
    const float Celda = 2f;
    const int Cols = 20, Filas = 10;
    static int capaSuelo;

    class Nivel
    {
        public string escena, tag, fondo, hoja, deco;
        public int[][] rHoja, rDeco;
        public int top, fill, slab, slabMovil, slabTemp, puerta; // indices (hoja / deco)
        public bool puertaEnDeco;
        public int[] decorPeq;
        public string[] mapa; // de arriba hacia abajo
        public int cols;
    }

    static Nivel[] Niveles()
    {
        return new[]
        {
            new Nivel{ escena="Nivel2", tag="n2", fondo="fondos de nivel/fondo_nivel2.png",
                hoja="nivel 2/segundoNivel_tiles.png", deco="nivel 2/assets_decoracion_N2.png",
                rHoja=new int[][]{new[]{14,14,112,114},new[]{128,14,226,114},new[]{235,14,433,114},new[]{448,25,551,113},new[]{560,34,676,111},new[]{687,38,814,111},new[]{825,25,951,111},new[]{1032,39,1199,95},new[]{1219,36,1325,95},new[]{1343,35,1509,90},new[]{335,127,432,236},new[]{448,128,550,219},new[]{561,131,675,245},new[]{687,132,814,244},new[]{825,131,951,245},new[]{1028,98,1199,181},new[]{1223,126,1295,170},new[]{1361,110,1499,176},new[]{14,127,333,366},new[]{1027,196,1487,311},new[]{122,243,221,366},new[]{230,243,433,366},new[]{448,259,643,427},new[]{655,259,759,427},new[]{784,265,944,312},new[]{14,377,113,459},new[]{122,377,222,459},new[]{233,377,330,467},new[]{338,377,437,467},new[]{785,344,942,425},new[]{1089,323,1182,419},new[]{1210,326,1299,419},new[]{1339,322,1420,411},new[]{15,481,111,559},new[]{120,473,343,565},new[]{448,439,643,575},new[]{655,439,759,575},new[]{1026,435,1114,591},new[]{1303,435,1406,601},new[]{1425,441,1520,592},new[]{817,438,944,653},new[]{1154,450,1261,591},new[]{14,590,112,746},new[]{116,590,218,745},new[]{225,590,326,745},new[]{334,590,436,745},new[]{447,590,544,745},new[]{554,590,652,743},new[]{660,589,759,743},new[]{1012,603,1064,723},new[]{1086,612,1133,723},new[]{1155,602,1203,723},new[]{1233,625,1343,723},new[]{1379,627,1518,741},new[]{874,732,921,866},new[]{959,737,1087,864},new[]{1116,736,1284,849},new[]{1337,768,1509,860},new[]{14,754,112,929},new[]{118,755,220,929},new[]{228,755,326,929},new[]{336,754,436,929},new[]{446,755,544,928},new[]{555,755,651,928},new[]{660,755,758,928},new[]{828,842,1073,991},new[]{1087,840,1253,992},new[]{1269,871,1509,992}}, rDeco=new int[][]{new[]{23,84,209,253},new[]{229,116,324,253},new[]{355,46,491,252},new[]{505,111,882,253},new[]{897,50,989,250},new[]{992,95,1138,255},new[]{1155,111,1265,248},new[]{1255,46,1367,262},new[]{1378,90,1510,261},new[]{23,284,141,416},new[]{167,287,306,416},new[]{337,283,480,416},new[]{508,288,629,414},new[]{656,284,732,409},new[]{841,308,964,408},new[]{989,308,1056,409},new[]{750,318,822,412},new[]{1069,337,1183,411},new[]{1199,308,1294,416},new[]{1319,346,1407,403},new[]{1434,320,1516,408},new[]{25,446,148,593},new[]{178,446,290,583},new[]{1161,466,1324,544},new[]{1347,441,1510,596},new[]{322,446,395,607},new[]{440,448,510,604},new[]{544,439,586,610},new[]{629,437,671,607},new[]{712,456,807,596},new[]{841,454,935,596},new[]{965,454,1143,595},new[]{845,656,1075,694},new[]{20,635,510,775},new[]{530,639,755,775},new[]{772,622,814,802},new[]{847,732,1075,775},new[]{1092,645,1143,801},new[]{1178,601,1230,808},new[]{1282,603,1328,802},new[]{1362,627,1407,792},new[]{1452,684,1498,808},new[]{20,843,179,987},new[]{379,820,527,985},new[]{547,845,669,986},new[]{681,825,781,986},new[]{1143,832,1370,984},new[]{1388,845,1516,984},new[]{201,878,361,987},new[]{812,866,966,986},new[]{1000,864,1134,986}},
                top=1, fill=2, slab=4, slabMovil=13, slabTemp=12, puerta=1, puertaEnDeco=true, cols=20,
                mapa=new[]{
                    "....................",
                    "..D.................",
                    ".=====..............",
                    ".......T.T.T.T.T...g",
                    "..................==",
                    "................==..",
                    "...........M........",
                    "...F..P.==..........",
                    "####################",
                    "####################" } },
            new Nivel{ escena="Nivel3", tag="n3", fondo="fondos de nivel/fondo_nivel3.png",
                hoja="nivel 3/spritesheet_N3.png", deco="nivel 3/assets_decoracion_N3.png",
                rHoja=new int[][]{new[]{26,32,140,145},new[]{156,32,270,145},new[]{284,32,407,145},new[]{421,32,536,145},new[]{550,32,661,145},new[]{676,32,792,145},new[]{803,32,863,89},new[]{803,115,890,234},new[]{880,41,1129,228},new[]{918,114,1033,234},new[]{1171,28,1324,255},new[]{25,160,140,281},new[]{156,160,270,278},new[]{285,160,406,278},new[]{421,160,536,278},new[]{550,160,661,278},new[]{677,160,761,278},new[]{1394,27,1509,517},new[]{293,312,414,349},new[]{442,313,567,370},new[]{817,272,1097,395},new[]{1115,274,1221,388},new[]{26,313,138,430},new[]{157,312,272,430},new[]{590,312,706,430},new[]{732,312,814,517},new[]{1253,275,1360,519},new[]{26,448,138,519},new[]{159,448,273,510},new[]{294,448,413,523},new[]{432,448,561,511},new[]{584,448,704,524},new[]{833,418,985,539},new[]{1010,422,1106,543},new[]{1130,422,1223,539},new[]{26,559,146,662},new[]{168,559,261,659},new[]{281,559,476,668},new[]{492,559,587,658},new[]{607,559,699,660},new[]{722,559,818,658},new[]{836,559,923,657},new[]{941,559,1059,657},new[]{1082,559,1160,657},new[]{1228,586,1293,657},new[]{1366,585,1431,658},new[]{569,695,702,812},new[]{825,685,1082,831},new[]{1100,685,1327,829},new[]{1343,685,1513,827},new[]{27,704,273,906},new[]{309,709,452,906},new[]{486,707,545,840},new[]{713,718,814,823},new[]{486,847,543,910},new[]{577,825,639,910},new[]{657,825,724,910},new[]{28,936,302,996},new[]{322,935,715,996},new[]{760,877,906,996},new[]{943,851,1006,999},new[]{1043,892,1236,957},new[]{1266,888,1489,996}}, rDeco=new int[][]{new[]{582,29,741,185},new[]{23,17,195,300},new[]{215,23,325,295},new[]{337,27,480,299},new[]{498,33,560,291},new[]{768,125,861,224},new[]{778,11,1509,260},new[]{585,205,670,285},new[]{681,193,741,292},new[]{768,239,861,305},new[]{883,129,976,296},new[]{1097,137,1283,313},new[]{1315,192,1491,289},new[]{999,271,1087,431},new[]{1104,321,1167,387},new[]{21,322,182,459},new[]{199,312,309,461},new[]{325,316,455,458},new[]{463,313,619,473},new[]{636,310,751,449},new[]{770,322,849,439},new[]{876,314,943,437},new[]{962,335,1011,426},new[]{1187,350,1243,432},new[]{1255,322,1524,434},new[]{1105,403,1155,493},new[]{27,474,159,591},new[]{179,477,256,591},new[]{281,470,357,591},new[]{372,479,452,591},new[]{463,471,532,590},new[]{543,479,629,591},new[]{651,463,717,605},new[]{734,455,854,608},new[]{871,450,1078,608},new[]{1100,520,1155,604},new[]{1174,449,1242,596},new[]{1256,444,1312,598},new[]{1328,454,1435,600},new[]{1443,450,1519,601},new[]{24,611,254,730},new[]{267,611,359,676},new[]{493,617,538,727},new[]{549,610,603,703},new[]{613,618,663,703},new[]{683,635,789,714},new[]{800,624,881,707},new[]{892,632,961,707},new[]{201,684,376,793},new[]{383,608,469,791},new[]{983,622,1065,748},new[]{1089,624,1205,753},new[]{1225,623,1277,756},new[]{1289,619,1385,864},new[]{1395,629,1515,858},new[]{24,739,92,835},new[]{114,718,182,821},new[]{201,764,309,819},new[]{479,753,554,834},new[]{561,751,605,846},new[]{615,728,697,835},new[]{725,741,824,828},new[]{847,725,919,848},new[]{939,743,976,848},new[]{999,764,1049,848},new[]{1071,780,1117,859},new[]{1138,770,1218,864},new[]{1231,768,1277,848},new[]{32,848,178,909},new[]{206,835,306,908},new[]{326,807,455,903},new[]{33,920,110,1008},new[]{132,919,205,1008},new[]{229,923,320,1007},new[]{341,917,448,1003},new[]{470,855,828,1006},new[]{843,863,919,1008},new[]{938,865,1061,1004},new[]{1082,881,1226,1005},new[]{1241,881,1419,1005},new[]{1434,873,1518,1005}},
                top=0, fill=1, slab=28, slabMovil=29, slabTemp=30, puerta=10, puertaEnDeco=false, cols=18,
                mapa=new[]{
                    "..................",
                    "..................",
                    ".D..E.............",
                    ".======..g........",
                    "........T.T.T.....",
                    "..............==..",
                    "............==....",
                    "....E.L...==......",
                    "##################",
                    "##################" } },
            new Nivel{ escena="Nivel4Jefe", tag="n4", fondo="fondos de nivel/fondo_N4.png",
                hoja="nivel 4 (boss)/tileset_N4.png", deco="nivel 4 (boss)/tileset_decor_N4.png",
                rHoja=new int[][]{new[]{19,19,130,118},new[]{143,19,253,118},new[]{265,19,362,118},new[]{373,19,586,119},new[]{595,19,705,118},new[]{719,19,826,118},new[]{1200,30,1248,179},new[]{1273,30,1317,179},new[]{20,129,130,222},new[]{143,129,253,222},new[]{265,129,362,222},new[]{373,129,477,222},new[]{488,129,586,222},new[]{594,129,705,222},new[]{719,130,826,222},new[]{850,30,1180,334},new[]{1057,119,1093,174},new[]{1350,34,1519,347},new[]{19,235,130,320},new[]{143,235,253,320},new[]{265,235,362,320},new[]{373,235,477,320},new[]{488,235,584,312},new[]{846,193,910,231},new[]{1057,187,1093,243},new[]{1126,200,1167,329},new[]{1185,200,1225,329},new[]{1255,200,1338,334},new[]{594,235,705,327},new[]{719,235,826,326},new[]{846,272,910,308},new[]{20,337,130,434},new[]{143,337,253,434},new[]{266,337,378,422},new[]{390,337,494,410},new[]{504,337,594,430},new[]{622,349,650,467},new[]{659,352,713,466},new[]{743,351,809,449},new[]{831,348,931,449},new[]{943,348,1039,448},new[]{1051,347,1149,448},new[]{19,449,129,517},new[]{143,449,254,517},new[]{265,449,373,517},new[]{386,449,495,525},new[]{515,449,591,523},new[]{745,471,827,553},new[]{1168,362,1251,611},new[]{1275,366,1396,612},new[]{1418,386,1515,577},new[]{19,539,127,611},new[]{144,539,252,611},new[]{270,539,376,611},new[]{389,539,496,611},new[]{515,540,591,611},new[]{621,486,720,611},new[]{751,559,824,611},new[]{849,474,910,611},new[]{921,474,969,611},new[]{979,474,1070,611},new[]{1081,463,1149,611},new[]{1232,631,1515,770},new[]{19,639,1065,989},new[]{1073,641,1193,968},new[]{1195,737,1241,988},new[]{1254,786,1509,977}}, rDeco=new int[][]{new[]{606,38,705,139},new[]{23,25,186,343},new[]{199,25,336,330},new[]{348,28,554,303},new[]{730,28,1042,316},new[]{1056,26,1523,295},new[]{350,174,407,314},new[]{566,134,638,293},new[]{656,137,722,291},new[]{972,176,1062,313},new[]{1065,135,1102,291},new[]{1122,134,1184,291},new[]{1202,135,1230,291},new[]{369,327,498,379},new[]{34,361,142,456},new[]{163,362,335,456},new[]{357,388,507,455},new[]{519,346,562,471},new[]{567,320,642,482},new[]{661,333,765,466},new[]{781,339,948,405},new[]{964,321,1188,468},new[]{27,477,339,537},new[]{789,421,845,519},new[]{854,421,949,520},new[]{1167,311,1519,643},new[]{235,557,337,635},new[]{345,478,432,619},new[]{449,478,508,624},new[]{524,493,586,613},new[]{614,489,717,615},new[]{731,490,775,614},new[]{796,533,969,608},new[]{993,475,1090,597},new[]{1106,475,1154,602},new[]{651,656,687,688},new[]{18,567,240,812},new[]{253,647,363,794},new[]{375,651,455,794},new[]{469,647,644,794},new[]{651,713,704,782},new[]{714,636,759,785},new[]{770,632,877,746},new[]{887,636,949,742},new[]{964,632,1059,736},new[]{1082,632,1389,851},new[]{1405,657,1517,850},new[]{640,802,756,867},new[]{775,764,820,863},new[]{846,755,911,874},new[]{928,755,994,868},new[]{1014,755,1078,858},new[]{1103,770,1223,858},new[]{29,831,100,972},new[]{103,833,147,985},new[]{160,824,205,911},new[]{218,826,275,890},new[]{289,811,490,983},new[]{507,811,613,987},new[]{779,881,954,956},new[]{152,920,224,985},new[]{234,913,278,983},new[]{629,882,761,986},new[]{975,884,1048,963},new[]{1057,883,1122,967},new[]{1133,900,1243,986},new[]{1255,863,1377,986},new[]{1401,864,1454,983},new[]{1467,871,1505,982}},
                top=1, fill=10, slab=18, slabMovil=19, slabTemp=20, puerta=56, puertaEnDeco=false, cols=20,
                mapa=new[]{
                    "....................",
                    "....................",
                    "....................",
                    "............===.....",
                    "........==..........",
                    "......==......==....",
                    "....==..............",
                    "S.==..g.........L.D.",
                    "####################",
                    "####################" } }
        };
    }

    [MenuItem("Esperanza/Crear niveles 2, 3 y jefe")]
    public static void CrearTodos()
    {
        string escenaOriginal = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.SaveOpenScenes();
        PrepararCarpetas();
        foreach (var n in Niveles()) { try { CrearNivel(n); } catch (System.Exception e) { Debug.LogError("Error en " + n.escena + ": " + e); } }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (!string.IsNullOrEmpty(escenaOriginal)) EditorSceneManager.OpenScene(escenaOriginal, OpenSceneMode.Single);
        Debug.Log("Niveles creados: Nivel2, Nivel3, Nivel4Jefe (agregados al Build Settings).");
    }

    static void PrepararCarpetas()
    {
        foreach (var c in new[] { "Tiles", "Paletas", "Scenes" })
            if (!AssetDatabase.IsValidFolder("Assets/" + c)) AssetDatabase.CreateFolder("Assets", c);
    }

    // ---------- cortar hojas ----------
    static void Cortar(string ruta, int[][] rects, string prefijo, SpriteAlignment alin)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = PPU;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.maxTextureSize = 4096;
        imp.SaveAndReimport();
        imp.GetSourceTextureWidthAndHeight(out int w, out int h);

        var fab = new SpriteDataProviderFactories(); fab.Init();
        var dp = fab.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        var lista = new List<SpriteRect>();
        for (int i = 0; i < rects.Length; i++)
        {
            var r = rects[i];
            lista.Add(new SpriteRect
            {
                name = prefijo + "_" + i,
                spriteID = GUID.Generate(),
                rect = new Rect(r[0], h - r[3], r[2] - r[0], r[3] - r[1]),
                alignment = alin,
                pivot = alin == SpriteAlignment.BottomCenter ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f)
            });
        }
        dp.SetSpriteRects(lista.ToArray());
        var pares = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        pares.SetNameFileIdPairs(lista.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();
    }

    // tira vertical de animacion: recortes dados en pixeles (arriba-izquierda)
    static void CortarTira(string ruta, int[][] rects, string prefijo, SpriteAlignment alin, float ppu)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.maxTextureSize = 4096;
        imp.SaveAndReimport();
        Cortar2(imp, rects, prefijo, alin);
    }

    static void Cortar2(TextureImporter imp, int[][] rects, string prefijo, SpriteAlignment alin)
    {
        imp.GetSourceTextureWidthAndHeight(out int w, out int h);
        var fab = new SpriteDataProviderFactories(); fab.Init();
        var dp = fab.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        var lista = new List<SpriteRect>();
        for (int i = 0; i < rects.Length; i++)
        {
            var r = rects[i];
            lista.Add(new SpriteRect
            {
                name = prefijo + "_" + i,
                spriteID = GUID.Generate(),
                rect = new Rect(r[0], h - r[3], r[2] - r[0], r[3] - r[1]),
                alignment = alin,
                pivot = alin == SpriteAlignment.BottomCenter ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f)
            });
        }
        dp.SetSpriteRects(lista.ToArray());
        var pares = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        pares.SetNameFileIdPairs(lista.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();
    }

    static Sprite[] Sprites(string ruta)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>()
            .OrderBy(s => { int i = s.name.LastIndexOf('_'); return int.Parse(s.name.Substring(i + 1)); }).ToArray();
    }

    // ---------- tiles ----------
    static Tile HacerTile(string carpeta, string nombre, Sprite sp, Tile.ColliderType col, Vector2? ajuste, bool arribaAlineado, bool rellenar = false)
    {
        string ruta = carpeta + "/" + nombre + ".asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
        bool nuevo = t == null;
        if (nuevo) t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = sp;
        t.colliderType = col;
        if (ajuste.HasValue)
        {
            float wu = sp.rect.width / sp.pixelsPerUnit, hu = sp.rect.height / sp.pixelsPerUnit;
            float sx = Celda / wu;
            float sy = rellenar ? Celda / hu : sx;
            float alto = hu * sy;
            float dy = arribaAlineado ? (Celda / 2f - alto / 2f) : 0f;
            t.transform = Matrix4x4.TRS(new Vector3(0, dy, 0), Quaternion.identity, new Vector3(sx, sy, 1));
        }
        if (nuevo) AssetDatabase.CreateAsset(t, ruta); else EditorUtility.SetDirty(t);
        return t;
    }

    static void CrearPaleta(string rutaPaleta, string nombre, Tile[] primeros, Sprite[] sprites, string carpeta, int columnas)
    {
        var raiz = new GameObject(nombre);
        var grid = raiz.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        var capa = new GameObject("Layer1");
        capa.transform.SetParent(raiz.transform);
        var tm = capa.AddComponent<Tilemap>();
        capa.AddComponent<TilemapRenderer>();
        float maxLado = 0;
        foreach (var sp in sprites) maxLado = Mathf.Max(maxLado, sp.rect.width / sp.pixelsPerUnit, sp.rect.height / sp.pixelsPerUnit);
        int sep = Mathf.CeilToInt(maxLado) + 1;
        int k = 0;
        foreach (var t in primeros) { tm.SetTile(new Vector3Int((k % columnas) * sep, -(k / columnas) * sep, 0), t); k++; }
        foreach (var sp in sprites)
        {
            string ruta = carpeta + "/" + sp.name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
            bool nuevo = tile == null;
            if (nuevo) tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sp;
            tile.colliderType = Tile.ColliderType.None;
            if (nuevo) AssetDatabase.CreateAsset(tile, ruta); else EditorUtility.SetDirty(tile);
            tm.SetTile(new Vector3Int((k % columnas) * sep, -(k / columnas) * sep, 0), tile);
            k++;
        }
        if (AssetDatabase.LoadMainAssetAtPath(rutaPaleta) != null) AssetDatabase.DeleteAsset(rutaPaleta);
        PrefabUtility.SaveAsPrefabAsset(raiz, rutaPaleta);
        Object.DestroyImmediate(raiz);
        var ajustes = ScriptableObject.CreateInstance<GridPalette>();
        ajustes.name = "Palette Settings";
        AssetDatabase.AddObjectToAsset(ajustes, rutaPaleta);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(rutaPaleta);
    }

    // ---------- construir un nivel ----------
    static void CrearNivel(Nivel n)
    {
        string rHoja = Base + n.hoja, rDeco = Base + n.deco, rFondo = Base + n.fondo;
        Cortar(rHoja, n.rHoja, n.tag, SpriteAlignment.Center);
        Cortar(rDeco, n.rDeco, n.tag + "d", SpriteAlignment.BottomCenter);
        AssetDatabase.Refresh();

        // fondo como sprite
        var impF = (TextureImporter)AssetImporter.GetAtPath(rFondo);
        impF.textureType = TextureImporterType.Sprite;
        impF.spriteImportMode = SpriteImportMode.Single;
        impF.spritePixelsPerUnit = 100;
        impF.filterMode = FilterMode.Point;
        impF.textureCompression = TextureImporterCompression.Uncompressed;
        impF.mipmapEnabled = false;
        impF.maxTextureSize = 4096;
        impF.SaveAndReimport();
        var spFondo = AssetDatabase.LoadAssetAtPath<Sprite>(rFondo);

        var sprH = Sprites(rHoja);
        var sprD = Sprites(rDeco);

        string carpeta = "Assets/Tiles/" + n.escena;
        if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder("Assets/Tiles", n.escena);
        string carpetaSp = carpeta + "/Sprites";
        if (!AssetDatabase.IsValidFolder(carpetaSp)) AssetDatabase.CreateFolder(carpeta, "Sprites");
        string carpetaDec = carpeta + "/Decoracion";
        if (!AssetDatabase.IsValidFolder(carpetaDec)) AssetDatabase.CreateFolder(carpeta, "Decoracion");

        var tTop = HacerTile(carpeta, "Suelo_superior", sprH[n.top], Tile.ColliderType.Grid, Vector2.one, true, false);
        var tFill = HacerTile(carpeta, "Suelo_relleno", sprH[n.fill], Tile.ColliderType.Grid, Vector2.one, true, true);
        var tSlab = HacerTile(carpeta, "Plataforma", sprH[n.slab], Tile.ColliderType.Sprite, Vector2.one, true, false);

        CrearPaleta("Assets/Paletas/Paleta_" + n.escena + ".prefab", "Paleta_" + n.escena, new[] { tTop, tFill, tSlab }, sprH, carpetaSp, 8);
        CrearPaleta("Assets/Paletas/Paleta_Decoracion_" + n.escena + ".prefab", "Paleta_Decoracion_" + n.escena, new Tile[0], sprD, carpetaDec, 8);

        // ---- escena ----
        string rutaEscena = "Assets/Scenes/" + n.escena + ".unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(rutaEscena) != null) AssetDatabase.DeleteAsset(rutaEscena);
        AssetDatabase.CopyAsset("Assets/Scenes/Tutorial.unity", rutaEscena);
        var escena = EditorSceneManager.OpenScene(rutaEscena, OpenSceneMode.Single);
        // al abrir la escena Unity descarga assets sin uso: se vuelven a cargar
        tTop = AssetDatabase.LoadAssetAtPath<Tile>(carpeta + "/Suelo_superior.asset");
        tFill = AssetDatabase.LoadAssetAtPath<Tile>(carpeta + "/Suelo_relleno.asset");
        tSlab = AssetDatabase.LoadAssetAtPath<Tile>(carpeta + "/Plataforma.asset");
        spFondo = AssetDatabase.LoadAssetAtPath<Sprite>(rFondo);
        sprH = Sprites(rHoja);
        sprD = Sprites(rDeco);

        // limpiar lo del tutorial
        foreach (var nombre in new[] { "GridAcido", "Escalera_Central" })
        { var g = GameObject.Find(nombre); if (g != null) Object.DestroyImmediate(g); }
        var gemaPlantilla = GameObject.Find("GemaVerde_01");

        var grid = GameObject.Find("Grid");
        grid.GetComponent<Grid>().cellSize = new Vector3(Celda, Celda, 0);
        Tilemap suelo = null;
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>(true))
        {
            if (tm.gameObject.name == "Tilemap") suelo = tm;
            else Object.DestroyImmediate(tm.gameObject);
        }
        suelo.ClearAllTiles();
        capaSuelo = suelo.gameObject.layer;
        // se recrean los colliders para no heredar la geometria del tutorial
        PhysicsMaterial2D matSuelo = null;
        var cViejo = suelo.GetComponent<CompositeCollider2D>();
        if (cViejo != null) { matSuelo = cViejo.sharedMaterial; Object.DestroyImmediate(cViejo); }
        var tViejo = suelo.GetComponent<TilemapCollider2D>();
        if (tViejo != null) Object.DestroyImmediate(tViejo);
        var rbSuelo = suelo.GetComponent<Rigidbody2D>();
        if (rbSuelo == null) rbSuelo = suelo.gameObject.AddComponent<Rigidbody2D>();
        rbSuelo.bodyType = RigidbodyType2D.Static;
        var tcol = suelo.gameObject.AddComponent<TilemapCollider2D>();
        tcol.compositeOperation = Collider2D.CompositeOperation.Merge;
        var ccol = suelo.gameObject.AddComponent<CompositeCollider2D>();
        ccol.geometryType = CompositeCollider2D.GeometryType.Polygons;
        ccol.generationType = CompositeCollider2D.GenerationType.Synchronous;
        if (matSuelo != null) ccol.sharedMaterial = matSuelo;

        // fondo
        var fondo = GameObject.Find("Fondo_Tutorial");
        fondo.name = "Fondo_" + n.escena;
        var sr = fondo.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sprite = spFondo;
        float altoSp = spFondo.rect.height / 100f;
        float esc = 20f / altoSp;
        fondo.transform.position = Vector3.zero;
        fondo.transform.localScale = new Vector3(esc, esc, 1f);
        float anchoMundo = spFondo.rect.width / 100f * esc;

        // geometria del mapa
        int filas = n.mapa.Length;
        float x0 = -n.cols * Celda / 2f;
        float y0 = -10f;
        System.Func<int, float> cx = c => x0 + c * Celda + Celda / 2f;
        // fila texto 0 = arriba. r = indice desde abajo
        System.Func<int, int> rowFromTop = i => filas - 1 - i;
        System.Func<int, float> cyBase = r => y0 + r * Celda; // y inferior de la celda
        Vector3 inicio = new Vector3(cx(1), cyBase(2) + 1.3f, 0);

        var padreObj = new GameObject("Objetos");
        var padreDec = new GameObject("Decoracion");

        // sprites utiles
        Sprite spPuerta = n.puertaEnDeco ? sprD[n.puerta] : sprH[n.puerta];
        var spFuego = CargarTira(Base + "nivel 2/fuego_spritesheet.png", "fuego", new[]{ new[]{273,58,450,418}, new[]{273,465,450,871}, new[]{274,902,450,1334}, new[]{274,1365,450,1769}, new[]{274,1819,450,2166} }, SpriteAlignment.BottomCenter, 100f);
        var spLaser = CargarTira(Base + "nivel 3/lasser.png", "laser", new[]{ new[]{328,49,400,437}, new[]{312,495,415,847}, new[]{312,896,417,1250}, new[]{305,1305,421,1675}, new[]{298,1720,429,2118} }, SpriteAlignment.BottomCenter, 100f);
        var spLaserR = CargarTira(Base + "nivel 4 (boss)/lasser_rojo.png", "laserR", new[]{ new[]{320,38,403,438}, new[]{301,478,421,841}, new[]{300,880,425,1251}, new[]{291,1297,432,1677}, new[]{287,1711,437,2119} }, SpriteAlignment.BottomCenter, 100f);
        var elecR = new int[11][];
        for (int i = 0; i < 11; i++) elecR[i] = new[]{ 260, 17 + (int)(i * 194.2f), 465, 17 + (int)((i + 1) * 194.2f) };
        var spElec = CargarTira(Base + "nivel 3/electricidad_N3_transparente.png", "elec", elecR, SpriteAlignment.BottomCenter, 100f);
        var spPinchos = (n.tag == "n2") ? sprH[6] : null;

        // pintar mapa
        for (int i = 0; i < filas; i++)
        {
            string fila = n.mapa[i];
            int r = rowFromTop(i);
            for (int c = 0; c < fila.Length && c < n.cols; c++)
            {
                char ch = fila[c];
                var cel = new Vector3Int(c + Mathf.RoundToInt(x0 / Celda), r + Mathf.RoundToInt(y0 / Celda), 0);
                Vector3 centro = new Vector3(cx(c), cyBase(r) + Celda / 2f, 0);
                Vector3 pie = new Vector3(cx(c), cyBase(r), 0);
                switch (ch)
                {
                    case '#':
                        bool arribaSolido = i > 0 && n.mapa[i - 1][c] == '#';
                        suelo.SetTile(cel, arribaSolido ? tFill : tTop);
                        break;
                    case '=':
                        suelo.SetTile(cel, tSlab);
                        break;
                    case 'T': CrearPlataformaTemp(padreObj.transform, sprH[n.slabTemp], new Vector3(cx(c), cyBase(r) + Celda, 0)); break;
                    case 'M': CrearPlataformaMovil(padreObj.transform, sprH[n.slabMovil], new Vector3(cx(c), cyBase(r) + Celda, 0), 4 * Celda); break;
                    case 'F': CrearFuego(padreObj.transform, spFuego, pie); break;
                    case 'L': CrearLaser(padreObj.transform, n.tag == "n4" ? spLaserR : spLaser, pie, n.tag == "n4"); break;
                    case 'E': CrearElectricidad(padreObj.transform, spElec, pie); break;
                    case 'P': CrearPinchos(padreObj.transform, spPinchos != null ? spPinchos : sprH[0], pie); break;
                    case 'D': CrearPuerta(padreObj.transform, spPuerta, pie, n.puertaEnDeco); break;
                    case 'g':
                        if (gemaPlantilla != null)
                        {
                            var gm = Object.Instantiate(gemaPlantilla, padreObj.transform);
                            gm.name = "GemaVerde";
                            gm.transform.position = centro;
                        }
                        break;
                    case 'S': inicio = new Vector3(cx(c), cyBase(r) + 1.3f, 0); break;
                }
            }
        }
        if (gemaPlantilla != null) Object.DestroyImmediate(gemaPlantilla);
        suelo.CompressBounds();
        suelo.RefreshAllTiles();
        EditorUtility.SetDirty(suelo);
        tcol.ProcessTilemapChanges();
        ccol.GenerateGeometry();
        Debug.Log(n.escena + ": tiles pintados = " + suelo.GetUsedTilesCount() + " en " + suelo.gameObject.name + " (" + suelo.gameObject.scene.name + ")");

        // decoracion de relleno (detras de Mateo)
        Decorar(n, padreDec.transform, sprD, cx, cyBase, filas);

        // Mateo al inicio
        var mateo = Object.FindFirstObjectByType<PlayerController>();
        mateo.transform.position = inicio;
        var rbm = mateo.GetComponent<Rigidbody2D>();
        if (rbm != null) { rbm.position = inicio; rbm.linearVelocity = Vector2.zero; }

        // limites de camara y de mapa (igual que el tutorial)
        Selection.activeGameObject = fondo;
        LimitarCamara.Ejecutar();
        Selection.activeGameObject = fondo;
        CrearLimitesMapa.Crear();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena, rutaEscena);
        AgregarABuild(rutaEscena);
    }

    static Sprite[] CargarTira(string ruta, string prefijo, int[][] rects, SpriteAlignment alin, float ppu)
    {
        CortarTira(ruta, rects, prefijo, alin, ppu);
        return Sprites(ruta);
    }

    // ---------- objetos ----------
    static GameObject Obj(Transform padre, string nombre, Sprite sp, Vector3 pos, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = orden;
        return go;
    }

    static void Escala(GameObject go, float altoDeseado)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        float h = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        float s = altoDeseado / h;
        go.transform.localScale = new Vector3(s, s, 1f);
    }

    static void CrearPlataformaTemp(Transform padre, Sprite sp, Vector3 posTope)
    {
        var go = Obj(padre, "PlataformaTemporal", sp, posTope, 2);
        float wu = sp.rect.width / sp.pixelsPerUnit, hu = sp.rect.height / sp.pixelsPerUnit;
        float s = Celda / wu;
        go.transform.localScale = new Vector3(s, s, 1f);
        go.transform.position = posTope + new Vector3(0, -hu * s / 2f, 0);
        go.tag = "Suelo"; go.layer = capaSuelo;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(wu, hu * 0.55f); bc.offset = new Vector2(0, hu * 0.22f);
        var pt = go.AddComponent<PlataformaTemporal>();
        pt.tiempoAntesDeCaer = 0.6f; pt.tiempoDesaparecida = 2.5f;
        var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Static;
    }

    static void CrearPlataformaMovil(Transform padre, Sprite sp, Vector3 posTope, float recorrido)
    {
        var go = Obj(padre, "PlataformaMovil", sp, posTope, 2);
        float wu = sp.rect.width / sp.pixelsPerUnit, hu = sp.rect.height / sp.pixelsPerUnit;
        float s = Celda / wu;
        go.transform.localScale = new Vector3(s, s, 1f);
        Vector3 p = posTope + new Vector3(0, -hu * s / 2f, 0);
        go.transform.position = p;
        go.tag = "Suelo"; go.layer = capaSuelo;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(wu, hu * 0.55f); bc.offset = new Vector2(0, hu * 0.22f);
        var pm = go.AddComponent<PlataformaMovil>();
        pm.puntoA = p; pm.puntoB = p + new Vector3(recorrido, 0, 0); pm.velocidad = 2f;
    }

    static void CrearFuego(Transform padre, Sprite[] cuadros, Vector3 pie)
    {
        var go = Obj(padre, "FuegoTemporal", cuadros[0], pie, 3);
        go.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
        var an = go.AddComponent<SpriteAnimador>(); an.cuadros = cuadros; an.fps = 7f;
        var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
        bc.size = new Vector2(1.1f, 1.6f); bc.offset = new Vector2(0f, 2.4f);
        go.AddComponent<ZonaMortal>();
        var zi = go.AddComponent<ZonaIntermitente>(); zi.tiempoEncendido = 1.6f; zi.tiempoApagado = 1.6f;
        // barril siempre visible
        var barril = Obj(go.transform, "Barril", cuadros[4], Vector3.zero, 2);
        barril.transform.localPosition = Vector3.zero; barril.transform.localScale = Vector3.one;
    }

    static void CrearLaser(Transform padre, Sprite[] cuadros, Vector3 pie, bool rojo)
    {
        var go = Obj(padre, "LaserTemporal", cuadros[0], pie, 3);
        go.transform.localScale = new Vector3(1f, 1f, 1f);
        var an = go.AddComponent<SpriteAnimador>(); an.cuadros = cuadros; an.fps = 10f;
        var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
        bc.size = new Vector2(0.6f, 3.8f); bc.offset = new Vector2(0f, 1.95f);
        go.AddComponent<ZonaMortal>();
        var zi = go.AddComponent<ZonaIntermitente>(); zi.tiempoEncendido = 1.5f; zi.tiempoApagado = 1.8f;
    }

    static void CrearElectricidad(Transform padre, Sprite[] cuadros, Vector3 pie)
    {
        // cuadros 0,1,2,4 = arco encendido; 3 = solo postes
        var go = Obj(padre, "Electricidad", cuadros[3], pie, 3);
        var an = go.AddComponent<SpriteAnimador>(); an.cuadros = new[] { cuadros[0], cuadros[1], cuadros[2], cuadros[4] }; an.fps = 12f;
        var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
        bc.size = new Vector2(1.6f, 1.0f); bc.offset = new Vector2(0f, 1.4f);
        go.AddComponent<ZonaMortal>();
        var zi = go.AddComponent<ZonaIntermitente>(); zi.tiempoEncendido = 1.2f; zi.tiempoApagado = 1.8f; zi.spriteApagado = cuadros[3];
    }

    static void CrearPinchos(Transform padre, Sprite sp, Vector3 pie)
    {
        var raiz = new GameObject("PinchosPresion");
        raiz.transform.SetParent(padre, false);
        raiz.transform.position = pie;
        var sensor = raiz.AddComponent<BoxCollider2D>(); sensor.isTrigger = true;
        sensor.size = new Vector2(5f, 2f); sensor.offset = new Vector2(0, 1f);
        var hijo = Obj(raiz.transform, "Pinchos", sp, pie, 3);
        float wu = sp.rect.width / sp.pixelsPerUnit, hu = sp.rect.height / sp.pixelsPerUnit;
        float s = Celda / wu;
        hijo.transform.localScale = new Vector3(s, s, 1f);
        hijo.transform.position = pie + new Vector3(0, hu * s / 2f, 0);
        var zona = hijo.AddComponent<BoxCollider2D>(); zona.isTrigger = true;
        zona.size = new Vector2(wu * 0.85f, hu * 0.8f);
        hijo.AddComponent<ZonaMortal>();
        var pp = raiz.AddComponent<PinchosPresion>(); pp.pinchos = hijo.transform; pp.zonaMortal = zona;
    }

    static void CrearPuerta(Transform padre, Sprite sp, Vector3 pie, bool deco)
    {
        var go = Obj(padre, "Salida", sp, pie, 1);
        Escala(go, 3.2f);
        if (!deco) go.transform.position += new Vector3(0, sp.rect.height / sp.pixelsPerUnit * go.transform.localScale.y / 2f, 0);
        var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
        float hu = sp.rect.height / sp.pixelsPerUnit;
        bc.size = new Vector2(sp.rect.width / sp.pixelsPerUnit * 0.6f, hu * 0.8f);
        bc.offset = deco ? new Vector2(0, hu * 0.4f) : new Vector2(0, 0);
        var sal = go.AddComponent<Salida>(); sal.escenaSiguiente = "SeleccionNiveles";
    }

    static void Decorar(Nivel n, Transform padre, Sprite[] deco, System.Func<int, float> cx, System.Func<int, float> cyBase, int filas)
    {
        // decoracion sencilla sobre el suelo: indices segun el nivel (sprites con pivote abajo)
        int[] idx; float[] xs;
        switch (n.tag)
        {
            case "n2": idx = new[] { 12, 14, 16, 11, 15, 20, 13 }; xs = new[] { 2f, 7.5f, 9.5f, 12f, 15f, 17f, 19f }; break;
            case "n3": idx = new[] { 16, 33, 20, 51, 26, 19, 34 }; xs = new[] { 1.5f, 6f, 9f, 12f, 14.5f, 15.6f, 11f }; break;
            default: idx = new[] { 57, 62, 40, 47, 39 }; xs = new[] { 3f, 8f, 12f, 14f, 16.5f }; break;
        }
        float ySuelo = cyBase(2);
        for (int i = 0; i < idx.Length; i++)
        {
            if (idx[i] >= deco.Length) continue;
            // evitar poner decoracion encima de objetos: las columnas se toman de forma holgada
            int c = Mathf.Clamp(Mathf.RoundToInt(xs[i]), 0, n.cols - 1);
            var go = Obj(padre, "Deco_" + deco[idx[i]].name, deco[idx[i]], new Vector3(cx(c), ySuelo, 0), -5);
            // altura maxima ~ 2.2 unidades
            float h = deco[idx[i]].rect.height / deco[idx[i]].pixelsPerUnit;
            if (h > 2.2f) { float s = 2.2f / h; go.transform.localScale = new Vector3(s, s, 1f); }
        }
    }

    static void AgregarABuild(string ruta)
    {
        var lista = EditorBuildSettings.scenes.ToList();
        if (lista.Any(s => s.path == ruta)) return;
        int idx = lista.FindLastIndex(s => s.path.EndsWith("SeleccionNiveles.unity") || s.path.EndsWith("Tutorial.unity"));
        var nueva = new EditorBuildSettingsScene(ruta, true);
        if (idx >= 0) lista.Insert(idx + 1, nueva); else lista.Add(nueva);
        EditorBuildSettings.scenes = lista.ToArray();
    }
}
#endif
