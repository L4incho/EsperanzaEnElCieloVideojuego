using UnityEngine;

// Cualquier enemigo que la onda de la gema roja pueda lanzar lejos
public interface IEmpujable { void Empujar(Vector2 origen, float fuerza); }

public enum TipoInfectado { Normal, Saltador, Pesado }

// Infectados del juego.
//  - Normal   (infectado 1 y dummy): lento, patrulla, persigue y golpea (1/3 de vida).
//  - Saltador (infectado 2): se mueve con los brazos, mas lento; ataca con un salto muy largo (2/3 de vida).
//  - Pesado   (infectado 3): grande y lento; ataque lento que quita toda la vida; no se aturde.
// Mateo no los atraviesa (hay que saltar por encima de ellos).
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoInfectado : MonoBehaviour, IAturdible, IEmpujable
{
    [Header("Tipo")]
    public TipoInfectado tipo = TipoInfectado.Normal;
    [Tooltip("Cuantos golpes (tercios de vida) quita su ataque")]
    public int golpesQueQuita = 1;
    public bool quitaTodaLaVida = false;
    [Tooltip("El disparo aturdidor de Mateo no lo detiene (solo cambia el sprite)")]
    public bool inmuneAturdimiento = false;
    [Tooltip("Si es true Mateo lo atraviesa (antes era asi)")]
    public bool atravesable = false;

    [Header("Movimiento")]
    public float velocidadPatrulla = 0.8f;
    public float velocidadPersecucion = 1.4f;
    public float distanciaPatrulla = 3f;
    public float rangoDeteccion = 7f;

    [Header("Ataque")]
    public float rangoAtaque = 1.3f;
    [Tooltip("Segundos que levanta el golpe antes de pegar (Mateo puede esquivarlo)")]
    public float tiempoPreparacion = 0.5f;
    public float tiempoRecuperacion = 0.35f;
    public float enfriamiento = 1.4f;

    [Header("Salto (solo Saltador)")]
    public float distanciaSalto = 8f;
    public float duracionSalto = 0.75f;

    [Header("Sprites (dummy, un cuadro)")]
    public Sprite quieto;
    public Sprite[] caminar;
    public Sprite ataque;
    public float tiempoPorFrame = 0.25f;

    [Header("Animaciones de 8 cuadros (infectados nuevos)")]
    public Sprite[] animCaminar;
    public Sprite[] animAtaque;
    public Sprite[] animDano;
    public float tiempoPorFrameAnim = 0.1f;
    public float tiempoPorFrameDano = 0.1f;

    [Header("Suelo")]
    public LayerMask capaSuelo;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D miCollider;
    PlayerController jugador;
    Vector2 inicio;
    int dir = 1;
    bool atacando, saltando;
    float proximoAtaque;
    float tFrame;
    int frame;
    float vx;
    float aturdidoHasta, danoDesde, danoHasta;
    Color colorBase = Color.white;
    float empujadoHasta;
    public bool EstaAturdido { get { return Time.time < aturdidoHasta; } }
    bool TieneAnim { get { return animCaminar != null && animCaminar.Length > 0; } }
    bool TieneDano { get { return animDano != null && animDano.Length >= 8; } }

    // El proyectil de Mateo lo aturde: no se mueve ni ataca durante 'segundos'
    public void Aturdir(float segundos)
    {
        if (inmuneAturdimiento)
        {
            // solo cambia al sprite de dano un momento; no se detiene ni pierde el ataque
            if (TieneDano) { danoDesde = Time.time; danoHasta = Time.time + animDano.Length * tiempoPorFrameDano; }
            return;
        }
        StopAllCoroutines();
        atacando = false; saltando = false;
        danoDesde = Time.time;
        aturdidoHasta = Time.time + segundos;
        proximoAtaque = aturdidoHasta + 0.6f;
        vx = 0f;
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // Onda de la gema roja: sale disparado lejos de Mateo y cancela su ataque
    public void Empujar(Vector2 origen, float fuerza)
    {
        StopAllCoroutines();
        atacando = false; saltando = false;
        float d = transform.position.x >= origen.x ? 1f : -1f;
        empujadoHasta = Time.time + 0.5f;
        proximoAtaque = Mathf.Max(proximoAtaque, Time.time + 1.2f);
        vx = 0f;
        if (rb != null) rb.linearVelocity = new Vector2(d * fuerza, fuerza * 0.45f);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();
        colorBase = sr.color;
        inicio = transform.position;
        jugador = FindFirstObjectByType<PlayerController>();
        if (atravesable && jugador != null && miCollider != null)
            foreach (var c in jugador.GetComponents<Collider2D>()) Physics2D.IgnoreCollision(miCollider, c);
    }

    void Update()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerController>();
        if (EstaAturdido)
        {
            if (TieneDano)
            {
                // golpe + estrellitas: 8 cuadros una vez y luego se repite la parte de mareado
                float k = (Time.time - danoDesde) / tiempoPorFrameDano;
                int f = k < 8f ? (int)k : 3 + ((int)(k - 8f)) % 5;
                sr.sprite = animDano[Mathf.Clamp(f, 0, animDano.Length - 1)];
                if (sr.color != colorBase) sr.color = colorBase;
            }
            else
            {
                // dummy: quieto, con un tinte azul que parpadea mas rapido cuando esta por despertar
                if (quieto != null) sr.sprite = quieto;
                float falta = aturdidoHasta - Time.time;
                float vel = falta < 3f ? 14f : 5f;
                sr.color = Color.Lerp(colorBase, new Color(0.45f, 0.65f, 1f, colorBase.a), 0.5f + 0.5f * Mathf.Sin(Time.time * vel));
            }
            return;
        }
        if (sr.color != colorBase) sr.color = colorBase;
        if (saltando) return;
        if (!atacando && Time.time < danoHasta && TieneDano)
        {
            float k = (Time.time - danoDesde) / tiempoPorFrameDano;
            sr.sprite = animDano[Mathf.Clamp((int)k, 0, animDano.Length - 1)];
            sr.flipX = dir < 0;
        }
        else AnimarSprite();
        if (atacando || jugador == null || jugador.EstaMuerto) return;

        float dx = jugador.transform.position.x - transform.position.x;
        float dy = Mathf.Abs(jugador.transform.position.y - transform.position.y);
        float alcance = tipo == TipoInfectado.Saltador ? distanciaSalto + 0.5f : rangoAtaque;
        float alturaMax = tipo == TipoInfectado.Saltador ? 3f : 1.8f;
        if (BarrilEntreMateo()) return;   // un barril de por medio: no lo ve ni ataca
        bool lejosParaSaltar = tipo != TipoInfectado.Saltador || Mathf.Abs(dx) > 2.0f;
        if (Mathf.Abs(dx) <= alcance && dy < alturaMax && Time.time >= proximoAtaque && lejosParaSaltar)
        {
            dir = dx >= 0 ? 1 : -1;
            StartCoroutine(Atacar());
        }
        else if (tipo == TipoInfectado.Saltador && Mathf.Abs(dx) <= rangoAtaque && dy < alturaMax && Time.time >= proximoAtaque)
        {
            dir = dx >= 0 ? 1 : -1;
            StartCoroutine(Atacar());   // si Mateo esta pegado, salta igual (pasa por encima)
        }
    }

    System.Collections.IEnumerator Atacar()
    {
        atacando = true;
        vx = 0f;
        sr.flipX = dir < 0;
        bool nuevo = animAtaque != null && animAtaque.Length >= 8;
        if (tipo == TipoInfectado.Saltador && nuevo) yield return StartCoroutine(Saltar());
        else if (nuevo) yield return StartCoroutine(GolpearAnimado());
        else yield return StartCoroutine(GolpearSimple());
        proximoAtaque = Time.time + enfriamiento;
        atacando = false;
        saltando = false;
    }

    int Danio()
    {
        if (quitaTodaLaVida && jugador != null) return jugador.golpesParaMorir;
        return Mathf.Max(1, golpesQueQuita);
    }

    void Pegar()
    {
        if (jugador == null || jugador.EstaMuerto) return;
        float dx = Mathf.Abs(jugador.transform.position.x - transform.position.x);
        float dy = Mathf.Abs(jugador.transform.position.y - transform.position.y);
        if (dx <= rangoAtaque + 0.3f && dy < 1.8f) jugador.RecibirGolpes(Danio(), transform.position);
    }

    System.Collections.IEnumerator GolpearSimple()
    {
        if (ataque != null) sr.sprite = ataque;
        yield return new WaitForSeconds(tiempoPreparacion);
        Pegar();
        yield return new WaitForSeconds(tiempoRecuperacion);
    }

    System.Collections.IEnumerator GolpearAnimado()
    {
        float total = tiempoPreparacion + tiempoRecuperacion, t = 0f;
        bool pego = false;
        while (t < total)
        {
            int f = Mathf.Min(animAtaque.Length - 1, (int)(t / total * animAtaque.Length));
            sr.sprite = animAtaque[f];
            if (!pego && t >= tiempoPreparacion) { pego = true; Pegar(); }
            t += Time.deltaTime;
            yield return null;
        }
    }

    // Infectado 2: se agacha (cuadros 0-2), salta con los brazos (3-6) y cae (7)
    System.Collections.IEnumerator Saltar()
    {
        float t = 0f;
        while (t < tiempoPreparacion)
        {
            int f = Mathf.Min(2, (int)(t / tiempoPreparacion * 3f));
            sr.sprite = animAtaque[f];
            if (jugador != null) { dir = jugador.transform.position.x >= transform.position.x ? 1 : -1; sr.flipX = dir < 0; }
            t += Time.deltaTime;
            yield return null;
        }
        if (jugador == null || jugador.EstaMuerto) yield break;

        // distancia del salto: llega hasta Mateo (y un poco mas) con tope 'distanciaSalto'; busca suelo donde caer
        float dist = Mathf.Min(distanciaSalto, Mathf.Abs(jugador.transform.position.x - transform.position.x) + 1.2f);
        float d = dist;
        while (d > 1f && !HaySueloEn(transform.position.x + dir * d)) d -= 0.5f;
        if (d <= 1f) d = 0f;

        float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        saltando = true;
        rb.linearVelocity = new Vector2(dir * d / duracionSalto, g * duracionSalto * 0.5f);
        float tv = 0f; bool pego = false;
        var colMateo = jugador.GetComponent<Collider2D>();
        while (tv < duracionSalto + 0.8f)
        {
            tv += Time.deltaTime;
            // si Mateo esta agachado el salto pasa de largo sin chocar con su cabeza (solo este ataque; los demas infectados lo siguen golpeando)
            if (colMateo != null) Physics2D.IgnoreCollision(miCollider, colMateo, jugador.EstaAgachado || atravesable);
            int f = 3 + Mathf.Clamp((int)(tv / duracionSalto * 4f), 0, 3);
            sr.sprite = animAtaque[f];
            if (!pego && jugador != null && !jugador.EstaMuerto)
            {
                var b = miCollider.bounds; b.Expand(new Vector3(1.2f, 2.4f, 0f));   // el salto pasa por encima: zona de golpe amplia
                var pc = jugador.GetComponent<Collider2D>();
                // si Mateo esta agachado el salto pasa por encima y no lo conecta
                if (pc != null && !jugador.EstaAgachado && b.Intersects(pc.bounds)) { pego = true; jugador.RecibirGolpes(Danio(), transform.position); }
            }
            if (tv > 0.25f && rb.linearVelocity.y <= 0.05f && EnSuelo()) break;
            yield return null;
        }
        if (colMateo != null) Physics2D.IgnoreCollision(miCollider, colMateo, atravesable);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        saltando = false;
        sr.sprite = animAtaque[7];
        yield return new WaitForSeconds(tiempoRecuperacion);
    }

    bool EnSuelo()
    {
        Bounds b = miCollider.bounds;
        return Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.2f, capaSuelo).collider != null;
    }

    bool HaySueloEn(float x)
    {
        Bounds b = miCollider.bounds;
        return Physics2D.Raycast(new Vector2(x, b.min.y + 1f), Vector2.down, 4f, capaSuelo).collider != null;
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (saltando) return;                      // en el aire: no se toca su velocidad
        if (Time.time < empujadoHasta) return;   // vuela por la onda: no se toca su velocidad
        if (EstaAturdido) { rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); return; }
        if (atacando || jugador == null) { rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); return; }

        float vel = velocidadPatrulla;
        float dx = jugador.transform.position.x - transform.position.x;
        float dy = Mathf.Abs(jugador.transform.position.y - transform.position.y);
        bool veAMateo = !jugador.EstaMuerto && Mathf.Abs(dx) <= rangoDeteccion && dy < 3f && !BarrilEntreMateo();

        if (veAMateo)
        {
            vel = velocidadPersecucion;
            dir = dx >= 0 ? 1 : -1;
            if (Mathf.Abs(dx) < rangoAtaque * 0.7f) vel = 0f;   // ya esta encima: se detiene a golpear
        }
        else
        {
            float desde = transform.position.x - inicio.x;
            if (desde > distanciaPatrulla) dir = -1;
            else if (desde < -distanciaPatrulla) dir = 1;
            if (BarrilAdelante()) dir = -dir;   // no se queda empujando el barril
        }

        if (vel > 0f && (HayPared() || !HaySuelo()))
        {
            if (veAMateo) vel = 0f; else dir = -dir;   // no se cae al acido ni atraviesa paredes
        }
        vx = vel * dir;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    // ---- Barriles: tapan la vista de Mateo (el infectado no lo detecta ni lo persigue a traves de un barril) ----
    static FuegoBarril[] barriles;
    static float barrilesRefrescados = -10f;
    static FuegoBarril[] Barriles()
    {
        if (barriles == null || Time.time - barrilesRefrescados > 1f)
        {
            barriles = FindObjectsByType<FuegoBarril>(FindObjectsSortMode.None);
            barrilesRefrescados = Time.time;
        }
        return barriles;
    }

    // true si hay un barril entre el infectado y Mateo (a una altura parecida)
    bool BarrilEntreMateo()
    {
        if (jugador == null) return false;
        float x0 = transform.position.x, x1 = jugador.transform.position.x;
        float min = Mathf.Min(x0, x1), max = Mathf.Max(x0, x1);
        foreach (var b in Barriles())
        {
            if (b == null) continue;
            float bx = b.transform.position.x;
            if (bx > min && bx < max && Mathf.Abs(b.transform.position.y - transform.position.y) < 2.5f) return true;
        }
        return false;
    }

    // true si hay un barril justo delante (en la direccion en que camina)
    bool BarrilAdelante()
    {
        foreach (var b in Barriles())
        {
            if (b == null) continue;
            float d = (b.transform.position.x - transform.position.x) * dir;
            if (d > 0f && d < 1.4f && Mathf.Abs(b.transform.position.y - transform.position.y) < 2.5f) return true;
        }
        return false;
    }

    bool HaySuelo()
    {
        Bounds b = miCollider != null ? miCollider.bounds : new Bounds(transform.position, Vector3.one);
        Vector2 o = new Vector2(b.center.x + dir * (b.extents.x + 0.15f), b.min.y + 0.1f);
        return Physics2D.Raycast(o, Vector2.down, 0.6f, capaSuelo).collider != null;
    }

    bool HayPared()
    {
        Bounds b = miCollider != null ? miCollider.bounds : new Bounds(transform.position, Vector3.one);
        Vector2 o = new Vector2(b.center.x, b.center.y);
        return Physics2D.Raycast(o, Vector2.right * dir, b.extents.x + 0.2f, capaSuelo).collider != null;
    }

    void AnimarSprite()
    {
        if (atacando) return;
        sr.flipX = dir < 0;
        if (TieneAnim)
        {
            // infectados nuevos: 8 cuadros al caminar, primer cuadro cuando esta quieto
            if (Mathf.Abs(vx) < 0.05f) { sr.sprite = animCaminar[0]; tFrame = 0f; return; }
            tFrame += Time.deltaTime;
            if (tFrame >= tiempoPorFrameAnim) { tFrame = 0f; frame = (frame + 1) % animCaminar.Length; }
            sr.sprite = animCaminar[frame % animCaminar.Length];
            return;
        }
        if (Mathf.Abs(vx) < 0.05f || caminar == null || caminar.Length == 0)
        {
            if (quieto != null) sr.sprite = quieto;
            return;
        }
        tFrame += Time.deltaTime;
        if (tFrame >= tiempoPorFrame) { tFrame = 0f; frame = (frame + 1) % caminar.Length; }
        sr.sprite = caminar[frame % caminar.Length];
    }
}
