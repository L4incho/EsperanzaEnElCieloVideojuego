
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControladorContinuar : MonoBehaviour
{
    [Header("Prefab de los perfiles")]
    [SerializeField] private TarjetaPerfil prefabTarjeta;

    [Header("Tarjeta utilizada como referencia")]
    [SerializeField] private RectTransform tarjetaReferencia;

    [Header("Distribucion")]
    [SerializeField] private float separacionVertical = 165f;
    [SerializeField] private int maximoTarjetas = 3;

    [Header("Confirmacion de eliminacion")]
    [SerializeField] private GameObject panelConfirmarEliminar;

    [Header("Limite de perfiles")]
    [SerializeField] private BotonMenuVisual botonNuevoPerfil;

    [Header("Regreso automatico al menu")]
    [SerializeField] private GameObject botonJugarMenuPrincipal;
    [SerializeField] private MenuPrincipalController controladorMenu;

    private readonly List<TarjetaPerfil> tarjetasCreadas =
        new List<TarjetaPerfil>();

    private const int TotalNiveles = 4;

    private TarjetaPerfil tarjetaSeleccionada;
    private string idPerfilSeleccionado;
    private string idPerfilPendienteEliminar;

    public string IdPerfilSeleccionado => idPerfilSeleccionado;

    private void OnEnable()
    {
        CerrarConfirmacion();
        MostrarPerfiles();
    }

    // Mostrar los perfiles existentes.
    public void MostrarPerfiles()
    {
        LimpiarTarjetas();
        ActualizarBotonNuevoPerfil();

        if (GestorGuardado.Instancia == null)
        {
            Debug.LogError(
                "ControladorContinuar: No existe GestorGuardado."
            );
            return;
        }

        if (prefabTarjeta == null || tarjetaReferencia == null)
        {
            Debug.LogError(
                "ControladorContinuar: Faltan referencias."
            );
            return;
        }

        List<DatosPerfil> perfiles =
            GestorGuardado.Instancia.ObtenerPerfiles();

        int cantidad = Mathf.Min(
            perfiles.Count,
            Mathf.Max(0, maximoTarjetas)
        );

        for (int i = 0; i < cantidad; i++)
        {
            DatosPerfil perfil = perfiles[i];

            TarjetaPerfil nuevaTarjeta = Instantiate(
                prefabTarjeta,
                tarjetaReferencia.parent,
                false
            );

            RectTransform rect =
                nuevaTarjeta.GetComponent<RectTransform>();

            rect.anchorMin = tarjetaReferencia.anchorMin;
            rect.anchorMax = tarjetaReferencia.anchorMax;
            rect.pivot = tarjetaReferencia.pivot;
            rect.sizeDelta = tarjetaReferencia.sizeDelta;
            rect.localScale = tarjetaReferencia.localScale;
            rect.localRotation = tarjetaReferencia.localRotation;

            rect.anchoredPosition =
                tarjetaReferencia.anchoredPosition +
                new Vector2(0f, -i * separacionVertical);

            float porcentaje = Mathf.Clamp(
                perfil.ultimoNivelCompletado * 100f /
                TotalNiveles,
                0f,
                100f
            );

            nuevaTarjeta.ActualizarTarjeta(
                perfil.nombrePerfil,
                Mathf.Clamp(perfil.nivelActual, 1, TotalNiveles),
                perfil.chipsGuardados,
                porcentaje
            );

            nuevaTarjeta.ConfigurarSeleccion(
                perfil.idPerfil,
                SeleccionarTarjeta
            );

            nuevaTarjeta.gameObject.name =
                "Perfil_" + (i + 1).ToString("00") +
                "_" + perfil.nombrePerfil;

            tarjetasCreadas.Add(nuevaTarjeta);
            nuevaTarjeta.gameObject.SetActive(true);
        }

        Debug.Log(
            "Perfiles mostrados: " + cantidad
        );
    }

    // Bloquear NUEVO PERFIL al llegar al limite.
    private void ActualizarBotonNuevoPerfil()
    {
        if (botonNuevoPerfil == null)
            return;

        bool disponible =
            GestorGuardado.Instancia != null &&
            GestorGuardado.Instancia.ObtenerPerfiles().Count <
            GestorGuardado.MaximoPerfiles;

        botonNuevoPerfil.EstablecerHabilitado(disponible);
    }

    // Seleccionar una tarjeta.
    private void SeleccionarTarjeta(TarjetaPerfil tarjeta)
    {
        if (tarjeta == null)
            return;

        if (panelConfirmarEliminar != null &&
            panelConfirmarEliminar.activeSelf)
            return;

        if (tarjetaSeleccionada != null)
            tarjetaSeleccionada.EstablecerSeleccionada(false);

        tarjetaSeleccionada = tarjeta;
        idPerfilSeleccionado = tarjeta.IdPerfil;

        tarjetaSeleccionada.EstablecerSeleccionada(true);

        Debug.Log(
            "Partida seleccionada: " +
            idPerfilSeleccionado
        );
    }

    // CARGAR: seleccionar el perfil y volver al menu.
    public void CargarPartidaSeleccionada()
    {
        if (string.IsNullOrEmpty(idPerfilSeleccionado))
        {
            Debug.LogWarning(
                "Selecciona primero una partida."
            );
            return;
        }

        if (GestorGuardado.Instancia == null)
        {
            Debug.LogError(
                "No existe GestorGuardado."
            );
            return;
        }

        bool cargado =
            GestorGuardado.Instancia.CargarPerfil(
                idPerfilSeleccionado
            );

        if (!cargado)
        {
            Debug.LogError(
                "No se pudo cargar la partida."
            );
            return;
        }

        Debug.Log(
            "Perfil activo: " +
            GestorGuardado.Instancia.PerfilActivo.nombrePerfil
        );

        // No iniciar el gameplay: regresar al menu.
        gameObject.SetActive(false);
    }

    // Abrir confirmacion de eliminacion.
    public void SolicitarEliminarPartida()
    {
        if (string.IsNullOrEmpty(idPerfilSeleccionado))
        {
            Debug.LogWarning(
                "Selecciona una partida antes de eliminar."
            );
            return;
        }

        if (panelConfirmarEliminar == null ||
            GestorGuardado.Instancia == null)
        {
            Debug.LogError(
                "Faltan referencias para eliminar una partida."
            );
            return;
        }

        List<DatosPerfil> perfiles =
            GestorGuardado.Instancia.ObtenerPerfiles();

        DatosPerfil perfil = perfiles.Find(
            p => p.idPerfil == idPerfilSeleccionado
        );

        if (perfil == null)
        {
            Debug.LogWarning(
                "La partida seleccionada ya no existe."
            );

            MostrarPerfiles();
            return;
        }

        idPerfilPendienteEliminar = perfil.idPerfil;

        // Mostrar encima de las tarjetas.
        panelConfirmarEliminar.transform.SetAsLastSibling();
        panelConfirmarEliminar.SetActive(true);
    }

    public void CancelarEliminarPartida()
    {
        CerrarConfirmacion();
    }

    // Confirmar eliminacion del perfil seleccionado.
    public void ConfirmarEliminarPartida()
    {
        if (string.IsNullOrEmpty(idPerfilPendienteEliminar))
        {
            CerrarConfirmacion();
            return;
        }

        if (GestorGuardado.Instancia == null)
        {
            Debug.LogError(
                "No existe GestorGuardado."
            );

            CerrarConfirmacion();
            return;
        }

        string idEliminar = idPerfilPendienteEliminar;

        bool eliminado =
            GestorGuardado.Instancia.EliminarPerfil(idEliminar);

        CerrarConfirmacion();

        if (!eliminado)
        {
            Debug.LogError(
                "No se pudo eliminar el perfil."
            );
            return;
        }

        Debug.Log(
            "Perfil eliminado correctamente."
        );

        // NUEVO: comprobar si ya no quedan perfiles.
        int perfilesRestantes =
            GestorGuardado.Instancia.ObtenerPerfiles().Count;

        if (perfilesRestantes == 0)
        {
            RegresarAlMenuSinPerfiles();
            return;
        }

        // Si quedan perfiles, actualizar las tarjetas.
        MostrarPerfiles();
    }

    // NUEVO: cerrar la pantalla cuando ya no hay partidas.
    private void RegresarAlMenuSinPerfiles()
    {
        Debug.Log(
            "No quedan perfiles. Regresando al menu principal."
        );

        // Limpiar seleccion y tarjetas generadas.
        LimpiarTarjetas();

        // Cerrar la confirmacion, si sigue abierta.
        CerrarConfirmacion();

        // Cerrar GUARDADOS DE PARTIDA.
        // El menu principal ya permanece detras
        // de este panel, por lo que no hay que crear otro.
        gameObject.SetActive(false);

        // Actualizar CONTINUAR y NIVELES.
        if (controladorMenu != null)
        {
            controladorMenu.ActualizarBotonesGuardado();
        }

        // Seleccionar JUGAR si esta disponible.
        if (EventSystem.current != null &&
            botonJugarMenuPrincipal != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            EventSystem.current.SetSelectedGameObject(
                botonJugarMenuPrincipal
            );
        }
    }

    private void CerrarConfirmacion()
    {
        idPerfilPendienteEliminar = null;

        if (panelConfirmarEliminar != null)
            panelConfirmarEliminar.SetActive(false);
    }

    // Destruir unicamente las tarjetas generadas.
    private void LimpiarTarjetas()
    {
        tarjetaSeleccionada = null;
        idPerfilSeleccionado = null;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        foreach (TarjetaPerfil tarjeta in tarjetasCreadas)
        {
            if (tarjeta != null)
            {
                tarjeta.gameObject.SetActive(false);
                Destroy(tarjeta.gameObject);
            }
        }

        tarjetasCreadas.Clear();
    }

    private void OnDisable()
    {
        CerrarConfirmacion();
        LimpiarTarjetas();
    }
}
