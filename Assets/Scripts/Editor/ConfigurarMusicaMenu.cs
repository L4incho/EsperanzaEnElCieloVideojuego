#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Pone la musica de los menus en MenuPrincipal, SeleccionModo y SeleccionNiveles.
public static class ConfigurarMusicaMenu
{
    const string RutaAudio = "Assets/Audio/Musica_Esperanzaen_elcielo.mp3";
    static readonly string[] Menus = { "MenuPrincipal", "SeleccionModo", "SeleccionNiveles" };

    [MenuItem("Esperanza/Poner musica en los menus")]
    public static void Crear()
    {
        var imp = AssetImporter.GetAtPath(RutaAudio) as AudioImporter;
        if (imp == null) { Debug.LogError("[Musica] No encontre " + RutaAudio); return; }
        // musica larga: se transmite en vez de cargarse completa en memoria
        var cfg = imp.defaultSampleSettings;
        cfg.loadType = AudioClipLoadType.Streaming;
        imp.defaultSampleSettings = cfg;
        imp.forceToMono = false;
        imp.SaveAndReimport();
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaAudio);
        if (clip == null) { Debug.LogError("[Musica] No se pudo cargar el audio"); return; }

        EditorSceneManager.SaveOpenScenes();
        string escenaOriginal = EditorSceneManager.GetActiveScene().path;
        foreach (var n in Menus)
        {
            string ruta = "Assets/Scenes/" + n + ".unity";
            if (!System.IO.File.Exists(ruta)) { Debug.LogWarning("[Musica] No existe " + ruta); continue; }
            var sc = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            var viejo = GameObject.Find("MusicaMenu");
            if (viejo != null) Object.DestroyImmediate(viejo);
            var go = new GameObject("MusicaMenu");
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false; src.loop = true; src.clip = clip;
            var m = go.AddComponent<MusicaMenu>();
            m.musica = clip; m.volumen = 0.6f;
            EditorSceneManager.MarkSceneDirty(sc);
            EditorSceneManager.SaveScene(sc);
            Debug.Log("[Musica] Agregada en " + n);
        }
        if (!string.IsNullOrEmpty(escenaOriginal)) EditorSceneManager.OpenScene(escenaOriginal, OpenSceneMode.Single);
        Debug.Log("[Musica] Listo");
    }
}
#endif
