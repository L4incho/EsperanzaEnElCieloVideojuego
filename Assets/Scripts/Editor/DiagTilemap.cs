#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
public static class DiagTilemap
{
    [MenuItem("Esperanza/Diagnostico tilemap nivel")]
    public static void Run()
    {
        var go = GameObject.Find("Tilemap");
        var tm = go.GetComponent<Tilemap>();
        var r = go.GetComponent<TilemapRenderer>();
        Debug.Log("bounds=" + tm.localBounds + " rendBounds=" + r.bounds + " enabled=" + r.enabled + " cellSize=" + tm.layoutGrid.cellSize + " tiles=" + tm.GetUsedTilesCount());
        foreach (var p in tm.cellBounds.allPositionsWithin)
        {
            var t = tm.GetTile(p); if (t == null) continue;
            Debug.Log(p + " tile=" + t.name + " sprite=" + tm.GetSprite(p) + " m=" + tm.GetTransformMatrix(p).lossyScale + " col=" + tm.GetColor(p) + " flags=" + tm.GetTileFlags(p));
            break;
        }
    }
}
#endif
