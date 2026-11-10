using UnityEngine;

// Objeto "Escalera de mano": Mateo la sube con W y la baja con S (ver PlayerController).
// Uso: arrastra el prefab "Escalera" a la escena y ponle el sprite que quieras en el Sprite Renderer.
// El área de la escalera (BoxCollider2D trigger) se ajusta sola al tamaño del sprite.
// Para una escalera más larga: Sprite Renderer > Draw Mode = Tiled y cambia Size (alto).
[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class Escalera : MonoBehaviour
{
    [Tooltip("Si está activo, el área de la escalera se adapta sola al sprite.")]
    public bool ajustarAreaAlSprite = true;

    void Reset() { Ajustar(); }
    void OnValidate() { Ajustar(); }

    void Update()
    {
        if (!Application.isPlaying) Ajustar();
    }

    void Ajustar()
    {
        var sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        if (!col.isTrigger) col.isTrigger = true;
        if (!ajustarAreaAlSprite || sr == null || sr.sprite == null) return;

        Vector2 tam, offset;
        if (sr.drawMode == SpriteDrawMode.Simple)
        {
            tam = sr.sprite.bounds.size;
            offset = sr.sprite.bounds.center;
        }
        else
        {
            tam = sr.size;
            offset = Vector2.zero;
        }
        if (col.size != tam) col.size = tam;
        if (col.offset != offset) col.offset = offset;
    }

    // Dibuja el área de la escalera en verde dentro del editor
    void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.9f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.offset, col.size);
    }
}
