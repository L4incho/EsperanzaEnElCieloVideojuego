using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Efectos de las gemas hechos con sprites en pixel art (mismo estilo que las gemas del juego).
//   Impulso = particulas pixeladas que se desprenden del cuerpo de Mateo al usar el doble salto.
//   Recoger = estallido pixelado con anillo y destellos al tomar cualquier gema.
// Reproducir(color) elige la paleta (verde, azul o roja) mas parecida al color de la gema.
public class EfectoGema : MonoBehaviour
{
    public enum Modo { Impulso, Recoger }

    [System.Serializable]
    public class Paleta
    {
        public string nombre;
        public Color referencia = Color.green;
        [Tooltip("8 cuadros: 0-3 familia cruz (grande a pequeño), 4-7 familia cuadro")] public Sprite[] chispas;
        public Sprite[] estallido;
    }

    class Chispa
    {
        public Transform t; public SpriteRenderer sr;
        public Vector2 vel; public float vida, edad; public int familia; public Sprite[] frames;
    }

    public Modo modo = Modo.Impulso;
    public Paleta[] paletas;
    [Tooltip("Cuadros por segundo del estallido")] public float fps = 16f;
    [Tooltip("Si es mayor que 0, este objeto se destruye solo pasado este tiempo (efectos de un solo uso)")]
    public float destruirDespues = 0f;
    public int ordenDibujo = 20;
    public float escalaEstallido = 4f;

    [Header("Particulas del doble salto")]
    [Tooltip("Tamano de las chispas (cada pixel del arte = 0.01 x este valor)")] public float escalaChispa = 4f;
    public int chispasAlInicio = 24;
    [Tooltip("Chispas por segundo mientras dura la emision")] public float chispasPorSegundo = 80f;
    public float duracionEmision = 0.3f;
    [Tooltip("Altura del centro del cuerpo respecto a los pies")] public float centroCuerpo = 0.85f;
    public Vector2 vidaChispa = new Vector2(0.35f, 0.7f);

    readonly List<Chispa> chispas = new List<Chispa>();
    Paleta paletaActual;
    float emisionRestante, acumulado;

    public void Reproducir(Color color)
    {
        var p = Elegir(color);
        if (p == null) return;
        if (modo == Modo.Impulso) Impulso(p); else Recoger(p);
        if (destruirDespues > 0f) Destroy(gameObject, destruirDespues);
    }

    Paleta Elegir(Color c)
    {
        if (paletas == null || paletas.Length == 0) return null;
        Paleta mejor = paletas[0];
        float dm = float.MaxValue;
        foreach (var p in paletas)
        {
            float d = Mathf.Pow(p.referencia.r - c.r, 2) + Mathf.Pow(p.referencia.g - c.g, 2) + Mathf.Pow(p.referencia.b - c.b, 2);
            if (d < dm) { dm = d; mejor = p; }
        }
        return mejor;
    }

    // ---------- Doble salto: chispas que se desprenden del cuerpo ----------
    void Impulso(Paleta p)
    {
        if (p.chispas == null || p.chispas.Length < 8) return;
        paletaActual = p;
        for (int i = 0; i < chispasAlInicio; i++) NuevaChispa();
        emisionRestante = duracionEmision;
        acumulado = 0f;
    }

    void NuevaChispa()
    {
        var go = new GameObject("Chispa_Pixel");
        float x = Random.Range(-0.3f, 0.3f);
        float y = Random.Range(-0.65f, 0.6f);
        go.transform.position = transform.position + new Vector3(x, centroCuerpo + y, 0f);
        go.transform.localScale = Vector3.one * escalaChispa;
        var sr = go.AddComponent<SpriteRenderer>();
        // algunas quedan detras de Mateo para dar profundidad
        sr.sortingOrder = Random.value < 0.35f ? 9 : ordenDibujo;
        var c = new Chispa
        {
            t = go.transform, sr = sr, frames = paletaActual.chispas,
            familia = Random.value < 0.5f ? 0 : 4,
            vida = Random.Range(vidaChispa.x, vidaChispa.y),
            // salen hacia los lados y un poco hacia abajo, como si el cuerpo las soltara al impulsarse
            vel = new Vector2(Mathf.Sign(x == 0f ? 1f : x) * Random.Range(0.3f, 1.6f) + Random.Range(-0.3f, 0.3f), Random.Range(-1.8f, 0.3f))
        };
        sr.sprite = c.frames[c.familia];
        chispas.Add(c);
    }

    void Update()
    {
        if (emisionRestante > 0f)
        {
            emisionRestante -= Time.deltaTime;
            acumulado += Time.deltaTime * chispasPorSegundo;
            while (acumulado >= 1f) { acumulado -= 1f; NuevaChispa(); }
        }

        for (int i = chispas.Count - 1; i >= 0; i--)
        {
            var c = chispas[i];
            c.edad += Time.deltaTime;
            float k = c.edad / c.vida;
            if (k >= 1f || c.t == null)
            {
                if (c.t != null) Destroy(c.t.gameObject);
                chispas.RemoveAt(i);
                continue;
            }
            c.vel *= Mathf.Max(0f, 1f - 2.2f * Time.deltaTime);   // frenan poco a poco
            c.t.position += (Vector3)(c.vel * Time.deltaTime);
            // el sprite pasa de grande a pequeño (y de claro a oscuro) como en un sprite sheet
            int idx = Mathf.Min(3, Mathf.FloorToInt(k * 4f));
            c.sr.sprite = c.frames[c.familia + idx];
        }
    }

    void OnDestroy()
    {
        foreach (var c in chispas) if (c.t != null) Destroy(c.t.gameObject);
    }

    // ---------- Tomar una gema: estallido pixelado ----------
    void Recoger(Paleta p)
    {
        if (p.estallido == null || p.estallido.Length == 0) return;
        var go = new GameObject("Animacion_Pixel");
        go.transform.SetParent(transform, false);
        float padre = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        go.transform.localScale = Vector3.one * (escalaEstallido / padre);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = ordenDibujo;
        sr.sprite = p.estallido[0];
        StartCoroutine(Animar(sr, p.estallido));
    }

    IEnumerator Animar(SpriteRenderer sr, Sprite[] frames)
    {
        float t = 1f / Mathf.Max(1f, fps);
        for (int i = 0; i < frames.Length; i++)
        {
            if (sr == null) yield break;
            sr.sprite = frames[i];
            yield return new WaitForSeconds(t);
        }
        if (sr != null) Destroy(sr.gameObject);
    }
}
