#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Esperanza > Crear efectos de gema
// Importa los cuadros pixel art de Assets/Arte/Efectos/Pixel, crea los prefabs EfectoImpulso (botas propulsoras)
// y EfectoRecoger (tomar gema) y los conecta a Mateo en la escena Tutorial. Se puede ejecutar las veces que quieras.
public static class CrearEfectosGema
{
    const string Carpeta = "Assets/Arte/Efectos/Pixel";
    const string RutaImpulso = "Assets/Prefabs/EfectoImpulso.prefab";
    const string RutaRecoger = "Assets/Prefabs/EfectoRecoger.prefab";

    [MenuItem("Esperanza/Crear efectos de gema")]
    public static void Ejecutar()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        Limpiar();

        var paletas = new List<EfectoGema.Paleta>();
        paletas.Add(Crear("verde", new Color(0.25f, 1f, 0.45f)));
        paletas.Add(Crear("azul", new Color(0.2f, 0.5f, 1f)));
        paletas.Add(Crear("roja", new Color(1f, 0.3f, 0.3f)));

        var impulso = CrearPrefab(RutaImpulso, "EfectoImpulso", EfectoGema.Modo.Impulso, paletas, 0f);
        var recoger = CrearPrefab(RutaRecoger, "EfectoRecoger", EfectoGema.Modo.Recoger, paletas, 2f);
        Conectar(impulso, recoger);
        Debug.Log("Efectos de gema (pixel art) listos: " + RutaImpulso + " y " + RutaRecoger);
    }

    static void Limpiar()
    {
        // restos del primer intento con particulas suaves
        string[] viejos = {
            "Assets/Arte/Efectos/particula_suave.png", "Assets/Arte/Efectos/anillo_energia.png",
            "Assets/Arte/Efectos/Particula_Brillo.mat", "Assets/Arte/Efectos/Anillo_Brillo.mat",
            "Assets/Scripts/ParticulaSprite.cs" };
        // llamas y ondas de la version anterior (botas propulsoras): ya no se usan
        foreach (var n in new[] { "verde", "azul", "roja" })
        {
            for (int i = 0; i < 6; i++) AssetDatabase.DeleteAsset(Carpeta + "/llama_" + n + "_" + i + ".png");
            for (int i = 0; i < 4; i++) AssetDatabase.DeleteAsset(Carpeta + "/onda_" + n + "_" + i + ".png");
        }
        foreach (var v in viejos) if (AssetDatabase.LoadMainAssetAtPath(v) != null) AssetDatabase.DeleteAsset(v);
    }

    static EfectoGema.Paleta Crear(string nombre, Color referencia)
    {
        var p = new EfectoGema.Paleta { nombre = nombre, referencia = referencia };
        p.chispas = Cargar("chispa_" + nombre, 8, new Vector2(0.5f, 0.5f));
        p.estallido = Cargar("estallido_" + nombre, 8, new Vector2(0.5f, 0.5f));
        return p;
    }

    static Sprite[] Cargar(string prefijo, int n, Vector2 pivote)
    {
        var lista = new Sprite[n];
        for (int i = 0; i < n; i++)
        {
            string ruta = Carpeta + "/" + prefijo + "_" + i + ".png";
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) { Debug.LogError("Falta el cuadro: " + ruta); continue; }
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100f;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.wrapMode = TextureWrapMode.Clamp;
            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Custom;
            s.spritePivot = pivote;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
            lista[i] = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }
        return lista;
    }

    static EfectoGema CrearPrefab(string ruta, string nombre, EfectoGema.Modo modo, List<EfectoGema.Paleta> paletas, float destruir)
    {
        AssetDatabase.DeleteAsset(ruta);
        var go = new GameObject(nombre);
        var e = go.AddComponent<EfectoGema>();
        e.modo = modo; e.paletas = paletas.ToArray(); e.destruirDespues = destruir; e.ordenDibujo = 20;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<EfectoGema>();
    }

    static void Conectar(EfectoGema impulso, EfectoGema recoger)
    {
        EditorSceneManager.SaveOpenScenes();
        var escena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null) { Debug.LogError("No encontre a Mateo en la escena Tutorial."); return; }
        pc.efectoImpulso = impulso;
        pc.efectoRecoger = recoger;
        EditorUtility.SetDirty(pc);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }
}
#endif
