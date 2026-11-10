using UnityEngine;

// Zona de seguridad bajo el mapa: si Mateo cae al vacio, reaparece en su punto de inicio.
// Se coloca en el objeto "ZonaCaida" (trigger) que crea el menu Esperanza > Crear limites del mapa.
[RequireComponent(typeof(Collider2D))]
public class ZonaCaida : MonoBehaviour
{
    [Tooltip("Mateo. Si lo dejas vacio se busca solo.")]
    public PlayerController jugador;

    Vector3 puntoInicio;

    void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerController>();
        if (jugador != null) puntoInicio = jugador.transform.position;
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;
        pc.transform.position = puntoInicio;
        var rb = pc.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.position = puntoInicio; }
    }
}
