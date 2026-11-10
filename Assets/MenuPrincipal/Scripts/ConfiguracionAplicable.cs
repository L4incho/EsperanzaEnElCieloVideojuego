
using UnityEngine;
using UnityEngine.UI;

public class ConfiguracionAplicable : MonoBehaviour
{
    [Header("Musica")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private AudioSource musicaMenu;

    [Header("Pantalla completa")]
    [SerializeField] private Button botonSi;
    [SerializeField] private Button botonNo;
    [SerializeField] private Image imagenPantallaCompleta;
    [SerializeField] private Sprite spriteSi;
    [SerializeField] private Sprite spriteNo;

    private const string ClaveMusica = "VolumenMusica";
    private const string ClavePantalla = "PantallaCompleta";

    private float volumenAplicado;
    private float volumenPendiente;

    private bool pantallaAplicada;
    private bool pantallaPendiente;

    private void Awake()
    {
        if (sliderMusica == null ||
            botonSi == null || botonNo == null ||
            imagenPantallaCompleta == null ||
            spriteSi == null || spriteNo == null)
        {
            Debug.LogError(
                "ConfiguracionAplicable: faltan referencias de interfaz."
            );

            enabled = false;
            return;
        }

        sliderMusica.minValue = 0f;
        sliderMusica.maxValue = 1f;
        sliderMusica.wholeNumbers = false;

        sliderMusica.onValueChanged.AddListener(
            CambiarVolumenPendiente
        );

        botonSi.onClick.AddListener(SeleccionarSi);
        botonNo.onClick.AddListener(SeleccionarNo);
    }

    private void OnEnable()
    {
        if (sliderMusica == null || botonSi == null ||
            botonNo == null || imagenPantallaCompleta == null)
            return;

        RecargarConfiguracion();
    }

    public void RecargarConfiguracion()
    {
        volumenAplicado = Mathf.Clamp01(
            PlayerPrefs.GetFloat(ClaveMusica, 0.5f)
        );

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
        volumenPendiente = volumenAplicado;
        pantallaPendiente = pantallaAplicada;

        sliderMusica.SetValueWithoutNotify(volumenAplicado);

        AjustarMusica(volumenAplicado);
        ActualizarSprite();
    }

    private void AjustarMusica(float volumen)
    {
        volumen = Mathf.Clamp01(volumen);

        if (musicaMenu != null)
            musicaMenu.volume = volumen;

        if (MusicaMenu.Instancia != null)
            MusicaMenu.Instancia.AjustarVolumen(volumen);
    }

    private void CambiarVolumenPendiente(float valor)
    {
        volumenPendiente = Mathf.Clamp01(valor);

        // Vista previa: no guarda todavia.
        AjustarMusica(volumenPendiente);
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
        volumenAplicado = volumenPendiente;
        pantallaAplicada = pantallaPendiente;

        PlayerPrefs.SetFloat(ClaveMusica, volumenAplicado);
        PlayerPrefs.SetInt(
            ClavePantalla,
            pantallaAplicada ? 1 : 0
        );

        PlayerPrefs.Save();

        AjustarMusica(volumenAplicado);
        EstablecerModoPantalla(pantallaAplicada);

        Debug.Log("Configuracion aplicada y guardada.");
    }

    public void CancelarCambios()
    {
        volumenPendiente = volumenAplicado;
        pantallaPendiente = pantallaAplicada;

        if (sliderMusica != null)
            sliderMusica.SetValueWithoutNotify(volumenAplicado);

        AjustarMusica(volumenAplicado);
        ActualizarSprite();

        Debug.Log("Cambios pendientes descartados.");
    }

    private void EstablecerModoPantalla(bool activar)
    {
        Screen.fullScreenMode = activar
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }

    private void OnDisable()
    {
        if (sliderMusica != null)
            CancelarCambios();
    }

    private void OnDestroy()
    {
        if (sliderMusica != null)
            sliderMusica.onValueChanged.RemoveListener(
                CambiarVolumenPendiente
            );

        if (botonSi != null)
            botonSi.onClick.RemoveListener(SeleccionarSi);

        if (botonNo != null)
            botonNo.onClick.RemoveListener(SeleccionarNo);
    }
}
