
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfiguracionTabsUI : MonoBehaviour
{
    [System.Serializable]
    public class PestanaUI
    {
        public string id;

        [Header("Referencias")]
        public Button boton;
        public Image imagen;
        public GameObject panelContenido;

        [Header("Sprites del boton")]
        public Sprite spriteNormal;
        public Sprite spriteSeleccionado;
        public Sprite spriteDeshabilitado;

        [Header("Imagen del panel central")]
        public Sprite spritePanel;

        [Header("Estado")]
        public bool habilitada = true;
    }

    [Header("Panel central")]
    public Image imagenPanelContenido;

    [Header("Pestañas")]
    public List<PestanaUI> pestanas = new List<PestanaUI>();

    [Header("Pestaña inicial")]
    public string pestanaInicial = "Sonido";

    private string pestanaActual;

    private void Start()
    {
        PestanaUI inicial = BuscarPestana(pestanaInicial);

        if (inicial != null && inicial.habilitada)
        {
            pestanaActual = inicial.id;
        }
        else
        {
            foreach (var p in pestanas)
            {
                if (p.habilitada)
                {
                    pestanaActual = p.id;
                    break;
                }
            }
        }

        ActualizarVisual();
    }

    public void SeleccionarPestana(string id)
    {
        PestanaUI pestana = BuscarPestana(id);

        if (pestana == null || !pestana.habilitada)
            return;

        pestanaActual = id;
        ActualizarVisual();
    }

    public void ReiniciarPestana()
    {
        SeleccionarPestana(pestanaInicial);
    }

    public void SetPestanaHabilitada(string id, bool valor)
    {
        PestanaUI pestana = BuscarPestana(id);

        if (pestana == null)
            return;

        pestana.habilitada = valor;

        if (!valor && pestanaActual == id)
        {
            pestanaActual = null;

            foreach (var p in pestanas)
            {
                if (p.habilitada)
                {
                    pestanaActual = p.id;
                    break;
                }
            }
        }

        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        foreach (var p in pestanas)
        {
            bool seleccionada =
                p.habilitada && p.id == pestanaActual;

            if (p.panelContenido != null)
                p.panelContenido.SetActive(seleccionada);

            if (p.boton != null)
                p.boton.interactable = p.habilitada;

            if (p.imagen != null)
            {
                if (!p.habilitada)
                {
                    if (p.spriteDeshabilitado != null)
                        p.imagen.sprite = p.spriteDeshabilitado;
                }
                else if (seleccionada)
                {
                    if (p.spriteSeleccionado != null)
                        p.imagen.sprite = p.spriteSeleccionado;
                }
                else
                {
                    if (p.spriteNormal != null)
                        p.imagen.sprite = p.spriteNormal;
                }
            }

            // Cambiar la imagen del panel central
            if (seleccionada &&
                imagenPanelContenido != null &&
                p.spritePanel != null)
            {
                imagenPanelContenido.sprite = p.spritePanel;
            }
        }
    }

    private PestanaUI BuscarPestana(string id)
    {
        foreach (var p in pestanas)
        {
            if (p.id == id)
                return p;
        }

        return null;
    }
}
