
using UnityEngine;
using UnityEngine.Audio;

public static class ConfiguracionAudioGlobal
{
    private const string ClaveMusica = "VolumenMusica";
    private const string ClaveEfectos = "VolumenEfectos";

    private const float MusicaInicial = 0.5f;
    private const float EfectosInicial = 0.8f;

    private static AudioMixer mixer;

    // Recuperar la configuracion al iniciar el juego.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void IniciarAutomaticamente()
    {
        mixer = null;
        Inicializar();

        AplicarVolumenMusica(ObtenerVolumenGuardado());
        AplicarVolumenEfectos(ObtenerVolumenEfectosGuardado());
    }

    private static void Inicializar()
    {
        if (mixer != null)
            return;

        mixer = Resources.Load<AudioMixer>("Mixer_Esperanza");

        if (mixer == null)
        {
            Debug.LogError(
                "No se encontro Mixer_Esperanza " +
                "en Assets/Resources."
            );
        }
    }

    private static void AplicarVolumen(
        string parametro, float volumen)
    {
        Inicializar();

        if (mixer == null)
            return;

        volumen = Mathf.Clamp01(volumen);

        float db = volumen <= 0f
            ? -80f
            : Mathf.Log10(volumen) * 20f;

        if (!mixer.SetFloat(parametro, db))
        {
            Debug.LogError(
                "No se encontro el parametro del Mixer: " +
                parametro
            );
        }
    }

    // MUSICA
    public static void AplicarVolumenMusica(float volumen)
    {
        AplicarVolumen("VolumenMusica", volumen);
    }

    public static float ObtenerVolumenGuardado()
    {
        return Mathf.Clamp01(
            PlayerPrefs.GetFloat(ClaveMusica, MusicaInicial)
        );
    }

    public static void GuardarVolumenMusica(float volumen)
    {
        volumen = Mathf.Clamp01(volumen);

        PlayerPrefs.SetFloat(ClaveMusica, volumen);
        PlayerPrefs.Save();

        AplicarVolumenMusica(volumen);
    }

    // EFECTOS DE SONIDO
    public static void AplicarVolumenEfectos(float volumen)
    {
        AplicarVolumen("VolumenEfectos", volumen);
    }

    public static float ObtenerVolumenEfectosGuardado()
    {
        return Mathf.Clamp01(
            PlayerPrefs.GetFloat(ClaveEfectos, EfectosInicial)
        );
    }

    public static void GuardarVolumenEfectos(float volumen)
    {
        volumen = Mathf.Clamp01(volumen);

        PlayerPrefs.SetFloat(ClaveEfectos, volumen);
        PlayerPrefs.Save();

        AplicarVolumenEfectos(volumen);
    }

    // Util para dirigir efectos nuevos al grupo correcto.
    public static AudioMixerGroup ObtenerGrupoEfectos()
    {
        Inicializar();

        if (mixer == null)
            return null;

        AudioMixerGroup[] grupos =
            mixer.FindMatchingGroups("Efectos");

        return grupos.Length > 0 ? grupos[0] : null;
    }
}
