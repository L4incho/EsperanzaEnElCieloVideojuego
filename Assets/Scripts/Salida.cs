using UnityEngine;
using UnityEngine.SceneManagement;

// Puerta de salida del nivel: al entrar Mateo, carga la escena indicada.
[RequireComponent(typeof(Collider2D))]
public class Salida : MonoBehaviour
{
    public string escenaSiguiente = "SeleccionNiveles";
    bool usada;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D o)
    {
        if (usada || o.GetComponentInParent<PlayerController>() == null) return;
        usada = true;
        SceneManager.LoadScene(escenaSiguiente);
    }
}
