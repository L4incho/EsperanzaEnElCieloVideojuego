#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

// Prueba en Play de la plataforma vertical del Nivel 2 (solo editor). Escribe prueba_plataforma_vertical.txt
public class PruebaPlataformaVerticalRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1f);
        var go = GameObject.Find("PlataformaVertical_N2");
        if (go == null) { log.AppendLine("NO existe PlataformaVertical_N2"); Fin(log); yield break; }
        var pm = go.GetComponent<PlataformaMovil>();
        var pc = FindFirstObjectByType<PlayerController>(); var rb = pc.GetComponent<Rigidbody2D>();
        var col = go.GetComponent<Collider2D>();
        pc.transform.position = new Vector3(col.bounds.center.x, col.bounds.max.y + 1.2f, 0f); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        // 1) sin Mateo encima no se mueve
        pc.transform.position = new Vector3(col.bounds.center.x - 6f, pm.puntoA.y + 3f, 0f); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(3f);
        float quieta = go.transform.position.y;
        log.AppendLine("sin Mateo: y=" + quieta.ToString("F2") + (Mathf.Abs(quieta - pm.puntoA.y) < 0.1f ? "  OK (quieta abajo)" : "  FALLO"));
        // 2) con Mateo encima sube
        pc.transform.position = new Vector3(col.bounds.center.x, col.bounds.max.y + 1.2f, 0f); rb.linearVelocity = Vector2.zero;
        var anim = pc.GetComponent<Animator>(); int cayendo = 0, muestras = 0; float t1 = Time.time;
        yield return new WaitForSeconds(1f);
        while (Time.time - t1 < 7f) { muestras++; if (anim.GetBool("isFalling") || !anim.GetBool("isGrounded")) cayendo++; yield return null; }
        log.AppendLine("animacion cayendo/en aire mientras sube: " + cayendo + " de " + muestras + " cuadros" + (cayendo == 0 ? "  OK" : "  FALLO"));
        { var pcol = pc.GetComponent<Collider2D>(); log.AppendLine("DEBUG solo=" + pm.soloConPasajero + " plat.bounds=" + col.bounds + " mateo.bounds=" + pcol.bounds + " mateoPos=" + pc.transform.position + " platPos=" + go.transform.position + " vy=" + rb.linearVelocity.y);
          foreach (var h in Physics2D.OverlapBoxAll(new Vector2(col.bounds.center.x, col.bounds.max.y + 0.12f), new Vector2(col.bounds.size.x * 0.9f, 0.3f), 0f)) log.AppendLine("  overlap: " + h.name); }
        float arriba = go.transform.position.y; float dy = pc.transform.position.y - arriba;
        log.AppendLine("con Mateo: y=" + arriba.ToString("F2") + " (meta " + pm.puntoB.y.ToString("F2") + ")" + (Mathf.Abs(arriba - pm.puntoB.y) < 0.1f ? "  OK" : "  FALLO") + "; Mateo encima dy=" + dy.ToString("F2") + (dy > 0.3f && dy < 1.5f ? "  OK" : "  FALLO"));
        // 3) Mateo se baja y regresa
        pc.transform.position = new Vector3(col.bounds.center.x + 6f, arriba + 2f, 0f); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(7f);
        float vuelta = go.transform.position.y;
        log.AppendLine("sin Mateo otra vez: y=" + vuelta.ToString("F2") + (Mathf.Abs(vuelta - pm.puntoA.y) < 0.1f ? "  OK (volvio abajo)" : "  FALLO"));
        Fin(log);
    }
    void Fin(System.Text.StringBuilder log)
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_plataforma_vertical.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}
#endif
