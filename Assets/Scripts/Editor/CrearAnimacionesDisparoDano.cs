#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// Crea las animaciones de DISPARAR y RECIBIR DANO para los 4 trajes (blanco, verde, rojo, azul)
// y las conecta al Animator de Mateo (triggers "disparar" y "herido").
public static class CrearAnimacionesDisparoDano
{
    const string Carpeta = "Assets/Arte/Personajes/mateo/";
    const string Anims = Carpeta + "Animations/";

    static readonly string[] Trajes = { "", "_verde", "_rojo", "_azul" };
    static readonly string[] Overrides = { null, "MateoVerde", "MateoRojo", "MateoAzul" };
    static readonly int[] CuadrosDisparo = { 24, 25, 26, 27, 28, 29, 30, 31 };
    static readonly int[] CuadrosDano = { 33, 34, 35, 34, 33 };

    static AnimationClip Crear(string nombre, string hoja, int[] cuadros, float fps)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Carpeta + hoja + ".png").OfType<Sprite>().ToDictionary(s => s.name, s => s);
        string baseNombre = hoja + "_";
        var keys = new List<ObjectReferenceKeyframe>();
        for (int i = 0; i < cuadros.Length; i++)
        {
            Sprite sp;
            if (!sprites.TryGetValue(baseNombre + cuadros[i], out sp)) { Debug.LogError("[Anim] Falta el sprite " + baseNombre + cuadros[i]); return null; }
            keys.Add(new ObjectReferenceKeyframe { time = i / fps, value = sp });
        }
        keys.Add(new ObjectReferenceKeyframe { time = cuadros.Length / fps, value = keys[keys.Count - 1].value });

        string ruta = Anims + nombre + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, ruta); }
        clip.frameRate = fps;
        var bind = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, bind, keys.ToArray());
        var set = AnimationUtility.GetAnimationClipSettings(clip);
        set.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, set);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    static void Estado(AnimatorController ctrl, AnimatorStateMachine sm, AnimatorState idle, string nombre, string trigger, AnimationClip clip)
    {
        var st = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == nombre);
        if (st == null) st = sm.AddState(nombre);
        st.motion = clip;
        st.speed = 1f;
        foreach (var t in sm.anyStateTransitions.Where(t => t.destinationState == st).ToArray()) sm.RemoveAnyStateTransition(t);
        foreach (var t in st.transitions.ToArray()) st.RemoveTransition(t);
        var any = sm.AddAnyStateTransition(st);
        any.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        any.hasExitTime = false; any.duration = 0f; any.canTransitionToSelf = true;
        var exit = st.AddTransition(idle);
        exit.hasExitTime = true; exit.exitTime = 1f; exit.duration = 0f;
    }

    [MenuItem("Esperanza/Crear animaciones de disparar y recibir dano")]
    public static void CrearTodo()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(Anims + "MateoController.controller");
        if (ctrl == null) { Debug.LogError("[Anim] No encuentro MateoController"); return; }
        foreach (var p in new[] { "disparar", "herido" })
            if (!ctrl.parameters.Any(x => x.name == p)) ctrl.AddParameter(p, AnimatorControllerParameterType.Trigger);

        var clips = new Dictionary<string, AnimationClip>();
        for (int i = 0; i < Trajes.Length; i++)
        {
            string hoja = "mateo_va_simple" + Trajes[i];
            clips["shoot" + Trajes[i]] = Crear("shoot" + Trajes[i], hoja, CuadrosDisparo, 14f);
            clips["hurt" + Trajes[i]] = Crear("hurt" + Trajes[i], hoja, CuadrosDano, 12.5f);
        }
        AssetDatabase.SaveAssets();

        var sm = ctrl.layers[0].stateMachine;
        var idle = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == "idle");
        if (idle == null) { Debug.LogError("[Anim] No hay estado idle"); return; }
        Estado(ctrl, sm, idle, "shoot", "disparar", clips["shoot"]);
        Estado(ctrl, sm, idle, "hurt", "herido", clips["hurt"]);
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();

        for (int i = 1; i < Trajes.Length; i++)
        {
            var oc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(Anims + Overrides[i] + ".overrideController");
            if (oc == null) { Debug.LogError("[Anim] Falta " + Overrides[i]); continue; }
            var lista = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            oc.GetOverrides(lista);
            for (int k = 0; k < lista.Count; k++)
            {
                string n = lista[k].Key != null ? lista[k].Key.name : "";
                if (n == "shoot") lista[k] = new KeyValuePair<AnimationClip, AnimationClip>(lista[k].Key, clips["shoot" + Trajes[i]]);
                else if (n == "hurt") lista[k] = new KeyValuePair<AnimationClip, AnimationClip>(lista[k].Key, clips["hurt" + Trajes[i]]);
            }
            oc.ApplyOverrides(lista);
            EditorUtility.SetDirty(oc);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Anim] Animaciones de disparar y recibir dano creadas para blanco, verde, rojo y azul.");
    }
}
#endif
