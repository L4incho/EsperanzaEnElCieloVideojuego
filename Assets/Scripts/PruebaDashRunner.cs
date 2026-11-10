#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Prueba en Play: Mateo con gema azul hace dash a traves de un infectado (solo editor). Escribe prueba_dash.txt
public class PruebaDashRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1f);
        var pc = FindFirstObjectByType<PlayerController>(); var rb = pc.GetComponent<Rigidbody2D>();
        var en = FindFirstObjectByType<EnemigoInfectado>();
        if (en == null) { log.AppendLine("no hay infectados en la escena"); Fin(log); yield break; }
        GemaVerde gema = null;
        foreach (var g in FindObjectsByType<GemaVerde>(FindObjectsSortMode.None)) if (g.tipo == TipoGema.Azul) { gema = g; break; }
        if (gema == null) { log.AppendLine("no hay gema azul en la escena"); Fin(log); yield break; }
        var f = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        typeof(PlayerController).GetField("gemaActual", f).SetValue(pc, gema);
        var sr = pc.GetComponent<SpriteRenderer>(); sr.flipX = false;
        float ex = en.transform.position.x;
        pc.transform.position = new Vector3(ex - 2.5f, en.transform.position.y + 0.1f, 0f); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.4f);
        float x0 = pc.transform.position.x;
        typeof(PlayerController).GetMethod("IniciarDash", f).Invoke(pc, null);
        yield return new WaitForSeconds(0.7f);
        float x1 = pc.transform.position.x;
        log.AppendLine("enemigo x=" + ex.ToString("F2") + "  Mateo antes x=" + x0.ToString("F2") + "  despues x=" + x1.ToString("F2") + (x1 > ex + 0.4f ? "  OK (lo atraveso)" : "  FALLO (se quedo bloqueado)"));
        yield return new WaitForSeconds(1.5f);
        var cm = pc.GetComponent<Collider2D>(); var ce = en.GetComponent<Collider2D>();
        log.AppendLine("colision restaurada despues: " + (!Physics2D.GetIgnoreCollision(cm, ce) ? "OK" : "FALLO (sigue ignorada)"));
        Fin(log);
    }
    void Fin(System.Text.StringBuilder log)
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_dash.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaDashArranque
{
    const string Clave = "dash_prueba";
    static PruebaDashArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaDash").AddComponent<PruebaDashRunner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA dash azul (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
