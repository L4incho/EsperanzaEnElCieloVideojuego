using UnityEngine;
using UnityEngine.EventSystems;

// Hace que el boton se resalte tambien cuando el mouse pasa por encima,
// para que mouse y teclado muestren siempre el mismo boton seleccionado.
public class BotonResaltar : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
