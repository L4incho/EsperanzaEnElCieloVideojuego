#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Cinemachine;

// Menu: Esperanza > Limitar camara al fondo
// Crea el objeto "LimitesCamara" con el tamano exacto del fondo (Fondo_Tutorial) y se lo asigna
// al CinemachineConfiner2D de la camara. Asi la camara nunca muestra espacio vacio fuera del fondo.
// Si quieres otro nivel: selecciona su objeto de fondo en la jerarquia y ejecuta el menu.
public static class LimitarCamara
{
    [MenuItem("Esperanza/Limitar camara al fondo")]
    public static void Ejecutar()
    {
        GameObject fondo = Selection.activeGameObject;
        if (fondo == null || fondo.GetComponent<SpriteRenderer>() == null) fondo = GameObject.Find("Fondo_Tutorial");
        if (fondo == null) { Debug.LogError("No encontre el fondo. Selecciona el objeto del fondo y vuelve a ejecutar."); return; }
        var sr = fondo.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogError("El fondo no tiene SpriteRenderer."); return; }

        var cam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (cam == null) { Debug.LogError("No encontre una CinemachineCamera en la escena."); return; }

        Bounds b = sr.bounds;

        var viejo = GameObject.Find("LimitesCamara");
        if (viejo != null) Undo.DestroyObjectImmediate(viejo);

        var go = new GameObject("LimitesCamara");
        Undo.RegisterCreatedObjectUndo(go, "Limites camara");
        go.transform.position = Vector3.zero;
        go.layer = 2; // Ignore Raycast
        var poly = go.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.points = new[]
        {
            new Vector2(b.min.x, b.min.y), new Vector2(b.min.x, b.max.y),
            new Vector2(b.max.x, b.max.y), new Vector2(b.max.x, b.min.y)
        };

        var conf = cam.GetComponent<CinemachineConfiner2D>();
        if (conf == null) conf = Undo.AddComponent<CinemachineConfiner2D>(cam.gameObject);
        conf.BoundingShape2D = poly;
        conf.Damping = 0.3f;
        conf.InvalidateBoundingShapeCache();

        EditorUtility.SetDirty(conf);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("Camara limitada al fondo: x " + b.min.x + ".." + b.max.x + ", y " + b.min.y + ".." + b.max.y + ". Guarda la escena (Ctrl+S).");
    }
}
#endif
