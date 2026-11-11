
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Puerta : MonoBehaviour
{
    [Header("Cambio de nivel")]
    public string escenaDestino = "Nivel2";

    [Tooltip("Numero del nivel que Mateo esta terminando.")]
    public int numeroNivelActual = 1;

    [Header("Controles")]
    public KeyCode tecla = KeyCode.W;
    public KeyCode teclaAlterna = KeyCode.UpArrow;

    [Header("Animacion")]
    public float segundosCaminar = 0.3f;
    public float segundosEntrar = 0.45f;
    public float segundosFundido = 0.45f;

    [Tooltip("Altura de la letra W sobre la puerta.")]
    public float alturaIndicador = 0.2f;

    private PlayerController mateo;
    private bool dentro;
    private bool entrando;
    private TextMesh indicador;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        CrearIndicador();
    }

    private void CrearIndicador()
    {
        GameObject g = new GameObject("Indicador_W");
        g.transform.SetParent(transform, false);

        Collider2D col = GetComponent<Collider2D>();

        g.transform.position = new Vector3(
            col.bounds.center.x,
            col.bounds.max.y + alturaIndicador,
            0f
        );

        indicador = g.AddComponent<TextMesh>();

        Font fuente = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );

        indicador.font = fuente;
        indicador.text = "W";
        indicador.fontSize = 64;
        indicador.characterSize = 0.06f;
        indicador.anchor = TextAnchor.LowerCenter;
        indicador.alignment = TextAlignment.Center;
        indicador.fontStyle = FontStyle.Bold;
        indicador.color = new Color(0.55f, 1f, 0.95f);

        MeshRenderer mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = fuente.material;
        mr.sortingOrder = 60;

        g.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        PlayerController m =
            otro.GetComponentInParent<PlayerController>();

        if (m == null || entrando)
            return;

        mateo = m;
        dentro = true;

        if (indicador != null)
            indicador.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.GetComponentInParent<PlayerController>() == null)
            return;

        dentro = false;

        if (indicador != null)
            indicador.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!dentro ||
            entrando ||
            mateo == null ||
            mateo.EstaMuerto)
            return;

        if (Input.GetKeyDown(tecla) ||
            Input.GetKeyDown(teclaAlterna))
        {
            Entrar();
        }
    }

    public void Entrar()
    {
        if (entrando)
            return;

        if (mateo == null)
            mateo = FindFirstObjectByType<PlayerController>();

        if (mateo == null)
            return;

        if (GestorGuardado.Instancia == null ||
            GestorGuardado.Instancia.PerfilActivo == null)
        {
            Debug.LogError(
                "Puerta: No existe un perfil activo."
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            Debug.LogError(
                "Puerta: Escena no disponible: " +
                escenaDestino
            );
            return;
        }

        entrando = true;

        if (indicador != null)
            indicador.gameObject.SetActive(false);

        StartCoroutine(Rutina());
    }

    private IEnumerator Rutina()
    {
        // Congelar controles de Mateo.
        mateo.enabled = false;

        Rigidbody2D rb =
            mateo.GetComponent<Rigidbody2D>();

        RigidbodyType2D tipoOriginal =
            rb != null
            ? rb.bodyType
            : RigidbodyType2D.Dynamic;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Animator anim = mateo.GetComponent<Animator>();

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        SpriteRenderer sr =
            mateo.GetComponent<SpriteRenderer>();

        Vector3 posicionOriginal = mateo.transform.position;
        Vector3 escalaOriginal = mateo.transform.localScale;

        // Caminar hacia el centro de la puerta.
        float xDestino =
            GetComponent<Collider2D>().bounds.center.x;

        float duracionCaminar =
            Mathf.Max(0.01f, segundosCaminar);

        for (float t = 0f;
             t < duracionCaminar;
             t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / duracionCaminar);

            Vector3 p = mateo.transform.position;
            p.x = Mathf.Lerp(
                posicionOriginal.x,
                xDestino,
                k
            );

            mateo.transform.position = p;

            yield return null;
        }

        // Animacion de entrada a la puerta.
        float duracionEntrar =
            Mathf.Max(0.01f, segundosEntrar);

        for (float t = 0f;
             t < duracionEntrar;
             t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / duracionEntrar);

            mateo.transform.localScale =
                escalaOriginal * (1f - 0.25f * k);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - k;
                sr.color = c;
            }

            yield return null;
        }

        if (sr != null)
            sr.enabled = false;

        // Fundido de pantalla.
        Image fondo = CrearFundido();

        float duracionFundido =
            Mathf.Max(0.01f, segundosFundido);

        for (float t = 0f;
             t < duracionFundido;
             t += Time.deltaTime)
        {
            fondo.color = new Color(
                0f,
                0f,
                0f,
                Mathf.Clamp01(t / duracionFundido)
            );

            yield return null;
        }

        fondo.color = Color.black;

        // CONFIRMAR PROGRESO Y MICROCHIPS UNICOS.
        GestorGuardado gestor = GestorGuardado.Instancia;

        bool guardado =
            gestor != null &&
            gestor.ConfirmarNivelCompletado(
                numeroNivelActual,
                Microchips.Cantidad
            );

        if (!guardado)
        {
            Debug.LogError(
                "Puerta: Fallo el guardado. " +
                "No se cambiara de escena."
            );

            RestaurarTrasError(
                rb,
                tipoOriginal,
                anim,
                sr,
                posicionOriginal,
                escalaOriginal,
                fondo
            );

            yield break;
        }

        Debug.Log(
            "Puerta: Nivel " + numeroNivelActual +
            " completado. Chips confirmados: " +
            gestor.PerfilActivo.chipsGuardados
        );

        SceneManager.LoadScene(escenaDestino);
    }

    private void RestaurarTrasError(
        Rigidbody2D rb,
        RigidbodyType2D tipoOriginal,
        Animator anim,
        SpriteRenderer sr,
        Vector3 posicionOriginal,
        Vector3 escalaOriginal,
        Image fondo)
    {
        if (fondo != null)
            Destroy(fondo.transform.parent.gameObject);

        mateo.transform.position = posicionOriginal;
        mateo.transform.localScale = escalaOriginal;

        if (sr != null)
        {
            sr.enabled = true;

            Color color = sr.color;
            color.a = 1f;
            sr.color = color;
        }

        if (rb != null)
        {
            rb.bodyType = tipoOriginal;
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        mateo.enabled = true;
        entrando = false;

        if (indicador != null && dentro)
            indicador.gameObject.SetActive(true);
    }

    private static Image CrearFundido()
    {
        GameObject g = new GameObject("FundidoPuerta");

        Canvas canvas = g.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        Image imagen =
            new GameObject("Negro").AddComponent<Image>();

        imagen.transform.SetParent(g.transform, false);

        imagen.rectTransform.anchorMin = Vector2.zero;
        imagen.rectTransform.anchorMax = Vector2.one;

        imagen.rectTransform.offsetMin = Vector2.zero;
        imagen.rectTransform.offsetMax = Vector2.zero;

        imagen.color = new Color(0f, 0f, 0f, 0f);
        imagen.raycastTarget = false;

        return imagen;
    }
}
