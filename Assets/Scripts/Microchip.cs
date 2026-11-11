
using System.Collections;
using UnityEngine;

// Contador global de microchips.
// Conserva los chips confirmados y los nuevos del intento.
public static class Microchips
{
    public static int Cantidad;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Cantidad = 0;
    }
}

[RequireComponent(typeof(Collider2D))]
public class Microchip : MonoBehaviour
{
    [Header("Identificacion unica")]
    [SerializeField] private string idUnico;

    public string IdUnico => idUnico;

    [Header("Animacion")]
    public Sprite[] cuadros;
    public float cuadrosPorSegundo = 8f;

    [Header("Movimiento flotante")]
    public float amplitudVaiven = 0.1f;
    public float velocidadVaiven = 2.5f;

    [Header("Sonido")]
    public AudioClip sonido;

    private SpriteRenderer sr;
    private Transform visual;
    private bool tomado;
    private float fase;
    private Vector3 baseLocal;

#if UNITY_EDITOR
    public void AsignarIdEditor(string nuevoId)
    {
        idUnico = nuevoId;
    }
#endif

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        visual = sr != null ? sr.transform : transform;

        if (visual != transform &&
            visual.localPosition.sqrMagnitude > 0.0001f)
        {
            Vector3 mundo = visual.position;

            transform.position = mundo;
            visual.localPosition = Vector3.zero;
        }

        baseLocal = visual.localPosition;

        GetComponent<Collider2D>().isTrigger = true;

        fase = Random.value * 10f;
    }

    private void Update()
    {
        if (tomado || sr == null ||
            cuadros == null || cuadros.Length == 0)
        {
            return;
        }

        int n = cuadros.Length;
        int ciclo = Mathf.Max(1, 2 * n - 2);

        int i = Mathf.FloorToInt(
            (Time.time + fase) * cuadrosPorSegundo
        ) % ciclo;

        sr.sprite = cuadros[i < n ? i : ciclo - i];

        float movimiento = Mathf.Sin(
            (Time.time + fase) * velocidadVaiven
        ) * amplitudVaiven;

        visual.localPosition =
            baseLocal + new Vector3(0f, movimiento, 0f);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (tomado ||
            otro.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        GestorGuardado gestor = GestorGuardado.Instancia;

        if (gestor == null || gestor.PerfilActivo == null)
        {
            Debug.LogWarning(
                "No se pudo recoger el microchip: " +
                "no existe un perfil activo."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(idUnico))
        {
            Debug.LogWarning(
                "Microchip sin ID: " + gameObject.name
            );
            return;
        }

        tomado = true;
        GetComponent<Collider2D>().enabled = false;

        // Solamente un microchip nuevo suma puntuacion.
        bool esNuevo = gestor.RegistrarMicrochipTemporal(idUnico);

        if (esNuevo)
        {
            Microchips.Cantidad++;

            Debug.Log(
                "Microchip NUEVO: " + idUnico +
                " | Contador: " + Microchips.Cantidad
            );
        }
        else
        {
            Debug.Log(
                "Microchip REPETIDO: " + idUnico +
                " | No suma puntos."
            );
        }

        if (sonido != null)
        {
            AudioSource.PlayClipAtPoint(
                sonido,
                transform.position
            );
        }

        StartCoroutine(Desvanecer());
    }

    private IEnumerator Desvanecer()
    {
        float t = 0f;
        float d = 0.25f;

        Vector3 escalaOriginal = visual.localScale;

        while (t < d)
        {
            t += Time.deltaTime;

            float k = Mathf.Clamp01(t / d);

            visual.localScale =
                escalaOriginal * (1f + 0.6f * k);

            visual.localPosition +=
                Vector3.up * Time.deltaTime * 2f;

            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f - k;
                sr.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
