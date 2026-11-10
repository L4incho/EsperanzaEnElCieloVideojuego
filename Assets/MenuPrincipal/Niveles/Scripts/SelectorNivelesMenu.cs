
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorNivelesMenu : MonoBehaviour
{
    [Header("Nombres de las escenas")]
    [SerializeField] private string escenaLaboratorio;
    [SerializeField] private string escenaCiudadInfectada;
    [SerializeField] private string escenaTorreOrbital;
    [SerializeField] private string escenaJefeFinal;

    public void JugarLaboratorio()
    {
        CargarNivel(escenaLaboratorio);
    }

    public void JugarCiudadInfectada()
    {
        CargarNivel(escenaCiudadInfectada);
    }

    public void JugarTorreOrbital()
    {
        CargarNivel(escenaTorreOrbital);
    }

    public void JugarJefeFinal()
    {
        CargarNivel(escenaJefeFinal);
    }

    private void CargarNivel(string nombreEscena)
    {
        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            Debug.LogWarning(
                "Este nivel todavía no tiene una escena asignada."
            );
            return;
        }

        SceneManager.LoadScene(nombreEscena);
    }
}
