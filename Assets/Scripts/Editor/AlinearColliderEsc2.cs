#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AlinearColliderEsc2
{
    [MenuItem("Esperanza/Alinear piso de segunda escalera")]
    public static void Alinear()
    {
        var esc = GameObject.Find("SegundaEscalera");
        if (esc == null) { Debug.LogError("[AlinearPiso] No existe SegundaEscalera"); return; }
        Transform plat = esc.transform.Find("PlataformaSuperior");
        var colPlat = plat != null ? plat.GetComponent<Collider2D>() : null;
        if (colPlat == null) { Debug.LogError("[AlinearPiso] Falta PlataformaSuperior"); return; }
        Physics2D.SyncTransforms();
        float tope = colPlat.bounds.max.y, izq = colPlat.bounds.min.x, mejor = float.NaN;
        foreach (float dx in new[] { 0.6f, 1.2f, 1.8f })
        {
            Vector2 o = new Vector2(izq - dx, tope + 1.5f);
            foreach (var h in Physics2D.RaycastAll(o, Vector2.down, 6f))
            {
                if (h.collider == null || h.collider.isTrigger) continue;
                if (h.collider.transform.IsChildOf(esc.transform)) continue;
                if (h.collider.GetComponent<PlayerController>() != null) continue;
                if (float.IsNaN(mejor)) mejor = h.point.y;
                break;
            }
        }
        if (float.IsNaN(mejor)) { Debug.LogError("[AlinearPiso] No encontre la plataforma izquierda"); return; }
        float delta = mejor - tope;
        Undo.RecordObject(plat, "Alinear piso");
        plat.position += new Vector3(0f, delta, 0f);
        EditorSceneManager.MarkSceneDirty(esc.scene);
        Debug.Log("[AlinearPiso] Piso movido " + delta + " en Y (tile y=" + mejor + ").");
    }
}
#endif
