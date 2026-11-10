#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text;

// Prueba automatica (solo para verificar; se puede borrar): en Play simula tomar gemas, usar K, cambiar de gema y morir.
public static class PruebaGemas
{
    static StringBuilder log;
    static int paso;
    static double t0;
    static PlayerController m;
    static GemaVerde verde, roja, azul;
    static EnemigoInfectado dummy;
    static Vector3 pRoja, pAzul, pVerde;
    static float xAntes;

    static void L(string s) { log.AppendLine(s); Debug.Log("[PruebaGemas] " + s); }

    static void Mover(Vector3 p)
    {
        m.transform.position = p;
        m.GetComponent<Rigidbody2D>().position = p;
        m.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }

    static string Estado()
    {
        return "gema=" + (m.GemaActual != null ? m.GemaActual.tipo.ToString() : "ninguna")
            + " dobleSalto=" + m.dobleSalto
            + " traje=" + (m.GetComponent<Animator>().runtimeAnimatorController != null ? m.GetComponent<Animator>().runtimeAnimatorController.name : "null")
            + " verdeActiva=" + verde.gameObject.activeSelf + " rojaActiva=" + roja.gameObject.activeSelf + " azulActiva=" + azul.gameObject.activeSelf
            + " vida=" + m.Vida01 + " cdK=" + m.HabilidadRestante;
    }

    [MenuItem("Esperanza/PRUEBA gemas (Play)")]
    public static void Iniciar()
    {
        if (!Application.isPlaying) { Debug.LogError("Entra a Play primero"); return; }
        log = new StringBuilder();
        m = Object.FindFirstObjectByType<PlayerController>();
        foreach (var g in Object.FindObjectsByType<GemaVerde>(FindObjectsSortMode.None))
        {
            if (g.tipo == TipoGema.Verde) { verde = g; pVerde = g.transform.position; }
            if (g.tipo == TipoGema.Roja) { roja = g; pRoja = g.transform.position; }
            if (g.tipo == TipoGema.Azul) { azul = g; pAzul = g.transform.position; }
        }
        dummy = Object.FindFirstObjectByType<EnemigoInfectado>();
        L("Inicio: verde=" + pVerde + " roja=" + pRoja + " azul=" + pAzul + " mateo=" + m.transform.position);
        paso = 0; t0 = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }

    static void Esperar(double s) { t0 = EditorApplication.timeSinceStartup + s; paso++; }

    static void Tick()
    {
        if (!Application.isPlaying || m == null) { Fin("Play detenido"); return; }
        if (EditorApplication.timeSinceStartup < t0) return;
        switch (paso)
        {
            case 0: L("1) Mateo toma la ROJA"); Mover(pRoja); Esperar(0.8); break;
            case 1:
                L(Estado());
                if (dummy != null)
                {
                    var rb = dummy.GetComponent<Rigidbody2D>();
                    Vector3 p = m.transform.position + new Vector3(1.6f, 0f, 0f);
                    dummy.transform.position = p; rb.position = p; rb.linearVelocity = Vector2.zero;
                    Physics2D.SyncTransforms();
                    xAntes = dummy.transform.position.x;
                }
                Esperar(0.3); break;
            case 2:
                L("2) K con la roja: onda");
                m.SendMessage("UsarHabilidad");
                Esperar(0.5); break;
            case 3:
                if (dummy != null) L("Dummy se movio de x=" + xAntes + " a x=" + dummy.transform.position.x + " (velocidad " + dummy.GetComponent<Rigidbody2D>().linearVelocity + ")");
                L(Estado());
                L("3) Mateo toma la AZUL (la roja debe volver a su sitio)"); Mover(pAzul); Esperar(0.8); break;
            case 4:
                L(Estado() + " | rojaPos=" + roja.transform.position + " (original " + pRoja + ")");
                xAntes = m.transform.position.x;
                L("4) K con la azul: dash");
                m.SendMessage("UsarHabilidad");
                Esperar(0.12); break;
            case 5:
                L("En el dash: x=" + m.transform.position.x + " (antes " + xAntes + ") invulnerable: probando golpe...");
                m.RecibirGolpe(m.transform.position + Vector3.left);
                L("Vida despues de golpe en dash = " + m.Vida01 + " (debe seguir 1)");
                Esperar(0.6); break;
            case 6:
                L("Tras el dash: x=" + m.transform.position.x + " " + Estado());
                m.RecibirGolpe(m.transform.position + Vector3.left);
                L("Vida despues de golpe fuera del dash = " + m.Vida01 + " (debe bajar)");
                L("5) Mateo muere con la azul");
                m.Morir(); Esperar(1.8); break;
            case 7:
                L(Estado() + " | azulPos=" + azul.transform.position + " (original " + pAzul + ") mateo=" + m.transform.position);
                L("6) Mateo toma la VERDE");
                Mover(pVerde); Esperar(0.8); break;
            case 8:
                L(Estado());
                L("7) Mateo toma la ROJA (la verde debe volver a su sitio y perder el doble salto)");
                Mover(pRoja); Esperar(0.8); break;
            case 9:
                L(Estado());
                L("8) Muere con la roja; luego toma la verde y muere: la verde NO debe volver");
                m.Morir(); Esperar(1.8); break;
            case 10:
                L(Estado());
                Mover(pVerde); Esperar(0.8); break;
            case 11:
                L(Estado());
                m.Morir(); Esperar(1.8); break;
            case 12:
                L("Tras morir con la verde: " + Estado());
                Fin("OK"); break;
        }
    }

    static void Fin(string motivo)
    {
        EditorApplication.update -= Tick;
        L("FIN: " + motivo);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../prueba_gemas.txt"), log.ToString());
    }
}
#endif
