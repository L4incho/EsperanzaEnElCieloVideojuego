
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuPrincipalController : MonoBehaviour
{
    [Header("Configuracion de niveles")]
    [SerializeField] private string nombrePrimerNivel;

    [Header("Panel de configuracion")]
    [SerializeField] private GameObject panelConfiguracion;
    [SerializeField] private ConfiguracionAplicable configuracion;


    [Header("Seleccion del menu")]
    [SerializeField] private GameObject botonJugar;
    [SerializeField] private GameObject botonOpciones;

    [Header("Panel de niveles")]
    [SerializeField] private GameObject panelNiveles;

    public void Jugar()
    {
        if (string.IsNullOrWhiteSpace(nombrePrimerNivel))
        {
            Debug.LogError("No se ha configurado el primer nivel.");
            return;
        }

        SceneManager.LoadScene(nombrePrimerNivel);
    }

    public void AbrirOpciones()
    {
        if (panelConfiguracion == null)
            return;

        if (configuracion != null)
            configuracion.PrepararCambios();

        panelConfiguracion.SetActive(true);

        // Restablecer siempre la pestaña SONIDO.
        ConfiguracionTabsUI tabs =
            panelConfiguracion.GetComponent<ConfiguracionTabsUI>();

        if (tabs != null)
            tabs.ReiniciarPestana();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void VolverMenuPrincipal()
    {
        if (panelConfiguracion == null)
            return;

        if (configuracion != null)
            configuracion.CancelarCambios();

        panelConfiguracion.SetActive(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (botonOpciones != null)
                EventSystem.current.SetSelectedGameObject(botonOpciones);
            else if (botonJugar != null)
                EventSystem.current.SetSelectedGameObject(botonJugar);
        }
    }

    public void Salir()
    {
        Debug.Log("Saliendo de Esperanza en el Cielo...");
        Application.Quit();
    }

    public void AbrirNiveles()
    {
        if (panelNiveles == null)
        {
            Debug.LogError("No se ha asignado Panel_Niveles.");
            return;
        }

        panelNiveles.SetActive(true);
    }

    public void VolverDesdeNiveles()
    {
        if (panelNiveles == null)
            return;

        panelNiveles.SetActive(false);
    }
}

