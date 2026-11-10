using System.Collections;
using UnityEngine;

// Peligro que se enciende y se apaga (fuego temporal, laseres temporales).
// Necesita en el mismo objeto: un Collider2D (trigger) y el script ZonaMortal.
// Cuando esta apagado el collider no mata y el dibujo se oculta (o muestra "spriteApagado").
public class ZonaIntermitente : MonoBehaviour
{
    public float tiempoEncendido = 1.5f;
    public float tiempoApagado = 1.5f;
    public float desfase = 0f;
    [Tooltip("Si se asigna, este sprite se muestra mientras esta apagado (por ejemplo la llama baja). Si no, el dibujo se oculta.")]
    public Sprite spriteApagado;

    Collider2D zona;
    SpriteRenderer sr;
    SpriteAnimador animador;

    void Awake()
    {
        zona = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        animador = GetComponent<SpriteAnimador>();
    }

    IEnumerator Start()
    {
        Poner(false);
        if (desfase > 0f) yield return new WaitForSeconds(desfase);
        while (true)
        {
            Poner(true);
            yield return new WaitForSeconds(tiempoEncendido);
            // aviso: parpadea justo antes de apagarse
            Poner(false);
            yield return new WaitForSeconds(tiempoApagado);
        }
    }

    void Poner(bool encendido)
    {
        if (zona != null) zona.enabled = encendido;
        if (animador != null) animador.enabled = encendido || spriteApagado == null;
        if (sr == null) return;
        if (encendido) { sr.enabled = true; return; }
        if (spriteApagado != null) { sr.enabled = true; sr.sprite = spriteApagado; }
        else sr.enabled = false;
    }
}
