
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AsignadorIDsMicrochips
{
    [MenuItem("Esperanza/Microchips/Asignar IDs automaticamente")]
    public static void AsignarIDs()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Deten Play antes de asignar IDs."
            );
            return;
        }

        Scene escena = SceneManager.GetActiveScene();

        if (!escena.IsValid() || !escena.isLoaded)
        {
            Debug.LogError(
                "No hay una escena valida abierta."
            );
            return;
        }

        // Buscar todos los microchips de la escena,
        // incluyendo objetos desactivados.
        List<Microchip> microchips = new List<Microchip>();

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            microchips.AddRange(
                raiz.GetComponentsInChildren<Microchip>(true)
            );
        }

        HashSet<string> idsUsados = new HashSet<string>();

        int asignados = 0;
        int existentes = 0;
        int duplicados = 0;

        // Registrar IDs ya existentes.
        foreach (Microchip microchip in microchips)
        {
            if (microchip == null)
                continue;

            string id = microchip.IdUnico;

            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (!idsUsados.Add(id))
            {
                duplicados++;
            }
        }

        if (duplicados > 0)
        {
            EditorUtility.DisplayDialog(
                "IDs duplicados",
                "Se encontraron microchips con IDs repetidos. " +
                "No se realizo ningun cambio. " +
                "Revisa los duplicados antes de continuar.",
                "Entendido"
            );

            Debug.LogError(
                "Se encontraron " + duplicados +
                " IDs duplicados en la escena."
            );

            return;
        }

        // Asignar IDs solamente a los microchips
        // que todavia no tienen uno.
        foreach (Microchip microchip in microchips)
        {
            if (microchip == null)
                continue;

            if (!string.IsNullOrWhiteSpace(microchip.IdUnico))
            {
                existentes++;
                continue;
            }

            string nuevoId;

            do
            {
                nuevoId = escena.name + "_" +
                    Guid.NewGuid().ToString("N");
            }
            while (!idsUsados.Add(nuevoId));

            Undo.RecordObject(
                microchip,
                "Asignar ID unico al microchip"
            );

            microchip.AsignarIdEditor(nuevoId);

            EditorUtility.SetDirty(microchip);

            asignados++;
        }

        if (asignados > 0)
        {
            EditorSceneManager.MarkSceneDirty(escena);
        }

        Debug.Log(
            "MICROCHIPS | Escena: " + escena.name +
            " | Total: " + microchips.Count +
            " | IDs nuevos: " + asignados +
            " | IDs existentes: " + existentes
        );

        EditorUtility.DisplayDialog(
            "Microchips identificados",
            "Escena: " + escena.name +
            "\nMicrochips encontrados: " + microchips.Count +
            "\nIDs nuevos: " + asignados +
            "\nIDs conservados: " + existentes +
            "\n\nGuarda la escena con Ctrl + S.",
            "Aceptar"
        );
    }
}

#endif
