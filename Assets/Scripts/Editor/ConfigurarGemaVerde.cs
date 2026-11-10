#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Menu: Esperanza > Configurar gema verde
// 1) Corta mateo_va_simple_verde/azul/rojo.png (recolores de mateo_va_simple) con los mismos cuadros y pivotes.
// 2) Crea MateoVerde/MateoAzul/MateoRojo.overrideController con las mismas animaciones pero con cada traje.
// 3) Crea el prefab Prefabs/GemaVerde con su Animator (estados Flotar y Recoger con clips vacios para que los edites).
// 4) Conecta a Mateo (animatorTrajeVerde) y coloca una gema de ejemplo en la escena Tutorial.
public static class ConfigurarGemaVerde
{
    const string Carpeta = "Assets/Arte/Personajes/mateo/";
    const string HojaSimple = Carpeta + "mateo_va_simple.png";   // hoja que usa Mateo por defecto
    const string ControllerMateo = Carpeta + "Animations/MateoController.controller";
    const string OverrideVerde = Carpeta + "Animations/MateoVerde.overrideController";
    static readonly string[] Trajes = { "verde", "azul", "rojo" };
    const string HojaGemasVieja = "Assets/Arte/Esperanza en el cielo/personajes/mateo/mateo_todas_las_gemas.png";
    const string SpriteGema = "Assets/Arte/Objetos/gema_verde.png";
    const string CarpetaGema = "Assets/Animaciones/GemaVerde";
    const string PrefabGema = "Assets/Prefabs/GemaVerde.prefab";

    [MenuItem("Esperanza/Configurar gema verde")]
    public static void Ejecutar()
    {
        AssetDatabase.DeleteAsset(HojaGemasVieja); // la hoja que ya no se usa
        foreach (var t in Trajes)
        {
            CortarHoja(t);
            CrearOverride(t);
        }
        var prefab = CrearPrefabGema();
        ConectarEnEscena(prefab);
        Debug.Log("Gema verde lista. Prefab: " + PrefabGema + " | Clips ya hechos: " + CarpetaGema);
    }

    // ---------- 1) Hoja de cada traje (recolor de mateo_va_simple, mismos cuadros y pivotes) ----------
    static string RutaHoja(string traje) { return Carpeta + "mateo_va_simple_" + traje + ".png"; }

