#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class CrearMenuPausa
{
    const string Dir = "Assets/Arte/Esperanza en el cielo/UI del juego/menu/";
    const string RutaFondo = Dir + "pestaña_continuar_partida/fondo_continuar_p.png";
    const string RutaConfig = Dir + "pestaña_opciones/menu_sonido.png";
    const string RutaBotones = Dir + "TODOS_los_botones/botones_menu.png";
    const string RutaVolver = Dir + "TODOS_los_botones/botones_jugar_volver&jugar.png";

    static Sprite Sub(string ruta, string nombre)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault(s => s.name == nombre);
    }

    static Sprite Unico(string ruta)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (imp == null) { Debug.LogError("[Pausa] No existe " + ruta); return null; }
        if (imp.textureType != TextureImporterType.Sprite || imp.spriteImportMode != SpriteImportMode.Single)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.maxTextureSize = 2048;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault();
    }

    [MenuItem("Esperanza/Crear menu de pausa")]
    public static void Crear()
    {
        var fondo = Unico(RutaFondo);
        var cfg = Unico(RutaConfig);
        var opc = new[] { Sub(RutaBotones, "btn_OPCIONES_Normal"), Sub(RutaBotones, "btn_OPCIONES_Resaltado"), Sub(RutaBotones, "btn_OPCIONES_Presionado") };
        var sal = new[] { Sub(RutaBotones, "btn_SALIR_Normal"), Sub(RutaBotones, "btn_SALIR_Resaltado"), Sub(RutaBotones, "btn_SALIR_Presionado") };
        var vol = new[] { Sub(RutaVolver, "botones_jugar_volver&jugar_1"), Sub(RutaVolver, "botones_jugar_volver&jugar_3"), Sub(RutaVolver, "botones_jugar_volver&jugar_5") };

        if (fondo == null || cfg == null || opc.Any(s => s == null) || sal.Any(s => s == null) || vol.Any(s => s == null))
        {
            Debug.LogError("[Pausa] Faltan imagenes: fondo=" + (fondo != null) + " config=" + (cfg != null)
                + " opciones=" + opc.Count(s => s != null) + " salir=" + sal.Count(s => s != null) + " volver=" + vol.Count(s => s != null));
            return;
        }

        var go = new GameObject("MenuPausa");
        var m = go.AddComponent<MenuPausa>();
        m.fondo = fondo; m.fondoConfig = cfg; m.opciones = opc; m.salir = sal; m.volver = vol;

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Resources/MenuPausa.prefab");
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        Debug.Log("[Pausa] Prefab creado en Assets/Resources/MenuPausa.prefab");
    }
}
#endif
