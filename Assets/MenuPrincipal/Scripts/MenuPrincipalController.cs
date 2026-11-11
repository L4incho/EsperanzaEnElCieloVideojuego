
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuPrincipalController : MonoBehaviour
{
    [Header("Escenas del videojuego")]
    [SerializeField] private string nombrePrimerNivel = "Tutorial";
    [SerializeField] private string escenaNivel2 = "Nivel2";
    [SerializeField] private string escenaNivel3 = "Nivel3";
    [SerializeField] private string escenaNivel4 = "Nivel4Jefe";

    [Header("Cinematica inicial")]
    [SerializeField] private string escenaCinematica = "CinematicaIntro";

    [Header("Panel de configuracion")]
    [SerializeField] private GameObject panelConfiguracion;
    [SerializeField] private ConfiguracionAplicable configuracion;

    [Header("Seleccion del menu")]
    [SerializeField] private GameObject botonJugar;
    [SerializeField] private GameObject botonOpciones;

    [Header("Panel de crear partida")]
    [SerializeField] private GameObject panelJugar;

    [Header("Panel de niveles")]
    [SerializeField] private GameObject panelNiveles;

    [Header("Continuar partida")]
    [SerializeField] private GameObject panelContinuarPartida;
    [SerializeField] private BotonMenuVisual botonContinuarVisual;

    [Header("Bloqueo automatico de niveles")]
    [SerializeField] private BotonMenuVisual botonNivelesVisual;

    private bool panelContinuarEstabaAbierto;

    private void Start()
    {
        panelContinuarEstabaAbierto =
            panelContinuarPartida != null &&
            panelContinuarPartida.activeSelf;

        ActualizarBotonesGuardado();
    }

    private void Update()
    {
        if (panelContinuarPartida == null)
            return;

        bool panelAbierto = panelContinuarPartida.activeSelf;

        if (panelContinuarEstabaAbierto && !panelAbierto)
            ActualizarBotonesGuardado();

        panelContinuarEstabaAbierto = panelAbierto;
    }

    public void ActualizarBotonContinuar()
    {
        ActualizarBotonesGuardado();
    }

    public void ActualizarBotonesGuardado()
    {
        int cantidadPerfiles = 0;

        if (GestorGuardado.Instancia != null)
        {
            cantidadPerfiles =
                GestorGuardado.Instancia.ObtenerPerfiles().Count;
        }

        bool hayPartidas = cantidadPerfiles > 0;

        if (botonContinuarVisual != null)
            botonContinuarVisual.EstablecerHabilitado(hayPartidas);

        if (botonNivelesVisual != null)
            botonNivelesVisual.EstablecerHabilitado(hayPartidas);
    }

    public void Jugar()
    {
        if (GestorGuardado.Instancia == null)
        {
            Debug.LogError("No existe GestorGuardado.");
            return;
        }

        int cantidad =
            GestorGuardado.Instancia.ObtenerPerfiles().Count;

        if (cantidad == 0)
        {
            if (panelJugar == null)
            {
                Debug.LogError("No se asigno Panel_Jugar.");
                return;
            }

            panelJugar.SetActive(true);
            Debug.Log("Abriendo creacion del primer perfil.");
            return;
        }

        DatosPerfil perfil =
            GestorGuardado.Instancia.PerfilActivo;

        if (perfil == null)
        {
            Debug.LogWarning(
                "Selecciona una partida desde CONTINUAR PARTIDA."
            );

            if (panelContinuarPartida != null)
                panelContinuarPartida.SetActive(true);

            return;
        }

        // NUEVO: introduccion solo para nivel 1
        // cuando este perfil todavia no la ha visto.
        if (perfil.nivelActual == 1 &&
            GestorGuardado.Instancia.DebeMostrarIntro())
        {
            Debug.Log(
    "PRUEBA INTRO | Perfil: " + perfil.nombrePerfil +
    " | Nivel: " + perfil.nivelActual +
    " | Intro vista: " + perfil.introVista +
    " | Debe mostrar: " +
    GestorGuardado.Instancia.DebeMostrarIntro()
);
            if (!Application.CanStreamedLevelBeLoaded(escenaCinematica))
            {
                Debug.LogError(
                    "No se puede cargar CinematicaIntro. " +
                    "Revisa Build Profiles."
                );
                return;
            }

            Debug.Log(
                "Mostrando introduccion del perfil: " +
                perfil.nombrePerfil
            );

            SceneManager.LoadScene(escenaCinematica);
            return;
        }

        string escena = ObtenerEscena(perfil.nivelActual);

        if (string.IsNullOrWhiteSpace(escena) ||
            !Application.CanStreamedLevelBeLoaded(escena))
        {
            Debug.LogError(
                "No se puede cargar el nivel " +
                perfil.nivelActual +
                ". Revisa Build Profiles."
            );
            return;
        }

        Debug.Log(
            "JUGANDO | Perfil: " + perfil.nombrePerfil +
            " | Nivel: " + perfil.nivelActual +
            " | Chips: " + perfil.chipsGuardados
        );

        SceneManager.LoadScene(escena);
    }

    private string ObtenerEscena(int nivel)
    {
        switch (nivel)
        {
            case 1: return nombrePrimerNivel;
            case 2: return escenaNivel2;
            case 3: return escenaNivel3;
            case 4: return escenaNivel4;
            default: return null;
        }
    }

    public void AbrirNiveles()
    {
        if (panelNiveles == null)
        {
            Debug.LogError("No se asigno Panel_Niveles.");
            return;
        }

        if (GestorGuardado.Instancia == null ||
            GestorGuardado.Instancia.ObtenerPerfiles().Count == 0)
        {
            Debug.LogWarning(
                "No existen perfiles. Primero crea una partida."
            );

            ActualizarBotonesGuardado();
            return;
        }

        if (GestorGuardado.Instancia.PerfilActivo == null)
        {
            Debug.LogWarning(
                "Selecciona un perfil desde CONTINUAR PARTIDA."
            );

            if (panelContinuarPartida != null)
                panelContinuarPartida.SetActive(true);

            return;
        }

        panelNiveles.SetActive(true);

        SelectorNivelesMenu selector =
            panelNiveles.GetComponent<SelectorNivelesMenu>();

        if (selector != null)
            selector.ActualizarEstadoNiveles();
    }

    public void VolverDesdeNiveles()
    {
        if (panelNiveles != null)
            panelNiveles.SetActive(false);
    }

    public void AbrirOpciones()
    {
        if (panelConfiguracion == null)
            return;

        if (configuracion != null)
            configuracion.PrepararCambios();

        panelConfiguracion.SetActive(true);

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
}
