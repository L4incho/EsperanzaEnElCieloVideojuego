using System.Collections;
using UnityEngine;

// Pinchos que se activan con presion: estan casi escondidos en el piso; cuando Mateo se acerca (entra al sensor)
// suben despues de un instante y matan al tocarlos, luego se vuelven a esconder.
// Estructura: objeto raiz con BoxCollider2D trigger (sensor) + hijo "Pinchos" con SpriteRenderer, BoxCollider2D trigger y ZonaMortal.
public class PinchosPresion : MonoBehaviour
{
    public Transform pinchos;
    public Collider2D zonaMortal;
    public float alturaVisible = 0.35f;   // cuanto se asoman escondidos
    public float retraso = 0.35f;
    public float tiempoArriba = 1.6f;

    float alturaPinchos;
    Vector3 posEscondido, posArriba;
    bool ocupado;

    void Start()
    {
        var sr = pinchos.GetComponent<SpriteRenderer>();
        alturaPinchos = sr != null ? sr.bounds.size.y : 1f;
        posArriba = pinchos.localPosition;
        posEscondido = posArriba + Vector3.down * Mathf.Max(0f, alturaPinchos - alturaVisible);
        pinchos.localPosition = posEscondido;
        if (zonaMortal != null) zonaMortal.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D o)
    {
        if (ocupado || o.GetComponentInParent<PlayerController>() == null) return;
        StartCoroutine(Rutina());
    }

    IEnumerator Rutina()
    {
        ocupado = true;
        yield return new WaitForSeconds(retraso);
        yield return Mover(posEscondido, posArriba, 0.08f);
        if (zonaMortal != null) zonaMortal.enabled = true;
        yield return new WaitForSeconds(tiempoArriba);
        if (zonaMortal != null) zonaMortal.enabled = false;
        yield return Mover(posArriba, posEscondido, 0.3f);
        ocupado = false;
    }

    IEnumerator Mover(Vector3 a, Vector3 b, float dur)
    {
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; pinchos.localPosition = Vector3.Lerp(a, b, t / dur); yield return null; }
        pinchos.localPosition = b;
    }
}
