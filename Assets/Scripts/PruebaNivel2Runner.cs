#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Prueba automatica del Nivel 2 (solo en el editor; la lanza el menu Esperanza > PRUEBA paletas Nivel 2)
public class PruebaNivel2 : MonoBehaviour
{
    StringBuilder log = new StringBuilder();
    void L(string s) { log.AppendLine(s); Debug.Log("[PruebaN2] " + s); }

    static GameObject Prefab(string n) { return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Nivel2/" + n + ".prefab"); }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        try { } finally { }
        var pc = FindFirstObjectByType<PlayerController>();
        var rb = pc.GetComponent<Rigidbody2D>();
        var viejo = GameObject.Find("Suelo_VIEJO_borrar"); if (viejo != null) viejo.SetActive(false); // en la prueba solo cuenta el piso nuevo
        var suelo = GameObject.Find("GridPisos/Suelo").GetComponent<Tilemap>();
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/Nivel2/n2_t_00.asset");
        L("tile piso: " + (tile != null) + " | suelo capa=" + LayerMask.LayerToName(suelo.gameObject.layer));

        Vector3 o = pc.transform.position;
        int cx = Mathf.RoundToInt(o.x);
        int fila = suelo.WorldToCell(new Vector3(cx, o.y - 4, 0)).y;
        int c0 = suelo.WorldToCell(new Vector3(cx - 8, 0, 0)).x, c1 = suelo.WorldToCell(new Vector3(cx + 14, 0, 0)).x;
        for (int x = c0; x <= c1; x++) suelo.SetTile(new Vector3Int(x, fila, 0), tile);
        yield return new WaitForSeconds(0.5f);

        // ---- 1) colision del piso
        pc.transform.position = new Vector3(cx, o.y + 3, 0); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(2f);
        var hit = Physics2D.Raycast(new Vector2(cx, o.y + 6), Vector2.down, 30f, 1 << suelo.gameObject.layer);
        float top = hit.point.y;
        var col = pc.GetComponent<Collider2D>();
        L("1) PISO: techo en y=" + top.ToString("F2") + ", pies de Mateo y=" + col.bounds.min.y.ToString("F2") + ", vel.y=" + rb.linearVelocity.y.ToString("F2")
          + (Mathf.Abs(col.bounds.min.y - top) < 0.15f ? "  OK (se para sobre el piso)" : "  FALLO"));

        // ---- 2) escalera
        var esc = Instantiate(Prefab("Escalera_N2"), new Vector3(cx - 6, top + 1f, 0), Quaternion.identity);
        var bc = esc.GetComponent<BoxCollider2D>();
        L("2) ESCALERA: Escalera=" + (esc.GetComponent<Escalera>() != null) + ", trigger=" + bc.isTrigger + ", tam=" + bc.size);
        pc.transform.position = new Vector3(cx - 6, top + 1f, 0); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
        pc.transform.position = new Vector3(cx, top + 1f, 0);

        // ---- 3) plataforma temporal
        var pt = Instantiate(Prefab("PlataformaTemporal_N2"), new Vector3(cx + 4, top + 3.5f, 0), Quaternion.identity);
        var ptc = pt.GetComponent<BoxCollider2D>();
        pc.transform.position = new Vector3(cx + 4, top + 5.5f, 0); rb.linearVelocity = Vector2.zero;
        float t0 = Time.time, tDes = -1, tVuelve = -1;
        while (Time.time - t0 < 8f)
        {
            if (tDes < 0 && !ptc.enabled) tDes = Time.time - t0;
            if (tDes >= 0 && tVuelve < 0 && ptc.enabled) tVuelve = Time.time - t0;
            yield return null;
        }
        L("3) PLATAFORMA TEMPORAL: desaparece a los " + tDes.ToString("F2") + " s, vuelve a los " + tVuelve.ToString("F2") + " s"
          + (tDes > 0 && tVuelve > tDes ? "  OK" : "  FALLO"));
        Destroy(pt);

        // ---- 4) pinchos (1/3 de vida)
        foreach (var n in new[] { "Pinchos_A_N2", "Pinchos_B_N2" })
        {
            var p = Instantiate(Prefab(n), new Vector3(cx + 8, top + 1f, 0), Quaternion.identity);
            var d = p.GetComponent<PinchosDanio>();
            float antes = pc.Vida01;
            pc.transform.position = (Vector3)(Vector2)p.transform.TransformPoint(d.zonaCentro); rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.4f);
            L("4) " + n + ": vida " + antes.ToString("F2") + " -> " + pc.Vida01.ToString("F2") + (Mathf.Abs((antes - pc.Vida01) - 1f / 3f) < 0.02f ? "  OK (1/3)" : "  FALLO"));
            // si Mateo esta de lado del barril/pinchos, pisa la base solida sin dano
            Destroy(p);
            yield return new WaitForSeconds(1.5f); // invulnerabilidad
            pc.transform.position = new Vector3(cx, top + 1f, 0); rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.3f);
        }

        // ---- 5) fuego
        var f = Instantiate(Prefab("Fuego_N2"), new Vector3(cx + 11, top + 0.5f, 0), Quaternion.identity);
        var fb = f.GetComponent<FuegoBarril>();
        var sb = new StringBuilder();
        float ts = Time.time;
        int ult = -1;
        while (Time.time - ts < 2f)
        {
            if (fb.CuadroActual != ult) { ult = fb.CuadroActual; sb.Append(ult + (fb.Peligroso ? "(dano) " : "(seguro) ")); }
            yield return null;
        }
        L("5) FUEGO animacion: " + sb);
        // de lado, sin pasar por encima: sin dano aunque este alto
        var fcol = f.GetComponent<BoxCollider2D>();
        L("   barril solido: " + !fcol.isTrigger + ", zona llama centro=" + fb.zonaCentro + " tam=" + fb.zonaTamano);
        fb.cuadrosPeligrosos = new int[0];
        pc.transform.position = (Vector3)(Vector2)f.transform.TransformPoint(fb.zonaCentro); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        L("   llama BAJA (sin cuadros peligrosos): Mateo muerto=" + pc.EstaMuerto + (pc.EstaMuerto ? "  FALLO" : "  OK"));
        fb.cuadrosPeligrosos = new[] { 0, 1, 2, 3, 4 };
        yield return new WaitForSeconds(0.5f);
        L("   llama ALTA: Mateo muerto=" + pc.EstaMuerto + (pc.EstaMuerto ? "  OK" : "  FALLO"));

        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_nivel2.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}
#endif
