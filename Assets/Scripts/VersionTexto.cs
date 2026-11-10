using UnityEngine;
using UnityEngine.UI;

// Muestra la version del juego (Project Settings > Player > Version) en un Text.
[RequireComponent(typeof(Text))]
public class VersionTexto : MonoBehaviour
{
    public string prefijo = "Versión ";

    void Start()
    {
        GetComponent<Text>().text = prefijo + Application.version;
    }
}
