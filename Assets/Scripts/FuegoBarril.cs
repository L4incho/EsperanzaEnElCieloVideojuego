using UnityEngine;

// Barril con fuego (nivel 2), como las plantas piranha de Mario: pasa un rato APAGADO (se puede pasar por arriba sin problema),
// luego se ENCIENDE (llama chica -> llama alta -> llama chica) y mientras la llama esta ALTA quema: quita 'golpesDano' tercios de vida
// (1 = un tercio, no mata de golpe). Despues se apaga y repite.
// El barril en si es solido: Mateo puede tocarlo de lado sin problema.
[RequireComponent(typeof(SpriteRenderer))]
public class FuegoBarril : MonoBehaviour
{
    public Sprite[] cuadros;
    public Sprite apagado;
    public float cuadrosPorSegundo = 6f;
    [Tooltip("Numeros de cuadro (empezando en 0) en los que la llama quema. Por defecto los 3 cuadros mas altos.")]
    public int[] cuadrosPeligrosos = { 1, 2, 3 };
    [Header("Ciclo")]
    [Tooltip("Segundos que el barril pasa apagado (seguro)")] public float tiempoApagado = 2.5f;
    [Tooltip("Segundos que el barril pasa encendido (con llama)")] public float tiempoEncendido = 2f;
    [Tooltip("Tercios de vida que quita la llama (1 = un tercio)")] public int golpesDano = 1;
    [Header("Zona de la llama (sobre la boca del barril), en unidades locales")]
    public Vector2 zonaCentro = new Vector2(0f, 1.65f);
    public Vector2 zonaTamano = new Vector2(0.8f, 1f);

    public bool Peligroso { get; private set; }
    public bool Encendido { get; private set; }
    public int CuadroActual { get; private set; }

    SpriteRenderer sr;
    float t;   // tiempo dentro del ciclo completo (apagado + encendido)

    float Ciclo { get { return Mathf.Max(0.1f, tiempoApagado + tiempoEncendido); } }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // cada barril empieza en un momento distinto del ciclo
        t = Mathf.Abs(transform.position.x * 0.37f + transform.position.y * 0.11f) % Ciclo;
    }

    void Update()
    {
        if (cuadros == null || cuadros.Length == 0) return;
        t = (t + Time.deltaTime) % Ciclo;
        Encendido = t >= tiempoApagado && apagado != null || (apagado == null);
        if (!Encendido)
        {
            sr.sprite = apagado;
            Peligroso = false;
            return;
        }
        float te = apagado == null ? t : t - tiempoApagado;   // tiempo desde que se encendio
        int n = cuadros.Length;
        int paso = (int)(te * cuadrosPorSegundo);
        // se enciende con el cuadro mas bajo (el ultimo), sube por los demas y vuelve a bajar al final
        bool ultimosPasos = te >= tiempoEncendido - 1f / Mathf.Max(0.1f, cuadrosPorSegundo);
        CuadroActual = (paso == 0 || ultimosPasos) ? n - 1 : (paso - 1) % (n - 1);
        sr.sprite = cuadros[CuadroActual];
        Peligroso = System.Array.IndexOf(cuadrosPeligrosos, CuadroActual) >= 0 && !(paso == 0 || ultimosPasos);
        if (Peligroso) Quemar();
    }

    void Quemar()
    {
        Vector2 centro = transform.TransformPoint(zonaCentro);
        Vector2 tam = Vector2.Scale(zonaTamano, transform.lossyScale);
        foreach (var h in Physics2D.OverlapBoxAll(centro, tam, 0f))
        {
            var pc = h.GetComponentInParent<PlayerController>();
            if (pc != null && !pc.EstaMuerto) { pc.RecibirGolpes(Mathf.Max(1, golpesDano), centro); return; }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(zonaCentro, zonaTamano);
    }
}
