using UnityEngine;
using UnityEngine.UI;

// HUD de Mateo con barras de rombos:
//   - Vida: 9 rombos (3 por cada golpe). Al recibir dano parpadean y se apagan; al curarse se encienden uno por uno.
//   - Disparo (J): rombos azul claro que se rellenan durante el cooldown.
//   - Habilidad de gema (K): rombos del color de la gema (roja / azul); solo se ve si Mateo lleva una de esas gemas.
// Los rombos se construyen solos al iniciar a partir de los contenedores del prefab HUD_Vida.
public class HudVida : MonoBehaviour
{
    public PlayerController jugador;

    [Header("Sprites de los rombos")]
    public Sprite spVacio;
    public Sprite spBlanco;
    public Sprite spVida;
    public Sprite spDisparo;
    public Sprite spRoja;
    public Sprite spAzul;

    [Header("Contenedores (se llenan de rombos al iniciar)")]
    public RectTransform contVida;
    public RectTransform contDisparo;
    public RectTransform contHabilidad;
    [Tooltip("Objeto completo de la barra K (letra + rombos): solo se ve con la gema roja o azul")]
    public GameObject contenedorHabilidad;

    [Header("Tamano de los rombos")]
    public Vector2 tamVida = new Vector2(51.8f, 30.3f);
    public float pasoVida = 43.4f;
    public Vector2 tamMini = new Vector2(36.3f, 21.2f);
    public float pasoMini = 30.4f;

    [Header("Coleccionables")]
    public Text textoMicrochips;

    // ---- campos antiguos (se conservan para que compilen los scripts de editor viejos) ----
    [HideInInspector] public Image barra;
    [HideInInspector] public Image barraDisparo;
    [HideInInspector] public Text textoDisparo;
    [HideInInspector] public Image barraHabilidad;
    [HideInInspector] public Text textoHabilidad;

    const int N = 9;

    class Capas { public Image[] vacio = new Image[N], lleno = new Image[N], blanco = new Image[N]; }
    Capas cVida, cJ, cK;
    bool[] encendido = new bool[N];
    int[] modo = new int[N];          // 0 estable, 1 danio, 2 cura
    float[] t = new float[N];
    bool iniciado;
    float temblor;
    Vector2 posVidaBase;
    TipoGema? tipoK;

    Capas Construir(RectTransform cont, Sprite lleno, Vector2 tam, float paso, bool relleno)
    {
        var c = new Capas();
        if (cont == null) return c;
        // limpiar por si se reconstruye
        for (int k = cont.childCount - 1; k >= 0; k--) Destroy(cont.GetChild(k).gameObject);
        var capaVacio = Capa(cont, "Vacios");
        var capaLleno = Capa(cont, "Llenos");
        var capaBlanco = Capa(cont, "Destellos");
        for (int i = 0; i < N; i++)
        {
            c.vacio[i] = Rombo(capaVacio, "V" + i, spVacio, i, tam, paso, false);
            c.lleno[i] = Rombo(capaLleno, "L" + i, lleno, i, tam, paso, relleno);
            c.blanco[i] = Rombo(capaBlanco, "B" + i, spBlanco, i, tam, paso, false);
            c.blanco[i].color = new Color(1, 1, 1, 0);
        }
        return c;
    }

    static RectTransform Capa(RectTransform padre, string nombre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        var r = g.GetComponent<RectTransform>();
        r.SetParent(padre, false);
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return r;
    }

