using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// B2: viste el almacén ya construido sin tocar "Almacen_Generado", donde viven las luces de A4.
/// Pone carteles, cajas eléctricas, alarmas, manchas, estantes y cuatro detalles que están mal,
/// todo dentro de "Almacen_Decoracion". Con la misma semilla siempre sale igual.
/// Ubica paredes y techos con raycasts sobre los colliders de la escena, así no depende del mapa.
/// Después de correrlo hay que hornear la NavMesh (los estantes y la silla tienen collider).
/// </summary>
public static class AlmacenDecorBuilder
{
    const string ScenePath = "Assets/00_Scenes/10_Warehouse.unity";
    const string RootName = "Almacen_Decoracion";
    const string ModelsFolder = "Assets/04_Models/Almacen";
    const string DecoMaterials = "Assets/03_Materials/Almacen";
    const string KitMaterials = "Assets/03_Materials/Kit";
    const string KitPrefabs = "Assets/02_Prefabs/Environment";
    const int Seed = 2002;
    const int GridSize = 32;
    const float Cell = 2f;

    // Cantidades: pocas y repartidas, que el almacén siga viéndose vacío
    const int AisleSigns = 8, StaffSigns = 3, PowerBoxes = 8, Alarms = 6, Racks = 8, CeilingStains = 18, WetSigns = 3;
    const float GrimeChance = 0.12f;

    struct Face
    {
        public Vector3 Point, Normal, CellCenter;
        public float Floor;
    }

