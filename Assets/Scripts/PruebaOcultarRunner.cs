#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Prueba en Play: todo tile con objeto instanciado debe tener su dibujo apagado (alpha 0). Escribe prueba_ocultar.txt
public class PruebaOcultarRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(1.5f);
        int total = 0, mal = 0;
        foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            foreach (var c in tm.cellBounds.allPositionsWithin)
            {
                var go = tm.GetInstantiatedObject(c);
                if (go == null || go.GetComponent<OcultarTile>() == null) continue;
                total++;
                bool oculto = tm.GetColor(c).a < 0.01f;
                if (!oculto) mal++;
                log.AppendLine(tm.name + " celda " + c + " obj " + go.name + (oculto ? "  tile oculto OK" : "  TILE VISIBLE (FALLO)"));
            }
        log.AppendLine("total " + total + ", mal " + mal + (mal == 0 && total > 0 ? "  => OK" : "  => FALLO"));
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_ocultar.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaOcultarArranque
{
    const string Clave = "ocultar_prueba";
    static PruebaOcultarArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); new GameObject("PruebaOcultar").AddComponent<PruebaOcultarRunner>(); }
        };
    }
    [MenuItem("Esperanza/PRUEBA tiles con objeto ocultos (Play)")]
    static void Lanzar() { EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode(); }
}
#endif
