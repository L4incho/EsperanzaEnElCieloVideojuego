#if UNITY_EDITOR
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;

// Prueba automatica de los infectados (solo editor). La lanza el menu Esperanza > PRUEBA infectados (Play)
public class PruebaInfectados : MonoBehaviour
{
    StringBuilder log = new StringBuilder();
    void L(string s) { log.AppendLine(s); Debug.Log("[PruebaInf] " + s); }
    static GameObject P(string n, string niv) { return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + niv + "/" + n + ".prefab"); }

    PlayerController pc; Rigidbody2D rb; Collider2D mcol; float sueloY; float x0;

    void PonerMateo(float x)
    {
        pc.transform.position = new Vector3(x, sueloY + 1.2f, 0); rb.linearVelocity = Vector2.zero;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        pc = FindFirstObjectByType<PlayerController>();
        rb = pc.GetComponent<Rigidbody2D>(); mcol = pc.GetComponent<Collider2D>();
        x0 = pc.transform.position.x;
        int capa = LayerMask.GetMask("Suelo");
        var h = Physics2D.Raycast(new Vector2(x0, pc.transform.position.y + 2f), Vector2.down, 40f, capa);
        if (h.collider == null) { L("NO hay suelo bajo Mateo"); Terminar(); yield break; }
        sueloY = h.point.y + 100f; x0 += 200f;   // arena de prueba lejos del nivel (sin paredes)
        // piso plano de prueba (el nivel tiene huecos): una losa larga en la capa Suelo
        var losa = new GameObject("LosaPrueba"); losa.layer = LayerMask.NameToLayer("Suelo");
        losa.transform.position = new Vector3(x0 + 10, sueloY - 0.5f, 0);
        losa.AddComponent<BoxCollider2D>().size = new Vector2(60f, 1f);
        // revisar que haya piso plano en +-12 m
        int ok = 0; for (int i = -2; i <= 14; i++) if (Physics2D.Raycast(new Vector2(x0 + i, sueloY + 2f), Vector2.down, 6f, capa).collider != null) ok++;
        L("suelo y=" + sueloY.ToString("F2") + ", columnas con suelo (de 17): " + ok);

        // ---- altura de salto de Mateo
        PonerMateo(x0); yield return new WaitForSeconds(1f);
        float baseY = mcol.bounds.min.y, max = baseY;
        rb.linearVelocity = new Vector2(0, pc.fuerzaSalto);
        float t0 = Time.time; while (Time.time - t0 < 1.5f) { max = Mathf.Max(max, mcol.bounds.min.y); yield return null; }
        float apex = max - baseY;
        L("0) Salto de Mateo: sube " + apex.ToString("F2") + " m (gravedad escala " + rb.gravityScale + ")");

        var cfg = new[] { ("Infectado1_N2", "Nivel2"), ("Infectado2_N2", "Nivel2"), ("Infectado1_N3", "Nivel3"), ("Infectado2_N3", "Nivel3"), ("Infectado3_N3", "Nivel3") };
        foreach (var c in cfg)
        {
            var pf = P(c.Item1, c.Item2);
            L("prefab " + c.Item1 + ": " + (pf != null));
        }

        // ---- 1) solidez + altura (cada tipo)
        foreach (var n in new[] { "Infectado1_N2", "Infectado2_N2", "Infectado3_N3" })
        {
            var go = Instantiate(P(n, n.EndsWith("N2") ? "Nivel2" : "Nivel3"), new Vector3(x0 + 4, sueloY + 0.02f, 0), Quaternion.identity);
            var en = go.GetComponent<EnemigoInfectado>(); en.enabled = false;
            var ecol = go.GetComponent<Collider2D>();
            yield return new WaitForSeconds(0.8f);
            float alto = ecol.bounds.max.y - sueloY;
            bool ign = Physics2D.GetIgnoreCollision(ecol, mcol);
            // mateo encima del infectado (solapado): la fisica debe separarlos
            PonerMateo(go.transform.position.x - 0.15f);
            yield return new WaitForSeconds(0.7f);
            float sep = Mathf.Abs(mcol.bounds.center.x - ecol.bounds.center.x);
            float minSep = mcol.bounds.extents.x + ecol.bounds.extents.x * 0.5f;
            L("1) " + n + ": alto=" + alto.ToString("F2") + " m (Mateo salta " + apex.ToString("F2") + ") " + (alto < apex - 0.2f ? "SE PUEDE SALTAR" : "MUY ALTO")
              + " | ignora colision=" + ign + " | separacion tras solape=" + sep.ToString("F2") + (!ign && ecol.enabled && !ecol.isTrigger ? "  SOLIDO OK" : "  FALLO"));
            Destroy(go);
            yield return new WaitForSeconds(0.2f);
        }

        // ---- 1b) agacharse
        PonerMateo(x0); yield return new WaitForSeconds(1f);
        var srM = pc.GetComponent<SpriteRenderer>();
        float hDe = mcol.bounds.size.y, pie0 = mcol.bounds.min.y, huecoPie = srM.bounds.min.y - mcol.bounds.min.y;
        pc.agacharForzado = true; yield return new WaitForSeconds(0.6f);
        float hAg = mcol.bounds.size.y, pie1 = mcol.bounds.min.y;
        L("1b) AGACHARSE: EstaAgachado=" + pc.EstaAgachado + ", collider " + hDe.ToString("F2") + " -> " + hAg.ToString("F2") + " m, pies " + pie0.ToString("F2") + " -> " + pie1.ToString("F2")
          + ", sprite=" + srM.sprite.name + ", hueco bajo los pies de pie=" + huecoPie.ToString("F2") + " agachado=" + (srM.bounds.min.y - pie1).ToString("F2") + ((pc.EstaAgachado && hAg < hDe * 0.7f && Mathf.Abs(pie0 - pie1) < 0.08f && srM.sprite.name.Contains("_c3") && Mathf.Abs((srM.bounds.min.y - pie1) - huecoPie) < 0.08f) ? "  OK" : "  FALLO"));
        pc.agacharForzado = false; yield return new WaitForSeconds(0.6f);
        L("   al soltar: EstaAgachado=" + pc.EstaAgachado + ", collider " + mcol.bounds.size.y.ToString("F2") + ", sprite=" + srM.sprite.name + (!pc.EstaAgachado && Mathf.Abs(mcol.bounds.size.y - hDe) < 0.05f ? "  OK" : "  FALLO"));

        // ---- 1c) no se puede usar la cabeza de un infectado como plataforma
        foreach (var n in new[] { "Infectado1_N2", "Infectado3_N3", "Infectado2_N2" })
        {
            var go = Instantiate(P(n, n.EndsWith("N2") ? "Nivel2" : "Nivel3"), new Vector3(x0 + 4, sueloY + 0.02f, 0), Quaternion.identity);
            var ecol = go.GetComponent<Collider2D>();
            ((IAturdible)go.GetComponent<EnemigoInfectado>()).Aturdir(6f);
            yield return new WaitForSeconds(0.6f);
            pc.transform.position = new Vector3(ecol.bounds.center.x, ecol.bounds.max.y + 0.9f, 0); rb.linearVelocity = Vector2.zero;
            float tTope = 0f; float t1 = Time.time; bool quieto = false;
            while (Time.time - t1 < 2.5f)
            {
                if (mcol.bounds.min.y > ecol.bounds.max.y - 0.2f && Mathf.Abs(rb.linearVelocity.y) < 0.05f && Mathf.Abs(mcol.bounds.center.x - ecol.bounds.center.x) < ecol.bounds.extents.x) { tTope += Time.deltaTime; if (tTope > 0.5f) quieto = true; } else tTope = 0f;
                yield return null;
            }
            bool abajo = Mathf.Abs(mcol.bounds.min.y - sueloY) < 0.15f;
            L("1c) " + n + ": Mateo cae sobre su cabeza -> " + (quieto ? "SE QUEDO ENCIMA" : "resbala") + ", acaba en el suelo=" + abajo + (!quieto && abajo ? "  OK" : "  FALLO"));
            Destroy(go); yield return new WaitForSeconds(0.3f);
        }

        // ---- 2) danos
        yield return Dano("Infectado1_N2", "Nivel2", 1.0f, 1f / 3f, "1/3", 2.6f);
        yield return Dano("Infectado2_N2", "Nivel2", 6.5f, 2f / 3f, "2/3", 4.5f);
        yield return Dano("Infectado3_N3", "Nivel3", 1.6f, 1f, "toda la vida", 5.5f);
        yield return Dano("Infectado2_N2", "Nivel2", 6.5f, 0f, "0 (esquiva agachado)", 4.5f, true);

        // ---- 3) aturdimiento
        foreach (var n in new[] { "Infectado1_N2", "Infectado3_N3" })
        {
            var go = Instantiate(P(n, n.EndsWith("N2") ? "Nivel2" : "Nivel3"), new Vector3(x0 + 12, sueloY + 0.02f, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            var en = go.GetComponent<EnemigoInfectado>();
            ((IAturdible)en).Aturdir(2f);
            yield return new WaitForSeconds(0.3f);
            var sr = go.GetComponent<SpriteRenderer>();
            bool dano = System.Array.IndexOf(en.animDano, sr.sprite) >= 0;
            L("3) " + n + " tras Aturdir: aturdido=" + en.EstaAturdido + ", sprite de dano=" + dano
              + (n.Contains("3") ? (!en.EstaAturdido && dano ? "  OK (inmune, solo cambia sprite)" : "  FALLO") : (en.EstaAturdido && dano ? "  OK" : "  FALLO")));
            Destroy(go);
        }
        Terminar();
    }

    IEnumerator Dano(string n, string niv, float distancia, float esperado, string txt, float espera, bool agachar = false)
    {
        pc.agacharForzado = false;
        PonerMateo(x0 - 2); yield return new WaitForSeconds(2.2f);
        if (pc.EstaMuerto) yield return new WaitForSeconds(2f);
        pc.CurarTodo();
        PonerMateo(x0);
        if (agachar) { pc.agacharForzado = true; yield return new WaitForSeconds(0.4f); }
        var go = Instantiate(P(n, niv), new Vector3(x0 + distancia, sueloY + 0.02f, 0), Quaternion.identity);
        var en = go.GetComponent<EnemigoInfectado>();
        yield return new WaitForSeconds(0.3f);
        float antes = pc.Vida01; bool murio = false;
        float t0 = Time.time; float tHit = -1; float nextLog = 0f;
        while (Time.time - t0 < espera)
        {
            if (pc.EstaMuerto) { murio = true; break; }
            if (n.Contains("2") && Time.time - t0 > nextLog) { nextLog += 0.2f; var sp = go.GetComponent<SpriteRenderer>().sprite; L("   t=" + (Time.time - t0).ToString("F1") + " enemigo=(" + go.transform.position.x.ToString("F1") + "," + go.transform.position.y.ToString("F1") + ") sprite=" + (sp != null ? sp.name : "-") + " mateo=(" + pc.transform.position.x.ToString("F1") + "," + pc.transform.position.y.ToString("F1") + ")"); }
            if (pc.Vida01 < antes - 0.01f && tHit < 0) tHit = Time.time;
            if (tHit > 0 && Time.time - tHit > 0.4f) break;
            yield return null;
        }
        float tMin = pc.Vida01;
        float perdido = antes - tMin;
        bool ok = esperado >= 1f ? (murio || perdido >= 0.99f) : Mathf.Abs(perdido - esperado) < 0.02f;
        L("2) " + n + (agachar ? " (Mateo AGACHADO)" : "") + ": ataca desde " + distancia + " m; Mateo pierde " + perdido.ToString("F2") + (murio ? " (murio)" : "") + " esperado " + txt + (ok ? "  OK" : "  FALLO")
          + " | enemigo x=" + go.transform.position.x.ToString("F1"));
        Destroy(go);
        yield return new WaitForSeconds(1.5f);
    }

    void Terminar()
    {
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_infectados.txt"), log.ToString());
        EditorApplication.ExitPlaymode();
    }
}
#endif
