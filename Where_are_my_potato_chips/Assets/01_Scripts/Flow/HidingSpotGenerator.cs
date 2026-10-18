using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Si un nivel tiene pocos escondites, crea algunos al cargar: busca rincones del NavMesh
/// (lugares con pared en dos o tres lados), pone cajas de cartón como señal y un HidingSpot.
/// No toca la escena guardada, así que no choca con lo que el equipo edite en ella.
/// </summary>
public static class HidingSpotGenerator
{
    const int MinimumSpots = 4;
    const int TargetSpots = 8;
    const float MinSpacing = 9f;
    const float KeepAwayFromStart = 10f;
    const float WallCheck = 1.6f;
    const int Seed = 4711;

    static readonly Vector3[] Directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

    public static void EnsureSpots()
    {
        if (Object.FindObjectsByType<HidingSpot>(FindObjectsSortMode.None).Length >= MinimumSpots) return;

        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0) return;

        var avoid = new List<Vector3>();
        var player = Object.FindAnyObjectByType<PlayerState>();
        if (player != null) avoid.Add(player.transform.position);
        var replica = Object.FindAnyObjectByType<ReplicaCheckout>();
        if (replica != null) avoid.Add(replica.transform.position);

        var placed = new List<Vector3>();
        var random = new System.Random(Seed);
        var candidates = Centroids(triangulation);

        // Primero rincones con pared en tres lados (callejones), luego esquinas
        for (int walls = 3; walls >= 2 && placed.Count < TargetSpots; walls--)
        {
            for (int i = 0; i < 1500 && placed.Count < TargetSpots; i++)
            {
                Vector3 point = candidates[random.Next(candidates.Count)];
                if (TooClose(point, placed, MinSpacing) || TooClose(point, avoid, KeepAwayFromStart)) continue;
                if (!IsNook(point, walls, out Vector3 back)) continue;

                Create(point, back, placed.Count);
                placed.Add(point);
            }
        }

        Debug.Log($"HidingSpotGenerator: {placed.Count} escondites creados en este nivel.");
    }

    static List<Vector3> Centroids(NavMeshTriangulation mesh)
    {
        var points = new List<Vector3>(mesh.indices.Length / 3);
        for (int i = 0; i < mesh.indices.Length; i += 3)
            points.Add((mesh.vertices[mesh.indices[i]] + mesh.vertices[mesh.indices[i + 1]] + mesh.vertices[mesh.indices[i + 2]]) / 3f);
        return points;
    }

    // Cuántas de las cuatro direcciones tienen pared cerca; 'back' apunta hacia el fondo del rincón
    static bool IsNook(Vector3 point, int requiredWalls, out Vector3 back)
    {
        back = Vector3.zero;
        Vector3 origin = point + Vector3.up * 0.6f;
        if (Physics.Raycast(origin, Vector3.up, 1.6f, ~0, QueryTriggerInteraction.Ignore)) return false;

        int walls = 0;
        bool previousBlocked = false, firstBlocked = false, adjacent = false;
        for (int i = 0; i < Directions.Length; i++)
        {
            bool blocked = Physics.Raycast(origin, Directions[i], WallCheck, ~0, QueryTriggerInteraction.Ignore);
            if (blocked)
            {
                walls++;
                back += Directions[i];
                if (previousBlocked) adjacent = true;
            }
            if (i == 0) firstBlocked = blocked;
            previousBlocked = blocked;
        }
        if (previousBlocked && firstBlocked) adjacent = true;

        // Dos paredes enfrentadas son un pasillo, no un rincón
        if (walls != requiredWalls || (walls == 2 && !adjacent)) return false;
        back.Normalize();
        return true;
    }

    static bool TooClose(Vector3 point, List<Vector3> others, float distance)
    {
        foreach (var other in others)
            if ((other - point).sqrMagnitude < distance * distance) return true;
        return false;
    }

    static void Create(Vector3 point, Vector3 back, int index)
    {
        var root = new GameObject("Escondite_Auto_" + index);
        root.transform.SetPositionAndRotation(point, Quaternion.LookRotation(-back));

        int layer = LayerMask.NameToLayer("HidingSpot");
        if (layer >= 0) root.layer = layer;

        var zone = root.AddComponent<BoxCollider>();
        zone.isTrigger = true;
        zone.center = new Vector3(0f, 0.8f, 0f);
        zone.size = new Vector3(1.6f, 1.6f, 1.6f);
        root.AddComponent<HidingSpot>();

        // Cajas apiladas contra el fondo: la señal visual de que ahí se puede uno esconder
        var cardboard = new Color(0.55f, 0.4f, 0.24f);
        Box(root.transform, new Vector3(-0.35f, 0.35f, -0.55f), new Vector3(0.7f, 0.7f, 0.5f), cardboard);
        Box(root.transform, new Vector3(0.4f, 0.3f, -0.6f), new Vector3(0.6f, 0.6f, 0.45f), cardboard * 0.85f);
        Box(root.transform, new Vector3(-0.3f, 0.9f, -0.6f), new Vector3(0.5f, 0.4f, 0.4f), cardboard * 1.1f);
    }

    static void Box(Transform parent, Vector3 localPosition, Vector3 size, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "Caja";
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPosition;
        box.transform.localScale = size;
        box.GetComponent<Renderer>().material.color = color;
        Object.Destroy(box.GetComponent<Collider>());
    }
}
