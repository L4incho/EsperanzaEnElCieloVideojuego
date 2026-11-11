
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CinematicaSalidaLabController : MonoBehaviour
{
    [Header("Reproductor")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioVideo;

    [Header("Siguiente nivel")]
    [SerializeField] private string escenaDestino = "Nivel2";

    [Header("Omitir cinematica")]
    [SerializeField] private KeyCode teclaSaltar = KeyCode.Escape;

    private bool terminando;

    private void Start()
    {
        Time.timeScale = 1f;

        if (videoPlayer == null || videoPlayer.clip == null)
        {
            Debug.LogError("Falta asignar el video.");
            return;
        }

        if (audioVideo != null)
        {
            videoPlayer.audioOutputMode =
                VideoAudioOutputMode.AudioSource;

            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.SetTargetAudioSource(0, audioVideo);
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        videoPlayer.loopPointReached += VideoTerminado;
        videoPlayer.errorReceived += ErrorVideo;
        videoPlayer.prepareCompleted += VideoPreparado;

        videoPlayer.Prepare();
    }

    private void VideoPreparado(VideoPlayer reproductor)
    {
        if (!terminando)
            reproductor.Play();
    }

    private void Update()
    {
        if (!terminando && Input.GetKeyDown(teclaSaltar))
            Finalizar();
    }

    private void VideoTerminado(VideoPlayer reproductor)
    {
        Finalizar();
    }

    private void ErrorVideo(VideoPlayer reproductor, string error)
    {
        Debug.LogError("Error al reproducir video: " + error);
    }

    public void Finalizar()
    {
        if (terminando)
            return;

        if (!Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            Debug.LogError(
                "No se encuentra la escena: " + escenaDestino
            );
            return;
        }

        terminando = true;

        videoPlayer.Stop();
        Time.timeScale = 1f;

        SceneManager.LoadScene(escenaDestino);
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.loopPointReached -= VideoTerminado;
        videoPlayer.errorReceived -= ErrorVideo;
        videoPlayer.prepareCompleted -= VideoPreparado;
    }
}
