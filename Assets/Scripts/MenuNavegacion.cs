using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavegacion : MonoBehaviour
{
    // Abre la escena donde elegimos Historia o Niveles.
    public void AbrirSeleccion()
    {
        SceneManager.LoadScene("SeleccionModo");
    }

    // Abre el primer nivel del videojuego.
    public void IniciarHistoria()
    {
        SceneManager.LoadScene("Tutorial");
    }

    // Abre la pantalla para elegir nivel.
    public void AbrirNiveles()
    {
        SceneManager.LoadScene("SeleccionNiveles");
    }

    // Regresa a la pantalla de Historia / Niveles.
    public void VolverSeleccion()
    {
        SceneManager.LoadScene("SeleccionModo");
    }

    // Regresa al menú principal.
    public void VolverMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    // Cierra el videojuego.
    public void SalirJuego()
    {
        Application.Quit();
    }
}
