using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Los pisos y paredes de los niveles son cubos estirados, así que una textura se estiraría con ellos.
/// Al cargar el nivel, a cada cubo texturizado se le da una malla con UV según su tamaño real:
/// el azulejo queda del mismo tamaño en una pared de 2 m y en una de 30 m.
/// </summary>
public static class SurfaceTiling
{
    const float MetersPerRepeat = 2f;
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    public static void Apply()
    {
        var meshes = new Dictionary<Vector3, Mesh>();
        foreach (var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
        {
            Mesh source = filter.sharedMesh;
            if (source == null || source.name != "Cube" || source.vertexCount != 24) continue;
            if (!filter.TryGetComponent(out Renderer renderer) || !HasTexture(renderer.sharedMaterial)) continue;

            Vector3 size = Abs(filter.transform.lossyScale);
            Vector3 key = new Vector3(Round(size.x), Round(size.y), Round(size.z));
            if (!meshes.TryGetValue(key, out Mesh tiled))
            {
                tiled = Tiled(source, size);
                meshes.Add(key, tiled);
            }
            filter.sharedMesh = tiled;
        }
    }

    static bool HasTexture(Material material) =>
        material != null && material.HasProperty(BaseMap) && material.GetTexture(BaseMap) != null;

    // Proyección por cara: cada cara usa los dos ejes que la recorren, medidos en metros
    static Mesh Tiled(Mesh source, Vector3 size)
    {
        var mesh = Object.Instantiate(source);
        mesh.name = "Cube_Tiled";
        Vector3[] vertices = source.vertices;
        Vector3[] normals = source.normals;
        var uvs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 p = Vector3.Scale(vertices[i], size) / MetersPerRepeat;
            Vector3 n = normals[i];
            if (Mathf.Abs(n.x) > 0.5f) uvs[i] = new Vector2(p.z, p.y);
            else if (Mathf.Abs(n.y) > 0.5f) uvs[i] = new Vector2(p.x, p.z);
            else uvs[i] = new Vector2(p.x, p.y);
        }

        mesh.uv = uvs;
        return mesh;
    }

    static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    static float Round(float value) => Mathf.Round(value * 100f) / 100f;
}
