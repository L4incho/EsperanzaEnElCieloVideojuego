#if UNITY_EDITOR
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
public static class DiagPuertaN3
{
    [MenuItem("Esperanza/ZZ Diagnostico puerta N3")]
    public static void Run()
    {
        var sb = new StringBuilder();
        float maxX = -1e9f;
        var lst = new System.Collections.Generic.List<(string t, string n, Vector3 c, Vector3 s)>();
        foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                var spr = tm.GetSprite(pos); if (spr == null) continue;
                var mat = tm.GetTransformMatrix(pos);
                Vector3 esc = Vector3.Scale(tm.transform.lossyScale, mat.lossyScale);
                Vector3 c = tm.transform.TransformPoint(tm.CellToLocal(pos) + Vector3.Scale(tm.tileAnchor, tm.cellSize)) + Vector3.Scale(spr.bounds.center, esc);
                Vector3 s = Vector3.Scale(spr.bounds.size, esc);
                lst.Add((tm.name, spr.name, c, s)); if (c.x > maxX) maxX = c.x;
            }
        foreach (var e in lst.Where(e => e.c.x > maxX - 12f && e.c.y > 18f).OrderByDescending(e => e.c.y))
            sb.AppendLine(e.t + " | " + e.n + " | c=" + e.c.ToString("F2") + " | size=" + e.s.ToString("F2"));
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (sr.sprite != null && sr.bounds.center.x > maxX - 12f && sr.bounds.center.y > 18f) sb.AppendLine("SR " + sr.name + " | " + sr.sprite.name + " | c=" + sr.bounds.center.ToString("F2") + " | size=" + sr.bounds.size.ToString("F2"));
        sb.Insert(0, "maxX=" + maxX + "\n");
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../diag_puerta_n3.txt"), sb.ToString());
        Debug.Log("[DiagN3] listo");
    }
}
#endif
