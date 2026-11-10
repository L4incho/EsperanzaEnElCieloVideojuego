#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Esperanza > Crear limites del mapa
// Crea paredes invisibles a izquierda y derecha, un techo y una zona de caida que devuelve a Mateo al inicio.
// Usa el tamano del fondo "Fondo_Tutorial" (o el fondo que tengas seleccionado en la jerarquia).
public static class CrearLimitesMapa
{
    [MenuItem("Esperanza/Crear limites del mapa")]
    public static void Crear()
    {
        GameObject fondo = Selection.activeGameObject;
        if (fondo == null || fondo.GetComponent<SpriteRenderer>() == null) fondo = GameObject.Find("Fondo_Tutorial");
        if (fondo == null || fondo.GetComponent<SpriteRenderer>() == null) { Debug.LogError("No encontre el fondo. Selecciona el objeto del fondo y vuelve a ejecutar."); return; }
        Bounds b = fondo.GetComponent<SpriteRenderer>().bounds;

        var viejo = GameObject.Find("LimitesMapa");
        if (viejo != null) Undo.DestroyObjectImmediate(viejo);

        // Material sin friccion para que Mateo no se pegue a las paredes al saltar
        const string rutaMat = "Assets/SinFriccion.physicsMaterial2D";
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(rutaMat);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, rutaMat);
        }

        var raiz = new GameObject("LimitesMapa");
        Undo.RegisterCreatedObjectUndo(raiz, "Limites del mapa");

        float grosor = 2f, extra = 30f;
        float alto = b.size.y + extra * 2f;
        Pared(raiz.transform, "ParedIzquierda", new Vector2(b.min.x - grosor / 2f, b.center.y), new Vector2(grosor, alto), mat);
        Pared(raiz.transform, "ParedDerecha", new Vector2(b.max.x + grosor / 2f, b.center.y), new Vector2(grosor, alto), mat);
        Pared(raiz.transform, "Techo", new Vector2(b.center.x, b.max.y + grosor / 2f + 10f), new Vector2(b.size.x + grosor * 2f, grosor), mat);

        var caida = new GameObject("ZonaCaida");
        caida.transform.SetParent(raiz.transform, false);
        caida.transform.position = new Vector3(b.center.x, b.min.y - 6f, 0f);
        var bc = caida.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(b.size.x + 40f, 2f);
        caida.AddComponent<ZonaCaida>();

        Selection.activeGameObject = raiz;
        EditorSceneManager.MarkSceneDirty(raiz.scene);
        Debug.Log("Limites creados: x " + b.min.x + ".." + b.max.x + ". Guarda la escena (Ctrl+S).");
    }

    static void Pared(Transform padre, string nombre, Vector2 pos, Vector2 tam, PhysicsMaterial2D mat)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        var c = go.AddComponent<BoxCollider2D>();
        c.size = tam;
        c.sharedMaterial = mat;
    }
}
#endif
