using UnityEngine;

// Marca la plataforma de un solo sentido que cubre el hueco de una escalera (la crea CrearEscaleraCentral).
// PlayerController la usa para que Mateo pueda atravesarla hacia abajo con S.
public class PlataformaEscalera : MonoBehaviour
{
    public Collider2D areaEscalera;   // el trigger de la escalera a la que pertenece
}
