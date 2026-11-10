#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Prueba automatica del Nivel 2 (se ejecuta en Play y escribe prueba_nivel2.txt en la carpeta del proyecto)
[InitializeOnLoad]
public static class PruebaNivel2Arranque
{
    const string Clave = "n2_prueba";
    static PruebaNivel2Arranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            {
                EditorPrefs.SetBool(Clave, false);
                new GameObject("PruebaNivel2").AddComponent<PruebaNivel2>();
            }
        };
    }

    [MenuItem("Esperanza/PRUEBA paletas Nivel 2 (Play)")]
    static void Lanzar()
    {
        EditorPrefs.SetBool(Clave, true);
        EditorApplication.EnterPlaymode();
    }
}
#endif
