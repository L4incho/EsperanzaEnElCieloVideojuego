
using UnityEngine;
using TMPro;

public class IndicadorPerfilActivo : MonoBehaviour
{
    [Header("Texto del perfil activo")]
    [SerializeField] private TMP_Text textoPerfil;

    private GestorGuardado gestorSuscrito;

    private void OnEnable()
    {
        Suscribirse();
        ActualizarTexto();
    }

    private void Start()
    {
        Suscribirse();
        ActualizarTexto();
    }

    private void OnDisable()
    {
        if (gestorSuscrito != null)
        {
            gestorSuscrito.OnPerfilActivoCambiado -= ActualizarTexto;
            gestorSuscrito = null;
        }
    }

    private void Suscribirse()
    {
        GestorGuardado gestor = GestorGuardado.Instancia;

        if (gestor == gestorSuscrito)
            return;

        if (gestorSuscrito != null)
            gestorSuscrito.OnPerfilActivoCambiado -= ActualizarTexto;

        gestorSuscrito = gestor;

        if (gestorSuscrito != null)
            gestorSuscrito.OnPerfilActivoCambiado += ActualizarTexto;
    }

    public void ActualizarTexto()
    {
        if (textoPerfil == null)
            return;

        DatosPerfil perfil = null;

        if (GestorGuardado.Instancia != null)
            perfil = GestorGuardado.Instancia.PerfilActivo;

        if (perfil == null)
        {
            textoPerfil.text = "Perfil: Ninguno";
        }
        else
        {
            textoPerfil.text = "Perfil: " + perfil.nombrePerfil;
        }
    }
}
