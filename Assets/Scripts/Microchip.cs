using System.Collections;
using UnityEngine;

// Contador global de microchips (se reinicia al entrar a Play)
public static class Microchips
{
    public static int Cantidad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar() { Cantidad = 0; }
}

// Coleccionable: microchip que gira (animacion de 5 cuadros) y flota. Al tocarlo Mateo suma 1 al contador del HUD.
[RequireComponent(typeof(Collider2D))]
public class Microchip : MonoBehaviour
{
    public Sprite[] cuadros;
    public float cuadrosPorSegundo = 8f;
    public float amplitudVaiven = 0.1f;
    public float velocidadVaiven = 2.5f;
    public AudioClip sonido;

    SpriteRenderer sr;
    Transform visual;
    bool tomado;
    float fase;
    Vector3 baseLocal;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        visual = sr != null ? sr.transform : transform;
        // Si el dibujo es un hijo que moviste en el editor, el microchip (y su zona de recogida) se mueve hasta el dibujo
        if (visual != transform && visual.localPosition.sqrMagnitude > 0.0001f)
        {
            Vector3 mundo = visual.position;
            transform.position = mundo;
            visual.localPosition = Vector3.zero;
        }
        baseLocal = visual.localPosition;   // el vaiven se suma a su posicion, no la reemplaza
        GetComponent<Collider2D>().isTrigger = true;
        fase = Random.value * 10f;
    }

    void Update()
    {
        if (tomado || sr == null || cuadros == null || cuadros.Length == 0) return;
        // ping-pong suave: 0 1 2 3 4 3 2 1 ... se ve como giro continuo
        int n = cuadros.Length;
        int ciclo = Mathf.Max(1, 2 * n - 2);
        int i = Mathf.FloorToInt((Time.time + fase) * cuadrosPorSegundo) % ciclo;
        sr.sprite = cuadros[i < n ? i : ciclo - i];
        visual.localPosition = baseLocal + new Vector3(0f, Mathf.Sin((Time.time + fase) * velocidadVaiven) * amplitudVaiven, 0f);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (tomado || otro.GetComponentInParent<PlayerController>() == null) return;
        tomado = true;
        GetComponent<Collider2D>().enabled = false;
        Microchips.Cantidad++;
        if (sonido != null) AudioSource.PlayClipAtPoint(sonido, transform.position);
        StartCoroutine(Desvanecer());
    }

    IEnumerator Desvanecer()
    {
        float t = 0f, d = 0.25f;
        Vector3 e0 = visual.localScale;
        while (t < d)
        {
            t += Time.deltaTime;
            float k = t / d;
            visual.localScale = e0 * (1f + 0.6f * k);
            visual.localPosition += Vector3.up * Time.deltaTime * 2f;
            if (sr != null) { var c = sr.color; c.a = 1f - k; sr.color = c; }
            yield return null;
        }
        Destroy(gameObject);
    }
}
