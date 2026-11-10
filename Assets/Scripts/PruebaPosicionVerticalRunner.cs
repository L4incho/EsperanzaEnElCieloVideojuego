#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

public class PruebaPosicionVerticalRunner : MonoBehaviour
{
    public float yEditor;
    IEnumerator Start()
    {
        var p = GameObject.Find("PlataformaVertical_N2");
        var log = new System.Text.StringBuilder();
        yield return new WaitForSeconds(2f);
        log.AppendLine("y en editor = " + yEditor.ToString("F3") + "; y tras 2 s en Play = " + p.transform.position.y.ToString("F3") + (Mathf.Abs(p.transform.position.y - yEditor) < 0.05f ? "  OK" : "  FALLO"));
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_posicion_vertical.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}

[InitializeOnLoad]
public static class PruebaPosicionVerticalArranque
{
    const string Clave = "posvert_prueba", ClaveY = "posvert_y";
    static PruebaPosicionVerticalArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            { EditorPrefs.SetBool(Clave, false); var g = new GameObject("PruebaPosVert").AddComponent<PruebaPosicionVerticalRunner>(); g.yEditor = EditorPrefs.GetFloat(ClaveY, 0f); }
        };
    }
    [MenuItem("Esperanza/PRUEBA posicion plataforma vertical (Play)")]
    static void Lanzar()
    {
        var p = GameObject.Find("PlataformaVertical_N2");
        EditorPrefs.SetFloat(ClaveY, p != null ? p.transform.position.y : 0f);
        EditorPrefs.SetBool(Clave, true); EditorApplication.EnterPlaymode();
    }
}
#endif
