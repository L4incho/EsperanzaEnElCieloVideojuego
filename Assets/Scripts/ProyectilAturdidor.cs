using UnityEngine;

// Cualquier enemigo que se pueda aturdir implementa esto (el dummy ya lo hace).
public interface IAturdible
{
    void Aturdir(float segundos);
}

// Proyectil de Mateo (tecla J): no mata, aturde al enemigo que toca. Viaja en linea recta y estalla al chocar.
[RequireComponent(typeof(SpriteRenderer))]
public class ProyectilAturdidor : MonoBehaviour
{
    [Header("Animacion (sprites municion)")]
    public Sprite[] vueloDerecha;
    public Sprite[] impactoDerecha;
    public Sprite[] vueloIzquierda;
    public Sprite[] impactoIzquierda;
    public float tiempoPorFrame = 0.08f;
    public float vidaMaxima = 2.5f;

    SpriteRenderer sr;
    float dir = 1f, vel = 12f, aturdimiento = 14f;
    GameObject dueno;
    bool impactado;
    float t, tFrame;
    int frame;

    public void Lanzar(float direccion, float velocidad, float segundosAturdimiento, GameObject quienDispara)
    {
        dir = direccion >= 0 ? 1f : -1f;
        vel = velocidad;
        aturdimiento = segundosAturdimiento;
        dueno = quienDispara;
    }

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        t += Time.deltaTime;
        if (!impactado)
        {
            transform.position += Vector3.right * dir * vel * Time.deltaTime;
            if (t > vidaMaxima) { Destroy(gameObject); return; }
        }
        Animar();
    }

    void Animar()
    {
        Sprite[] lista = impactado
            ? (dir >= 0 ? impactoDerecha : impactoIzquierda)
            : (dir >= 0 ? vueloDerecha : vueloIzquierda);
        if (lista == null || lista.Length == 0) return;
        tFrame += Time.deltaTime;
        if (tFrame >= tiempoPorFrame)
        {
            tFrame = 0f;
            frame++;
            if (impactado && frame >= lista.Length) { Destroy(gameObject); return; }
            frame %= lista.Length;
        }
        sr.sprite = lista[Mathf.Min(frame, lista.Length - 1)];
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (impactado) return;
        if (otro.isTrigger) return;
        if (dueno != null && otro.transform.IsChildOf(dueno.transform)) return;
        if (otro.GetComponentInParent<PlayerController>() != null) return;

        var objetivo = otro.GetComponentInParent<IAturdible>();
        if (objetivo != null) objetivo.Aturdir(aturdimiento);
        Impactar();
    }

    void Impactar()
    {
        impactado = true;
        frame = 0; tFrame = 0f;
        var c = GetComponent<Collider2D>();
        if (c != null) c.enabled = false;
    }
}
