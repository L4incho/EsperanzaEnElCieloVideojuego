#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;

// Prueba en Play de la plataforma movil del Nivel 2 (solo editor)
public class PruebaPlataformaRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new StringBuilder();
        yield return new WaitForSeconds(1f);
        var go = GameObject.Find("PlataformaMovil_N2");
        if (go == null) { log.AppendLine("NO existe PlataformaMovil_N2"); Fin(log); yield break; }
        var pc = FindFirstObjectByType<PlayerController>();
        var rbM = pc.GetComponent<Rigidbody2D>();
        float xMin = 999, xMax = -999;
        // Mateo encima de la plataforma
        yield return new WaitForSeconds(2.5f);
        var col = go.GetComponent<Collider2D>();
        pc.transform.position = new Vector3(col.bounds.center.x, col.bounds.max.y + 1.2f, 0f); rbM.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        float dx0 = pc.transform.position.x - go.transform.position.x;
        float y0 = pc.transform.position.y;
        float t0 = Time.time; float tIni = Time.time;
        while (Time.time - t0 < 9f)
        {
            xMin = Mathf.Min(xMin, go.transform.position.x); xMax = Mathf.Max(xMax, go.transform.position.x);
            yield return null;
        }
        float dx1 = pc.transform.position.x - go.transform.position.x;
        log.AppendLine("plataforma recorre x de " + xMin.ToString("F1") + " a " + xMax.ToString("F1") + " (esperado -4.2 a 10.9)  " + ((xMax - xMin) > 14f ? "OK" : "FALLO"));
        log.AppendLine("Mateo encima: dx al inicio " + dx0.ToString("F2") + ", despues " + dx1.ToString("F2") + ", y " + y0.ToString("F2") + " -> " + pc.transform.position.y.ToString("F2")
            + (Mathf.Abs(dx1 - dx0) < 0.6f && Mathf.Abs(pc.transform.position.y - y0) < 0.4f ? "  OK (se lo lleva)" : "  FALLO"));
        Fin(log);
    }
    void Fin(StringBuilder log)
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_plataforma.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}
#endif
