using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de seleccion de niveles.
// Cada panel abre la escena indicada en "escenas". Si la escena todavia no existe en Build Settings,
// el panel se muestra bloqueado ("Proximamente"). Cuando crees la escena y la agregues a
// File > Build Profiles > Scene List, se desbloquea sola.
public class SelectorNiveles : MonoBehaviour
{
    public Button[] botones;          // un boton por nivel (mismo orden que "escenas")
    public string[] escenas;          // nombre exacto de la escena de cada nivel
    public GameObject[] candados;     // capa oscura "proximamente" de cada panel
    public TMP_Text mensaje;          // texto de aviso (opcional)

    Coroutine ocultar;

    void Start()
    {
        for (int i = 0; i < botones.Length; i++)
            if (candados != null && i < candados.Length && candados[i] != null)
                candados[i].SetActive(!Disponible(i));
        if (mensaje != null) mensaje.gameObject.SetActive(false);
    }

    bool Disponible(int i)
    {
        return i < escenas.Length && !string.IsNullOrEmpty(escenas[i]) && Application.CanStreamedLevelBeLoaded(escenas[i]);
    }

    // Se llama desde el OnClick de cada panel con su numero (0 = nivel 1)
    public void Seleccionar(int i)
    {
        if (Disponible(i)) { SceneManager.LoadScene(escenas[i]); return; }
        if (mensaje == null) return;
        mensaje.text = "Este nivel estara disponible pronto";
        mensaje.gameObject.SetActive(true);
        if (ocultar != null) StopCoroutine(ocultar);
        ocultar = StartCoroutine(OcultarMensaje());
    }

    IEnumerator OcultarMensaje()
    {
        yield return new WaitForSecondsRealtime(2f);
        mensaje.gameObject.SetActive(false);
    }
}
