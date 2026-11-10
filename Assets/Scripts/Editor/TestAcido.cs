#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TestAcido
{
    [MenuItem("Esperanza/Diagnostico acido")]
    public static void Diagnostico()
    {
        foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            int n = 0; string lista = "";
            foreach (var p in tm.cellBounds.allPositionsWithin)
            {
                var t = tm.GetTile(p);
                if (t != null && t.name.StartsWith("Acido")) { n++; lista += p + " "; }
            }
            if (n > 0) Debug.Log("[Diag] " + tm.name + " tiles acido=" + n + " celdas " + lista + " cellSize=" + tm.cellSize + " hijos=" + tm.transform.childCount);
        }
        if (!Application.isPlaying) return;
        string s = "";
        for (float y = 1f; y >= -2.5f; y -= 0.25f)
        {
            var c = Physics2D.OverlapPointAll(new Vector2(4.5f, y), ~0);
            string n=""; foreach (var k in c) if (k.name != "LimitesCamara") n += k.name + "+"; s += y.ToString("0.00") + ":" + (n == "" ? "-" : n) + "  ";
        }
        Debug.Log("[Diag] x=4.5 " + s);
    }
}
#endif
