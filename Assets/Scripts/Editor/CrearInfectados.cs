#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Importa las hojas de animacion de los infectados y crea sus prefabs:
//  Nivel 2: Infectado1_N2, Infectado2_N2   |   Nivel 3: Infectado1_N3, Infectado2_N3, Infectado3_N3
public static class CrearInfectados
{
    const string Carpeta = "Assets/Arte/Esperanza en el cielo/personajes/infectados/animaciones/";

    // nombre, ancho de cuadro, alto, x del ancla (pie/centro), 8 cuadros
    static readonly (string n, int w, int h, int ax)[] Hojas =
    {
        ("E1_caminar",100,84,44), ("E1_atacar",100,84,44), ("E1_dano_stun",100,84,44),
        ("E2_arrastrarse",130,80,52), ("E2_atacar_salto",130,80,52), ("E2_dano_stun",130,80,52),
        ("E3_caminar",130,124,46), ("E3_atacar",130,124,46), ("E3_dano",130,124,46),
    };

    static Sprite[] Cuadros(string n)
    {
        return AssetDatabase.LoadAllAssetsAtPath(Carpeta + n + ".png").OfType<Sprite>()
            .OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1))).ToArray();
    }

    public static void Importar()
    {
        foreach (var h in Hojas)
        {
            string ruta = Carpeta + h.n + ".png";
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) { Debug.LogError("[Infectados] falta " + ruta); continue; }
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Multiple;
            imp.spritePixelsPerUnit = 100;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
            var f = new SpriteDataProviderFactories(); f.Init();
            var dp = f.GetSpriteEditorDataProviderFromObject(imp);
            dp.InitSpriteEditorDataProvider();
            var rects = new List<SpriteRect>();
            for (int i = 0; i < 8; i++)
                rects.Add(new SpriteRect
                {
                    name = h.n + "_" + i,
                    rect = new Rect(i * h.w, 0, h.w, h.h),
                    alignment = SpriteAlignment.Custom,
                    pivot = new Vector2((float)h.ax / h.w, 1f / h.h),
                    spriteID = GUID.Generate()
                });
            dp.SetSpriteRects(rects.ToArray());
            dp.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            dp.Apply();
            imp.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Esperanza/Crear infectados 1, 2 y 3 (prefabs)")]
    public static void Crear()
    {
        Importar();
        foreach (var d in new[] { "Assets/Prefabs/Nivel2", "Assets/Prefabs/Nivel3" })
            if (!AssetDatabase.IsValidFolder(d)) AssetDatabase.CreateFolder("Assets/Prefabs", d.Substring(d.LastIndexOf('/') + 1));

        int capaSuelo = LayerMask.GetMask("Suelo");
        var e1 = Armar("Infectado1", 1, 2.6f, capaSuelo);
        var e2 = Armar("Infectado2", 2, 2.6f, capaSuelo);
        var e3 = Armar("Infectado3", 3, 2.1f, capaSuelo);
        Guardar(e1, "Assets/Prefabs/Nivel2/Infectado1_N2.prefab");
        Guardar(e2, "Assets/Prefabs/Nivel2/Infectado2_N2.prefab");
        Guardar(e1, "Assets/Prefabs/Nivel3/Infectado1_N3.prefab");
        Guardar(e2, "Assets/Prefabs/Nivel3/Infectado2_N3.prefab");
        Guardar(e3, "Assets/Prefabs/Nivel3/Infectado3_N3.prefab");
        Object.DestroyImmediate(e1); Object.DestroyImmediate(e2); Object.DestroyImmediate(e3);
        AssetDatabase.SaveAssets();
        Debug.Log("[Infectados] Prefabs creados en Assets/Prefabs/Nivel2 y Nivel3");
    }

    static void Guardar(GameObject g, string ruta)
    {
        PrefabUtility.SaveAsPrefabAsset(g, ruta);
    }

    static GameObject Armar(string nombre, int tipo, float escala, int capaSuelo)
    {
        string p = "E" + tipo;
        var go = new GameObject(nombre);
        go.transform.localScale = Vector3.one * escala;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 8;   // Mateo esta en 10
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.mass = 20f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var en = go.AddComponent<EnemigoInfectado>();
        en.capaSuelo = capaSuelo;

        var col = go.AddComponent<CapsuleCollider2D>();
        if (tipo == 1)
        {
            sr.sprite = Cuadros("E1_caminar")[0];
            col.direction = CapsuleDirection2D.Vertical; col.size = new Vector2(0.40f, 0.70f); col.offset = new Vector2(0f, 0.36f);
            en.tipo = TipoInfectado.Normal; en.golpesQueQuita = 1;
            en.velocidadPatrulla = 0.8f; en.velocidadPersecucion = 1.4f;
            en.rangoAtaque = 1.3f; en.tiempoPreparacion = 0.5f; en.tiempoRecuperacion = 0.35f; en.enfriamiento = 1.4f;
            en.animCaminar = Cuadros("E1_caminar"); en.animAtaque = Cuadros("E1_atacar"); en.animDano = Cuadros("E1_dano_stun");
            en.tiempoPorFrameAnim = 0.12f;
        }
        else if (tipo == 2)
        {
            sr.sprite = Cuadros("E2_arrastrarse")[0];
            col.direction = CapsuleDirection2D.Horizontal; col.size = new Vector2(0.75f, 0.38f); col.offset = new Vector2(0.12f, 0.2f);
            en.tipo = TipoInfectado.Saltador; en.golpesQueQuita = 2;
            en.velocidadPatrulla = 0.55f; en.velocidadPersecucion = 0.95f; en.rangoDeteccion = 9f;
            en.rangoAtaque = 1.5f; en.distanciaSalto = 8f; en.duracionSalto = 0.75f;
            en.tiempoPreparacion = 0.6f; en.tiempoRecuperacion = 0.6f; en.enfriamiento = 2.5f;
            en.animCaminar = Cuadros("E2_arrastrarse"); en.animAtaque = Cuadros("E2_atacar_salto"); en.animDano = Cuadros("E2_dano_stun");
            en.tiempoPorFrameAnim = 0.14f;
        }
        else
        {
            sr.sprite = Cuadros("E3_caminar")[0];
            col.direction = CapsuleDirection2D.Vertical; col.size = new Vector2(0.55f, 0.88f); col.offset = new Vector2(0f, 0.45f);
            en.tipo = TipoInfectado.Pesado; en.quitaTodaLaVida = true; en.inmuneAturdimiento = true;
            en.velocidadPatrulla = 0.4f; en.velocidadPersecucion = 0.7f; en.rangoDeteccion = 7f;
            en.rangoAtaque = 2.0f; en.tiempoPreparacion = 1.0f; en.tiempoRecuperacion = 0.8f; en.enfriamiento = 2.5f;
            en.animCaminar = Cuadros("E3_caminar"); en.animAtaque = Cuadros("E3_atacar"); en.animDano = Cuadros("E3_dano");
            en.tiempoPorFrameAnim = 0.16f; en.tiempoPorFrameDano = 0.09f;
        }
        return go;
    }
}
#endif
