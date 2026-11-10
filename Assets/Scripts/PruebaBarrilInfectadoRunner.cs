#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class PruebaBarrilInfectadoRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1f);
        var m = FindFirstObjectByType<PlayerController>(); var rb = m.GetComponent<Rigidbody2D>();
        var inf = FindObjectsByType<EnemigoInfectado>(FindObjectsSortMode.None);
        var bar = FindObjectsByType<FuegoBarril>(FindObjectsSortMode.None);
        EnemigoInfectado ei = null; FuegoBarril bb = null; float best = 1e9f;
        foreach (var e in inf) foreach (var b in bar) { float d = Mathf.Abs(e.transform.position.x - b.transform.position.x) + Mathf.Abs(e.transform.position.y - b.transform.position.y) * 3f; if (d < best) { best = d; ei = e; bb = b; } }
        if (ei == null) { log.AppendLine("sin infectado/barril"); Fin(log); yield break; }
        var mi = typeof(EnemigoInfectado).GetMethod("BarrilEntreMateo", BindingFlags.NonPublic | BindingFlags.Instance);
        float lado = Mathf.Sign(bb.transform.position.x - ei.transform.position.x); if (lado == 0) lado = 1;
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Mateo del otro lado del barril
        Vector3 p = new Vector3(bb.transform.position.x + lado * 2f, ei.transform.position.y + 0.3f, 0);
        m.transform.position = p; rb.position = p; rb.linearVelocity = Vector2.zero; Physics2D.SyncTransforms();
        yield return null;
        log.AppendLine("infectado x=" + ei.transform.position.x.ToString("F2") + " barril x=" + bb.transform.position.x.ToString("F2") + " Mateo x=" + p.x.ToString("F2"));
        log.AppendLine("barril entre (esperado True): " + mi.Invoke(ei, null));
        float x0 = ei.transform.position.x;
        yield return new WaitForSeconds(4f);
        log.AppendLine("tras 4 s infectado x=" + ei.transform.position.x.ToString("F2") + " (se movio " + (ei.transform.position.x - x0).ToString("F2") + ")");
        // Mateo del mismo lado: debe poder verlo
        p = new Vector3(ei.transform.position.x - lado * 2f, ei.transform.position.y + 0.3f, 0);
        m.transform.position = p; rb.position = p; Physics2D.SyncTransforms();
        yield return null;
        log.AppendLine("barril entre con Mateo del mismo lado (esperado False): " + mi.Invoke(ei, null));
        Fin(log);
    }
    void Fin(System.Text.StringBuilder log)
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_barril_infectado.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaBarrilInfectadoArranque
{
    const string Clave = "barrilinf_prueba";
    static PruebaBarrilInfectadoArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaBarrilInf").AddComponent<PruebaBarrilInfectadoRunner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA barril e infectado (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
