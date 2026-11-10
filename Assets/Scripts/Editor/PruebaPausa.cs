#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PruebaPausa
{
    const string Flag = "PruebaPausaEtapa";
    static StringBuilder log = new StringBuilder();
    static double t0;

    static PruebaPausa()
    {
        EditorApplication.update += Tick;
    }

    [MenuItem("Esperanza/PRUEBA pausa (Play)")]
    public static void Iniciar()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        SessionState.SetInt(Flag, 1);
        SessionState.SetString("PruebaPausaLog", "");
        EditorApplication.isPlaying = true;
    }

    static void Tick()
    {
        int etapa = SessionState.GetInt(Flag, 0);
        if (etapa == 0 || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (etapa == 1) { t0 = EditorApplication.timeSinceStartup; SessionState.SetInt(Flag, 2); return; }
        if (EditorApplication.timeSinceStartup - t0 < 3.0) return;

        var l = new StringBuilder();
        var m = Object.FindFirstObjectByType<MenuPausa>();
        l.AppendLine("MenuPausa en escena: " + (m != null));
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (m != null && pc != null)
        {
            l.AppendLine("antes: timeScale=" + Time.timeScale + " jugadorActivo=" + pc.enabled);
            m.Pausar();
            l.AppendLine("pausado: timeScale=" + Time.timeScale + " jugadorActivo=" + pc.enabled + " EstaPausado=" + MenuPausa.EstaPausado);
            m.AbrirConfig();
            m.CerrarConfig();
            m.Reanudar();
            l.AppendLine("reanudado: timeScale=" + Time.timeScale + " jugadorActivo=" + pc.enabled + " EstaPausado=" + MenuPausa.EstaPausado);
            m.Pausar();   // se queda pausado para tomar captura
            l.AppendLine("FIN: OK (queda pausado para captura)");
        }
        else l.AppendLine("FIN: FALLO");
        File.WriteAllText("prueba_pausa.txt", l.ToString());
        SessionState.SetInt(Flag, 0);
    }
}
#endif
