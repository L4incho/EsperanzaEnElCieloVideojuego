
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorJugar : MonoBehaviour
{
    [Header("Nombre del jugador")]
    [SerializeField] private TMP_InputField nombrePerfil;

    [Header("Escena inicial")]
    [SerializeField] private string escenaInicial = "Tutorial";

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

        // Verificar que la escena exista antes de crear el perfil.
        if (Application.CanStreamedLevelBeLoaded(escenaInicial) == false)
        {
            Debug.LogError(
                "La escena inicial no esta incluida en Build Profiles: "
                + escenaInicial
            );
            return;
        }

        bool creado = GestorGuardado.Instancia.CrearPerfil(nombre);

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

        SceneManager.LoadScene(escenaInicial);
    }
}
