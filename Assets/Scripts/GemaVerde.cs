using System.Collections;
using UnityEngine;

public enum TipoGema { Verde, Roja, Azul }

// Gema de habilidad (el nombre de la clase se quedo "GemaVerde" para no romper el prefab original, pero sirve para las 3):
//   Verde = traje verde + doble salto (pasivo)          -> NO vuelve a aparecer si Mateo muere
//   Roja  = traje rojo + onda de impulso (tecla K)      -> vuelve a aparecer en su sitio si Mateo muere
//   Azul  = traje azul + dash invulnerable (tecla K)    -> vuelve a aparecer en su sitio si Mateo muere
// Mateo solo puede tener UNA gema a la vez: al tomar otra, la anterior vuelve a su sitio.
// Animaciones (opcionales): el Animator de la gema puede tener "Flotar" (bucle) y "Recoger" (al ser recogida).
// Si la gema no tiene Animator (roja/azul), flota sola con un vaiven.
[RequireComponent(typeof(Collider2D))]
public class GemaVerde : MonoBehaviour
{
    public TipoGema tipo = TipoGema.Verde;

    [Tooltip("Tiempo minimo (segundos) antes de que desaparezca, aunque la animacion Recoger sea muy corta")]
    public float tiempoMinimo = 0.05f;

    [Tooltip("Segundos que tarda en poder volver a tomarse despues de reaparecer")]
    public float esperaAlReaparecer = 1.0f;

    [Tooltip("Sonido al recoger (opcional)")]
    public AudioClip sonidoRecoger;

    [Header("Vaiven (solo si no tiene Animator)")]
    public float amplitudVaiven = 0.12f;
    public float velocidadVaiven = 2.2f;

    [Header("Animacion de la gema (cuadros del sprite)")]
    public Sprite[] cuadros;
    public float cuadrosPorSegundo = 10f;

    SpriteRenderer sr;
    bool recogida;
    float bloqueadaHasta;
    Animator animator;
    Collider2D col;
    Vector3 posInicial;
    Transform visual;

    void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        visual = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (visual != null)
        {
            sr = visual.GetComponent<SpriteRenderer>();
            // Si en el editor moviste solo el dibujo (hijo "Visual") y no la gema, la gema (y su zona de recogida) se mueve
            // hasta donde esta el dibujo; asi nunca se "pierde" el dibujo al darle Play.
            if (visual.localPosition.sqrMagnitude > 0.0001f)
            {
                Vector3 mundo = visual.position;
                transform.position = mundo;
                visual.localPosition = Vector3.zero;
            }
        }
        posInicial = transform.position;
    }

    void Update()
    {
        if (sr != null && cuadros != null && cuadros.Length > 0)
            sr.sprite = cuadros[(int)(Time.time * cuadrosPorSegundo) % cuadros.Length];
        // Sin Animator: flota suavemente (mueve el dibujo, no la gema)
        if (recogida || visual == null || (animator != null && animator.runtimeAnimatorController != null)) return;
        visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * velocidadVaiven) * amplitudVaiven, 0f);
    }

    void OnTriggerEnter2D(Collider2D otro) { Intentar(otro); }
    void OnTriggerStay2D(Collider2D otro) { Intentar(otro); }

    void Intentar(Collider2D otro)
    {
        if (recogida || Time.time < bloqueadaHasta) return;
        var mateo = otro.GetComponentInParent<PlayerController>();
        if (mateo == null || mateo.EstaMuerto) return;

        if (!mateo.TomarGema(this)) return;
        recogida = true;
        col.enabled = false;
        if (sonidoRecoger != null) AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);
        StartCoroutine(DesaparecerDespuesDeAnimacion());
    }

    IEnumerator DesaparecerDespuesDeAnimacion()
    {
        float espera = tiempoMinimo;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Recoger");
            yield return null;
            yield return null; // deja que el Animator pase al estado "Recoger"
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Recoger")) espera = Mathf.Max(espera, info.length);
        }
        yield return new WaitForSeconds(espera);
        gameObject.SetActive(false);   // no se destruye: Mateo puede soltarla y debe volver a su sitio
    }

    /// <summary>La gema vuelve a su lugar original (Mateo murio o tomo otra gema).</summary>
    public void Reaparecer()
    {
        StopAllCoroutines();
        recogida = false;
        bloqueadaHasta = Time.time + esperaAlReaparecer;
        transform.position = posInicial;
        gameObject.SetActive(true);
        if (col != null) col.enabled = true;
        if (visual != null) visual.localPosition = Vector3.zero;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.ResetTrigger("Recoger");
            animator.Rebind();
            animator.Update(0f);
        }
    }
}
