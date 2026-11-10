#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Lanza la prueba de infectados en Play (escribe prueba_infectados.txt en la carpeta del proyecto)
[InitializeOnLoad]
public static class PruebaInfectadosArranque
{
    const string Clave = "inf_prueba";
    static PruebaInfectadosArranque()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Clave, false))
            {
                EditorPrefs.SetBool(Clave, false);
                new GameObject("PruebaInfectados").AddComponent<PruebaInfectados>();
            }
        };
    }

    [MenuItem("Esperanza/PRUEBA infectados (Play)")]
    static void Lanzar()
    {
        EditorPrefs.SetBool(Clave, true);
        EditorApplication.EnterPlaymode();
    }
}
#endif
