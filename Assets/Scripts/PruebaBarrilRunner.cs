#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

// Prueba en Play del barril en llamas con ciclo (solo editor). Escribe prueba_barril.txt
public class PruebaBarrilRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1f);
        var b = FindFirstObjectByType<FuegoBarril>();
        if (b == null) { log.AppendLine("no hay barril"); Fin(log); yield break; }
        var pc = FindFirstObjectByType<PlayerController>(); var rb = pc.GetComponent<Rigidbody2D>();
        log.AppendLine("apagado asignado: " + (b.apagado != null) + "  ciclo " + b.tiempoApagado + "s apagado / " + b.tiempoEncendido + "s encendido, dano " + b.golpesDano);
        // Mateo se mantiene sobre la boca del barril; se anota si hay dano con el barril apagado y cuanto quita el primer golpe
        float t0 = Time.time; bool vioApagado = false, vioEncendido = false; float vida0 = pc.Vida01, ultima = vida0; float primerGolpe = 0f;
        while (Time.time - t0 < (b.tiempoApagado + b.tiempoEncendido) * 2.2f && primerGolpe == 0f)
        {
            pc.transform.position = b.transform.TransformPoint(new Vector3(0f, 1.9f, 0f)); rb.linearVelocity = Vector2.zero;
            bool enc = b.Encendido;
            if (!enc) vioApagado = true; else vioEncendido = true;
            float v = pc.Vida01;
            if (v < ultima - 0.001f)
            {
                primerGolpe = ultima - v;
                log.AppendLine("primer dano con el barril " + (enc ? "ENCENDIDO (cuadro " + b.CuadroActual + ")" : "APAGADO  FALLO"));
            }
            ultima = v;
            yield return null;
        }
        log.AppendLine("vio apagado: " + vioApagado + ", vio encendido: " + vioEncendido);
        log.AppendLine("primer golpe quita " + primerGolpe.ToString("F2") + " de vida" + (Mathf.Abs(primerGolpe - 1f / 3f) < 0.02f ? "  OK (un tercio)" : "  FALLO") + ", murio: " + pc.EstaMuerto);
        Fin(log);
    }
    void Fin(System.Text.StringBuilder log)
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_barril.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaBarrilArranque
{
    const string Clave = "barril_prueba";
    static PruebaBarrilArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaBarril").AddComponent<PruebaBarrilRunner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA barril ciclo (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
