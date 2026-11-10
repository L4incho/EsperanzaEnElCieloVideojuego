using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Puerta de nivel: Mateo se para frente a ella, presiona W y entra (camina al centro, se desvanece dentro de la puerta,
// la pantalla se va a negro) y se carga la escena indicada.
[RequireComponent(typeof(Collider2D))]
public class Puerta : MonoBehaviour
{
    public string escenaDestino = "Nivel2";
    public KeyCode tecla = KeyCode.W;
    public KeyCode teclaAlterna = KeyCode.UpArrow;
    public float segundosCaminar = 0.3f;
    public float segundosEntrar = 0.45f;
    public float segundosFundido = 0.45f;
    [Tooltip("Altura sobre el centro de la puerta donde aparece la letra W")]
    public float alturaIndicador = 0.2f;

    PlayerController mateo;
    bool dentro, entrando;
    TextMesh indicador;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        CrearIndicador();
    }

    void CrearIndicador()
    {
        var g = new GameObject("Indicador_W");
        g.transform.SetParent(transform, false);
        var col = GetComponent<Collider2D>();
        g.transform.position = new Vector3(col.bounds.center.x, col.bounds.max.y + alturaIndicador, 0f);
        indicador = g.AddComponent<TextMesh>();
        var fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        indicador.font = fuente;
        indicador.text = "W";
        indicador.fontSize = 64;
        indicador.characterSize = 0.06f;
        indicador.anchor = TextAnchor.LowerCenter;
        indicador.alignment = TextAlignment.Center;
        indicador.fontStyle = FontStyle.Bold;
        indicador.color = new Color(0.55f, 1f, 0.95f);
        var mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = fuente.material;
        mr.sortingOrder = 60;
        g.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D o)
    {
        var m = o.GetComponentInParent<PlayerController>();
        if (m == null || entrando) return;
        mateo = m; dentro = true;
        if (indicador != null) indicador.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D o)
    {
        if (o.GetComponentInParent<PlayerController>() == null) return;
        dentro = false;
        if (indicador != null) indicador.gameObject.SetActive(false);
    }

    void Update()
    {
        if (indicador != null && indicador.gameObject.activeSelf)
            indicador.transform.localPosition += Vector3.zero; // (sin animacion extra)
        if (!dentro || entrando || mateo == null || mateo.EstaMuerto) return;
        if (Input.GetKeyDown(tecla) || Input.GetKeyDown(teclaAlterna)) Entrar();
    }

    /// <summary>Inicia la entrada (la usa Update al presionar W; tambien sirve para pruebas).</summary>
    public void Entrar()
    {
        if (entrando) return;
        if (mateo == null) mateo = FindFirstObjectByType<PlayerController>();
        if (mateo == null) return;
        entrando = true;
        if (indicador != null) indicador.gameObject.SetActive(false);
        StartCoroutine(Rutina());
    }

    IEnumerator Rutina()
    {
        // congelar a Mateo
        mateo.enabled = false;
        var rb = mateo.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
        var anim = mateo.GetComponent<Animator>();
        if (anim != null) { anim.Rebind(); anim.Update(0f); }
        var sr = mateo.GetComponent<SpriteRenderer>();

        // caminar al centro de la puerta
        Vector3 ini = mateo.transform.position;
        float xDestino = GetComponent<Collider2D>().bounds.center.x;
        for (float t = 0; t < segundosCaminar; t += Time.deltaTime)
        {
            float k = t / segundosCaminar;
            Vector3 p = mateo.transform.position; p.x = Mathf.Lerp(ini.x, xDestino, k);
            mateo.transform.position = p;
            yield return null;
        }

        // entrar: se desvanece y se achica un poco, como si pasara al interior
        Vector3 esc = mateo.transform.localScale;
        for (float t = 0; t < segundosEntrar; t += Time.deltaTime)
        {
            float k = t / segundosEntrar;
            mateo.transform.localScale = esc * (1f - 0.25f * k);
            if (sr != null) { var c = sr.color; c.a = 1f - k; sr.color = c; }
            yield return null;
        }
        if (sr != null) sr.enabled = false;

        // pantalla a negro y cambio de escena
        var fondo = CrearFundido();
        for (float t = 0; t < segundosFundido; t += Time.deltaTime)
        {
            fondo.color = new Color(0, 0, 0, t / segundosFundido);
            yield return null;
        }
        fondo.color = Color.black;
        SceneManager.LoadScene(escenaDestino);
    }

    static Image CrearFundido()
    {
        var g = new GameObject("FundidoPuerta");
        var c = g.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 500;
        var im = new GameObject("Negro").AddComponent<Image>();
        im.transform.SetParent(g.transform, false);
        im.rectTransform.anchorMin = Vector2.zero; im.rectTransform.anchorMax = Vector2.one;
        im.rectTransform.offsetMin = im.rectTransform.offsetMax = Vector2.zero;
        im.color = new Color(0, 0, 0, 0);
        im.raycastTarget = false;
        return im;
    }
}
