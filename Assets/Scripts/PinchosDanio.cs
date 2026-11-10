using UnityEngine;

// Pinchos (nivel 2): si Mateo toca la zona de las puntas pierde una parte de su vida (1 golpe = 1/3 con 3 golpes)
// y es lanzado hacia atras; despues tiene unos segundos de invulnerabilidad (lo controla PlayerController).
public class PinchosDanio : MonoBehaviour
{
    [Header("Zona de las puntas, en unidades locales")]
    public Vector2 zonaCentro = new Vector2(0f, 0.2f);
    public Vector2 zonaTamano = new Vector2(1.6f, 0.8f);

    void FixedUpdate()
    {
        Vector2 centro = transform.TransformPoint(zonaCentro);
        Vector2 tam = Vector2.Scale(zonaTamano, transform.lossyScale);
        foreach (var h in Physics2D.OverlapBoxAll(centro, tam, 0f))
        {
            var pc = h.GetComponentInParent<PlayerController>();
            if (pc != null && !pc.EstaMuerto) { pc.RecibirGolpe(transform.position); return; }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0.3f, 0.9f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(zonaCentro, zonaTamano);
    }
}
