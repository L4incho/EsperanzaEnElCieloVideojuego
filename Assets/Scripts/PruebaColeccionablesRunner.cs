#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;

// Compara la posicion del DIBUJO de cada gema/microchip en el editor contra Play: no debe cambiar.
public class PruebaColeccionablesRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        var antes = EditorPrefs.GetString("colec_antes", "").Split(';');
        var ahora = Capturar();
        var sb = new StringBuilder(); int mal = 0, n = 0;
        var d = new System.Collections.Generic.Dictionary<string, Vector2>();
        foreach (var a in antes) { var p = a.Split('|'); if (p.Length == 3) d[p[0]] = new Vector2(float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture)); }
        foreach (var kv in ahora)
        {
            if (!d.ContainsKey(kv.Key)) continue;
            n++;
            // el vaiven mueve el dibujo como mucho ~0.3; mas que eso = se desplazo
            if (Vector2.Distance(d[kv.Key], kv.Value) > 0.4f) { mal++; sb.AppendLine("DESPLAZADO " + kv.Key + " editor=" + d[kv.Key] + " play=" + kv.Value); }
        }
        sb.Insert(0, "revisados " + n + ", desplazados " + mal + (mal == 0 ? "  OK" : "  FALLO") + "\n");
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_coleccionables.txt"), sb.ToString());
        EditorApplication.ExitPlaymode();
    }

    public static System.Collections.Generic.Dictionary<string, Vector2> Capturar()
    {
        var r = new System.Collections.Generic.Dictionary<string, Vector2>();
        foreach (var g in Object.FindObjectsByType<GemaVerde>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var sr = g.GetComponentInChildren<SpriteRenderer>(true); if (sr == null) continue;
            r["G" + g.name] = sr.transform.position;
        }
        foreach (var g in Object.FindObjectsByType<Microchip>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var sr = g.GetComponentInChildren<SpriteRenderer>(true); if (sr == null) continue;
            r["M" + g.name] = sr.transform.position;
        }
        return r;
    }
}

[InitializeOnLoad]
public static class PruebaColeccionablesArranque
{
    const string Clave = "colec_prueba";
    static PruebaColeccionablesArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaColec").AddComponent<PruebaColeccionablesRunner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA gemas y microchips en Play")]
    static void Lanzar()
    {
        // en editor las claves de microchip usan la posicion del objeto raiz (puede cambiar en Play si movieron solo el hijo): usamos el nombre del objeto + posicion del DIBUJO
        var sb = new StringBuilder();
        foreach (var kv in PruebaColeccionablesRunner.Capturar()) sb.Append(kv.Key + "|" + kv.Value.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + kv.Value.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
        EditorPrefs.SetString("colec_antes", sb.ToString());
        EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode();
    }
}
#endif