    static void CortarHoja(string traje)
    {
        string ruta = RutaHoja(traje);
        var impN = (TextureImporter)AssetImporter.GetAtPath(HojaSimple);
        var impV = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (impV == null) { Debug.LogError("Falta " + ruta); return; }

        var f = new SpriteDataProviderFactories();
        f.Init();
        var dpN = f.GetSpriteEditorDataProviderFromObject(impN);
        dpN.InitSpriteEditorDataProvider();
        var rectsN = dpN.GetSpriteRects();

        impV.textureType = TextureImporterType.Sprite;
        impV.spriteImportMode = SpriteImportMode.Multiple;
        impV.spritePixelsPerUnit = impN.spritePixelsPerUnit;
        impV.filterMode = impN.filterMode;
        impV.textureCompression = impN.textureCompression;
        impV.mipmapEnabled = false;
        impV.SaveAndReimport();

        var dpV = f.GetSpriteEditorDataProviderFromObject(impV);
        dpV.InitSpriteEditorDataProvider();
        var nuevos = new List<SpriteRect>();
        string pref = "mateo_va_simple_";
        foreach (var r in rectsN)
        {
            string n = pref + traje + "_" + r.name.Substring(pref.Length);
            nuevos.Add(new SpriteRect { name = n, rect = r.rect, alignment = r.alignment, pivot = r.pivot, spriteID = GUID.Generate() });
        }
        dpV.SetSpriteRects(nuevos.ToArray());
        dpV.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(nuevos.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dpV.Apply();
        impV.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    // ---------- 2) Override del Animator de cada traje ----------
    static void CrearOverride(string traje)
    {
        string rutaOv = Carpeta + "Animations/Mateo" + char.ToUpper(traje[0]) + traje.Substring(1) + ".overrideController";
        var baseCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerMateo);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(RutaHoja(traje)).OfType<Sprite>().ToDictionary(s => s.name, s => s);

        AssetDatabase.DeleteAsset(rutaOv);
        var ov = new AnimatorOverrideController(baseCtrl);
        var pares = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        ov.GetOverrides(pares);

        var resultado = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var par in pares)
        {
            var clip = par.Key;
            string rutaClip = AssetDatabase.GetAssetPath(clip);
            string rutaNueva = rutaClip.Replace(".anim", "_" + traje + ".anim");
            AssetDatabase.DeleteAsset(rutaNueva);
            var copia = new AnimationClip();
            EditorUtility.CopySerialized(clip, copia);
            foreach (var b in AnimationUtility.GetObjectReferenceCurveBindings(copia))
            {
                var frames = AnimationUtility.GetObjectReferenceCurve(copia, b);
                for (int i = 0; i < frames.Length; i++)
                {
                    var sp = frames[i].value as Sprite;
                    if (sp == null) continue;
                    string nombre = sp.name.StartsWith("mateo_va_simple_") && !sp.name.StartsWith("mateo_va_simple_" + traje)
                        ? "mateo_va_simple_" + traje + "_" + sp.name.Substring("mateo_va_simple_".Length) : sp.name;
                    if (sprites.TryGetValue(nombre, out var sv)) frames[i].value = sv;
                    else Debug.LogWarning("No hay sprite " + traje + " para " + sp.name);
                }
                AnimationUtility.SetObjectReferenceCurve(copia, b, frames);
            }
            AssetDatabase.CreateAsset(copia, rutaNueva);
            resultado.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, copia));
        }
        ov.ApplyOverrides(resultado);
        AssetDatabase.CreateAsset(ov, rutaOv);
        AssetDatabase.SaveAssets();
    }

    // ---------- 3) Prefab de la gema ----------
    static GameObject CrearPrefabGema()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animaciones")) AssetDatabase.CreateFolder("Assets", "Animaciones");
        if (!AssetDatabase.IsValidFolder(CarpetaGema)) AssetDatabase.CreateFolder("Assets/Animaciones", "GemaVerde");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        var sprite = AssetDatabase.LoadAllAssetsAtPath(SpriteGema).OfType<Sprite>().First();

        // Clips listos (flotar en bucle y efecto al recoger). Se regeneran cada vez que ejecutas el menu.
        AssetDatabase.DeleteAsset(CarpetaGema + "/GemaVerde_Flotar.anim");
        AssetDatabase.DeleteAsset(CarpetaGema + "/GemaVerde_Recoger.anim");
        AssetDatabase.DeleteAsset(CarpetaGema + "/GemaVerde.controller");
        var flotar = CrearClipFlotar(CarpetaGema + "/GemaVerde_Flotar.anim");
        var recoger = CrearClipRecoger(CarpetaGema + "/GemaVerde_Recoger.anim");

        string rutaCtrl = CarpetaGema + "/GemaVerde.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(rutaCtrl);
        if (ctrl == null)
        {
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(rutaCtrl);
            ctrl.AddParameter("Recoger", AnimatorControllerParameterType.Trigger);
            var sm = ctrl.layers[0].stateMachine;
            var sFlotar = sm.AddState("Flotar", new Vector3(250, 100));
            sFlotar.motion = flotar;
            var sRecoger = sm.AddState("Recoger", new Vector3(250, 200));
            sRecoger.motion = recoger;
            sm.defaultState = sFlotar;
            var t = sFlotar.AddTransition(sRecoger);
            t.hasExitTime = false;
            t.duration = 0f;
            t.AddCondition(AnimatorConditionMode.If, 0, "Recoger");
            EditorUtility.SetDirty(ctrl);
        }

        var go = new GameObject("GemaVerde");
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        go.AddComponent<GemaVerde>();

        // El dibujo va en un hijo "Visual" para poder animarlo (flotar, escalar, desvanecer) sin mover el collider
        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localScale = Vector3.one * 3f; // el sprite es pequeno (28x32 px)
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabGema);
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    static void Curva(AnimationClip clip, string prop, System.Type tipo, params Keyframe[] keys)
    {
        clip.SetCurve("Visual", tipo, prop, new AnimationCurve(keys));
    }

    // Flotar: sube y baja suave + late un poco (bucle de 1.6 s)
    static AnimationClip CrearClipFlotar(string ruta)
    {
        var clip = new AnimationClip { frameRate = 30 };
        Curva(clip, "localPosition.y", typeof(Transform),
            new Keyframe(0f, 0f), new Keyframe(0.8f, 0.15f), new Keyframe(1.6f, 0f));
        Curva(clip, "localScale.x", typeof(Transform),
            new Keyframe(0f, 3f), new Keyframe(0.8f, 3.3f), new Keyframe(1.6f, 3f));
        Curva(clip, "localScale.y", typeof(Transform),
            new Keyframe(0f, 3f), new Keyframe(0.8f, 3.3f), new Keyframe(1.6f, 3f));
        Curva(clip, "localScale.z", typeof(Transform), new Keyframe(0f, 1f), new Keyframe(1.6f, 1f));
        var cfg = AnimationUtility.GetAnimationClipSettings(clip);
        cfg.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, cfg);
        AssetDatabase.CreateAsset(clip, ruta);
        return clip;
    }

    // Recoger: crece rapido, sube y se desvanece (0.4 s); luego la gema se destruye
    static AnimationClip CrearClipRecoger(string ruta)
    {
        var clip = new AnimationClip { frameRate = 30 };
        Curva(clip, "localScale.x", typeof(Transform), new Keyframe(0f, 3f), new Keyframe(0.4f, 5.5f));
        Curva(clip, "localScale.y", typeof(Transform), new Keyframe(0f, 3f), new Keyframe(0.4f, 5.5f));
        Curva(clip, "localScale.z", typeof(Transform), new Keyframe(0f, 1f), new Keyframe(0.4f, 1f));
        Curva(clip, "localPosition.y", typeof(Transform), new Keyframe(0f, 0f), new Keyframe(0.4f, 0.6f));
        clip.SetCurve("Visual", typeof(SpriteRenderer), "m_Color.a", new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.3f, 1f), new Keyframe(0.4f, 0f)));
        var cfg = AnimationUtility.GetAnimationClipSettings(clip);
        cfg.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, cfg);
        AssetDatabase.CreateAsset(clip, ruta);
        return clip;
    }

    // ---------- 4) Escena ----------
    static void ConectarEnEscena(GameObject prefab)
    {
        EditorSceneManager.SaveOpenScenes();
        var escena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);

        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.animatorTrajeVerde = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OverrideVerde);
            pc.dobleSalto = false;
            EditorUtility.SetDirty(pc);
        }
        else Debug.LogError("No encontre a Mateo (PlayerController) en la escena Tutorial.");

        var vieja = GameObject.Find("GemaVerde_01");
        Vector3 pos = vieja != null ? vieja.transform.position : new Vector3(-5f, -3.6f, 0f);
        if (vieja != null) Object.DestroyImmediate(vieja);
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.name = "GemaVerde_01";
            inst.transform.position = pos; // sobre el segundo piso, a la izquierda del inicio
            Undo.RegisterCreatedObjectUndo(inst, "Gema verde");
        }
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }
}
#endif
