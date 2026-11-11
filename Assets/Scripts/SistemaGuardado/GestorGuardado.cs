
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorGuardado : MonoBehaviour
{
    public static GestorGuardado Instancia { get; private set; }

    private const string ClaveUltimoPerfil =
        "Esperanza_UltimoPerfilActivo";

    public const int MaximoPerfiles = 3;

    [Header("Perfil seleccionado")]
    [SerializeField] private string idPerfilActivo;

    private DatosPerfil perfilActivo;

    public DatosPerfil PerfilActivo => perfilActivo;

    public event Action OnPerfilActivoCambiado;

    private string CarpetaGuardados =>
        Path.Combine(Application.persistentDataPath, "Partidas");

    // Microchips recogidos en el intento actual.
    private readonly HashSet<string> microchipsTemporales =
        new HashSet<string>();

    public int CantidadMicrochipsTemporales =>
        microchipsTemporales.Count;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        Directory.CreateDirectory(CarpetaGuardados);

        RecuperarUltimoPerfil();
    }

    private void OnEnable()
    {
        if (Instancia == this)
            SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        if (Instancia == this)
            SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    // Evita arrastrar chips temporales entre escenas.
    private void AlCargarEscena(
        Scene escena,
        LoadSceneMode modo)
    {
        microchipsTemporales.Clear();
    }

    private void RecuperarUltimoPerfil()
    {
        string idGuardado = PlayerPrefs.GetString(
            ClaveUltimoPerfil, ""
        );

        if (string.IsNullOrEmpty(idGuardado))
            return;

        if (CargarPerfil(idGuardado))
        {
            Debug.Log(
                "Ultimo perfil recuperado: " +
                perfilActivo.nombrePerfil
            );
        }
        else
        {
            PlayerPrefs.DeleteKey(ClaveUltimoPerfil);
            PlayerPrefs.Save();

            idPerfilActivo = null;
            perfilActivo = null;

            microchipsTemporales.Clear();
        }
    }

    private void RecordarPerfilActivo()
    {
        if (perfilActivo == null)
            return;

        PlayerPrefs.SetString(
            ClaveUltimoPerfil,
            perfilActivo.idPerfil
        );

        PlayerPrefs.Save();
    }

    private void NotificarCambioPerfil()
    {
        OnPerfilActivoCambiado?.Invoke();
    }

    // ==========================================
    // CREAR PERFIL
    // ==========================================

    public bool CrearPerfil(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            Debug.LogWarning(
                "El nombre del perfil no puede estar vacio."
            );
            return false;
        }

        int cantidad = ObtenerPerfiles().Count;

        if (cantidad >= MaximoPerfiles)
        {
            Debug.LogWarning(
                "Limite de 3 perfiles alcanzado."
            );
            return false;
        }

        string id = Guid.NewGuid().ToString("N");

        DatosPerfil nuevo =
            new DatosPerfil(id, nombre.Trim());

        if (!GuardarPerfil(nuevo))
            return false;

        perfilActivo = nuevo;
        idPerfilActivo = nuevo.idPerfil;

        microchipsTemporales.Clear();

        RecordarPerfilActivo();
        NotificarCambioPerfil();

        Debug.Log(
            "Perfil creado: " + nuevo.nombrePerfil
        );

        return true;
    }

    // ==========================================
    // OBTENER PERFILES
    // ==========================================

    public List<DatosPerfil> ObtenerPerfiles()
    {
        List<DatosPerfil> perfiles =
            new List<DatosPerfil>();

        if (!Directory.Exists(CarpetaGuardados))
            return perfiles;

        foreach (string archivo in Directory.GetFiles(
                     CarpetaGuardados, "*.json"))
        {
            try
            {
                DatosPerfil perfil =
                    JsonUtility.FromJson<DatosPerfil>(
                        File.ReadAllText(archivo)
                    );

                if (perfil == null ||
                    string.IsNullOrEmpty(perfil.idPerfil))
                    continue;

                if (perfil.microchipsObtenidos == null)
                {
                    perfil.microchipsObtenidos =
                        new List<string>();
                }

                perfiles.Add(perfil);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "Error al leer perfil: " + ex.Message
                );
            }
        }

        perfiles.Sort((a, b) =>
            string.Compare(
                a.fechaCreacion,
                b.fechaCreacion,
                StringComparison.Ordinal
            )
        );

        return perfiles;
    }

    // ==========================================
    // CARGAR PERFIL
    // ==========================================

    public bool CargarPerfil(string id)
    {
        if (!Guid.TryParse(id, out Guid guid))
            return false;

        string idNormalizado = guid.ToString("N");
        string ruta = RutaPerfil(idNormalizado);

        if (!File.Exists(ruta))
            return false;

        try
        {
            DatosPerfil perfil =
                JsonUtility.FromJson<DatosPerfil>(
                    File.ReadAllText(ruta)
                );

            if (perfil == null ||
                perfil.idPerfil != idNormalizado)
                return false;

            if (perfil.microchipsObtenidos == null)
            {
                perfil.microchipsObtenidos =
                    new List<string>();
            }

            perfilActivo = perfil;
            idPerfilActivo = perfil.idPerfil;

            microchipsTemporales.Clear();

            RecordarPerfilActivo();
            NotificarCambioPerfil();

            Debug.Log(
                "Perfil cargado: " + perfil.nombrePerfil
            );

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Error al cargar perfil: " + ex.Message
            );
            return false;
        }
    }

    public bool GuardarPerfilActivo()
    {
        if (perfilActivo == null)
        {
            Debug.LogWarning("No hay perfil activo.");
            return false;
        }

        return GuardarPerfil(perfilActivo);
    }

    // ==========================================
    // MICROCHIPS UNICOS
    // ==========================================

    public bool MicrochipYaConfirmado(string id)
    {
        if (perfilActivo == null ||
            string.IsNullOrWhiteSpace(id))
            return false;

        if (perfilActivo.microchipsObtenidos == null)
            return false;

        return perfilActivo.microchipsObtenidos.Contains(id);
    }

    public bool RegistrarMicrochipTemporal(string id)
    {
        if (perfilActivo == null)
        {
            Debug.LogWarning(
                "No existe perfil activo."
            );
            return false;
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning(
                "Microchip sin identificador."
            );
            return false;
        }

        if (MicrochipYaConfirmado(id))
        {
            Debug.Log(
                "Microchip repetido, no suma: " + id
            );
            return false;
        }

        if (!microchipsTemporales.Add(id))
        {
            Debug.Log(
                "Microchip duplicado en este intento: " + id
            );
            return false;
        }

        Debug.Log(
            "Microchip nuevo: " + id +
            " | Temporales: " +
            microchipsTemporales.Count
        );

        return true;
    }

    public List<string> ObtenerMicrochipsTemporales()
    {
        return new List<string>(microchipsTemporales);
    }

    public void DescartarMicrochipsTemporales()
    {
        microchipsTemporales.Clear();
    }

    // ==========================================
    // CONFIRMAR NIVEL Y MICROCHIPS
    // ==========================================

    public bool ConfirmarNivelCompletado(
        int nivelCompletado,
        int chipsTotales)
    {
        if (perfilActivo == null)
        {
            Debug.LogError(
                "No hay perfil activo para confirmar el nivel."
            );
            return false;
        }

        if (nivelCompletado < 1 || nivelCompletado > 4)
        {
            Debug.LogError(
                "Numero de nivel no valido."
            );
            return false;
        }

        // Bloquear niveles que aun no estan disponibles.
        if (nivelCompletado > perfilActivo.nivelActual)
        {
            Debug.LogError(
                "No se puede confirmar un nivel bloqueado."
            );
            return false;
        }

        if (perfilActivo.microchipsObtenidos == null)
        {
            perfilActivo.microchipsObtenidos =
                new List<string>();
        }

        // Guardar respaldo para poder revertir
        // cualquier cambio si falla el archivo JSON.
        int nivelAnterior = perfilActivo.nivelActual;
        int completadoAnterior =
            perfilActivo.ultimoNivelCompletado;
        int chipsAnteriores = perfilActivo.chipsGuardados;

        List<string> idsAnteriores =
            new List<string>(perfilActivo.microchipsObtenidos);

        // Registrar solamente microchips nuevos.
        int chipsNuevos = 0;

        foreach (string id in microchipsTemporales)
        {
            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (perfilActivo.microchipsObtenidos.Contains(id))
                continue;

            perfilActivo.microchipsObtenidos.Add(id);

            chipsNuevos++;
        }

        // Agregar exclusivamente los chips nuevos.
        perfilActivo.chipsGuardados =
            chipsAnteriores + chipsNuevos;

        // Solamente avanzar si es la primera vez
        // que el jugador completa este nivel.
        bool nivelNuevo =
            nivelCompletado >
            perfilActivo.ultimoNivelCompletado;

        if (nivelNuevo)
        {
            perfilActivo.ultimoNivelCompletado =
                nivelCompletado;

            perfilActivo.nivelActual = Mathf.Min(
                nivelCompletado + 1,
                4
            );
        }

        // Si es un nivel repetido, se conservan
        // exactamente los niveles desbloqueados.

        if (!GuardarPerfilActivo())
        {
            perfilActivo.nivelActual = nivelAnterior;

            perfilActivo.ultimoNivelCompletado =
                completadoAnterior;

            perfilActivo.chipsGuardados = chipsAnteriores;

            perfilActivo.microchipsObtenidos =
                idsAnteriores;

            Debug.LogError(
                "No se pudo confirmar el progreso."
            );

            return false;
        }

        // Se guardo correctamente: limpiar temporales.
        microchipsTemporales.Clear();

        Microchips.Cantidad = perfilActivo.chipsGuardados;

        Debug.Log(
            "NIVEL CONFIRMADO | Nivel: " +
            nivelCompletado +
            " | Chips nuevos: " + chipsNuevos +
            " | Chips totales: " +
            perfilActivo.chipsGuardados +
            " | Nivel desbloqueado: " +
            perfilActivo.nivelActual
        );

        return true;
    }

    // ==========================================
    // ELIMINAR PERFIL
    // ==========================================

    public bool EliminarPerfil(string id)
    {
        if (!Guid.TryParse(id, out Guid guid))
            return false;

        string idNormalizado = guid.ToString("N");
        string ruta = RutaPerfil(idNormalizado);

        if (!File.Exists(ruta))
            return false;

        try
        {
            File.Delete(ruta);

            if (perfilActivo != null &&
                perfilActivo.idPerfil == idNormalizado)
            {
                perfilActivo = null;
                idPerfilActivo = null;

                microchipsTemporales.Clear();

                PlayerPrefs.DeleteKey(ClaveUltimoPerfil);
                PlayerPrefs.Save();

                NotificarCambioPerfil();
            }
            else if (PlayerPrefs.GetString(
                ClaveUltimoPerfil, "") == idNormalizado)
            {
                PlayerPrefs.DeleteKey(ClaveUltimoPerfil);
                PlayerPrefs.Save();
            }

            Debug.Log("Perfil eliminado.");

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Error al eliminar perfil: " + ex.Message
            );
            return false;
        }
    }

    // ==========================================
    // GUARDADO JSON
    // ==========================================

    private bool GuardarPerfil(DatosPerfil perfil)
    {
        if (perfil == null ||
            !Guid.TryParse(perfil.idPerfil, out Guid guid))
            return false;

        try
        {
            Directory.CreateDirectory(CarpetaGuardados);

            string ruta = RutaPerfil(guid.ToString("N"));
            string temporal = ruta + ".tmp";

            string json = JsonUtility.ToJson(perfil, true);

            File.WriteAllText(temporal, json);

            if (File.Exists(ruta))
            {
                File.Replace(temporal, ruta, null);
            }
            else
            {
                File.Move(temporal, ruta);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Error al guardar perfil: " + ex.Message
            );
            return false;
        }
    }

    private string RutaPerfil(string id)
    {
        return Path.Combine(
            CarpetaGuardados,
            id + ".json"
        );
    }
}
