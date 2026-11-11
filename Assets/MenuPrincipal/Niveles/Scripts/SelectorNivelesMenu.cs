
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SelectorNivelesMenu : MonoBehaviour
{
    [Header("Nombres de las escenas")]
    [SerializeField] private string escenaLaboratorio = "Tutorial";
    [SerializeField] private string escenaCiudadInfectada = "Nivel2";
    [SerializeField] private string escenaTorreOrbital = "Nivel3";
    [SerializeField] private string escenaJefeFinal = "Nivel4Jefe";

    [Header("Botones visuales de niveles")]
    [SerializeField] private BotonMenuVisual btnNivel01Laboratorio;
    [SerializeField] private BotonMenuVisual btnNivel02Ciudad;
    [SerializeField] private BotonMenuVisual btnNivel03Torre;
    [SerializeField] private BotonMenuVisual btnNivel04JefeFinal;

    [Header("Boton volver")]
    [SerializeField] private GameObject btnVolver;

    private int ultimoNivelCompletado;
    private Coroutine rutinaInicializacion;

    private void OnEnable()
    {
        // Esperar a que los componentes de los botones
        // terminen de inicializarse.
        rutinaInicializacion =
            StartCoroutine(ActualizarAlIniciar());
    }

    private IEnumerator ActualizarAlIniciar()
    {
        yield return null;

        rutinaInicializacion = null;
        ActualizarEstadoNiveles();
    }

    private void OnDisable()
    {
        if (rutinaInicializacion != null)
        {
            StopCoroutine(rutinaInicializacion);
            rutinaInicializacion = null;
        }
    }

    public void ActualizarEstadoNiveles()
    {
        ultimoNivelCompletado = 0;

        if (GestorGuardado.Instancia != null &&
            GestorGuardado.Instancia.PerfilActivo != null)
        {
            DatosPerfil perfil =
                GestorGuardado.Instancia.PerfilActivo;

            ultimoNivelCompletado =
                Mathf.Clamp(perfil.ultimoNivelCompletado, 0, 4);

            Debug.Log(
                "PERFIL ACTIVO: " + perfil.nombrePerfil +
                " | ULTIMO NIVEL COMPLETADO: " +
                ultimoNivelCompletado
            );
        }
        else
        {
            Debug.Log(
                "No hay perfil activo. Solo Laboratorio disponible."
            );
        }

        // El nivel 1 siempre esta disponible.
        ConfigurarBoton(btnNivel01Laboratorio, true, 1);

        // Desbloqueo segun el progreso confirmado.
        ConfigurarBoton(
            btnNivel02Ciudad,
            ultimoNivelCompletado >= 1,
            2
        );

        ConfigurarBoton(
            btnNivel03Torre,
            ultimoNivelCompletado >= 2,
            3
        );

        ConfigurarBoton(
            btnNivel04JefeFinal,
            ultimoNivelCompletado >= 3,
            4
        );

        // Evitar que quede un nivel bloqueado seleccionado.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void ConfigurarBoton(
        BotonMenuVisual boton,
        bool habilitado,
        int numeroNivel)
    {
        if (boton == null)
        {
            Debug.LogError(
                "Falta asignar el boton del nivel " +
                numeroNivel + " en el Inspector."
            );
            return;
        }

        boton.EstablecerHabilitado(habilitado);

        Debug.Log(
            "NIVEL " + numeroNivel + ": " +
            (habilitado ? "DESBLOQUEADO" : "BLOQUEADO")
        );
    }

    public void JugarLaboratorio()
    {
        CargarNivelSiDesbloqueado(1, escenaLaboratorio);
    }

    public void JugarCiudadInfectada()
    {
        CargarNivelSiDesbloqueado(2, escenaCiudadInfectada);
    }

    public void JugarTorreOrbital()
    {
        CargarNivelSiDesbloqueado(3, escenaTorreOrbital);
    }

    public void JugarJefeFinal()
    {
        CargarNivelSiDesbloqueado(4, escenaJefeFinal);
    }

    private void CargarNivelSiDesbloqueado(
        int numeroNivel,
        string nombreEscena)
    {
        // Comprobar otra vez el progreso real.
        ActualizarEstadoNiveles();

        if (numeroNivel > 1 &&
            ultimoNivelCompletado < numeroNivel - 1)
        {
            Debug.LogWarning(
                "El nivel " + numeroNivel +
                " esta bloqueado."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            Debug.LogWarning(
                "El nivel no tiene escena configurada."
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError(
                "La escena no esta disponible: " +
                nombreEscena
            );
            return;
        }

        SceneManager.LoadScene(nombreEscena);
    }
}
