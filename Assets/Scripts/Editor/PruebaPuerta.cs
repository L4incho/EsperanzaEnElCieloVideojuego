#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Text;

// Prueba automatica (se puede borrar): en Play lleva a Mateo a la puerta, la "usa" y revisa que se cargue Nivel2.
public static class PruebaPuerta
{
    static double t0; static int paso; static Puerta puerta; static PlayerController m; static StringBuilder log;
    static void L(string s) { log.AppendLine(s); Debug.Log("[PruebaPuerta] " + s); }

    [MenuItem("Esperanza/PRUEBA puerta (Play)")]
    public static void Iniciar()
    {
        if (!Application.isPlaying) { Debug.LogError("Entra a Play primero"); return; }
        log = new StringBuilder();
        puerta = Object.FindFirstObjectByType<Puerta>();
        m = Object.FindFirstObjectByType<PlayerController>();
        if (puerta == null || m == null) { Debug.LogError("Falta Puerta o Mateo"); return; }
        var c = puerta.GetComponent<Collider2D>().bounds;
        var rb = m.GetComponent<Rigidbody2D>();
        Vector3 p = new Vector3(c.center.x - 0.4f, c.min.y + 0.8f, 0f);
        m.transform.position = p; rb.position = p; rb.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        L("Mateo puesto frente a la puerta en " + p + " escena=" + SceneManager.GetActiveScene().name);
        paso = 0; t0 = EditorApplication.timeSinceStartup + 1.0;
        EditorApplication.update += Tick;
    }

    static void Tick()
    {
        if (!Application.isPlaying) { Fin("Play detenido"); return; }
        if (EditorApplication.timeSinceStartup < t0) return;
        switch (paso)
        {
            case 0:
                L("Indicador W visible: " + (puerta != null && puerta.transform.Find("Indicador_W") != null && puerta.transform.Find("Indicador_W").gameObject.activeSelf));
                puerta.Entrar();
                paso++; t0 = EditorApplication.timeSinceStartup + 3.0; break;
            case 1:
                L("Escena actual tras entrar: " + SceneManager.GetActiveScene().name);
                Fin("OK"); break;
        }
    }

    static void Fin(string motivo)
    {
        EditorApplication.update -= Tick;
        L("FIN: " + motivo);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_puerta.txt"), log.ToString());
    }
}
#endif
