
using UnityEngine;

// Cualquier enemigo que se pueda aturdir implementa esto.
public interface IAturdible
{
    void Aturdir(float segundos);
}

// Proyectil de Mateo (tecla J).
// Aturde enemigos y reproduce un sonido
// solamente cuando impacta contra uno.
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

    [Header("Sonido de impacto contra zombis")]
    public AudioClip sonidoDanioZombie;

    [Range(0f, 1f)]
    public float volumenDanioZombie = 0.8f;

    SpriteRenderer sr;

    float dir = 1f;
    float vel = 12f;
    float aturdimiento = 14f;

    GameObject dueno;

    bool impactado;

    float t;
    float tFrame;

    int frame;

    public void Lanzar(
        float direccion,
        float velocidad,
        float segundosAturdimiento,
        GameObject quienDispara)
    {
        dir = direccion >= 0 ? 1f : -1f;
        vel = velocidad;
        aturdimiento = segundosAturdimiento;
        dueno = quienDispara;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        t += Time.deltaTime;

        if (!impactado)
        {
            transform.position +=
                Vector3.right * dir * vel * Time.deltaTime;

            if (t > vidaMaxima)
            {
                Destroy(gameObject);
                return;
            }
        }

        Animar();
    }

    void Animar()
    {
        Sprite[] lista = impactado
            ? (dir >= 0 ? impactoDerecha : impactoIzquierda)
            : (dir >= 0 ? vueloDerecha : vueloIzquierda);

        if (lista == null || lista.Length == 0)
            return;

        tFrame += Time.deltaTime;

        if (tFrame >= tiempoPorFrame)
        {
            tFrame = 0f;
            frame++;

            if (impactado && frame >= lista.Length)
            {
                Destroy(gameObject);
                return;
            }

            frame %= lista.Length;
        }

        sr.sprite = lista[
            Mathf.Min(frame, lista.Length - 1)
        ];
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (impactado)
            return;

        if (otro.isTrigger)
            return;

        if (dueno != null &&
            otro.transform.IsChildOf(dueno.transform))
            return;

        if (otro.GetComponentInParent<PlayerController>() != null)
            return;

        // Comprobar si el proyectil impacto un enemigo.
        var objetivo =
            otro.GetComponentInParent<IAturdible>();

        if (objetivo != null)
        {
            // Mantener el aturdimiento original.
            objetivo.Aturdir(aturdimiento);

            // NUEVO: reproducir el sonido
            // solamente cuando alcanza al enemigo.
            ReproducirSonidoImpactoZombie();
        }

        // Mantener el impacto visual original.
        Impactar();
    }

    void ReproducirSonidoImpactoZombie()
    {
        if (sonidoDanioZombie == null)
            return;

        // Crear un reproductor temporal.
        // No se agrega sonido permanente al zombi.
        GameObject objetoAudio = new GameObject(
            "Audio_Impacto_Zombie"
        );

        objetoAudio.transform.position =
            transform.position;

        AudioSource audio =
            objetoAudio.AddComponent<AudioSource>();

        audio.clip = sonidoDanioZombie;
        audio.volume = volumenDanioZombie;

        // Audio 2D para que se escuche correctamente.
        audio.spatialBlend = 0f;
        audio.outputAudioMixerGroup =
        ConfiguracionAudioGlobal.ObtenerGrupoEfectos();

        audio.playOnAwake = false;
        audio.loop = false;

        audio.Play();

        // Eliminar el reproductor cuando termine el audio.
        Destroy(
            objetoAudio,
            sonidoDanioZombie.length + 0.1f
        );
    }

    void Impactar()
    {
        impactado = true;

        frame = 0;
        tFrame = 0f;

        var c = GetComponent<Collider2D>();

        if (c != null)
            c.enabled = false;
    }
}