    static Image Rombo(RectTransform capa, string nombre, Sprite sp, int i, Vector2 tam, float paso, bool relleno)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        var r = g.GetComponent<RectTransform>();
        r.SetParent(capa, false);
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = tam;
        r.anchoredPosition = new Vector2(i * paso + tam.x * 0.5f, -tam.y * 0.5f);
        var im = g.AddComponent<Image>();
        im.sprite = sp;
        im.raycastTarget = false;
        if (relleno)
        {
            im.type = Image.Type.Filled;
            im.fillMethod = Image.FillMethod.Horizontal;
            im.fillOrigin = 0;
        }
        return im;
    }

    void Awake()
    {
        cVida = Construir(contVida, spVida, tamVida, pasoVida, false);
        cJ = Construir(contDisparo, spDisparo, tamMini, pasoMini, true);
        cK = Construir(contHabilidad, spRoja, tamMini, pasoMini, true);
        if (contVida != null) posVidaBase = contVida.anchoredPosition;
    }

    void Update()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerController>();
        if (jugador == null) return;
        if (textoMicrochips != null) textoMicrochips.text = "x " + Microchips.Cantidad;
        float dt = Time.unscaledDeltaTime;
        ActualizarVida(dt);
        ActualizarBarra(cJ, 1f - jugador.CooldownRestante / Mathf.Max(0.01f, jugador.CooldownTotal));
        ActualizarHabilidad();
    }

    // ---------------- Vida ----------------
    void ActualizarVida(float dt)
    {
        if (cVida.lleno[0] == null) return;
        int objetivo = Mathf.Clamp(Mathf.CeilToInt(jugador.Vida01 * N - 0.001f), 0, N);
        if (!iniciado)
        {
            for (int i = 0; i < N; i++) { encendido[i] = i < objetivo; modo[i] = 0; }
            iniciado = true;
        }
        int nuevosCura = 0, nuevosDanio = 0;
        for (int i = 0; i < N; i++)
        {
            bool quiere = i < objetivo;
            if (encendido[i] && !quiere)
            {
                encendido[i] = false; modo[i] = 1; t[i] = -0.05f * nuevosDanio; nuevosDanio++; temblor = 0.3f;
            }
            else if (!encendido[i] && quiere)
            {
                encendido[i] = true; modo[i] = 2; t[i] = -0.12f * nuevosCura; nuevosCura++;
            }
        }

        for (int i = 0; i < N; i++)
        {
            var on = cVida.lleno[i]; var fl = cVida.blanco[i];
            var rt = on.rectTransform;
            float escala = 1f, alfa = 1f, destello = 0f; bool ver = encendido[i];
            if (modo[i] != 0)
            {
                t[i] += dt;
                float tt = t[i];
                if (modo[i] == 1)
                {
                    ver = true;
                    if (tt < 0f) { }
                    else if (tt < 0.18f) destello = ((int)(tt / 0.06f) % 2 == 0) ? 0.9f : 0.3f;
                    else if (tt < 0.55f) { float u = (tt - 0.18f) / 0.37f; escala = 1f - 0.3f * u; alfa = 1f - u; }
                    else { ver = false; modo[i] = 0; }
                }
                else
                {
                    if (tt < 0f) ver = false;
                    else
                    {
                        ver = true;
                        escala = 1f + 0.18f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(tt / 0.25f));
                        destello = Mathf.Max(0f, 0.8f - tt * 3f);
                        if (tt > 0.3f) modo[i] = 0;
                    }
                }
            }
            on.enabled = ver;
            on.color = new Color(1, 1, 1, alfa);
            rt.localScale = new Vector3(escala, escala, 1f);
            fl.color = new Color(1, 1, 1, destello);
            fl.rectTransform.localScale = rt.localScale;
        }

        if (contVida != null)
        {
            if (temblor > 0f)
            {
                temblor -= dt;
                contVida.anchoredPosition = posVidaBase + new Vector2(Random.Range(-4f, 4f), 0f);
            }
            else contVida.anchoredPosition = posVidaBase;
        }
    }

    // ---------------- Disparo (J) y habilidad (K) ----------------
    void ActualizarBarra(Capas c, float fraccion)
    {
        if (c == null || c.lleno[0] == null) return;
        fraccion = Mathf.Clamp01(fraccion);
        bool listo = fraccion >= 1f;
        float brillo = listo ? 0.22f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f)) : 0f;
        float total = fraccion * N;
        for (int i = 0; i < N; i++)
        {
            c.lleno[i].fillAmount = Mathf.Clamp01(total - i);
            c.blanco[i].color = new Color(1, 1, 1, brillo);
        }
    }

    void ActualizarHabilidad()
    {
        if (contenedorHabilidad == null) return;
        bool ver = jugador.TieneHabilidadK;
        if (contenedorHabilidad.activeSelf != ver) contenedorHabilidad.SetActive(ver);
        if (!ver) return;
        var tipo = jugador.TipoHabilidadK;
        if (tipoK == null || tipoK.Value != tipo)
        {
            tipoK = tipo;
            var sp = tipo == TipoGema.Roja ? spRoja : spAzul;
            for (int i = 0; i < N; i++) if (cK.lleno[i] != null) cK.lleno[i].sprite = sp;
        }
        float total = Mathf.Max(0.01f, jugador.HabilidadTotal);
        ActualizarBarra(cK, 1f - jugador.HabilidadRestante / total);
    }
}
