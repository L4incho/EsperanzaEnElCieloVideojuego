#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.IO;

public static class VersionYExportar
{
    const string Version = "1.0.0";
    static readonly string[] Escenas = { "MenuPrincipal", "SeleccionModo", "SeleccionNiveles", "Tutorial", "Nivel2", "Nivel3", "Nivel4Jefe" };

    [MenuItem("Esperanza/Poner version en el menu")]
    public static void PonerVersion()
    {
        PlayerSettings.bundleVersion = Version;
        EditorSceneManager.SaveOpenScenes();
        var original = EditorSceneManager.GetActiveScene().path;
        var sc = EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity", OpenSceneMode.Single);
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("[Version] No hay Canvas en MenuPrincipal"); return; }
        var viejo = canvas.transform.Find("TextoVersion");
        if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

        var go = new GameObject("TextoVersion", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(1, 0);
        r.anchoredPosition = new Vector2(-24, 18);
        r.sizeDelta = new Vector2(500, 44);
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 28;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.LowerRight;
        t.color = new Color(0.75f, 1f, 0.9f, 0.95f);
        t.raycastTarget = false;
        t.text = "Versión " + Version;
        var sombra = go.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.85f);
        sombra.effectDistance = new Vector2(2, -2);
        go.AddComponent<VersionTexto>();
        go.transform.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log("[Version] Texto de version agregado al menu: " + Version);
        if (!string.IsNullOrEmpty(original) && original != sc.path) EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
    }

    [MenuItem("Esperanza/Exportar juego (Windows)")]
    public static void Exportar()
    {
        EditorSceneManager.SaveOpenScenes();
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.productName = "Esperanza en el Cielo";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;

        var lista = new System.Collections.Generic.List<string>();
        foreach (var n in Escenas)
        {
            string p = "Assets/Scenes/" + n + ".unity";
            if (File.Exists(p)) lista.Add(p); else Debug.LogWarning("[Exportar] No existe " + p);
        }
        var opts = new BuildPlayerOptions
        {
            scenes = lista.ToArray(),
            locationPathName = "Build/EsperanzaEnElCielo/EsperanzaEnElCielo.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        var rep = BuildPipeline.BuildPlayer(opts);
        string res = "Exportar: " + rep.summary.result + " | " + (rep.summary.totalSize / 1048576) + " MB | errores=" + rep.summary.totalErrors + " | " + opts.locationPathName;
        File.WriteAllText("prueba_exportar.txt", res);
        Debug.Log("[Exportar] " + res);
    }
}
#endif
