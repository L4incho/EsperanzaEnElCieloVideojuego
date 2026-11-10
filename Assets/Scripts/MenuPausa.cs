using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu de pausa: Esc pausa el juego y muestra el fondo con dos botones (Opciones y Salir).
// Esc otra vez reanuda. Se crea solo en cualquier escena que tenga a Mateo (ver MenuPausaArranque).
// La interfaz se construye por codigo; las imagenes vienen del prefab Assets/Resources/MenuPausa.
public class MenuPausa : MonoBehaviour
{
    [Header("Imagenes (las pone el menu Esperanza > Crear menu de pausa)")]
    public Sprite fondo;
    public Sprite fondoConfig;
    [Tooltip("normal, resaltado, presionado")] public Sprite[] opciones;
    [Tooltip("normal, resaltado, presionado")] public Sprite[] salir;
    [Tooltip("normal, resaltado, presionado")] public Sprite[] volver;
    public string escenaMenu = "MenuPrincipal";

    public static bool EstaPausado { get; private set; }

    GameObject panelPausa, panelConfig;
    GameObject panelOpcionesPersonalizado;
    Button btnOpciones, btnSalir, btnVolver;
    Slider sMusica, sEfectos, sMaestro;
    bool pausado, enConfig;
    float escalaAntes = 1f;
    readonly List<PlayerController> bloqueados = new List<PlayerController>();

    public const string ClaveMusica = "vol_musica", ClaveEfectos = "vol_efectos", ClaveMaestro = "vol_maestro";

