using UnityEngine;
using UnityEngine.EventSystems;

// Agranda un poco el boton cuando esta seleccionado (con teclado o con el mouse encima).
public class EfectoSeleccion : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public float escalaSeleccionado = 1.07f;
    public float velocidad = 12f;

    Vector3 escalaBase;
    float objetivo = 1f;
    float actual = 1f;

    void Awake() { escalaBase = transform.localScale; }

    void OnEnable()
    {
        actual = objetivo = 1f;
        transform.localScale = escalaBase;
    }

    public void OnSelect(BaseEventData eventData) { objetivo = escalaSeleccionado; }
    public void OnDeselect(BaseEventData eventData) { objetivo = 1f; }

    void Update()
    {
        actual = Mathf.Lerp(actual, objetivo, 1f - Mathf.Exp(-velocidad * Time.unscaledDeltaTime));
        transform.localScale = escalaBase * actual;
    }
}
