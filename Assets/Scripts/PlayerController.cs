using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Escaleras de mano")]
    public float velocidadEscalada = 3f;
    [Tooltip("Pequeño impulso al llegar a la parte de arriba para pasar al piso.")]
    public float impulsoSalidaSuperior = 4f;

    [Header("Habilidades (gemas)")]
    [Tooltip("Se activa solo al recoger la gema verde")]
    public bool dobleSalto = false;
    public float fuerzaDobleSalto = 7f;
    [Tooltip("Animator con los sprites del traje verde (MateoVerde). Se pone al recoger la gema verde.")]
    public RuntimeAnimatorController animatorTrajeVerde;

    [Header("Gemas roja y azul (habilidad con tecla K)")]
    [Tooltip("Traje rojo (MateoRojo)")] public RuntimeAnimatorController animatorTrajeRojo;
    [Tooltip("Traje azul (MateoAzul)")] public RuntimeAnimatorController animatorTrajeAzul;
    public KeyCode teclaHabilidad = KeyCode.K;
    public Color colorGemaRoja = new Color(1f, 0.3f, 0.3f);
    public Color colorGemaAzul = new Color(0.35f, 0.65f, 1f);
    [Tooltip("Roja: segundos de espera entre usos")] public float cooldownRoja = 8f;
    [Tooltip("Roja: radio de la onda que empuja a los enemigos")] public float radioOnda = 4.5f;
    [Tooltip("Roja: fuerza con la que salen disparados")] public float fuerzaOnda = 12f;
    [Tooltip("Azul: segundos de espera entre usos")] public float cooldownAzul = 6f;
    public float velocidadDash = 18f;
    public float duracionDash = 0.25f;
    private float habilidadRestante = 0f;
    private float dashRestante = 0f;
    private float dashDir = 1f;
    private float graciaDash = 0f;
    private RuntimeAnimatorController animatorBase;
    private GemaVerde gemaActual;   // la gema que Mateo lleva (solo una a la vez)

    /// <summary>Gema activa (null si no lleva ninguna).</summary>
    public GemaVerde GemaActual { get { return gemaActual; } }
    /// <summary>true si lleva la gema roja o azul (las que usan la tecla K).</summary>
    public bool TieneHabilidadK { get { return gemaActual != null && gemaActual.tipo != TipoGema.Verde; } }
    public TipoGema TipoHabilidadK { get { return gemaActual != null ? gemaActual.tipo : TipoGema.Verde; } }
    public float HabilidadRestante { get { return habilidadRestante; } }
    public float HabilidadTotal { get { return (gemaActual != null && gemaActual.tipo == TipoGema.Azul) ? cooldownAzul : cooldownRoja; } }

    [Header("Efectos de gemas")]
    [Tooltip("Efecto al tomar una gema (prefab EfectoRecoger)")]
    public EfectoGema efectoRecoger;
    [Tooltip("Impulso de botas propulsoras al usar el doble salto (prefab EfectoImpulso)")]
    public EfectoGema efectoImpulso;
    public Color colorGemaVerde = new Color(0.25f, 1f, 0.45f);

    [Header("Muerte")]
    [Tooltip("Segundos que dura la animacion de morir antes de reaparecer")]
    public float duracionMuerte = 0.8f;
    [Tooltip("Color al que se tine Mateo al disolverse (acido)")]
    public Color colorDisolver = new Color(0.4f, 1f, 0.55f, 0f);
    private bool muerto = false;

    [Header("Vida")]
    [Tooltip("Cuantos golpes de enemigo aguanta Mateo antes de morir")]
    public int golpesParaMorir = 3;
    [Tooltip("Segundos de invulnerabilidad despues de recibir un golpe")]
    public float invulnerabilidad = 1.2f;
    public Vector2 retroceso = new Vector2(5f, 4f);
    private int golpesRecibidos = 0;
    private float tiempoInvulnerable = 0f;
    private float tiempoRetroceso = 0f;

    /// <summary>Vida de 0 a 1 (1 = llena). La lee el HUD.</summary>
    public float Vida01 { get { return Mathf.Clamp01(1f - golpesRecibidos / (float)Mathf.Max(1, golpesParaMorir)); } }
    public bool EstaMuerto { get { return muerto; } }
    [Header("Disparo aturdidor (tecla J)")]
    [Tooltip("Si se deja vacio se carga Resources/Proyectil")]
    public GameObject prefabProyectil;
    public KeyCode teclaDisparo = KeyCode.J;
    [Tooltip("Segundos de espera entre disparos (municion infinita)")]
    public float recargaDisparoSeg = 7f;   // antes 30 s (campo renombrado para que las escenas tomen el valor nuevo)
    [Tooltip("Segundos que queda aturdido el enemigo")]
    public float tiempoAturdimiento = 14f;
    public float velocidadProyectil = 12f;
    [Tooltip("Segundos desde que empieza la animacion de disparo hasta que sale el proyectil (cuadro del fogonazo)")]
    public float retrasoProyectil = 0.21f;
    private float cooldownRestante = 0f;

    [Header("Agacharse (tecla S / flecha abajo)")]
    [Tooltip("Altura del collider al agacharse (fraccion de la normal)")] public float alturaAgachado = 0.6f;
    [Tooltip("Solo para pruebas automaticas")] public bool agacharForzado = false;
    public bool EstaAgachado { get { return agachado; } }
    private bool agachado;
    private BoxCollider2D cajaMateo;
    private Vector2 cajaTamOriginal, cajaOffsetOriginal;

    public float CooldownRestante { get { return cooldownRestante; } }
    public float CooldownTotal { get { return recargaDisparoSeg; } }

    private Vector3 puntoReaparicion;
    private Color colorOriginal = Color.white;

    private int saltosAereosRestantes = 0;
    private Color colorHabilidad = new Color(0.25f, 1f, 0.45f);
    private EfectoGema impulsoInstancia;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float movimientoHorizontal;
    private float movimientoVertical;
    private bool enSuelo = true;

    // Escaleras
    private int escalerasTocando = 0;  // cuántas escaleras (objeto Escalera) está tocando
    private Collider2D escaleraActual;  // la escalera que está tocando (para centrar a Mateo en ella)
    private bool enEscalera = false;   // Mateo está sobre una escalera
    private bool escalando = false;    // Mateo está agarrado a la escalera
    private float gravedadOriginal;
    private float tiempoImpulsoSalida = 0f; // mientras sea > 0, Mateo avanza solo hacia el piso
    private float direccionSalida = 1f;
    private Collider2D miCollider;
    private Collider2D plataformaIgnorada; // plataforma de arriba de la escalera que se ignora mientras Mateo baja
    private float graciaEscalera = 0f;     // tiempo en el que no se suelta la escalera recien agarrada

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gravedadOriginal = rb.gravityScale;
        miCollider = GetComponent<Collider2D>();
        cajaMateo = miCollider as BoxCollider2D;
        if (cajaMateo != null) { cajaTamOriginal = cajaMateo.size; cajaOffsetOriginal = cajaMateo.offset; }
        puntoReaparicion = transform.position;
        colorOriginal = spriteRenderer.color;
        animatorBase = animator.runtimeAnimatorController;

        // El impulso de botas vive en los pies de Mateo y se reproduce al usar la habilidad
        if (efectoImpulso != null)
        {
            Transform pies = groundCheck != null ? groundCheck : transform;
            impulsoInstancia = Instantiate(efectoImpulso, pies.position, Quaternion.identity, pies);
        }
    }

    // lo marca PlataformaMovil mientras Mateo va encima: sube/baja con ella y debe verse de pie, no cayendo
    [HideInInspector] public float ultimoContactoPlataforma = -99f;
    float tiempoEnAire;
    float tiempoDesdeSalto = 10f;

    // Respaldo: cualquier collider solido (no trigger) justo debajo de los pies cuenta como suelo, aunque su capa no sea "Suelo"
    bool TocaSueloExtra()
    {
        if (miCollider == null || rb.linearVelocity.y > 0.1f) return false;
        Bounds b = miCollider.bounds;
        var hits = Physics2D.OverlapBoxAll(new Vector2(b.center.x, b.min.y - 0.06f), new Vector2(b.size.x * 0.8f, 0.2f), 0f);
        foreach (var h in hits)
        {
            if (h == null || h.isTrigger || h.attachedRigidbody == rb) continue;
            if (h.GetComponent<Escalera>() != null) continue;
            // un enemigo solo cuenta como suelo si Mateo esta realmente encima de el (no al rozarlo de lado en el salto):
            // asi no se recarga el doble salto ni se puede volver a saltar en el aire junto a un infectado
            if (h.GetComponentInParent<EnemigoInfectado>() != null) continue;   // la cabeza de un infectado nunca es suelo
            return true;
        }
        return false;
    }

    void Update()
    {
        if (muerto) { if (agachado) { agachado = false; AplicarAgachado(false); } return; }
        VerificarAcido();
        if (muerto) return;
        movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        movimientoVertical = Input.GetAxisRaw("Vertical"); // W / flecha arriba = 1, S / flecha abajo = -1

        // 1. Detección de suelo
        enSuelo = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) || TocaSueloExtra();
        if (!enSuelo && Time.time - ultimoContactoPlataforma < 0.15f && tiempoDesdeSalto > 0.3f && rb.linearVelocity.y <= 3f) enSuelo = true;
        // Pequeña tolerancia: si lleva muy poco tiempo en el aire no se activa la animación de caer
        if (enSuelo) tiempoEnAire = 0f; else tiempoEnAire += Time.deltaTime;
        tiempoDesdeSalto += Time.deltaTime;
        // Para la animacion: pequeños rebotes en bordes irregulares de las plataformas no cuentan como aire
        bool sueloAnim = enSuelo || (tiempoEnAire < 0.2f && tiempoDesdeSalto > 0.3f && rb.linearVelocity.y < 0.5f);

        // Al tocar el suelo (o agarrarse a una escalera) se recupera el salto extra
        if ((enSuelo && rb.linearVelocity.y <= 0.1f) || escalando)
            saltosAereosRestantes = dobleSalto ? 1 : 0;

        // 2. Escaleras
        enEscalera = escalerasTocando > 0;
        ActualizarEscalera();

        // 2b. Agacharse (S / flecha abajo): solo en el suelo, no en escaleras ni durante dash/retroceso
        bool quiereAgacharse = movimientoVertical < -0.5f || agacharForzado;
        bool puedeAgacharse = enSuelo && !escalando && !enEscalera && dashRestante <= 0f && tiempoRetroceso <= 0f && tiempoImpulsoSalida <= 0f;
        bool nuevoAgachado = puedeAgacharse && (quiereAgacharse || (agachado && !HayEspacioParaLevantarse()));
        if (nuevoAgachado != agachado) { agachado = nuevoAgachado; AplicarAgachado(agachado); }

        // 3. Actualizar Animator
        if (escalando)
        {
            // Animación de escalar: avanza solo mientras sube o baja (si se detiene en la escalera se queda quieto)
            animator.SetBool("isGrounded", true);
            animator.SetBool("isRunning", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isClimbing", true);
            animator.SetFloat("climbSpeed", Mathf.Abs(movimientoVertical) > 0.1f ? 1f : 0f);
        }
        else
        {
            animator.SetBool("isClimbing", false);
            animator.SetFloat("climbSpeed", 1f);
            animator.SetBool("isGrounded", sueloAnim);

            // Solo corre si se mueve y está en el suelo
            animator.SetBool("isRunning", Mathf.Abs(movimientoHorizontal) > 0.01f && sueloAnim);

            // La animación de caer SOLO se activa si está en el aire Y su velocidad es negativa.
            bool estaCayendo = !sueloAnim && rb.linearVelocity.y < -0.5f && tiempoEnAire > 0.2f;
            animator.SetBool("isFalling", estaCayendo);
            if (agachado) animator.SetBool("isRunning", false);
        }
        animator.SetBool("isCrouching", agachado && !escalando);

        // 4. Voltear sprite
        if (dashRestante <= 0f)
        {
            if (movimientoHorizontal < 0)
                spriteRenderer.flipX = true;
            else if (movimientoHorizontal > 0)
                spriteRenderer.flipX = false;
        }

        // Al escalar Mateo se ve de espaldas: nunca se voltea

        // Disparo aturdidor
        if (cooldownRestante > 0f) cooldownRestante = Mathf.Max(0f, cooldownRestante - Time.deltaTime);
        if (Input.GetKeyDown(teclaDisparo) && cooldownRestante <= 0f && !escalando) Disparar();

        // Habilidad de la gema roja / azul (tecla K)
        if (habilidadRestante > 0f) habilidadRestante = Mathf.Max(0f, habilidadRestante - Time.deltaTime);
        if (graciaDash > 0f) graciaDash -= Time.deltaTime;
        if (Input.GetKeyDown(teclaHabilidad) && TieneHabilidadK && habilidadRestante <= 0f && !escalando && dashRestante <= 0f)
            UsarHabilidad();

        // 5. Salto (también sirve para soltarse de la escalera) y doble salto en el aire
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (enSuelo || escalando)
            {
                if (escalando) SoltarEscalera();

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
                tiempoDesdeSalto = 0f;
                saltosAereosRestantes = dobleSalto ? 1 : 0;

                // Forzamos estados inmediatamente para que no haya lag en la animación
                enSuelo = false;
                animator.SetBool("isGrounded", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isFalling", false);
            }
            else if (dobleSalto && saltosAereosRestantes > 0)
            {
                // Doble salto: un impulso extra estando en el aire
                saltosAereosRestantes--;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaDobleSalto);
                animator.SetBool("isFalling", false);
                if (impulsoInstancia != null) impulsoInstancia.Reproducir(colorHabilidad);
            }
        }
    }

    // Encoge o restaura el collider (los pies se quedan en el mismo sitio)
    void AplicarAgachado(bool si)
    {
        if (cajaMateo == null) return;
        if (si)
        {
            float h = cajaTamOriginal.y * alturaAgachado;
            cajaMateo.size = new Vector2(cajaTamOriginal.x, h);
            cajaMateo.offset = new Vector2(cajaOffsetOriginal.x, cajaOffsetOriginal.y - (cajaTamOriginal.y - h) * 0.5f);
        }
        else
        {
            cajaMateo.size = cajaTamOriginal;
            cajaMateo.offset = cajaOffsetOriginal;
        }
    }

    // Hay techo encima? (para no levantarse dentro de un hueco bajo)
    bool HayEspacioParaLevantarse()
    {
        if (cajaMateo == null) return true;
        Vector3 e = transform.lossyScale;
        float d = cajaTamOriginal.y * (1f - alturaAgachado) * Mathf.Abs(e.y);
        float topeY = transform.position.y + cajaOffsetOriginal.y * e.y + cajaTamOriginal.y * Mathf.Abs(e.y) * 0.5f;
        Vector2 centro = new Vector2(transform.position.x + cajaOffsetOriginal.x * e.x, topeY - d * 0.5f);
        Vector2 tam = new Vector2(cajaTamOriginal.x * Mathf.Abs(e.x) * 0.9f, d * 0.9f);
        return Physics2D.OverlapBox(centro, tam, 0f, groundLayer) == null;
    }

    // Efecto al tomar CUALQUIER gema (cada gema pasa su color)
    public void RecogerGema(Color color)
    {
        colorHabilidad = color;
        if (efectoRecoger != null)
        {
            var e = Instantiate(efectoRecoger, transform.position, Quaternion.identity, transform);
            e.Reproducir(color);
        }
    }

    void Disparar()
    {
        if (prefabProyectil == null) prefabProyectil = Resources.Load<GameObject>("Proyectil");
        if (prefabProyectil == null) { Debug.LogWarning("[Disparo] Falta el prefab Resources/Proyectil"); return; }
        cooldownRestante = recargaDisparoSeg;
        animator.SetTrigger("disparar");
        StartCoroutine(LanzarProyectil());
    }

    // El proyectil sale en el cuadro del fogonazo de la animacion
    System.Collections.IEnumerator LanzarProyectil()
    {
        yield return new WaitForSeconds(retrasoProyectil);
        if (muerto) yield break;
        float dir = spriteRenderer.flipX ? -1f : 1f;
        Vector3 pos = transform.position + new Vector3(dir * 0.8f, agachado ? -0.3f : 0.15f, 0f);
        var go = Instantiate(prefabProyectil, pos, Quaternion.identity);
        var p = go.GetComponent<ProyectilAturdidor>();
        if (p != null) p.Lanzar(dir, velocidadProyectil, tiempoAturdimiento, gameObject);
    }

    /// <summary>Cura golpes (1 = un tercio de vida). Devuelve false si ya tenia la vida llena (el objeto no se gasta).</summary>
    public bool Curar(int golpes)
    {
        if (muerto || golpesRecibidos <= 0) return false;
        golpesRecibidos = Mathf.Max(0, golpesRecibidos - Mathf.Max(1, golpes));
        return true;
    }

    /// <summary>Cura toda la vida.</summary>
    public bool CurarTodo()
    {
        if (muerto || golpesRecibidos <= 0) return false;
        golpesRecibidos = 0;
        return true;
    }

    // Un enemigo golpea a Mateo: pierde 1/golpesParaMorir de vida, retrocede y es invulnerable un momento
    public void RecibirGolpe(Vector2 origen) { RecibirGolpes(1, origen); }

    // Golpe fuerte: quita 'cantidad' tercios de vida (2 = dos tercios, golpesParaMorir = toda la vida)
    public void RecibirGolpes(int cantidad, Vector2 origen)
    {
        if (muerto || tiempoInvulnerable > 0f || dashRestante > 0f || graciaDash > 0f) return;
        golpesRecibidos += Mathf.Max(1, cantidad);
        if (golpesRecibidos >= golpesParaMorir) { Morir(); return; }
        animator.SetTrigger("herido");
        tiempoInvulnerable = invulnerabilidad;
        tiempoRetroceso = 0.25f;
        if (escalando) SoltarEscalera();
        float dir = transform.position.x >= origen.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * retroceso.x, retroceso.y);
        StartCoroutine(ParpadeoDano());
    }

    System.Collections.IEnumerator ParpadeoDano()
    {
        while (tiempoInvulnerable > 0f && !muerto)
        {
            tiempoInvulnerable -= 0.1f;
            Color c = colorOriginal;
            c.a = (Mathf.RoundToInt(tiempoInvulnerable * 10f) % 2 == 0) ? 0.35f : 1f;
            if (tiempoInvulnerable > invulnerabilidad - 0.25f) c = new Color(1f, 0.35f, 0.35f, c.a);   // destello rojo al ser golpeado
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);
        }
        tiempoInvulnerable = 0f;
        if (!muerto) spriteRenderer.color = colorOriginal;
    }

    // Mateo muere (acido, etc.): se disuelve y reaparece en el punto de inicio del nivel
    public void Morir()
    {
        if (muerto) return;
        StartCoroutine(RutinaMuerte());
    }

    System.Collections.IEnumerator RutinaMuerte()
    {
        muerto = true;
        tiempoInvulnerable = 0f;
        dashRestante = 0f;
        if (escalando) SoltarEscalera();
        movimientoHorizontal = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;   // se queda hundido en el acido mientras se disuelve
        animator.SetBool("isClimbing", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isFalling", false);
        animator.ResetTrigger("disparar");
        animator.ResetTrigger("herido");

        float t = 0f;
        while (t < duracionMuerte)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(colorOriginal, colorDisolver, t / duracionMuerte);
            yield return null;
        }

        // Reaparece
        transform.position = puntoReaparicion;
        rb.position = puntoReaparicion;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravedadOriginal;
        spriteRenderer.color = colorOriginal;
        escalerasTocando = 0;
        saltosAereosRestantes = dobleSalto ? 1 : 0;
        golpesRecibidos = 0;   // al reaparecer recupera toda la vida
        habilidadRestante = 0f;
        // Al reaparecer Mateo vuelve sin gema (verde, roja o azul): la gema vuelve a su sitio, igual que las vendas y botiquines
        if (gemaActual != null) SoltarGemaActual();
        RecogibleCuracion.ReaparecerTodos();
        tiempoInvulnerable = 0f;
        muerto = false;
    }

    // La gema verde (y la roja / azul) llaman a este metodo. Mateo solo puede tener UNA gema:
    // la que llevaba vuelve a su lugar original y sus habilidades se pierden.
    public bool TomarGema(GemaVerde g)
    {
        if (g == null || muerto) return false;
        if (gemaActual == g) return false;
        SoltarGemaActual();
        gemaActual = g;
        habilidadRestante = 0f;
        dashRestante = 0f;
        switch (g.tipo)
        {
            case TipoGema.Verde:
                RecogerGema(colorGemaVerde);
                dobleSalto = true;
                saltosAereosRestantes = 1;
                PonerTraje(animatorTrajeVerde);
                break;
            case TipoGema.Roja:
                RecogerGema(colorGemaRoja);
                PonerTraje(animatorTrajeRojo);
                break;
            case TipoGema.Azul:
                RecogerGema(colorGemaAzul);
                PonerTraje(animatorTrajeAzul);
                break;
        }
        return true;
    }

    // Quita la gema actual: pierde sus habilidades, vuelve el traje normal y la gema reaparece en su sitio
    void SoltarGemaActual()
    {
        if (gemaActual == null) return;
        var g = gemaActual;
        gemaActual = null;
        dobleSalto = false;
        saltosAereosRestantes = 0;
        habilidadRestante = 0f;
        if (dashRestante > 0f) TerminarDash();
        PonerTraje(animatorBase);
        g.Reaparecer();
    }

    void PonerTraje(RuntimeAnimatorController c)
    {
        if (c != null && animator != null) animator.runtimeAnimatorController = c;
    }

    // Compatibilidad con scripts anteriores
    public void ActivarTrajeVerde()
    {
        dobleSalto = true;
        saltosAereosRestantes = 1;
        RecogerGema(colorGemaVerde);
        PonerTraje(animatorTrajeVerde);
    }

    void UsarHabilidad()
    {
        if (gemaActual == null) return;
        if (gemaActual.tipo == TipoGema.Roja)
        {
            habilidadRestante = cooldownRoja;
            OndaDeImpulso();
        }
        else if (gemaActual.tipo == TipoGema.Azul)
        {
            habilidadRestante = cooldownAzul;
            IniciarDash();
        }
    }

    // Roja: onda que lanza lejos a los enemigos que estan alrededor de Mateo
    void OndaDeImpulso()
    {
        var yaEmpujados = new System.Collections.Generic.HashSet<IEmpujable>();
        foreach (var c in Physics2D.OverlapCircleAll(transform.position, radioOnda))
        {
            if (c == null) continue;
            var e = c.GetComponentInParent<IEmpujable>();
            if (e == null || !yaEmpujados.Add(e)) continue;
            e.Empujar(transform.position, fuerzaOnda);
        }
        if (efectoRecoger != null)
        {
            var fx = Instantiate(efectoRecoger, transform.position, Quaternion.identity, transform);
            fx.Reproducir(colorGemaRoja);
        }
    }

    // Azul: dash horizontal; durante el dash atraviesa enemigos y nada le hace dano
    // Colliders de enemigos que Mateo atraviesa mientras dura el dash (se restauran al terminar, cuando ya no se solapan)
    readonly System.Collections.Generic.List<Collider2D> atravesados = new System.Collections.Generic.List<Collider2D>();

    void AtravesarEnemigos()
    {
        if (miCollider == null) return;
        foreach (var e in FindObjectsByType<EnemigoInfectado>(FindObjectsSortMode.None))
            foreach (var c in e.GetComponentsInChildren<Collider2D>())
            {
                if (c == null || c.isTrigger) continue;
                Physics2D.IgnoreCollision(miCollider, c, true);
                if (!atravesados.Contains(c)) atravesados.Add(c);
            }
    }

    void RestaurarEnemigos()
    {
        for (int i = atravesados.Count - 1; i >= 0; i--)
        {
            var c = atravesados[i];
            if (c == null) { atravesados.RemoveAt(i); continue; }
            if (miCollider.Distance(c).isOverlapped) continue;   // todavia dentro del enemigo: se restaura al salir
            var en = c.GetComponentInParent<EnemigoInfectado>();
            Physics2D.IgnoreCollision(miCollider, c, en != null && en.atravesable);
            atravesados.RemoveAt(i);
        }
    }

    void IniciarDash()
    {
        AtravesarEnemigos();
        dashDir = spriteRenderer.flipX ? -1f : 1f;
        dashRestante = duracionDash;
        graciaDash = duracionDash + 0.15f;
        tiempoRetroceso = 0f;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDir * velocidadDash, 0f);
        spriteRenderer.color = new Color(0.45f, 0.75f, 1f, 0.55f);
        if (efectoRecoger != null)
        {
            var fx = Instantiate(efectoRecoger, transform.position, Quaternion.identity, transform);
            fx.Reproducir(colorGemaAzul);
        }
    }

    void TerminarDash()
    {
        dashRestante = 0f;
        if (!escalando) rb.gravityScale = gravedadOriginal;
        rb.linearVelocity = new Vector2(dashDir * velocidad * 0.5f, 0f);
        if (!muerto) spriteRenderer.color = colorOriginal;
    }

    void FixedUpdate()
    {
        if (muerto) return;
        if (dashRestante <= 0f && atravesados.Count > 0) RestaurarEnemigos();
        if (dashRestante > 0f)
        {
            AtravesarEnemigos();   // tambien los que aparezcan o salten durante el dash
            dashRestante -= Time.fixedDeltaTime;
            if (dashRestante <= 0f) TerminarDash();
            else rb.linearVelocity = new Vector2(dashDir * velocidadDash, 0f);
            return;
        }
        if (escalando)
        {
            // Sube o baja por la escalera, sin moverse hacia los lados
            rb.linearVelocity = new Vector2(0f, movimientoVertical * velocidadEscalada);
        }
        else if (tiempoImpulsoSalida > 0f)
        {
            tiempoImpulsoSalida -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(direccionSalida * velocidad * 0.5f, rb.linearVelocity.y);
        }
        else if (tiempoRetroceso > 0f)
        {
            tiempoRetroceso -= Time.fixedDeltaTime;   // empujon del golpe: no se controla un instante
        }
        else
        {
            rb.linearVelocity = new Vector2(agachado ? 0f : movimientoHorizontal * velocidad, rb.linearVelocity.y);
        }

        // Encima de un infectado: resbala hacia un lado (no se puede usar su cabeza como plataforma ni para atajos)
        if (sobreEnemigo && !escalando)
        {
            rb.linearVelocity = new Vector2(dirResbalar * 6f, Mathf.Min(rb.linearVelocity.y, 0f));
            sobreEnemigo = false;
        }
    }

    bool sobreEnemigo; float dirResbalar = 1f;
    void OnCollisionStay2D(Collision2D c)
    {
        if (muerto || miCollider == null) return;
        var en = c.collider.GetComponentInParent<EnemigoInfectado>();
        if (en == null) return;
        Bounds m = miCollider.bounds, e = c.collider.bounds;
        // Mateo esta encima: sus pies a la altura de la parte alta del enemigo y solapados en horizontal
        if (m.min.y >= e.max.y - 0.25f && Mathf.Abs(m.center.x - e.center.x) < m.extents.x + e.extents.x)
        {
            sobreEnemigo = true;
            float dx = m.center.x - e.center.x;
            dirResbalar = Mathf.Abs(dx) > 0.05f ? Mathf.Sign(dx) : (movimientoHorizontal != 0f ? Mathf.Sign(movimientoHorizontal) : 1f);
        }
    }

    // ---------- ACIDO ----------

    Tilemap[] tilemapsMortales;
    readonly System.Collections.Generic.List<Bounds> zonasAcido = new System.Collections.Generic.List<Bounds>();

    // Cada tile "Acido..." se dibuja de 2x2 unidades (la celda es de 1x1), asi que aqui se crea para cada uno una caja solida
    // que cubre el dibujo completo: el fondo hace de suelo/techo y el liquido es la zona mortal.
    // Funciona sin importar en que Tilemap este pintado el tile.
    void PrepararAcido()
    {
        var lista = new System.Collections.Generic.List<Tilemap>();
        foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if (tm.GetComponent<TilemapRenderer>() != null) lista.Add(tm);
        tilemapsMortales = lista.ToArray();

        foreach (var tm in tilemapsMortales)
        {
            GameObject go = null;
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                if (!EsAcido(tm.GetTile(pos))) continue;
                if (go == null)
                {
                    go = new GameObject("ColisionAcido_auto");
                    go.transform.SetParent(tm.transform, false);
                    go.layer = tm.gameObject.layer;
                    go.tag = tm.gameObject.tag;
                }
                Vector3 c = tm.GetCellCenterLocal(pos);
                var caja = go.AddComponent<BoxCollider2D>();
                caja.offset = new Vector2(c.x, c.y - 0.345f);
                caja.size = new Vector2(2f, 1.31f);

                // Zona mortal en mundo: casi todo el liquido, sin los ultimos 15 cm de abajo (asi tocar el fondo desde abajo no mata)
                Vector3 w = tm.transform.TransformPoint(c);
                float abajo = w.y - 1f + 0.15f;
                float arriba = w.y + 0.31f + 0.1f;
                zonasAcido.Add(new Bounds(new Vector3(w.x, (abajo + arriba) / 2f, 0f), new Vector3(2f, arriba - abajo, 10f)));
            }
        }
    }

    void VerificarAcido()
    {
        if (muerto) return;
        if (tilemapsMortales == null) PrepararAcido();
        if (zonasAcido.Count == 0 || miCollider == null) return;
        // Solo muere si sus PIES (centro de la base) entran en la superficie del liquido: caer encima.
        // Rozar el borde con el cuerpo o estar en una plataforma al lado no cuenta.
        Bounds yo = miCollider.bounds;
        Vector2 pies = new Vector2(yo.center.x, yo.min.y);
        foreach (var z in zonasAcido)
            if (pies.x > z.min.x && pies.x < z.max.x && pies.y <= z.max.y && pies.y >= z.min.y)
            {
                Morir();
                return;
            }
    }

    static bool EsAcido(TileBase t)
    {
        return t != null && t.name.StartsWith("Acido");
    }

    // ---------- ESCALERAS ----------

    // Unity avisa cuando Mateo entra o sale del área (trigger) de un objeto Escalera
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponent<ZonaMortal>() != null)
        {
            // El tilemap de acido ya se controla con VerificarAcido (pies dentro del liquido); su collider es mas grande que el liquido.
            if (otro is TilemapCollider2D || otro.GetComponent<Tilemap>() != null) return;
            Morir(); return;
        }
        if (otro.GetComponent<Escalera>() != null)
        {
            escalerasTocando++;
            escaleraActual = otro;
        }
    }

    void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.GetComponent<Escalera>() != null) escalerasTocando = Mathf.Max(0, escalerasTocando - 1);
    }

    void ActualizarEscalera()
    {
        if (graciaEscalera > 0f) graciaEscalera -= Time.deltaTime;

        if (!escalando)
        {
            // Se agarra a la escalera al presionar W (o flecha arriba) estando sobre ella (y no arriba del todo)
            if (enEscalera && movimientoVertical > 0.1f && escaleraActual != null
                && groundCheck.position.y < escaleraActual.bounds.max.y - 0.3f)
                AgarrarEscalera();
            // Desde arriba, S (o flecha abajo) atraviesa la plataforma y baja por la escalera
            else if (enSuelo && movimientoVertical < -0.1f)
                IntentarBajarPorPlataforma();
            return;
        }

        if (!enEscalera)
        {
            if (graciaEscalera > 0f) return;

            // Llegó al final de la escalera: arriba pasa al piso con un pequeño impulso
            bool llegoArriba = movimientoVertical > 0.1f;
            SoltarEscalera();
            if (llegoArriba)
            {
                float dir = movimientoHorizontal != 0 ? Mathf.Sign(movimientoHorizontal) : (spriteRenderer.flipX ? -1f : 1f);
                direccionSalida = dir;
                tiempoImpulsoSalida = 0.3f;
                rb.linearVelocity = new Vector2(dir * velocidad * 0.5f, impulsoSalidaSuperior);
            }
        }
        else if (enSuelo && movimientoVertical < -0.1f && graciaEscalera <= 0f)
        {
            // Bajó hasta el suelo
            SoltarEscalera();
        }
    }

    // Si Mateo está parado sobre la plataforma de una escalera, la atraviesa para bajar
    void IntentarBajarPorPlataforma()
    {
        Collider2D[] golpes = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius + 0.1f, groundLayer);
        foreach (Collider2D c in golpes)
        {
            PlataformaEscalera p = c.GetComponent<PlataformaEscalera>();
            if (p == null) continue;
            escaleraActual = p.areaEscalera;
            plataformaIgnorada = c;
            Physics2D.IgnoreCollision(miCollider, c, true);
            AgarrarEscalera();
            graciaEscalera = 0.3f;
            return;
        }
    }

    void AgarrarEscalera()
    {
        escalando = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        // Centra a Mateo en la escalera para que no choque con los bordes del hueco
        if (escaleraActual != null)
            rb.position = new Vector2(escaleraActual.bounds.center.x, rb.position.y);
    }

    void SoltarEscalera()
    {
        escalando = false;
        rb.gravityScale = gravedadOriginal;
        if (plataformaIgnorada != null)
        {
            Physics2D.IgnoreCollision(miCollider, plataformaIgnorada, false);
            plataformaIgnorada = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
