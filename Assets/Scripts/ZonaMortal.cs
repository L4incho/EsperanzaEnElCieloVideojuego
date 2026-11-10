using UnityEngine;

// Marca un objeto como zona mortal (acido, etc.): si Mateo la toca, muere.
// Se pone en un Tilemap con TilemapCollider2D (Is Trigger) o en cualquier objeto con un Collider2D trigger.
public class ZonaMortal : MonoBehaviour
{
    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }
}
