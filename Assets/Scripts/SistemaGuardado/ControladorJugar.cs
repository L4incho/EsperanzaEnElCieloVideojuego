
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorJugar : MonoBehaviour
{
    [Header("Nombre del jugador")]
    [SerializeField] private TMP_InputField nombrePerfil;

    [Header("Escenas")]
    [SerializeField] private string escenaInicial = "Tutorial";
    [SerializeField] private string escenaCinematica = "CinematicaIntro";

    public void CrearPartida()
    {
        if (nombrePerfil == null)
        {
            Debug.LogError(
                "ControladorJugar: falta asignar Nombre_Perfil."
            );
            return;
        }

        string nombre = nombrePerfil.text.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            Debug.LogWarning(
                "Escribe un nombre para crear la partida."
            );
            return;
        }

        if (GestorGuardado.Instancia == null)
        {
            Debug.LogError(
                "No se encontro el sistema de guardado."
            );
            return;
        }

        // Comprobar las escenas antes de crear el perfil.
        if (!Application.CanStreamedLevelBeLoaded(escenaInicial))
        {
            Debug.LogError(
                "No se encuentra la escena: " + escenaInicial
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(escenaCinematica))
        {
            Debug.LogError(
                "No se encuentra la cinematica: " + escenaCinematica
            );
            return;
        }

        // Crear el perfil normalmente.
        bool creado =
            GestorGuardado.Instancia.CrearPerfil(nombre);

        if (!creado)
        {
            Debug.LogError(
                "No se pudo crear el perfil."
            );
            return;
        }

        Debug.Log(
            "Nueva partida creada para: " + nombre
        );

        // Un perfil nuevo debe ver la introduccion.
        if (GestorGuardado.Instancia.DebeMostrarIntro())
        {
            Debug.Log(
                "Iniciando cinematica del nuevo perfil."
            );

            SceneManager.LoadScene(escenaCinematica);
        }
        else
        {
            SceneManager.LoadScene(escenaInicial);
        }
    }
}
