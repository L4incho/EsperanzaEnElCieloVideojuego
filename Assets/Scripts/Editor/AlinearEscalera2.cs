#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AlinearEscalera2
{
    [MenuItem("Esperanza/Alinear segunda escalera con plataforma izquierda")]
    public static void Alinear()
    {
        var esc = GameObject.Find("SegundaEscalera");
        if (esc == null) { Debug.LogError("[Alinear] No existe SegundaEscalera"); return; }
        Transform plat = esc.transform.Find("PlataformaSuperior");
        var colPlat = plat != null ? plat.GetComponent<Collider2D>() : null;
        if (colPlat == null) { Debug.LogError("[Alinear] Falta PlataformaSuperior con collider"); return; }
        Physics2D.SyncTransforms();
        float topeEsc = colPlat.bounds.max.y;
        float izq = colPlat.bounds.min.x;
        float mejor = float.NaN;
        foreach (float dx in new[] { 0.6f, 1.2f, 1.8f })
        {
            Vector2 o = new Vector2(izq - dx, topeEsc + 1.5f);
            foreach (var h in Physics2D.RaycastAll(o, Vector2.down, 6f))
            {
                if (h.collider == null || h.collider.isTrigger) continue;
                if (h.collider.transform.IsChildOf(esc.transform)) continue;
                if (h.collider.GetComponent<PlayerController>() != null) continue;
                Debug.Log("[Alinear] x=" + (izq - dx) + " golpea " + h.collider.name + " en y=" + h.point.y + " (tope escalera " + topeEsc + ")");
                if (float.IsNaN(mejor)) mejor = h.point.y;
                break;
            }
        }
        if (float.IsNaN(mejor)) { Debug.LogError("[Alinear] No encontre la plataforma de la izquierda"); return; }
        float delta = mejor - topeEsc;
        Undo.RecordObject(esc.transform, "Alinear escalera");
        esc.transform.position += new Vector3(0f, delta, 0f);
        EditorSceneManager.MarkSceneDirty(esc.scene);
        Debug.Log("[Alinear] Escalera movida " + delta + " en Y. Guarda la escena (Ctrl+S).");
    }
}
#endif
