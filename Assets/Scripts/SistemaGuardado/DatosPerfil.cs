
using System;
using System.Collections.Generic;

[Serializable]
public class DatosPerfil
{
    // Identificacion del perfil
    public string idPerfil;
    public string nombrePerfil;

    // Progreso del videojuego
    public int nivelActual;
    public int ultimoNivelCompletado;

    // Cinematica de introduccion
    public bool introVista;

    // Puntuacion confirmada
    public int chipsGuardados;

    // IDs de los microchips obtenidos.
    // Se almacenan en el JSON de cada perfil.
    public List<string> microchipsObtenidos =
        new List<string>();

    // Estadisticas
    public float tiempoJugado;

    // Fecha de creacion
    public string fechaCreacion;

    // Constructor para perfiles nuevos
    public DatosPerfil(string id, string nombre)
    {
        idPerfil = id;
        nombrePerfil = nombre;

        nivelActual = 1;
        ultimoNivelCompletado = 0;

        // Un perfil nuevo todavia no vio la introduccion.
        introVista = false;

        chipsGuardados = 0;
        microchipsObtenidos = new List<string>();

        tiempoJugado = 0f;

        fechaCreacion = DateTime.UtcNow.ToString("o");
    }
}
