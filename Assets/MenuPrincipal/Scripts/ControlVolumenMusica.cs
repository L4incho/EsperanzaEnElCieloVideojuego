
using UnityEngine;
using UnityEngine.UI;

public class ControlVolumenMusica : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private AudioSource musicaMenu;

    private const string ClaveVolumen = "VolumenMusica";

    private void Start()
    {
        if (sliderMusica == null || musicaMenu == null)
        {
            Debug.LogError("Faltan referencias del volumen de música.");
            return;
        }

        float volumenGuardado = PlayerPrefs.GetFloat(
            ClaveVolumen, 0.5f
        );

        volumenGuardado = Mathf.Clamp01(volumenGuardado);

        sliderMusica.minValue = 0f;
        sliderMusica.maxValue = 1f;
        sliderMusica.wholeNumbers = false;

        sliderMusica.SetValueWithoutNotify(volumenGuardado);
        musicaMenu.volume = volumenGuardado;

        sliderMusica.onValueChanged.AddListener(CambiarVolumen);
    }

    private void CambiarVolumen(float volumen)
    {
        musicaMenu.volume = volumen;

        PlayerPrefs.SetFloat(ClaveVolumen, volumen);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (sliderMusica != null)
            sliderMusica.onValueChanged.RemoveListener(CambiarVolumen);
    }
}
