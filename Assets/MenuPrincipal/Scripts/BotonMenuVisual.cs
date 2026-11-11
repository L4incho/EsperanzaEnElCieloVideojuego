
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class BotonMenuVisual : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("Sprites del boton")]
    public Sprite spriteNormal;
    public Sprite spriteSeleccionado;
    public Sprite spriteDeshabilitado;

    [Header("Configuracion inicial")]
    public bool seleccionadoInicialmente = false;
    public bool deshabilitadoInicialmente = false;

    private Image imagen;
    private Button boton;

    private bool cursorEncima;

    private void Awake()
    {
        InicializarComponentes();

        boton.transition = Selectable.Transition.None;
        boton.interactable = !deshabilitadoInicialmente;

        ActualizarImagen();
    }

    private void InicializarComponentes()
    {
        if (imagen == null)
            imagen = GetComponent<Image>();

        if (boton == null)
            boton = GetComponent<Button>();
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

        ActualizarImagen();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorEncima = true;
        ActualizarImagen();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cursorEncima = false;
        ActualizarImagen();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ActualizarImagen();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ActualizarImagen();
    }

    private void ActualizarImagen()
    {
        InicializarComponentes();

        if (imagen == null || boton == null)
            return;

        // Prioridad 1: mostrar el sprite deshabilitado.
        if (!boton.interactable)
        {
            if (spriteDeshabilitado != null)
                imagen.sprite = spriteDeshabilitado;

            return;
        }

        // Prioridad 2: resaltar solo mientras
        // el cursor este encima del boton.
        if (cursorEncima)
        {
            if (spriteSeleccionado != null)
                imagen.sprite = spriteSeleccionado;
        }
        else
        {
            if (spriteNormal != null)
                imagen.sprite = spriteNormal;
        }
    }

    public void EstablecerHabilitado(bool habilitado)
    {
        InicializarComponentes();

        if (boton == null || imagen == null)
            return;

        boton.interactable = habilitado;

        if (!habilitado)
        {
            cursorEncima = false;

            if (EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject ==
                gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        ActualizarImagen();
    }

    private void OnDisable()
    {
        cursorEncima = false;
    }
}
