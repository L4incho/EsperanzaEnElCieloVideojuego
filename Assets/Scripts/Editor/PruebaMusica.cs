#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;

// Prueba (se puede borrar): abre MenuPrincipal en Play, pasa a SeleccionModo y SeleccionNiveles, y luego al Tutorial.
public static class PruebaMusica
{
    static int paso; static double t0; static StringBuilder log; static AudioSource primera;
    static void L(string s) { log.AppendLine(s); Debug.Log("[PruebaMusica] " + s); }

    [MenuItem("Esperanza/PRUEBA musica menus")]
    public static void Iniciar()
    {
        if (Application.isPlaying) { Debug.LogError("Sal de Play primero"); return; }
        log = new StringBuilder();
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity", OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
        paso = 0; t0 = EditorApplication.timeSinceStartup + 3;
        EditorApplication.update += Tick;
    }

    static string Estado() { return "escena=" + SceneManager.GetActiveScene().name + " sonando=" + MusicaMenu.Sonando + " objetosMusica=" + Object.FindObjectsByType<MusicaMenu>(FindObjectsSortMode.None).Length; }

    static void Tick()
    {
        if (!Application.isPlaying) { if (paso > 0) Fin("Play detenido"); return; }
        if (EditorApplication.timeSinceStartup < t0) return;
        switch (paso)
        {
            case 0:
                L("MenuPrincipal: " + Estado());
                primera = MusicaMenu.Instancia != null ? MusicaMenu.Instancia.GetComponent<AudioSource>() : null;
                if (primera != null) L("tiempo de la pista: " + primera.time);
                SceneManager.LoadScene("SeleccionModo"); paso++; t0 = EditorApplication.timeSinceStartup + 2; break;
            case 1:
                L("SeleccionModo: " + Estado());
                if (primera != null) L("tiempo de la pista (debe seguir avanzando, no reiniciarse): " + primera.time + " misma fuente=" + (MusicaMenu.Instancia != null && MusicaMenu.Instancia.GetComponent<AudioSource>() == primera));
                SceneManager.LoadScene("SeleccionNiveles"); paso++; t0 = EditorApplication.timeSinceStartup + 2; break;
            case 2:
                L("SeleccionNiveles: " + Estado());
                SceneManager.LoadScene("Tutorial"); paso++; t0 = EditorApplication.timeSinceStartup + 2.5; break;
            case 3:
                L("Tutorial (debe estar apagada): " + Estado());
                Fin("OK"); break;
        }
    }

    static void Fin(string motivo)
    {
        EditorApplication.update -= Tick;
        L("FIN: " + motivo);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_musica.txt"), log.ToString());
        EditorApplication.isPlaying = false;
    }
}
#endif
