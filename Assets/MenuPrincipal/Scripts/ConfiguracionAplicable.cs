
using UnityEngine;
using UnityEngine.UI;

public class ConfiguracionAplicable : MonoBehaviour
{
    [Header("Musica")]
    [SerializeField] private Slider sliderMusica;

    [Header("Efectos de sonido")]
    [SerializeField] private Slider sliderEfectos;

    [Header("Pantalla completa")]
    [SerializeField] private Button botonSi;
    [SerializeField] private Button botonNo;
    [SerializeField] private Image imagenPantallaCompleta;
    [SerializeField] private Sprite spriteSi;
    [SerializeField] private Sprite spriteNo;

    private const string ClavePantalla = "PantallaCompleta";

    private float musicaAplicada;
    private float musicaPendiente;

    private float efectosAplicados;
    private float efectosPendientes;

    private bool pantallaAplicada;
    private bool pantallaPendiente;

    private void Awake()
    {
        if (sliderMusica == null ||
            sliderEfectos == null ||
            botonSi == null ||
            botonNo == null ||
            imagenPantallaCompleta == null ||
            spriteSi == null ||
            spriteNo == null)
        {
            Debug.LogError(
                "ConfiguracionAplicable: " +
                "faltan referencias en el Inspector."
            );

            enabled = false;
            return;
        }

        ConfigurarSlider(sliderMusica);
        ConfigurarSlider(sliderEfectos);

        sliderMusica.onValueChanged.AddListener(
            CambiarMusicaPendiente
        );

        sliderEfectos.onValueChanged.AddListener(
            CambiarEfectosPendientes
        );

        botonSi.onClick.AddListener(SeleccionarSi);
        botonNo.onClick.AddListener(SeleccionarNo);
    }

    private void ConfigurarSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        if (sliderMusica == null ||
            sliderEfectos == null ||
            botonSi == null ||
            botonNo == null ||
            imagenPantallaCompleta == null)
            return;

        RecargarConfiguracion();
    }

    public void RecargarConfiguracion()
    {
        musicaAplicada =
            ConfiguracionAudioGlobal.ObtenerVolumenGuardado();

        efectosAplicados =
            ConfiguracionAudioGlobal.ObtenerVolumenEfectosGuardado();

        pantallaAplicada = PlayerPrefs.GetInt(
            ClavePantalla,
            Screen.fullScreen ? 1 : 0
        ) == 1;

        PrepararCambios();

        if (PlayerPrefs.HasKey(ClavePantalla))
            EstablecerModoPantalla(pantallaAplicada);
    }

    public void PrepararCambios()
    {
        musicaPendiente = musicaAplicada;
        efectosPendientes = efectosAplicados;

        pantallaPendiente = pantallaAplicada;

        if (sliderMusica != null)
        {
            sliderMusica.SetValueWithoutNotify(
                musicaAplicada
            );
        }

        if (sliderEfectos != null)
        {
            sliderEfectos.SetValueWithoutNotify(
                efectosAplicados
            );
        }

        ConfiguracionAudioGlobal.AplicarVolumenMusica(
            musicaAplicada
        );

        ConfiguracionAudioGlobal.AplicarVolumenEfectos(
            efectosAplicados
        );

        ActualizarSprite();
    }

    private void CambiarMusicaPendiente(float valor)
    {
        musicaPendiente = Mathf.Clamp01(valor);

        ConfiguracionAudioGlobal.AplicarVolumenMusica(
            musicaPendiente
        );
    }

    private void CambiarEfectosPendientes(float valor)
    {
        efectosPendientes = Mathf.Clamp01(valor);

        ConfiguracionAudioGlobal.AplicarVolumenEfectos(
            efectosPendientes
        );
    }

    private void SeleccionarSi()
    {
        pantallaPendiente = true;
        ActualizarSprite();
    }

    private void SeleccionarNo()
    {
        pantallaPendiente = false;
        ActualizarSprite();
    }

    private void ActualizarSprite()
    {
        if (imagenPantallaCompleta == null)
            return;

        imagenPantallaCompleta.sprite =
            pantallaPendiente ? spriteSi : spriteNo;
    }

    public void AplicarCambios()
    {
        musicaAplicada = musicaPendiente;
        efectosAplicados = efectosPendientes;
        pantallaAplicada = pantallaPendiente;

        ConfiguracionAudioGlobal.GuardarVolumenMusica(
            musicaAplicada
        );

        ConfiguracionAudioGlobal.GuardarVolumenEfectos(
            efectosAplicados
        );

        PlayerPrefs.SetInt(
            ClavePantalla,
            pantallaAplicada ? 1 : 0
        );

        PlayerPrefs.Save();

        EstablecerModoPantalla(pantallaAplicada);

        Debug.Log(
            "Configuracion guardada | Musica: " +
            Mathf.RoundToInt(musicaAplicada * 100f) +
            "% | Efectos: " +
            Mathf.RoundToInt(efectosAplicados * 100f) + "%"
        );
    }

    public void CancelarCambios()
    {
        musicaPendiente = musicaAplicada;
        efectosPendientes = efectosAplicados;
        pantallaPendiente = pantallaAplicada;

        if (sliderMusica != null)
        {
            sliderMusica.SetValueWithoutNotify(
                musicaAplicada
            );
        }

        if (sliderEfectos != null)
        {
            sliderEfectos.SetValueWithoutNotify(
                efectosAplicados
            );
        }

        ConfiguracionAudioGlobal.AplicarVolumenMusica(
            musicaAplicada
        );

        ConfiguracionAudioGlobal.AplicarVolumenEfectos(
            efectosAplicados
        );

        ActualizarSprite();
    }

    private void EstablecerModoPantalla(bool activar)
    {
        Screen.fullScreenMode = activar
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }

    private void OnDisable()
    {
        if (sliderMusica != null && sliderEfectos != null)
            CancelarCambios();
    }

    private void OnDestroy()
    {
        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveListener(
                CambiarMusicaPendiente
            );
        }

        if (sliderEfectos != null)
        {
            sliderEfectos.onValueChanged.RemoveListener(
                CambiarEfectosPendientes
            );
        }

        if (botonSi != null)
            botonSi.onClick.RemoveListener(SeleccionarSi);

        if (botonNo != null)
            botonNo.onClick.RemoveListener(SeleccionarNo);
    }
}
