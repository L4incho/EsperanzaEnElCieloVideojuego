#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

// Mateo toma una gema y una venda/botiquin, muere y debe volver sin gema con todo reaparecido. Escribe prueba_reaparecer.txt
public class PruebaReaparecerRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1f);
        var m = FindFirstObjectByType<PlayerController>(); var rb = m.GetComponent<Rigidbody2D>();
        var gemas = FindObjectsByType<GemaVerde>(FindObjectsSortMode.None);
        var cura = FindObjectsByType<RecogibleCuracion>(FindObjectsSortMode.None);
        log.AppendLine("gemas: " + gemas.Length + ", vendas/botiquines: " + cura.Length);
        // cada tipo de gema
        foreach (var g in gemas)
        {
            m.TomarGema(g);
            yield return new WaitForSeconds(0.8f);
            bool inactiva = !g.gameObject.activeSelf;
            m.Morir();
            yield return new WaitForSeconds(2.5f);
            log.AppendLine("gema " + g.tipo + " (" + g.name + "): desaparece al tomarla=" + inactiva + "; tras morir: activa=" + g.gameObject.activeSelf + ", Mateo sin gema=" + (m.GemaActual == null) + (inactiva && g.gameObject.activeSelf && m.GemaActual == null ? "  OK" : "  FALLO"));
        }
        // curacion: bajarle vida y recoger todas
        foreach (var c in cura)
        {
            m.RecibirGolpes(1, c.transform.position + Vector3.left);
            yield return new WaitForSeconds(1.3f);
            m.transform.position = c.transform.position; rb.position = c.transform.position; Physics2D.SyncTransforms();
            yield return new WaitForSeconds(0.8f);
            bool recogida = !c.gameObject.activeSelf;
            m.Morir();
            yield return new WaitForSeconds(2.5f);
            log.AppendLine("curacion " + c.name + ": recogida=" + recogida + "; tras morir activa=" + c.gameObject.activeSelf + (recogida && c.gameObject.activeSelf ? "  OK" : "  FALLO"));
        }
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_reaparecer.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaReaparecerArranque
{
    const string Clave = "reaparecer_prueba";
    static PruebaReaparecerArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaReaparecer").AddComponent<PruebaReaparecerRunner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA reaparecer objetos al morir (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
