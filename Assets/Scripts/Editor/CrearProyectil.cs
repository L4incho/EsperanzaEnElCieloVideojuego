#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class CrearProyectil
{
    static Sprite[] Cargar(string ruta)
    {
        // de arriba hacia abajo en la hoja = orden de la animacion
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().OrderByDescending(s => s.rect.y).ToArray();
    }

    [MenuItem("Esperanza/Crear proyectil aturdidor")]
    public static void Crear()
    {
        var der = Cargar("Assets/Arte/Personajes/mateo/munición_derecha.png");
        var izq = Cargar("Assets/Arte/Personajes/mateo/munición_izquierda.png");
        if (der.Length < 4 || izq.Length < 4) { Debug.LogError("[Proyectil] Sprites de municion no encontrados"); return; }

        var go = new GameObject("Proyectil");
        go.transform.localScale = Vector3.one * 4f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = der[0];
        sr.sortingOrder = 30;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.07f;
        var p = go.AddComponent<ProyectilAturdidor>();
        p.vueloDerecha = der.Take(2).ToArray();
        p.impactoDerecha = der.Skip(2).ToArray();
        p.vueloIzquierda = izq.Take(2).ToArray();
        p.impactoIzquierda = izq.Skip(2).ToArray();

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Resources/Proyectil.prefab");
        Object.DestroyImmediate(go);
        Debug.Log("[Proyectil] Prefab creado en Assets/Resources/Proyectil.prefab (frames vuelo=" + 2 + ", impacto=" + (der.Length - 2) + ")");
    }
}
#endif
