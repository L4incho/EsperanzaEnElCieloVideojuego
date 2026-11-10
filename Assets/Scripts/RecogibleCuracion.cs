using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Objeto de curacion: venda (cura 1 tercio de la vida) o botiquin (cura toda la vida).
// Si Mateo ya tiene la vida llena no se recoge, para no desperdiciarlo.
[RequireComponent(typeof(Collider2D))]
public class RecogibleCuracion : MonoBehaviour
{
    [Tooltip("Marcado = cura toda la vida (botiquin). Desmarcado = cura 'golpesQueCura' (venda)")]
    public bool curaTodo = false;
    [Tooltip("Cuantos golpes recupera (1 = un tercio de la vida)")]
    public int golpesQueCura = 1;
    public AudioClip sonidoRecoger;

    [Header("Flotar")]
    public float amplitud = 0.08f;
    public float velocidad = 2.2f;

    [Header("Animacion (cuadros del sprite)")]
    public Sprite[] cuadros;
    public float cuadrosPorSegundo = 8f;

    SpriteRenderer srAnim;
    Vector3 posInicial, escalaInicial;
    Color colorInicial = Color.white;
    bool recogido;

    // Todas las vendas/botiquines del nivel: cuando Mateo muere y reaparece, vuelven a su sitio
    static readonly List<RecogibleCuracion> todos = new List<RecogibleCuracion>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar() { todos.Clear(); }

    public static void ReaparecerTodos()
    {
        foreach (var r in todos) if (r != null) r.Reaparecer();
    }

    void OnDestroy() { todos.Remove(this); }

    public void Reaparecer()
    {
        StopAllCoroutines();
        recogido = false;
        transform.position = posInicial;
        transform.localScale = escalaInicial;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colorInicial;
        GetComponent<Collider2D>().enabled = true;
        gameObject.SetActive(true);
    }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        posInicial = transform.position;
        escalaInicial = transform.localScale;
        srAnim = GetComponent<SpriteRenderer>();
        if (srAnim != null) colorInicial = srAnim.color;
        todos.Add(this);
    }

    void Update()
    {
        if (srAnim != null && cuadros != null && cuadros.Length > 0)
            srAnim.sprite = cuadros[(int)(Time.time * cuadrosPorSegundo) % cuadros.Length];
        if (recogido) return;
        transform.position = posInicial + Vector3.up * (Mathf.Sin(Time.time * velocidad) * amplitud);
    }

    void OnTriggerStay2D(Collider2D otro) { Intentar(otro); }
    void OnTriggerEnter2D(Collider2D otro) { Intentar(otro); }

    void Intentar(Collider2D otro)
    {
        if (recogido) return;
        var mateo = otro.GetComponentInParent<PlayerController>();
        if (mateo == null) return;
        bool curo = curaTodo ? mateo.CurarTodo() : mateo.Curar(golpesQueCura);
        if (!curo) return;   // vida llena: se queda ahi

        recogido = true;
        GetComponent<Collider2D>().enabled = false;
        if (sonidoRecoger != null) AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);
        StartCoroutine(Desvanecer());
    }

    IEnumerator Desvanecer()
    {
        var sr = GetComponent<SpriteRenderer>();
        Vector3 ini = transform.position, esc = transform.localScale;
        float t = 0f, dur = 0.35f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            transform.position = ini + Vector3.up * (0.6f * k);
            transform.localScale = esc * (1f + 0.3f * k);
            if (sr != null) { var c = sr.color; c.a = 1f - k; sr.color = c; }
            yield return null;
        }
        gameObject.SetActive(false);   // no se destruye: reaparece cuando Mateo muere y vuelve a empezar
    }
}
