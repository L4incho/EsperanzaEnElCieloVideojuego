#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public static class CrearCuracion
{
    const string RutaBotiquin = "Assets/Arte/Objetos/botiquin.png";
    const string RutaVenda = "Assets/Arte/Objetos/venda.png";

    static void ImportarSimple(string ruta, float ppu)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = ppu;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        var ajustes = new TextureImporterSettings();
        imp.ReadTextureSettings(ajustes);
        ajustes.spriteAlignment = (int)SpriteAlignment.Center;
        imp.SetTextureSettings(ajustes);
        imp.SaveAndReimport();
    }

    static GameObject HacerPrefab(string nombre, string rutaSprite, bool todo, int golpes)
    {
        var spr = AssetDatabase.LoadAllAssetsAtPath(rutaSprite).OfType<Sprite>().FirstOrDefault();
        if (spr == null) { Debug.LogError("[Cura] Sin sprite en " + rutaSprite); return null; }
        var go = new GameObject(nombre);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.55f;
        var rc = go.AddComponent<RecogibleCuracion>();
        rc.curaTodo = todo;
        rc.golpesQueCura = golpes;
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        string ruta = "Assets/Prefabs/" + nombre + ".prefab";
        var p = PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
        return p;
    }

    [MenuItem("Esperanza/Crear venda y botiquin en el Tutorial")]
    public static void Crear()
    {
        ImportarSimple(RutaBotiquin, 1600f);
        ImportarSimple(RutaVenda, 1500f);
        var pVenda = HacerPrefab("Venda", RutaVenda, false, 1);
        var pBoti = HacerPrefab("Botiquin", RutaBotiquin, true, 3);
        if (pVenda == null || pBoti == null) return;

        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Tutorial") sc = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        foreach (var v in Object.FindObjectsByType<RecogibleCuracion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(v.gameObject);

        var mateo = Object.FindFirstObjectByType<PlayerController>();
        var sm = mateo != null ? mateo.GetComponent<SpriteRenderer>() : null;
        int capa = LayerMask.GetMask("Suelo");
        float x0 = mateo != null ? mateo.transform.position.x : 0f;
        float y0 = mateo != null ? mateo.transform.position.y : 0f;
        Physics2D.SyncTransforms();

        Colocar(pVenda, x0 + 6f, y0, capa, sm, "Venda");
        Colocar(pBoti, x0 + 17f, y0, capa, sm, "Botiquin");

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log("[Cura] Venda y botiquin colocados y escena guardada.");
    }

    static void Colocar(GameObject prefab, float x, float y0, int capa, SpriteRenderer sm, string nombre)
    {
        foreach (float d in new[] { 0f, 1f, -1f, 2f, -2f, 3f })
        {
            var h = Physics2D.Raycast(new Vector2(x + d, y0 + 1.5f), Vector2.down, 20f, capa);
            if (h.collider == null || h.collider.isTrigger) continue;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.name = nombre;
            var spr = inst.GetComponent<SpriteRenderer>().sprite;
            float alto = spr.bounds.size.y;
            inst.transform.position = new Vector3(x + d, h.point.y + alto * 0.5f + 0.25f, 0f);
            var sr = inst.GetComponent<SpriteRenderer>();
            if (sm != null) { sr.sortingLayerID = sm.sortingLayerID; sr.sortingOrder = sm.sortingOrder - 1; }
            Debug.Log("[Cura] " + nombre + " en " + inst.transform.position);
            return;
        }
        Debug.LogError("[Cura] No encontre suelo para " + nombre + "; colocalo a mano");
    }
}
#endif
