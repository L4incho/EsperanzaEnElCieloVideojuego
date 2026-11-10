using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Permite usar el menu con el teclado:
//   W / S o flechas arriba/abajo = cambiar de boton (da la vuelta al llegar al final)
//   Enter o Espacio              = presionar el boton
//   Escape                       = volver al menu principal (si "escapeVuelveAlMenu" esta activo)
// Ponlo en el Canvas y arrastra los botones en el orden en que aparecen de arriba hacia abajo.
public class MenuTeclado : MonoBehaviour
{
    [Tooltip("Botones del menu, de arriba hacia abajo")]
    public Button[] botones;

    [Tooltip("Si esta activo, Escape regresa a la escena MenuPrincipal")]
    public bool escapeVuelveAlMenu = false;

    [Tooltip("Nombre de la escena a la que vuelve Escape (si esta vacio, se usa la opcion de arriba)")]
    public string escenaEscape = "";

    GameObject ultimoSeleccionado;

    void OnEnable()
    {
        StartCoroutine(SeleccionarPrimero());
    }

    IEnumerator SeleccionarPrimero()
    {
        yield return null; // espera un frame a que el EventSystem este listo
        if (EventSystem.current != null && botones != null && botones.Length > 0 && botones[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(botones[0].gameObject);
            ultimoSeleccionado = botones[0].gameObject;
        }
    }

    void Update()
    {
        var es = EventSystem.current;
        if (es == null) return;

        if (es.currentSelectedGameObject != null) ultimoSeleccionado = es.currentSelectedGameObject;

        // Si se hizo clic en un lugar vacio, se pierde la seleccion: al usar el teclado se recupera
        bool teclaNavegar = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)
                         || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);
        if (es.currentSelectedGameObject == null && teclaNavegar && ultimoSeleccionado != null)
            es.SetSelectedGameObject(ultimoSeleccionado);

        // Espacio tambien presiona el boton (Enter ya lo hace Unity por defecto)
        if (Input.GetKeyDown(KeyCode.Space) && !Input.GetButtonDown("Submit") && es.currentSelectedGameObject != null)
            ExecuteEvents.Execute(es.currentSelectedGameObject, new BaseEventData(es), ExecuteEvents.submitHandler);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string destino = !string.IsNullOrEmpty(escenaEscape) ? escenaEscape : (escapeVuelveAlMenu ? "MenuPrincipal" : null);
            if (!string.IsNullOrEmpty(destino)) SceneManager.LoadScene(destino);
        }
    }
}
