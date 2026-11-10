#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Agrega la animacion de agacharse (sprites 34-38 de las hojas de Mateo) al animator, con los 3 trajes de color
public static class CrearAgachado
{
    const string Carp = "Assets/Arte/Personajes/mateo/";
    const string CarpAnim = "Assets/Arte/Personajes/mateo/Animations/";

    static AnimationClip HacerClip(string hoja, string nombreClip)
    {
        int[] cuadros = { 34, 35, 36, 37, 38 };
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Carp + hoja + ".png").OfType<Sprite>().ToDictionary(s => s.name, s => s);
        var keys = new List<ObjectReferenceKeyframe>();
        for (int i = 0; i < cuadros.Length; i++)
        {
            string n = hoja + "_c" + cuadros[i];
            if (!sprites.ContainsKey(n)) { Debug.LogError("[Agachado] falta sprite " + n); return null; }
            keys.Add(new ObjectReferenceKeyframe { time = i * 0.05f, value = sprites[n] });
        }
        var clip = new AnimationClip { frameRate = 30 };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys.ToArray());
        var st = AnimationUtility.GetAnimationClipSettings(clip);
        st.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, st);
        string ruta = CarpAnim + nombreClip + ".anim";
        var existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if (existente != null)
        {
            // se actualiza en el mismo archivo (mantiene su GUID y los overrides)
            AnimationUtility.SetObjectReferenceCurve(existente, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys.ToArray());
            EditorUtility.SetDirty(existente);
            return existente;
        }
        AssetDatabase.CreateAsset(clip, ruta);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
    }

    // Huecos bajo los pies (px, en celdas de 204 de alto) de los cuadros 34-38 y del cuadro de pie (32).
    // Se crean sprites nuevos (c34..c38) con el pivote ajustado para que los pies queden al ras del suelo.
    static readonly Dictionary<int, int> HuecoPies = new Dictionary<int, int> { { 34, 49 }, { 35, 64 }, { 36, 63 }, { 37, 63 }, { 38, 56 } };
    const int HuecoDePie = 33, AltoCelda = 204;

    static void CrearSpritesAgachado(string hoja)
    {
        string ruta = Carp + hoja + ".png";
        var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
        var f = new SpriteDataProviderFactories(); f.Init();
        var dp = f.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        var rects = dp.GetSpriteRects().Where(r => !r.name.Contains("_c3")).ToList();
        var pares = dp.GetDataProvider<ISpriteNameFileIdDataProvider>().GetNameFileIdPairs().Where(p => !p.name.Contains("_c3")).ToList();
        foreach (var kv in HuecoPies)
        {
            var orig = rects.First(r => r.name == hoja + "_" + kv.Key);
            var nuevo = new SpriteRect
            {
                name = hoja + "_c" + kv.Key,
                rect = orig.rect,
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, 0.5f + (kv.Value - HuecoDePie) / (float)AltoCelda),
                spriteID = GUID.Generate()
            };
            rects.Add(nuevo);
            pares.Add(new SpriteNameFileIdPair(nuevo.name, nuevo.spriteID));
        }
        dp.SetSpriteRects(rects.ToArray());
        dp.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pares);
        dp.Apply();
        imp.SaveAndReimport();
    }

    [MenuItem("Esperanza/Agregar animacion de agacharse a Mateo")]
    public static void Crear()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(CarpAnim + "MateoController.controller");
        if (ctrl == null) { Debug.LogError("[Agachado] no encuentro MateoController"); return; }

        foreach (var h in new[] { "mateo_va_simple", "mateo_va_simple_azul", "mateo_va_simple_rojo", "mateo_va_simple_verde" }) CrearSpritesAgachado(h);
        var normal = HacerClip("mateo_va_simple", "crouch");
        var azul = HacerClip("mateo_va_simple_azul", "crouch_azul");
        var rojo = HacerClip("mateo_va_simple_rojo", "crouch_rojo");
        var verde = HacerClip("mateo_va_simple_verde", "crouch_verde");
        if (normal == null || azul == null || rojo == null || verde == null) return;

        if (!ctrl.parameters.Any(p => p.name == "isCrouching")) ctrl.AddParameter("isCrouching", AnimatorControllerParameterType.Bool);
        var sm = ctrl.layers[0].stateMachine;
        // si ya existia el estado, se rehace
        if (sm.states.Any(e => e.state.name == "crouch"))
        {
            // ya estaba armado: solo se actualizaron los clips y los sprites
            AssetDatabase.SaveAssets();
            Debug.Log("[Agachado] sprites y clips actualizados (pies al ras del suelo)");
            return;
        }
        var crouch = sm.AddState("crouch", new Vector3(300, 300, 0));
        crouch.motion = normal;
        crouch.writeDefaultValues = false;

        AnimatorState Estado(string n) { return sm.states.First(e => e.state.name == n).state; }
        var idle = Estado("idle"); var run = Estado("run");
        foreach (var origen in new[] { idle, run })
        {
            var t = origen.AddTransition(crouch);
            t.hasExitTime = false; t.duration = 0f; t.canTransitionToSelf = false;
            t.AddCondition(AnimatorConditionMode.If, 0f, "isCrouching");
        }
        var salir = crouch.AddTransition(idle);
        salir.hasExitTime = false; salir.duration = 0f;
        salir.AddCondition(AnimatorConditionMode.IfNot, 0f, "isCrouching");
        // si cae o se mueve el estado crouch no debe bloquear: sale tambien al dejar el suelo
        var aire = crouch.AddTransition(Estado("fall"));
        aire.hasExitTime = false; aire.duration = 0f;
        aire.AddCondition(AnimatorConditionMode.If, 0f, "isFalling");
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();

        foreach (var (nombre, clip) in new[] { ("MateoAzul", azul), ("MateoRojo", rojo), ("MateoVerde", verde) })
        {
            var oc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(CarpAnim + nombre + ".overrideController");
            if (oc == null) { Debug.LogError("[Agachado] falta " + nombre); continue; }
            var lista = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            oc.GetOverrides(lista);
            for (int i = 0; i < lista.Count; i++)
                if (lista[i].Key == normal) lista[i] = new KeyValuePair<AnimationClip, AnimationClip>(normal, clip);
            oc.ApplyOverrides(lista);
            EditorUtility.SetDirty(oc);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Agachado] animacion creada y agregada al animator (normal, azul, rojo, verde)");
    }
}
#endif
