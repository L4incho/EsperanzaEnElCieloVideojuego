
using UnityEngine;
using UnityEngine.UI;

public class AccesoNuevoPerfil : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelJugar;
    [SerializeField] private GameObject panelContinuarPartida;

    [Header("Boton Nuevo Perfil")]
    [SerializeField] private Button botonNuevoPerfil;

    public void IntentarAbrirNuevoPerfil()
    {
        if (GestorGuardado.Instancia == null)
        {
            Debug.LogError(
                "No existe GestorGuardado."
            );
            return;
        }

        int cantidad =
            GestorGuardado.Instancia.ObtenerPerfiles().Count;

        // Impedir crear un cuarto perfil.
        if (cantidad >= GestorGuardado.MaximoPerfiles)
        {
            Debug.LogWarning(
                "Limite de perfiles alcanzado: " +
                cantidad + "/" +
                GestorGuardado.MaximoPerfiles
            );

            ActualizarBoton();
            return;
        }

        if (panelJugar == null ||
            panelContinuarPartida == null)
        {
            Debug.LogError(
                "Faltan referencias de los paneles."
            );
            return;
        }

        Debug.Log(
            "Abriendo creacion de perfil. " +
            "Perfiles existentes: " + cantidad
        );

        // Primero cerrar GUARDADOS DE PARTIDA.
        panelContinuarPartida.SetActive(false);

        // Despues abrir NUEVA PARTIDA.
        panelJugar.SetActive(true);
    }

    public void ActualizarBoton()
    {
        if (GestorGuardado.Instancia == null ||
            botonNuevoPerfil == null)
            return;

        int cantidad =
            GestorGuardado.Instancia.ObtenerPerfiles().Count;

        bool disponible =
            cantidad < GestorGuardado.MaximoPerfiles;

        // Utilizar BotonMenuVisual si existe para
        // actualizar tambien el sprite.
        BotonMenuVisual visual =
            botonNuevoPerfil.GetComponent<BotonMenuVisual>();

        if (visual != null)
        {
            visual.EstablecerHabilitado(disponible);
        }
        else
        {
            botonNuevoPerfil.interactable = disponible;
        }
    }
}
