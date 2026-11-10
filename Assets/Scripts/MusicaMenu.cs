using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Musica de los menus: suena en bucle y NO se reinicia al pasar de un menu a otro.
// Al entrar a un nivel (cualquier escena que no este en la lista) baja el volumen y se apaga.
// Se pone un objeto "MusicaMenu" en cada escena de menu; si ya hay una sonando, el nuevo se elimina solo.
[RequireComponent(typeof(AudioSource))]
public class MusicaMenu : MonoBehaviour
{
    public AudioClip musica;
    [Range(0f, 1f)] public float volumen = 0.6f;
    public float segundosFundidoSalida = 0.8f;
    public string[] escenasConMusica = { "MenuPrincipal", "SeleccionModo", "SeleccionNiveles" };

    static MusicaMenu instancia;
    AudioSource fuente;
    bool saliendo;

    void Awake()
    {
        if (instancia != null && instancia != this) { Destroy(gameObject); return; }
        instancia = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        fuente = GetComponent<AudioSource>();
        fuente.clip = musica;
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.spatialBlend = 0f;
        fuente.volume = volumen * PlayerPrefs.GetFloat("vol_musica", 1f);
        if (musica != null) fuente.Play();
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDestroy()
    {
        if (instancia == this) { instancia = null; SceneManager.sceneLoaded -= AlCargarEscena; }
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (saliendo) return;
        foreach (var n in escenasConMusica) if (n == escena.name) return;   // sigue en un menu
        saliendo = true;
        StartCoroutine(ApagarSuave());
    }

    IEnumerator ApagarSuave()
    {
        float inicio = fuente.volume;
        for (float t = 0; t < segundosFundidoSalida; t += Time.unscaledDeltaTime)
        {
            fuente.volume = Mathf.Lerp(inicio, 0f, t / segundosFundidoSalida);
            yield return null;
        }
        Destroy(gameObject);
    }

    // Lo usa el menu de opciones (control de Musica)
    public void AjustarVolumen(float v) { if (fuente != null && !saliendo) fuente.volume = volumen * v; }

    // Para pruebas
    public static bool Sonando { get { return instancia != null && instancia.fuente != null && instancia.fuente.isPlaying; } }
    public static MusicaMenu Instancia { get { return instancia; } }
}
