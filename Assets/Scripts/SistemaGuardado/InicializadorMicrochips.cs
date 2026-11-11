
using UnityEngine;

public class InicializadorMicrochips : MonoBehaviour
{
    private void Awake()
    {
        if (GestorGuardado.Instancia == null ||
            GestorGuardado.Instancia.PerfilActivo == null)
        {
            Microchips.Cantidad = 0;

            Debug.LogWarning(
                "No hay perfil activo. Los chips empiezan en 0."
            );

            return;
        }

        Microchips.Cantidad =
            GestorGuardado.Instancia.PerfilActivo.chipsGuardados;

        Debug.Log(
            "Chips recuperados del perfil: " +
            Microchips.Cantidad
        );
    }
}
