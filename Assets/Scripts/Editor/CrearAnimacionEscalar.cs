#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Esperanza > Crear animacion de escalar
// Crea el clip "climb" de Mateo (y su version para cada traje), agrega el estado "climb" al Animator
// (parametros isClimbing y climbSpeed) y deja los overrides de los trajes listos. Se puede ejecutar varias veces.
public static class CrearAnimacionEscalar
{
    const string Carpeta = "Assets/Arte/Personajes/mateo/";
    const string Controller = Carpeta + "Animations/MateoController.controller";
    const string RutaClip = Carpeta + "Animations/climb.anim";

    // Cuadros de Mateo de espaldas subiendo la escalera: Assets/Arte/Personajes/mateo/Escalar/escalar_<traje>_<n>.png
    const string CarpetaEscalar = Carpeta + "Escalar/";
    const int NumCuadros = 4;
    const float Paso = 0.15f;   // segundos por cuadro

    [MenuItem("Esperanza/Crear animacion de escalar")]
    public static void Ejecutar()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
        if (ctrl == null) { Debug.LogError("No encuentro " + Controller); return; }

        var clip = CrearClip(RutaClip, "normal");
        ConfigurarController(ctrl, clip);

        foreach (var traje in new[] { "verde", "azul", "rojo" })
        {
            if (AssetDatabase.LoadMainAssetAtPath(CarpetaEscalar + "escalar_" + traje + "_0.png") == null) continue;
            var clipT = CrearClip(Carpeta + "Animations/climb_" + traje + ".anim", traje);
            string rutaOv = Carpeta + "Animations/Mateo" + char.ToUpper(traje[0]) + traje.Substring(1) + ".overrideController";
            var ov = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(rutaOv);
            if (ov == null) continue;
            var pares = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            ov.GetOverrides(pares);
            for (int i = 0; i < pares.Count; i++)
                if (pares[i].Key == clip) pares[i] = new KeyValuePair<AnimationClip, AnimationClip>(pares[i].Key, clipT);
            ov.ApplyOverrides(pares);
            EditorUtility.SetDirty(ov);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Animacion de escalar lista (clip climb + estado en MateoController + trajes).");
    }

    static Sprite CargarCuadro(string traje, int i)
    {
        string ruta = CarpetaEscalar + "escalar_" + traje + "_" + i + ".png";
        var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (imp == null) { Debug.LogError("Falta el cuadro " + ruta); return null; }
        var hoja = AssetImporter.GetAtPath(Carpeta + "mateo_va_simple.png") as TextureImporter;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = hoja != null ? hoja.spritePixelsPerUnit : 100f;
        imp.filterMode = hoja != null ? hoja.filterMode : FilterMode.Point;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        var s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.spriteAlignment = (int)SpriteAlignment.Center;
        imp.SetTextureSettings(s);
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    static AnimationClip CrearClip(string ruta, string traje)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, ruta); }
        clip.frameRate = 60;
        var frames = new List<ObjectReferenceKeyframe>();
        for (int i = 0; i < NumCuadros; i++)
        {
            var sp = CargarCuadro(traje, i);
            if (sp != null) frames.Add(new ObjectReferenceKeyframe { time = i * Paso, value = sp });
        }
        var b = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, b, frames.ToArray());
        var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
        ajustes.loopTime = true;
        ajustes.stopTime = NumCuadros * Paso;
        AnimationUtility.SetAnimationClipSettings(clip, ajustes);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    static void ConfigurarController(AnimatorController ctrl, AnimationClip clip)
    {
        if (!ctrl.parameters.Any(p => p.name == "isClimbing")) ctrl.AddParameter("isClimbing", AnimatorControllerParameterType.Bool);
        if (!ctrl.parameters.Any(p => p.name == "climbSpeed")) ctrl.AddParameter("climbSpeed", AnimatorControllerParameterType.Float);
        // climbSpeed empieza en 1
        var ps = ctrl.parameters;
        for (int i = 0; i < ps.Length; i++) if (ps[i].name == "climbSpeed") ps[i].defaultFloat = 1f;
        ctrl.parameters = ps;

        var sm = ctrl.layers[0].stateMachine;
        var viejo = sm.states.FirstOrDefault(s => s.state.name == "climb");
        if (viejo.state != null) sm.RemoveState(viejo.state);
        foreach (var t in sm.anyStateTransitions.Where(t => t.destinationState == null).ToArray()) sm.RemoveAnyStateTransition(t);

        var st = sm.AddState("climb");
        st.motion = clip;
        st.speedParameterActive = true;
        st.speedParameter = "climbSpeed";

        var entra = sm.AddAnyStateTransition(st);
        entra.hasExitTime = false; entra.duration = 0f; entra.canTransitionToSelf = false;
        entra.AddCondition(AnimatorConditionMode.If, 0, "isClimbing");

        var idle = sm.states.FirstOrDefault(s => s.state.name == "idle").state;
        if (idle != null)
        {
            var sale = st.AddTransition(idle);
            sale.hasExitTime = false; sale.duration = 0f;
            sale.AddCondition(AnimatorConditionMode.IfNot, 0, "isClimbing");
        }
        EditorUtility.SetDirty(ctrl);
    }
}
#endif
