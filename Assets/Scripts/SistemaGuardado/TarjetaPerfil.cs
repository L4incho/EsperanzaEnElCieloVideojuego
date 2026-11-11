
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TarjetaPerfil : MonoBehaviour
{
    [Header("Informacion del perfil")]
    [SerializeField] private TMP_Text nombrePerfil;
    [SerializeField] private Image avatarMateo;
    [SerializeField] private Image ubicacionPerfil;

    [Header("Progreso")]
    [SerializeField] private Slider barraProgreso;
    [SerializeField] private TMP_Text porcentajeProgreso;

    [Header("Chips")]
    [SerializeField] private TMP_Text chipsGuardados;

    [Header("Avatares de Mateo")]
    [SerializeField] private Sprite mateoNormal;
    [SerializeField] private Sprite mateoAlerta;
    [SerializeField] private Sprite mateoVisor;
    [SerializeField] private Sprite mateoHerido;

    [Header("Ubicaciones")]
    [SerializeField] private Sprite laboratorio;
    [SerializeField] private Sprite ciudadInfestada;
    [SerializeField] private Sprite torreOrbital;
    [SerializeField] private Sprite jefeFinal;

    [Header("Seleccion de partida")]
    [SerializeField] private Button areaClick;
    [SerializeField] private Image contenedorProgreso;
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteSeleccionado;

    private Action<TarjetaPerfil> alSeleccionar;

    public string IdPerfil { get; private set; }

    private void Awake()
    {
        if (areaClick != null)
            areaClick.onClick.AddListener(SeleccionarEstaTarjeta);

        EstablecerSeleccionada(false);
    }

    public void ConfigurarSeleccion(
        string id,
        Action<TarjetaPerfil> callback)
    {
        IdPerfil = id;
        alSeleccionar = callback;

        EstablecerSeleccionada(false);
    }

    private void SeleccionarEstaTarjeta()
    {
        if (alSeleccionar != null)
            alSeleccionar.Invoke(this);
    }

    public void EstablecerSeleccionada(bool seleccionada)
    {
        if (contenedorProgreso == null)
            return;

        Sprite nuevoSprite = seleccionada
            ? spriteSeleccionado
            : spriteNormal;

        if (nuevoSprite != null)
            contenedorProgreso.sprite = nuevoSprite;
    }

    public void ActualizarTarjeta(
        string nombre,
        int nivel,
        int chips,
        float progreso)
    {
        if (nombrePerfil != null)
            nombrePerfil.text = nombre;

        if (chipsGuardados != null)
            chipsGuardados.text = "Chips: " + chips;

        float porcentaje = Mathf.Clamp(
            progreso, 0f, 100f
        );

        if (barraProgreso != null)
        {
            barraProgreso.minValue = 0f;
            barraProgreso.maxValue = 100f;

            barraProgreso.SetValueWithoutNotify(
                porcentaje
            );
        }

        if (porcentajeProgreso != null)
            porcentajeProgreso.text =
                porcentaje.ToString("0") + "%";

        ActualizarAvatar(nivel);
        ActualizarUbicacion(nivel);
    }

    private void ActualizarAvatar(int nivel)
    {
        if (avatarMateo == null)
            return;

        switch (nivel)
        {
            case 1:
                avatarMateo.sprite = mateoNormal;
                break;

            case 2:
                avatarMateo.sprite = mateoAlerta;
                break;

            case 3:
                avatarMateo.sprite = mateoVisor;
                break;

            default:
                avatarMateo.sprite = mateoHerido;
                break;
        }
    }

    private void ActualizarUbicacion(int nivel)
    {
        if (ubicacionPerfil == null)
            return;

        switch (nivel)
        {
            case 1:
                ubicacionPerfil.sprite = laboratorio;
                break;

            case 2:
                ubicacionPerfil.sprite = ciudadInfestada;
                break;

            case 3:
                ubicacionPerfil.sprite = torreOrbital;
                break;

            default:
                ubicacionPerfil.sprite = jefeFinal;
                break;
        }
    }

    private void OnDestroy()
    {
        if (areaClick != null)
            areaClick.onClick.RemoveListener(
                SeleccionarEstaTarjeta
            );
    }
}
