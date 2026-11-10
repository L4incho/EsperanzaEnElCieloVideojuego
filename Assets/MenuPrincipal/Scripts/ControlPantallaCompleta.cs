
using UnityEngine;
using UnityEngine.UI;

public class ControlPantallaCompleta : MonoBehaviour
{
    [Header("Botones interactivos")]
    [SerializeField] private Button botonSi;
    [SerializeField] private Button botonNo;

    [Header("Imagen del selector")]
    [SerializeField] private Image imagenPantallaCompleta;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteSi;
    [SerializeField] private Sprite spriteNo;

    private const string ClaveGuardado = "PantallaCompleta";

    private void Start()
    {
        if (botonSi == null || botonNo == null ||
            imagenPantallaCompleta == null ||
            spriteSi == null || spriteNo == null)
        {
            Debug.LogError(
                "ControlPantallaCompleta: faltan referencias en el Inspector."
            );
            return;
        }

        botonSi.onClick.AddListener(ActivarPantallaCompleta);
        botonNo.onClick.AddListener(DesactivarPantallaCompleta);

        bool activar = PlayerPrefs.GetInt(
            ClaveGuardado,
            Screen.fullScreen ? 1 : 0
        ) == 1;

        AplicarConfiguracion(activar);
    }

    public void ActivarPantallaCompleta()
    {
        AplicarConfiguracion(true);
    }

    public void DesactivarPantallaCompleta()
    {
        AplicarConfiguracion(false);
    }

    private void AplicarConfiguracion(bool activar)
    {
        Screen.fullScreenMode = activar
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        imagenPantallaCompleta.sprite = activar
            ? spriteSi
            : spriteNo;

        PlayerPrefs.SetInt(ClaveGuardado, activar ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log(activar
            ? "Pantalla completa ACTIVADA"
            : "Pantalla completa DESACTIVADA");
    }

    private void OnDestroy()
    {
        if (botonSi != null)
            botonSi.onClick.RemoveListener(ActivarPantallaCompleta);

        if (botonNo != null)
            botonNo.onClick.RemoveListener(DesactivarPantallaCompleta);
    }
}
