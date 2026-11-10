
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class BotonMenuVisual : MonoBehaviour,
    IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    [Header("Sprites del botón")]
    public Sprite spriteNormal;
    public Sprite spriteSeleccionado;
    public Sprite spriteDeshabilitado;

    [Header("Configuración inicial")]
    public bool seleccionadoInicialmente = false;
    public bool deshabilitadoInicialmente = false;

    private Image imagen;
    private Button boton;

    private void Awake()
    {
        imagen = GetComponent<Image>();
        boton = GetComponent<Button>();

        boton.transition = Selectable.Transition.None;

        boton.interactable = !deshabilitadoInicialmente;

        ActualizarImagen(false);
    }

    private IEnumerator Start()
    {
        yield return null;

        if (seleccionadoInicialmente &&
            boton.interactable &&
            EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!boton.interactable)
            return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        ActualizarImagen(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ActualizarImagen(false);
    }

    private void ActualizarImagen(bool seleccionado)
    {
        if (!boton.interactable)
        {
            if (spriteDeshabilitado != null)
                imagen.sprite = spriteDeshabilitado;

            return;
        }

        if (seleccionado && spriteSeleccionado != null)
        {
            imagen.sprite = spriteSeleccionado;
        }
        else if (spriteNormal != null)
        {
            imagen.sprite = spriteNormal;
        }
    }

    public void EstablecerHabilitado(bool habilitado)
    {
        boton.interactable = habilitado;

        bool seleccionado = EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == gameObject;

        if (!habilitado && seleccionado)
        {
            EventSystem.current.SetSelectedGameObject(null);
            seleccionado = false;
        }

        ActualizarImagen(seleccionado);
    }
}
