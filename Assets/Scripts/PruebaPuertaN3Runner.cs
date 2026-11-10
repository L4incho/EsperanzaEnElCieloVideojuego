#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Prueba en Play: Mateo frente a la puerta del Nivel 2 ve la W y al entrar se carga Nivel3. Escribe prueba_puerta_n3.txt
public class PruebaPuertaN3Runner : MonoBehaviour
{
    IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1f);
        var puerta = GameObject.Find("Puerta_Nivel3") != null ? GameObject.Find("Puerta_Nivel3").GetComponent<Puerta>() : null;
        var m = FindFirstObjectByType<PlayerController>();
        if (puerta == null || m == null) { log.AppendLine("falta Puerta_Nivel3 o Mateo"); Fin(log); yield break; }
        var c = puerta.GetComponent<Collider2D>().bounds; var rb = m.GetComponent<Rigidbody2D>();
        m.transform.position = new Vector3(c.center.x - 0.3f, c.min.y + 0.8f, 0f); rb.position = m.transform.position; rb.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.6f);
        var ind = puerta.transform.Find("Indicador_W");
        log.AppendLine("destino: " + puerta.escenaDestino + "; indicador W visible al estar frente a la puerta: " + (ind != null && ind.gameObject.activeSelf ? "OK" : "FALLO"));
        // fuera de la puerta no debe verse
        m.transform.position = new Vector3(c.center.x - 6f, c.min.y + 0.8f, 0f); rb.position = m.transform.position;
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.6f);
        log.AppendLine("indicador oculto lejos de la puerta: " + (ind != null && !ind.gameObject.activeSelf ? "OK" : "FALLO"));
        m.transform.position = new Vector3(c.center.x - 0.3f, c.min.y + 0.8f, 0f); rb.position = m.transform.position;
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.6f);
        puerta.Entrar();
        yield return new WaitForSeconds(3f);
        log.AppendLine("escena tras entrar: " + SceneManager.GetActiveScene().name + (SceneManager.GetActiveScene().name == "Nivel3" ? "  OK" : "  FALLO"));
        Fin(log);
    }
    void Fin(System.Text.StringBuilder log)
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_puerta_n3.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaPuertaN3Arranque
{
    const string Clave = "puertan3_prueba";
    static PruebaPuertaN3Arranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaPuertaN3").AddComponent<PruebaPuertaN3Runner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA puerta nivel 3 (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
