using System.Collections;
using UnityEngine;

// Plataforma temporal: cuando Mateo se para encima, tiembla un momento, desaparece y vuelve a aparecer.
[RequireComponent(typeof(Collider2D))]
public class PlataformaTemporal : MonoBehaviour
{
    public float tiempoAntesDeCaer = 0.6f;
    public float tiempoDesaparecida = 2.5f;

    Collider2D col;
    SpriteRenderer sr;
    Vector3 posInicial;
    bool activa = true;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        posInicial = transform.position;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (!activa) return;
        if (c.collider.GetComponentInParent<PlayerController>() == null) return;
        // solo cuenta si Mateo cae encima (la normal del contacto apunta hacia abajo desde su punto de vista)
        if (c.collider.bounds.min.y >= col.bounds.max.y - 0.3f) StartCoroutine(Rutina());
    }

    IEnumerator Rutina()
    {
        activa = false;
        float t = 0f;
        while (t < tiempoAntesDeCaer)
        {
            t += Time.deltaTime;
            transform.position = posInicial + new Vector3(Mathf.Sin(t * 60f) * 0.04f, 0f, 0f);
            yield return null;
        }
        transform.position = posInicial;
        col.enabled = false;
        if (sr != null) sr.enabled = false;
        yield return new WaitForSeconds(tiempoDesaparecida);
        col.enabled = true;
        if (sr != null) sr.enabled = true;
        activa = true;
    }
}
