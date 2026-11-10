#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text;

// Prueba (se puede borrar): en Play dispara los triggers de disparar/herido con cada traje y anota que animacion/sprite se ve.
public static class PruebaAnimaciones
{
    static StringBuilder log; static int paso; static double t0;
    static PlayerController m; static Animator an; static SpriteRenderer sr;
    static RuntimeAnimatorController[] ctrls; static string[] nombres = { "base", "verde", "rojo", "azul" };
    static int traje, fase;

    static void L(string s) { log.AppendLine(s); Debug.Log("[PruebaAnim] " + s); }

    [MenuItem("Esperanza/PRUEBA animaciones disparo-dano (Play)")]
    public static void Iniciar()
    {
        if (!Application.isPlaying) { Debug.LogError("Entra a Play"); return; }
        log = new StringBuilder();
        m = Object.FindFirstObjectByType<PlayerController>(); an = m.GetComponent<Animator>(); sr = m.GetComponent<SpriteRenderer>();
        string d = "Assets/Arte/Personajes/mateo/Animations/";
        ctrls = new RuntimeAnimatorController[] {
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(d + "MateoController.controller"),
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(d + "MateoVerde.overrideController"),
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(d + "MateoRojo.overrideController"),
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(d + "MateoAzul.overrideController") };
        traje = 0; fase = 0; paso = 0; t0 = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }

    static string Estado()
    {
        var info = an.GetCurrentAnimatorClipInfo(0);
        return "clip=" + (info.Length > 0 ? info[0].clip.name : "?") + " sprite=" + (sr.sprite != null ? sr.sprite.name : "null");
    }

    static void Tick()
    {
        if (!Application.isPlaying || m == null) { Fin("Play detenido"); return; }
        if (EditorApplication.timeSinceStartup < t0) return;
        if (traje >= 4) { Fin("OK"); return; }
        switch (fase)
        {
            case 0: an.runtimeAnimatorController = ctrls[traje]; L("== Traje " + nombres[traje]); t0 = EditorApplication.timeSinceStartup + 0.3; fase = 1; break;
            case 1: L("reposo: " + Estado()); an.SetTrigger("disparar"); t0 = EditorApplication.timeSinceStartup + 0.25; fase = 2; break;
            case 2: L("disparar a 0.25s: " + Estado()); t0 = EditorApplication.timeSinceStartup + 0.8; fase = 3; break;
            case 3: L("tras disparar: " + Estado()); an.SetTrigger("herido"); t0 = EditorApplication.timeSinceStartup + 0.2; fase = 4; break;
            case 4: L("herido a 0.2s: " + Estado()); t0 = EditorApplication.timeSinceStartup + 0.8; fase = 5; break;
            case 5: L("tras herido: " + Estado()); traje++; fase = 0; break;
        }
    }

    static void Fin(string motivo)
    {
        EditorApplication.update -= Tick;
        L("FIN: " + motivo);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_anim.txt"), log.ToString());
    }
}
#endif
