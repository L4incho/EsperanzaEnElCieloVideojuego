#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class DebugTilemap
{
    [MenuItem("Esperanza/Debug Tilemap")]
    public static void Volcar()
    {
        var sb = new StringBuilder();
        foreach (var t in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            sb.AppendLine("TILEMAP " + t.name + " origin=" + t.origin + " size=" + t.size + " cell=" + t.cellSize + " anchor=" + t.tileAnchor + " pos=" + t.transform.position);
            BoundsInt b = t.cellBounds;
            for (int y = b.yMax - 1; y >= b.yMin; y--)
                for (int x = b.xMin; x < b.xMax; x++)
                {
                    var tile = t.GetTile(new Vector3Int(x, y, 0));
                    if (tile == null) continue;
                    var sp = t.GetSprite(new Vector3Int(x, y, 0));
                    string tam = sp != null ? sp.bounds.size.ToString("F2") + " pivot=" + sp.pivot : "-";
                    sb.AppendLine(x + "," + y + "," + tile.name + ",size=" + tam + ",col=" + t.GetColliderType(new Vector3Int(x, y, 0)));
                }
        }
        foreach (var m in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            var c = m.GetComponent<Collider2D>();
            sb.AppendLine("MATEO pos=" + m.transform.position + " bounds=" + c.bounds);
        }
        System.IO.File.WriteAllText("tilemap_debug.txt", sb.ToString());
        Debug.Log("tilemap_debug.txt escrito");
    }
}
#endif
