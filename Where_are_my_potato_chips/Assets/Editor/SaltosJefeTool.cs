// =====================================================================
//  SaltosJefeTool.cs   tarjeta E-10 (NavMeshLinks del jefe)
//  COMO USARLO: abrir la escena del nivel (con el NavMesh ya horneado) y
//  Tools > Kit > Generar saltos del jefe (NavMeshLinks). Despues, Ctrl+S.
//
//  Busca bloques de 0.4 a 2.2 m de alto con el techo caminable (parkour,
//  escalones, cajas grandes) y pone un NavMeshLink en cada lado libre,
//  del piso al borde superior. Todo queda en "SaltosJefe_Auto"; al repetir,
//  se borra y se recrea. Ctrl+Z deshace la generacion.
// =====================================================================
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class SaltosJefeTool
{
    const string NOMBRE_RAIZ = "SaltosJefe_Auto";
    const float ALTO_MIN = 0.4f;
    const float ALTO_MAX = 2.2f;
    const float LADO_MIN = 0.8f;
    const float LADO_MAX = 10f;
    const float SEPARACION = 0.7f;      // distancia del pie del link al bloque
    const float ANCHO_MAX = 1.5f;

    [MenuItem("Tools/Kit/Generar saltos del jefe (NavMeshLinks)")]
    public static void Generar()
    {
        Scene escena = SceneManager.GetActiveScene();
        NavMeshSurface surface = Object.FindAnyObjectByType<NavMeshSurface>();
        if (surface == null || NavMesh.CalculateTriangulation().vertices.Length == 0)
        {
            EditorUtility.DisplayDialog("Falta el NavMesh",
                "Esta escena no tiene un NavMesh horneado. Selecciona el objeto con NavMeshSurface, presiona Bake y vuelve a intentar.", "Ok");
            return;
        }

        GameObject viejo = GameObject.Find(NOMBRE_RAIZ);
        if (viejo != null) Undo.DestroyObjectImmediate(viejo);

        GameObject raiz = new GameObject(NOMBRE_RAIZ);
        Undo.RegisterCreatedObjectUndo(raiz, "Generar saltos del jefe");

        var filtro = new NavMeshQueryFilter { agentTypeID = surface.agentTypeID, areaMask = NavMesh.AllAreas };
        var usados = new List<Vector3>();
        int bloques = 0, links = 0;

        foreach (GameObject go in escena.GetRootGameObjects())
            foreach (Collider col in go.GetComponentsInChildren<Collider>())
            {
                if (!EsBloque(col, out Bounds b, out float piso)) continue;

                int antes = links;
                links += CrearLinksDelBloque(raiz.transform, b, piso, surface.agentTypeID, filtro, usados);
                if (links > antes) bloques++;
            }

        EditorSceneManager.MarkSceneDirty(escena);
        Selection.activeGameObject = raiz;
        Debug.Log($"Saltos del jefe: {links} NavMeshLinks en {bloques} bloques. Guarda la escena con Ctrl+S.");
    }

    // Un bloque de parkour: solido, de tamano medio y con piso debajo a menos de 2.2 m del techo.
    static bool EsBloque(Collider col, out Bounds b, out float piso)
    {
        b = col.bounds;
        piso = 0f;
        if (!col.enabled || col.isTrigger || col.GetComponentInParent<HidingSpot>() != null) return false;
        if (b.size.x < LADO_MIN || b.size.z < LADO_MIN || b.size.x > LADO_MAX || b.size.z > LADO_MAX) return false;

        // Piso: lo primero que hay debajo, justo afuera del bloque
        Vector3 afuera = new Vector3(b.min.x - 0.3f, b.max.y + 0.1f, b.center.z);
        if (!Physics.Raycast(afuera, Vector3.down, out RaycastHit hit, ALTO_MAX + 1f, ~0, QueryTriggerInteraction.Ignore))
            return false;
        piso = hit.point.y;

        float alto = b.max.y - piso;
        return alto >= ALTO_MIN && alto <= ALTO_MAX;
    }

    static int CrearLinksDelBloque(Transform padre, Bounds b, float piso, int agente, NavMeshQueryFilter filtro, List<Vector3> usados)
    {
        int creados = 0;
        Vector3[] lados = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

        foreach (Vector3 lado in lados)
        {
            float mitad = Mathf.Abs(lado.x) > 0f ? b.extents.x : b.extents.z;
            float largoBorde = Mathf.Abs(lado.x) > 0f ? b.size.z : b.size.x;

            Vector3 borde = b.center + lado * mitad;
            Vector3 abajo = new Vector3(borde.x, piso, borde.z) + lado * SEPARACION;
            Vector3 arriba = new Vector3(borde.x, b.max.y, borde.z) - lado * Mathf.Min(0.4f, mitad * 0.5f);

            if (!NavMesh.SamplePosition(abajo, out NavMeshHit hitAbajo, 0.6f, filtro)) continue;
            if (!NavMesh.SamplePosition(arriba, out NavMeshHit hitArriba, 0.6f, filtro)) continue;
            if (hitArriba.position.y - hitAbajo.position.y < ALTO_MIN * 0.8f) continue;
            if (YaHayLink(usados, hitAbajo.position)) continue;

            GameObject go = new GameObject("Salto");
            go.transform.SetParent(padre, false);
            NavMeshLink link = go.AddComponent<NavMeshLink>();
            link.agentTypeID = agente;
            link.startPoint = hitAbajo.position;
            link.endPoint = hitArriba.position;
            link.width = Mathf.Min(ANCHO_MAX, largoBorde * 0.8f);
            link.bidirectional = true;

            usados.Add(hitAbajo.position);
            creados++;
        }
        return creados;
    }

    static bool YaHayLink(List<Vector3> usados, Vector3 punto)
    {
        foreach (Vector3 u in usados)
            if ((u - punto).sqrMagnitude < 1f) return true;
        return false;
    }
}
