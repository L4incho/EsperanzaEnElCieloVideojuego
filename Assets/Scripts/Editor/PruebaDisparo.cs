#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Solo para pruebas en Play: acerca el dummy a Mateo (a 7 unidades, en su mismo piso).
public static class PruebaDisparo
{
    [MenuItem("Esperanza/PRUEBA acercar dummy a Mateo (Play)")]
    public static void Acercar()
    {
        var d = Object.FindFirstObjectByType<EnemigoInfectado>();
        var m = Object.FindFirstObjectByType<PlayerController>();
        if (d == null || m == null) { Debug.LogWarning("[Prueba] Falta dummy o Mateo (usar en Play)"); return; }
        var rb = d.GetComponent<Rigidbody2D>();
        Vector2 p = new Vector2(m.transform.position.x + 7f, m.transform.position.y);
        d.transform.position = p;
        if (rb != null) { rb.position = p; rb.linearVelocity = Vector2.zero; }
        Debug.Log("[Prueba] Dummy movido a " + p);
    }
}
#endif