    [MenuItem("Tools/Kit/Decorar almacén (B2)")]
    public static void BuildFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!Build()) return;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Almacén decorado. Falta hornear la NavMesh (botón Bake del objeto NavMesh) y guardar.");
    }

    /// <summary>Recrea la decoración en la escena abierta. Devuelve false si el almacén no está construido.</summary>
    public static bool Build()
    {
        var generated = GameObject.Find("Almacen_Generado");
        if (generated == null)
        {
            Debug.LogError("AlmacenDecorBuilder: no encontré Almacen_Generado. Primero Tools > Kit > Construir almacen (N-02).");
            return false;
        }
        var old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        Physics.SyncTransforms();
        Random.InitState(Seed);

        var root = new GameObject(RootName).transform;
        var exclusions = Exclusions(generated);
        var cells = WalkableCells(exclusions);
        var faces = WallFaces(cells);
        Shuffle(faces);
        var used = new List<Vector3>();

        PlaceAisleSigns(Group(root, "Pasillos"), faces, used);
        PlaceWallItems(Group(root, "Paredes"), faces, used);
        PlaceWrongExit(Group(root, "Mal_Salida"), faces, used);
        PlaceRacks(Group(root, "Estantes"), faces, used);
        PlaceDoorToNowhere(Group(root, "Mal_Puerta"), faces, used);
        PlaceGrime(Group(root, "Mugre"), faces, used);
        PlaceCeiling(Group(root, "Techo"), cells);
        PlaceDeadEnds(Group(root, "Ramas"), generated);
        PlaceChair(Group(root, "Mal_Silla"), cells);

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = true;
        return true;
    }

    // ---------- Dónde se puede poner cosas ----------

    // La tienda réplica es zona segura y el spawn tiene que quedar despejado
    static List<Bounds> Exclusions(GameObject generated)
    {
        var list = new List<Bounds>();
        foreach (Transform t in generated.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "PlayerSpawn") list.Add(new Bounds(t.position, new Vector3(8f, 6f, 8f)));
            if (t.name.StartsWith("ReplicaStore"))
            {
                var b = new Bounds(t.position, Vector3.zero);
                foreach (var r in t.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
                b.Expand(3f);
                list.Add(b);
            }
        }
        return list;
    }

    static List<Vector3> WalkableCells(List<Bounds> exclusions)
    {
        var cells = new List<Vector3>();
        float half = (GridSize - 1) / 2f;
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
            {
                var center = new Vector3((c - half) * Cell, 0f, (half - r) * Cell);
                if (Excluded(center, exclusions)) continue;
                // Celda libre: nada sólido entre 0.3 y 2.3 m, y suelo debajo
                if (Physics.CheckBox(center + Vector3.up * 1.3f, new Vector3(0.9f, 1f, 0.9f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (!Physics.Raycast(center + Vector3.up * 1f, Vector3.down, out var floor, 1.5f, ~0, QueryTriggerInteraction.Ignore)) continue;
                cells.Add(new Vector3(center.x, floor.point.y, center.z));
            }
        return cells;
    }

    static bool Excluded(Vector3 p, List<Bounds> exclusions)
    {
        foreach (var b in exclusions)
            if (p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z) return true;
        return false;
    }

    static readonly Vector3[] Directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

    static List<Face> WallFaces(List<Vector3> cells)
    {
        var faces = new List<Face>();
        foreach (var cell in cells)
            foreach (var dir in Directions)
                if (Physics.Raycast(cell + Vector3.up * 1.2f, dir, out var hit, Cell * 0.65f, ~0, QueryTriggerInteraction.Ignore) && IsWall(hit.collider))
                    faces.Add(new Face { Point = new Vector3(hit.point.x, cell.y, hit.point.z), Normal = hit.normal, CellCenter = cell, Floor = cell.y });
        return faces;
    }

    static bool IsWall(Collider c) =>
        c.name.StartsWith("Bloque_") || (c.transform.parent != null && c.transform.parent.name.StartsWith("Kit_Pared"));

    // Pasillo ancho: del lado de la pared hacia adentro hay al menos `depth` metros libres
    static bool IsWide(Face f, float depth) =>
        !Physics.Raycast(f.Point + Vector3.up * 1.2f + f.Normal * 0.05f, f.Normal, depth, ~0, QueryTriggerInteraction.Ignore);

    static bool FarFrom(Vector3 p, List<Vector3> used, float distance)
    {
        foreach (var u in used)
            if ((u - p).sqrMagnitude < distance * distance) return false;
        return true;
    }

    // Toma la siguiente cara que cumpla la condición y quede lejos de lo ya puesto
    static bool Take(List<Face> faces, List<Vector3> used, float spacing, System.Func<Face, bool> ok, out Face face)
    {
        for (int i = 0; i < faces.Count; i++)
        {
            var f = faces[i];
            if (!FarFrom(f.Point, used, spacing) || (ok != null && !ok(f))) continue;
            faces.RemoveAt(i);
            used.Add(f.Point);
            face = f;
            return true;
        }
        face = default;
        return false;
    }

    // ---------- Paredes ----------

    // Números de pasillo del 01 al 07 y otro 07 más adelante: algo que está mal
    static void PlaceAisleSigns(Transform parent, List<Face> faces, List<Vector3> used)
    {
        var sheet = Load<Material>(KitMaterials + "/Kit_Decal_AisleNumbers.mat");
        for (int i = 0; i < AisleSigns; i++)
        {
            if (!Take(faces, used, 10f, null, out var f)) break;
            int number = Mathf.Min(i, 7); // la celda 7 de la hoja es el 07 repetido
            var mat = AisleMaterial(sheet, number);
            WallQuad(parent, "Pasillo_" + (number == 7 ? "07_otra_vez" : (number + 1).ToString("00")), f, 2.25f, new Vector2(0.55f, 0.55f), mat);
        }
    }

    static Material AisleMaterial(Material sheet, int cellIndex)
    {
        string path = KitMaterials + "/Kit_Decal_Pasillo_" + (cellIndex + 1) + ".mat";
        var mat = Load<Material>(path);
        if (mat != null || sheet == null) return mat != null ? mat : sheet;
        mat = new Material(sheet);
        int col = cellIndex % 4, row = cellIndex / 4;
        mat.SetTextureScale("_BaseMap", new Vector2(0.25f, 0.5f));
        mat.SetTextureOffset("_BaseMap", new Vector2(col * 0.25f, row == 0 ? 0.5f : 0f));
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void PlaceWallItems(Transform parent, List<Face> faces, List<Vector3> used)
    {
        var staff = Load<Material>(KitMaterials + "/Kit_Decal_StaffOnly.mat");
        var rust = Load<Material>(KitMaterials + "/Kit_Decal_RustDrip.mat");

        for (int i = 0; i < StaffSigns; i++)
            if (Take(faces, used, 14f, null, out var f)) WallQuad(parent, "Cartel_StaffOnly", f, 1.65f, new Vector2(0.9f, 0.45f), staff);

        for (int i = 0; i < PowerBoxes; i++)
        {
            if (!Take(faces, used, 7f, null, out var f)) break;
            var box = Model(parent, "Caja_Electrica", new Vector3(0.38f, 0.5f, 0.12f));
            box.transform.SetPositionAndRotation(f.Point + Vector3.up * (f.Floor + 1.4f) + f.Normal * 0.01f, Quaternion.LookRotation(f.Normal));
            if (Random.value < 0.5f) WallQuad(parent, "Oxido", f, 0.85f, new Vector2(0.35f, 0.7f), rust);
        }

        for (int i = 0; i < Alarms; i++)
        {
            if (!Take(faces, used, 9f, null, out var f)) break;
            var alarm = Model(parent, "Alarma_Incendio", new Vector3(0.12f, 0.16f, 0.07f));
            alarm.transform.SetPositionAndRotation(f.Point + Vector3.up * (f.Floor + 1.5f) + f.Normal * 0.04f, Quaternion.LookRotation(f.Normal));
        }
    }

    // Cartel de salida en una esquina, con la flecha apuntando a la pared de al lado
    static void PlaceWrongExit(Transform parent, List<Face> faces, List<Vector3> used)
    {
        var exit = Load<Material>(KitMaterials + "/Kit_Decal_Exit.mat");
        bool ArrowHitsWall(Face f)
        {
            Vector3 right = Vector3.Cross(Vector3.up, -f.Normal); // derecha de quien mira el cartel
            return Physics.Raycast(f.CellCenter + Vector3.up * 1.2f, right, Cell * 0.65f, ~0, QueryTriggerInteraction.Ignore);
        }
        if (Take(faces, used, 6f, ArrowHitsWall, out var face))
            WallQuad(parent, "Salida_Hacia_La_Pared", face, 2.3f, new Vector2(0.8f, 0.3f), exit);
    }

    // Marco de puerta pegado a un bloque: una puerta que no lleva a ningún lado
    static void PlaceDoorToNowhere(Transform parent, List<Face> faces, List<Vector3> used)
    {
        var prefab = Load<GameObject>(KitPrefabs + "/Kit_Puerta.prefab");
        if (prefab == null || !Take(faces, used, 6f, f => IsWide(f, 3f), out var face)) return;
        var door = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        door.name = "Puerta_A_Ningun_Lado";
        door.transform.SetParent(parent, false);
        door.transform.SetPositionAndRotation(new Vector3(face.Point.x, face.Floor, face.Point.z) + face.Normal * 0.1f, Quaternion.LookRotation(face.Normal));
    }

    static void PlaceGrime(Transform parent, List<Face> faces, List<Vector3> used)
    {
        var grime = Load<Material>(KitMaterials + "/Kit_Decal_Grime.mat");
        foreach (var f in faces)
            if (Random.value < GrimeChance && FarFrom(f.Point, used, 1f))
                WallQuad(parent, "Mugre", f, 0.25f, new Vector2(Random.Range(1.2f, 1.9f), 0.5f), grime);
    }

    // ---------- Piso ----------

    // Estantes solo donde el pasillo es ancho: en los de 2 m el jefe y el jugador quedarían apretados
    static void PlaceRacks(Transform parent, List<Face> faces, List<Vector3> used)
    {
        for (int i = 0; i < Racks; i++)
        {
            if (!Take(faces, used, 4f, f => IsWide(f, 3.5f), out var f)) break;
            var rack = Rack(parent);
            rack.transform.SetPositionAndRotation(new Vector3(f.Point.x, f.Floor, f.Point.z) + f.Normal * 0.02f, Quaternion.LookRotation(f.Normal));

            float[] shelves = { 0.14f, 0.74f, 1.34f };
            foreach (float h in shelves)
            {
                if (Random.value < 0.35f) continue;
                bool crate = Random.value < 0.3f;
                var item = Model(rack.transform, crate ? "Cajon_Plastico" : "Caja_Carton", crate ? new Vector3(0.6f, 0.3f, 0.4f) : new Vector3(0.39f, 0.34f, 0.52f), false);
                item.transform.localPosition = new Vector3(Random.Range(-0.6f, 0.6f), h, crate ? 0.05f : 0.25f);
                item.transform.localRotation = Quaternion.Euler(0f, crate ? 0f : 90f + Random.Range(-8f, 8f), 0f);
            }
        }
    }

    static void PlaceCeiling(Transform parent, List<Vector3> cells)
    {
        var stain = Load<Material>(KitMaterials + "/Kit_Decal_WaterStain.mat");
        var shuffled = new List<Vector3>(cells);
        Shuffle(shuffled);
        var used = new List<Vector3>();
        int wet = 0;
        foreach (var cell in shuffled)
        {
            if (used.Count >= CeilingStains) break;
            if (!FarFrom(cell, used, 6f)) continue;
            if (!Physics.Raycast(cell + Vector3.up * 1.2f, Vector3.up, out var ceiling, 4f, ~0, QueryTriggerInteraction.Ignore)) continue;
            used.Add(cell);

            float size = Random.Range(1.4f, 2.4f);
            var q = Quad(parent, "Mancha_Humedad", stain, new Vector2(size, size));
            q.transform.SetPositionAndRotation(ceiling.point - Vector3.up * 0.01f,
                Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Quaternion.LookRotation(Vector3.up));

            // Debajo de algunas goteras, el cartel de piso mojado (y el piso está seco)
            if (wet < WetSigns)
            {
                wet++;
                var sign = Model(parent, "Cartel_PisoMojado", new Vector3(0.3f, 0.63f, 0.36f), true);
                sign.transform.SetPositionAndRotation(cell + new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f)),
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            }
        }
    }

    // Ramas sin salida: una caja más sobre la caja grande que ya está
    static void PlaceDeadEnds(Transform parent, GameObject generated)
    {
        foreach (Transform t in generated.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("Rama_")) continue;
            var big = t.Find("Caja1");
            if (big == null) continue;
            var box = Model(parent, "Caja_Carton", new Vector3(0.39f, 0.34f, 0.52f), true);
            box.transform.SetPositionAndRotation(big.position + Vector3.up * 0.5f, Quaternion.Euler(0f, Random.Range(-20f, 20f), 0f));
        }
    }

    // Silla mirando a la esquina exterior de una curva en L: se ve al doblar y deja libre el lado de adentro
    static void PlaceChair(Transform parent, List<Vector3> cells)
    {
        var shuffled = new List<Vector3>(cells);
        Shuffle(shuffled);
        foreach (var cell in shuffled)
        {
            var blocked = new List<Vector3>();
            foreach (var d in Directions)
                if (Physics.Raycast(cell + Vector3.up * 1.2f, d, Cell * 0.65f, ~0, QueryTriggerInteraction.Ignore)) blocked.Add(d);
            if (blocked.Count != 2 || Vector3.Dot(blocked[0], blocked[1]) < -0.5f) continue;

            var corner = (blocked[0] + blocked[1]).normalized;
            var chair = Model(parent, "Silla_Plastico", new Vector3(0.64f, 0.88f, 0.63f), true);
            chair.name = "Silla_Mirando_La_Esquina";
            chair.transform.SetPositionAndRotation(cell + corner * 0.6f, Quaternion.LookRotation(corner));
            return;
        }
    }

    // ---------- Piezas ----------

    // Modelo del kit si ya está el FBX; si no, un cubo provisorio con su textura (se reemplaza al volver a correr)
    static GameObject Model(Transform parent, string name, Vector3 placeholderSize, bool collider = false)
    {
        var mat = Load<Material>(DecoMaterials + "/Deco_" + name + ".mat");
        var fbx = Load<GameObject>(ModelsFolder + "/" + name + "/" + name + ".fbx");
        GameObject go;
        if (fbx != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            if (mat != null)
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    r.sharedMaterials = mats;
                }
        }
        else
        {
            go = new GameObject(name + "_Provisorio");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(go.transform, false);
            cube.transform.localPosition = new Vector3(0f, placeholderSize.y / 2f, placeholderSize.z / 2f - 0.02f);
            cube.transform.localScale = placeholderSize;
            if (mat != null) cube.GetComponent<Renderer>().sharedMaterial = mat;
        }
        go.name = fbx != null ? name : go.name;
        go.transform.SetParent(parent, false);

        if (collider)
        {
            var bounds = new Bounds(go.transform.position, Vector3.zero);
            bool first = true;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                if (first) { bounds = r.bounds; first = false; }
                else bounds.Encapsulate(r.bounds);
            }
            var box = go.AddComponent<BoxCollider>();
            box.center = go.transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size;
        }
        return go;
    }

    // Estante provisorio armado con piezas finas hasta que llegue el FBX del estante
    static GameObject Rack(Transform parent)
    {
        var fbx = Load<GameObject>(ModelsFolder + "/Estante_Metal/Estante_Metal.fbx");
        if (fbx != null) return Model(parent, "Estante_Metal", Vector3.one, true);

        var mat = Load<Material>(DecoMaterials + "/Deco_Estante_Metal.mat");
        var rack = new GameObject("Estante_Metal_Provisorio");
        rack.transform.SetParent(parent, false);
        const float width = 2f, depth = 0.5f, height = 2f;
        foreach (float x in new[] { -width / 2f + 0.03f, width / 2f - 0.03f })
            foreach (float z in new[] { 0.03f, depth - 0.03f })
                Piece(rack.transform, "Poste", new Vector3(x, height / 2f, z), new Vector3(0.05f, height, 0.05f), mat);
        foreach (float y in new[] { 0.1f, 0.7f, 1.3f, 1.9f })
            Piece(rack.transform, "Repisa", new Vector3(0f, y, depth / 2f), new Vector3(width, 0.04f, depth), mat);
        var box = rack.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, height / 2f, depth / 2f);
        box.size = new Vector3(width, height, depth);
        return rack;
    }

    static void Piece(Transform parent, string name, Vector3 position, Vector3 size, Material mat)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = size;
        if (mat != null) cube.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // Quad pegado a la pared, mirando al pasillo
    static void WallQuad(Transform parent, string name, Face f, float height, Vector2 size, Material mat)
    {
        var q = Quad(parent, name, mat, size);
        q.transform.SetPositionAndRotation(f.Point + Vector3.up * (f.Floor + height) + f.Normal * 0.012f, Quaternion.LookRotation(-f.Normal));
    }

    static GameObject Quad(Transform parent, string name, Material mat, Vector2 size)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = name;
        Object.DestroyImmediate(q.GetComponent<Collider>());
        q.transform.SetParent(parent, false);
        q.transform.localScale = new Vector3(size.x, size.y, 1f);
        var r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;
        return q;
    }

    static Transform Group(Transform root, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        return go.transform;
    }

    static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