    void Awake()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var e = new GameObject("EventSystem");
            e.transform.SetParent(transform, false);
            e.AddComponent<EventSystem>();
            e.AddComponent<StandaloneInputModule>();
        }

        var c = gameObject.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        var esc = gameObject.AddComponent<CanvasScaler>();
        esc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        esc.referenceResolution = new Vector2(1920, 1080);
        esc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        esc.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        ConstruirPausa();
        ConstruirConfig();

        Transform opcionesPersonalizadas = transform.Find("Panel_Opciones");

        if (opcionesPersonalizadas != null)
        {
            panelOpcionesPersonalizado = opcionesPersonalizadas.gameObject;
            panelOpcionesPersonalizado.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "[MenuPausa] No se encontro Panel_Opciones."
            );
        }

        panelPausa.SetActive(false);
        panelConfig.SetActive(false);
    }

    // ---------------- construccion de la interfaz ----------------

    static RectTransform Nuevo(string nombre, Transform padre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        g.transform.SetParent(padre, false);
        return g.GetComponent<RectTransform>();
    }

    static void Estirar(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static void Centrar(RectTransform r, Vector2 pos, Vector2 tam)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = tam;
    }

    static Image Capa(Transform padre, string nombre, Color color, Sprite sprite = null)
    {
        var r = Nuevo(nombre, padre);
        Estirar(r);
        var i = r.gameObject.AddComponent<Image>();
        i.color = color;
        i.sprite = sprite;
        i.raycastTarget = true;   // bloquea clics al juego
        return i;
    }

    Button CrearBoton(Transform padre, string nombre, Vector2 pos, Vector2 tam, Sprite[] sp, UnityAction accion)
    {
        var r = Nuevo(nombre, padre);
        Centrar(r, pos, tam);
        var img = r.gameObject.AddComponent<Image>();
        img.sprite = sp != null && sp.Length > 0 ? sp[0] : null;
        img.preserveAspect = true;
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        if (sp != null && sp.Length >= 3)
        {
            b.transition = Selectable.Transition.SpriteSwap;
            b.spriteState = new SpriteState { highlightedSprite = sp[1], selectedSprite = sp[1], pressedSprite = sp[2] };
        }
        b.onClick.AddListener(accion);
        r.gameObject.AddComponent<BotonResaltar>();
        r.gameObject.AddComponent<EfectoSeleccion>();
        return b;
    }

    void ConstruirPausa()
    {
        var p = Nuevo("PanelPausa", transform);
        Estirar(p);
        panelPausa = p.gameObject;
        Capa(p, "Oscuro", new Color(0, 0, 0, 0.55f));
        Capa(p, "Fondo", Color.white, fondo);
        btnOpciones = CrearBoton(p, "BotonOpciones", new Vector2(0, 40), new Vector2(360, 125), opciones, AbrirConfig);
        btnSalir = CrearBoton(p, "BotonSalir", new Vector2(0, -110), new Vector2(360, 125), salir, Salir);
    }

    Slider CrearSlider(Transform padre, string nombre, Vector2 pos, string clave, UnityAction<float> extra)
    {
        var r = Nuevo(nombre, padre);
        Centrar(r, pos, new Vector2(400, 34));
        var pista = r.gameObject.AddComponent<Image>();
        pista.color = new Color(0.02f, 0.10f, 0.10f, 1f);

        var rel = Nuevo("Relleno", r);
        Estirar(rel);
        var imgRel = rel.gameObject.AddComponent<Image>();
        imgRel.color = new Color(0.35f, 1f, 0.78f, 1f);

        var man = Nuevo("Mango", r);
        man.sizeDelta = new Vector2(20, 52);
        var imgMan = man.gameObject.AddComponent<Image>();
        imgMan.color = Color.white;

        var s = r.gameObject.AddComponent<Slider>();
        s.fillRect = rel;
        s.handleRect = man;
        s.targetGraphic = imgMan;
        s.transition = Selectable.Transition.ColorTint;
        var col = s.colors;
        col.normalColor = Color.white;
        col.highlightedColor = new Color(0.7f, 1f, 0.9f);
        col.selectedColor = new Color(0.55f, 1f, 0.85f);
        col.pressedColor = new Color(0.4f, 0.9f, 0.7f);
        s.colors = col;
        s.minValue = 0f; s.maxValue = 1f;
        s.value = PlayerPrefs.GetFloat(clave, 1f);
        s.onValueChanged.AddListener(v => { PlayerPrefs.SetFloat(clave, v); if (extra != null) extra(v); });
        r.gameObject.AddComponent<BotonResaltar>();
        return s;
    }

    void ConstruirConfig()
    {
        var p = Nuevo("PanelConfig", transform);
        Estirar(p);
        panelConfig = p.gameObject;
        Capa(p, "Oscuro", new Color(0, 0, 0, 0.55f));
        Capa(p, "Fondo", Color.white, fondo);

        var panel = Nuevo("Panel", p);
        Centrar(panel, new Vector2(0, 40), new Vector2(1100, 563));
        var im = panel.gameObject.AddComponent<Image>();
        im.sprite = fondoConfig;
        im.preserveAspect = true;

        sMusica = CrearSlider(panel, "SliderMusica", new Vector2(240, 75), ClaveMusica, AplicarMusica);
        sEfectos = CrearSlider(panel, "SliderEfectos", new Vector2(240, -24), ClaveEfectos, null);
        sMaestro = CrearSlider(panel, "SliderMaestro", new Vector2(240, -122), ClaveMaestro, v => AudioListener.volume = v);

        btnVolver = CrearBoton(p, "BotonVolver", new Vector2(0, -385), new Vector2(380, 127), volver, CerrarConfig);
    }

    static void AplicarMusica(float v)
    {
        var m = MusicaMenu.Instancia;
        if (m != null) m.AjustarVolumen(v);
    }

    // ---------------- comportamiento ----------------

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enConfig) CerrarConfig();
            else if (pausado) Reanudar();
            else Pausar();
        }

        if (!pausado) return;
        var es = EventSystem.current;
        if (es == null) return;

        bool tecla = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)
                  || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);
        if (es.currentSelectedGameObject == null && tecla) Seleccionar(enConfig ? sMusica.gameObject : btnOpciones.gameObject);

        if (Input.GetKeyDown(KeyCode.Space) && !Input.GetButtonDown("Submit") && es.currentSelectedGameObject != null)
            ExecuteEvents.Execute(es.currentSelectedGameObject, new BaseEventData(es), ExecuteEvents.submitHandler);
    }

    static void Seleccionar(GameObject g)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(g);
    }

    public void Pausar()
    {
        if (pausado) return;
        pausado = true; EstaPausado = true;
        escalaAntes = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        // Mateo no debe poder disparar ni moverse mientras esta en pausa
        bloqueados.Clear();
        foreach (var pc in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            if (pc.enabled) { pc.enabled = false; bloqueados.Add(pc); }

        panelConfig.SetActive(false);
        if (panelOpcionesPersonalizado != null)
            panelOpcionesPersonalizado.SetActive(false);
        panelPausa.SetActive(true);
        enConfig = false;
        Seleccionar(btnOpciones.gameObject);
    }

    public void Reanudar()
    {
        if (!pausado) return;
        pausado = false; EstaPausado = false; enConfig = false;
        panelPausa.SetActive(false);
        panelConfig.SetActive(false);
        if (panelOpcionesPersonalizado != null)
            panelOpcionesPersonalizado.SetActive(false);
        Time.timeScale = escalaAntes;
        foreach (var pc in bloqueados) if (pc != null) pc.enabled = true;
        bloqueados.Clear();
        PlayerPrefs.Save();
        Input.ResetInputAxes();
    }

    public void AbrirConfig()
    {
        enConfig = true;

        panelPausa.SetActive(false);

        if (panelOpcionesPersonalizado != null)
        {
            panelConfig.SetActive(false);
            panelOpcionesPersonalizado.SetActive(true);
        }
        else
        {
            panelConfig.SetActive(true);
            Seleccionar(sMusica.gameObject);
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void CerrarConfig()
    {
        enConfig = false;

        if (panelOpcionesPersonalizado != null)
            panelOpcionesPersonalizado.SetActive(false);

        panelConfig.SetActive(false);
        panelPausa.SetActive(true);

        Seleccionar(btnOpciones.gameObject);
    }

    public void Salir()
    {
        Time.timeScale = 1f;
        EstaPausado = false;
        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaMenu);
    }
    
}

// Pone el menu de pausa en cualquier escena donde haya un PlayerController (Tutorial, niveles 2-4, etc.)
// y aplica el volumen maestro guardado al iniciar.
public static class MenuPausaArranque
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Iniciar()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(MenuPausa.ClaveMaestro, 1f);
        SceneManager.sceneLoaded -= AlCargar;
        SceneManager.sceneLoaded += AlCargar;
    }

    static void AlCargar(Scene escena, LoadSceneMode modo)
    {
        Time.timeScale = 1f;
        if (Object.FindFirstObjectByType<MenuPausa>() != null) return;
        if (Object.FindFirstObjectByType<PlayerController>() == null) return;
        var prefab = Resources.Load<GameObject>("MenuPausa");
        if (prefab == null) { Debug.LogWarning("[Pausa] Falta Assets/Resources/MenuPausa.prefab"); return; }
        var g = Object.Instantiate(prefab);
        g.name = "MenuPausa";
    }
}
