using System.Collections.Generic;
using UnityEngine;

// Plataforma que va y viene entre dos puntos y se lleva a Mateo encima.
[RequireComponent(typeof(Collider2D))]
public class PlataformaMovil : MonoBehaviour
{
    public Vector2 puntoA;
    public Vector2 puntoB;
    public float velocidad = 2f;
    public float pausaEnExtremos = 0.4f;
    [Tooltip("Si esta activo, sube a B solo mientras Mateo esta encima y vuelve a A cuando se baja.")]
    public bool soloConPasajero = false;
    [Tooltip("Solo con 'soloConPasajero': metros que sube desde donde la dejes colocada en el editor (esa es su posicion de reposo).")]
    public float alturaSubida = 7.69f;

    Rigidbody2D rb;
    bool haciaB = true;
    float espera;
    float ultimoPasajero = -99f;
    Vector2 inicio;
    readonly HashSet<Rigidbody2D> pasajeros = new HashSet<Rigidbody2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        inicio = transform.position;   // donde esta colocada en el editor
    }

    void FixedUpdate()
    {
        if (soloConPasajero)
        {
            // Mateo encima = algo suyo dentro de una franja delgada sobre la cara superior
            var mc = GetComponent<Collider2D>();
            var b = mc.bounds;
            pasajeros.Clear();
            foreach (var h in Physics2D.OverlapBoxAll(new Vector2(b.center.x, b.max.y + 0.5f), new Vector2(b.size.x * 0.9f, 1.0f), 0f))
            {
                var pj = h.GetComponentInParent<PlayerController>();
                if (pj == null) continue;
                var prb = pj.GetComponent<Rigidbody2D>();
                if (prb != null && h.bounds.min.y >= b.max.y - 0.3f && h.bounds.min.y <= b.max.y + 0.8f) pasajeros.Add(prb);
            }
            if (pasajeros.Count > 0) ultimoPasajero = Time.time;
            Vector2 objetivo = (Time.time - ultimoPasajero < 0.3f) ? inicio + Vector2.up * alturaSubida : inicio;
            Vector2 sig = Vector2.MoveTowards(rb.position, objetivo, velocidad * Time.fixedDeltaTime);
            Vector2 d = sig - rb.position;
            if (d.sqrMagnitude > 0f) { rb.MovePosition(sig); foreach (var p in pasajeros) p.position += d; }
            MarcarPasajeros();
            return;
        }
        if (espera > 0f) { espera -= Time.fixedDeltaTime; return; }
        Vector2 destino = haciaB ? puntoB : puntoA;
        Vector2 nueva = Vector2.MoveTowards(rb.position, destino, velocidad * Time.fixedDeltaTime);
        Vector2 delta = nueva - rb.position;
        rb.MovePosition(nueva);
        foreach (var p in pasajeros) if (p != null) p.position += delta;
        MarcarPasajeros();
        if ((nueva - destino).sqrMagnitude < 0.0001f) { haciaB = !haciaB; espera = pausaEnExtremos; }
    }

    void MarcarPasajeros()
    {
        foreach (var p in pasajeros)
        {
            if (p == null) continue;
            var pj = p.GetComponent<PlayerController>();
            if (pj != null) pj.ultimoContactoPlataforma = Time.time;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        var r = c.rigidbody;
        if (r == null || c.collider.GetComponentInParent<PlayerController>() == null) return;
        if (c.collider.bounds.min.y >= GetComponent<Collider2D>().bounds.max.y - 0.3f) pasajeros.Add(r);
    }

    void OnCollisionStay2D(Collision2D c)
    {
        var r = c.rigidbody;
        if (r == null || pasajeros.Contains(r) || c.collider.GetComponentInParent<PlayerController>() == null) return;
        if (c.collider.bounds.min.y >= GetComponent<Collider2D>().bounds.max.y - 0.3f) pasajeros.Add(r);
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (c.rigidbody != null) pasajeros.Remove(c.rigidbody);
    }
}
