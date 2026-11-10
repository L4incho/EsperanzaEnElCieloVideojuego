using UnityEngine;
using UnityEngine.Tilemaps;

// Los tiles "con objeto" (fuego, plataforma temporal) dibujan su propio sprite animado.
// Al empezar el juego este componente apaga el dibujo del tile para que no se vea doble.
// Busca la celda a la que pertenece el objeto (no solo la que cae bajo su posicion): en tilemaps con ancla distinta
// del centro (p. ej. Suelo del Nivel 2, ancla en el borde superior) el objeto nace justo en el borde de la celda
// y la celda "por posicion" seria la de arriba, dejando el tile visible aunque la plataforma desaparezca.
public class OcultarTile : MonoBehaviour
{
    void Start()
    {
        if (!Application.isPlaying) return;
        var tm = GetComponentInParent<Tilemap>();
        if (tm == null) return;
        var baseCelda = tm.WorldToCell(transform.position);
        Vector3Int? encontrada = null;
        for (int dy = -1; dy <= 1 && encontrada == null; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                var c = baseCelda + new Vector3Int(dx, dy, 0);
                if (tm.GetInstantiatedObject(c) == gameObject) { encontrada = c; break; }
            }
        var celda = encontrada ?? baseCelda;
        tm.SetTileFlags(celda, TileFlags.None);
        tm.SetColor(celda, new Color(1f, 1f, 1f, 0f));
    }
}
