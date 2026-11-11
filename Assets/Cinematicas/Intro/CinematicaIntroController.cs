
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicaIntroController : MonoBehaviour
{
    [Header("Ilustraciones en orden numerico")]
    [SerializeField] private Sprite[] imagenes = new Sprite[11];

    [Header("Interfaz")]
    [SerializeField] private Image imagenCinematica;

    [Header("Duracion por imagen")]
    [SerializeField]
    private float[] duraciones =
    {
        3f, 2.5f, 3f, 3.5f, 2.5f, 2.5f,
        2f, 2f, 2.5f, 2.5f, 2.5f
    };

    [Header("Transiciones")]
    [SerializeField] private float duracionFundido = 0.4f;

    [Header("Controles")]
    [SerializeField] private KeyCode teclaAvanzar = KeyCode.Return;
    [SerializeField] private KeyCode teclaSaltar = KeyCode.Escape;

    [Header("Escena de destino")]
    [SerializeField] private string escenaDestino = "Tutorial";

    private Coroutine reproduccion;
    private bool terminando;
    private bool avanzarSolicitado;

    private void Start()
    {
        if (imagenCinematica == null ||
            imagenes == null ||
            imagenes.Length == 0)
        {
            Debug.LogError(
                "CinematicaIntro: faltan referencias."
            );
            return;
        }

        foreach (Sprite imagen in imagenes)
        {
            if (imagen == null)
            {
                Debug.LogError(
                    "CinematicaIntro: hay una imagen sin asignar."
                );
                return;
            }
        }

        if (!Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            Debug.LogError(
                "CinematicaIntro: no se encuentra la escena " +
                escenaDestino
            );
            return;
        }

        reproduccion = StartCoroutine(Reproducir());
    }

    private void Update()
    {
        if (terminando)
            return;

        if (Input.GetKeyDown(teclaSaltar))
        {
            Finalizar();
            return;
        }

        if (Input.GetKeyDown(teclaAvanzar))
            avanzarSolicitado = true;
    }

    private IEnumerator Reproducir()
    {
        for (int i = 0; i < imagenes.Length; i++)
        {
            avanzarSolicitado = false;
            imagenCinematica.sprite = imagenes[i];

            yield return Fundido(0f, 1f);

            float tiempo = i < duraciones.Length
                ? Mathf.Max(0f, duraciones[i])
                : 3f;

            float transcurrido = 0f;

            while (transcurrido < tiempo && !avanzarSolicitado)
            {
                transcurrido += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return Fundido(1f, 0f);
        }

        Finalizar();
    }

    private IEnumerator Fundido(float inicial, float final)
    {
        float duracion = Mathf.Max(0.01f, duracionFundido);
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);

            Color color = imagenCinematica.color;
            color.a = Mathf.Lerp(inicial, final, progreso);

            imagenCinematica.color = color;

            yield return null;
        }

        Color colorFinal = imagenCinematica.color;
        colorFinal.a = final;
        imagenCinematica.color = colorFinal;
    }


    public void Finalizar()
    {
        if (terminando)
            return;

        terminando = true;

        if (reproduccion != null)
            StopCoroutine(reproduccion);

        // Registrar la introduccion como vista.
        if (GestorGuardado.Instancia != null &&
            GestorGuardado.Instancia.PerfilActivo != null)
        {
            bool guardado =
                GestorGuardado.Instancia.MarcarIntroVista();

            if (!guardado)
            {
                Debug.LogError(
                    "No se pudo guardar la introduccion."
                );

                terminando = false;
                return;
            }
        }
        else
        {
            Debug.LogWarning(
                "No hay perfil activo. La intro no se guardara."
            );
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaDestino);
    }

}
