using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MazeBuilder : MonoBehaviour
{
    [Header("Medidas")]
    public float cell = 2f;
    public float wallHeight = 4f;
    public float poolDepth = 0.3f;        // debe ser menor que el Step Height del jefe (0,4)
    public float waterLevel = -0.08f;
    public float storeOffsetZ = 4f;       // qué tan al norte se coloca la tienda

    [Header("Materiales (URP Lit)")]
    public Material wallMat, floorMat, poolMat, waterMat, ceilingMat, lightMat, columnMat, blockMat;

    [Header("Prefabs opcionales")]
    public GameObject hidingSpotPrefab;
    public GameObject replicaStorePrefab;

    [Header("Techo y luces")]
    public bool addCeiling = true;
    public int panelEvery = 4;            // un panel de luz cada N celdas
    public int lightEveryNPanels = 10;    // luz real cada N paneles (0 = ninguna)

    [Header("Parkour")]
    public bool createLinks = true;       // crea los NavMesh Links de los bloques K y b

    [TextArea(24, 24)]
    public string map =
@"###########################################
#p.B...########......#......########..EE.p#
#.wwww...............................wwww.#
#.wwww....p......o.o...o.o......p....wwww.#
#.wwww.#.#..#H#......#......#H#..#.#.wwww.#
#......#.#..###.p....#....p.###..#.#......#
##..######..######..###..######..######..##
##..###p......p###..###..###p......p###..##
##..H##.o....o.##H..###..H##.o....o.##H..##
##..###..wwww..###.p###p.###..wwww..###..##
##bb###..wwww.......###.......wwww..###bb##
##..###..wwww.......###.......wwww..###..##
##..###..wwww..###..###..###..wwww..###..##
##p.....o....o.###..###..###.o....o.....p##
##.....p......p###..###..###p......p.....##
##..######..###p.....p.....p###..######..##
#......###..###.o....o....o.###..###......#
#.......K...p.................p...K.......#
#..o.op.K.........wwwwwww.........K.po.o..#
#......##.##H##...wwwwwww...##H##.##......#
#......##.#####...wwwwwww...#####.##......#
##..###########po.wwwwwww.op##########..###
#......##.#####...wwwwwww...#####.##......#
#......##.##H##...wwwwwww...##H##.##......#
#..o.op.K.........wwwwwww.........K.po.o..#
#.......K...p.................p...K.......#
#......###..###.o....o....o.###..###......#
##..######..###p.....p.....p###..######..##
##.....p......p###..###..###p......p.....##
##p.....o....o.###..###..###.o....o.....p##
##..###..wwww..###..###..###..wwww..###..##
##..###..wwww.......###.......wwww..###..##
##bb###..wwww.......###.......wwww..###bb##
##..###..wwww..###.p###p.###..wwww..###..##
##..H##.o....o.##H..###..H##.o....o.##H..##
##..###p......p###..###..###p......p###..##
##..######..######..###..######..######..##
#......#.#..###.p....#....p.###..#.#......#
#.wwww.#.#..#H#.............#H#..#.#.wwww.#
#.wwww....p......o.o...o.o......p....wwww.#
#.wwww...............#...............wwww.#
#pS....########......#......########.....p#
###########################################";

    [ContextMenu("Build")]
    public void Build()
    {
        Clear();
        string[] rows = map.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int h = rows.Length, w = rows[0].Length;
        foreach (var row in rows)
            if (row.Length != w) { Debug.LogError("MazeBuilder: todas las filas del mapa deben tener el mismo largo."); return; }

        int ground = LayerMask.NameToLayer("Ground");
        int ignore = LayerMask.NameToLayer("Ignore Raycast");
        int hideLayer = LayerMask.NameToLayer("HidingSpot");
        if (ground < 0) Debug.LogWarning("MazeBuilder: falta la capa Ground (la crea Grover en C-03).");

        Transform geo = Group("Geometry"), water = Group("Water"),
                  gameplay = Group("Gameplay"), patrol = Group("PatrolPoints");

        Vector3 Pos(int r, int c) => new Vector3((c + 0.5f) * cell, 0f, (h - r - 0.5f) * cell);
        bool Opening(int r, int c) => rows[r][c] == '#' && r + 1 < h && rows[r + 1][c] == 'E';
        bool IsFloor(int r, int c) => (rows[r][c] != '#' && rows[r][c] != 'w') || Opening(r, c);
        bool IsWall(int r, int c) => rows[r][c] == '#' && !Opening(r, c);
        bool IsWater(int r, int c) => rows[r][c] == 'w';

        // junta celdas seguidas de una fila en un solo objeto (menos objetos = más rendimiento)
        void Runs(Func<int, int, bool> test, Action<int, int, int> make)
        {
            for (int r = 0; r < h; r++)
            {
                int c = 0;
                while (c < w)
                {
                    if (!test(r, c)) { c++; continue; }
                    int s = c;
                    while (c < w && test(r, c)) c++;
                    make(r, s, c - s);
                }
            }
        }

        // 1) fondo de las piscinas
        Box(geo, PrimitiveType.Cube, "PoolBottom",
            new Vector3(w * cell / 2f, -poolDepth - 0.1f, h * cell / 2f),
            new Vector3(w * cell, 0.2f, h * cell), poolMat, ground);

        // 2) pisos
        Runs(IsFloor, (row, start, len) =>
            Box(geo, PrimitiveType.Cube, $"Floor_{row}_{start}",
                new Vector3((start + len / 2f) * cell, -poolDepth / 2f, (h - row - 0.5f) * cell),
                new Vector3(len * cell, poolDepth, cell), floorMat, ground));

        // 3) paredes
        Runs(IsWall, (row, start, len) =>
            Box(geo, PrimitiveType.Cube, $"Wall_{row}_{start}",
                new Vector3((start + len / 2f) * cell, (wallHeight - poolDepth) / 2f, (h - row - 0.5f) * cell),
                new Vector3(len * cell, wallHeight + poolDepth, cell), wallMat, -1));

        // 4) agua (superficie + zona trigger)
        Runs(IsWater, (row, start, len) =>
        {
            var root = new GameObject($"Water_{row}_{start}");
            root.transform.SetParent(water, false);
            root.transform.localPosition = new Vector3((start + len / 2f) * cell, 0f, (h - row - 0.5f) * cell);
            if (ignore >= 0) root.layer = ignore;
            var bc = root.AddComponent<BoxCollider>();
            bc.isTrigger = true; bc.center = new Vector3(0f, -0.05f, 0f); bc.size = new Vector3(len * cell, 0.6f, cell);
            root.AddComponent<WaterZone>();

            var surf = GameObject.CreatePrimitive(PrimitiveType.Quad);
            surf.name = "Surface";
            DestroyImmediate(surf.GetComponent<Collider>());
            surf.transform.SetParent(root.transform, false);
            surf.transform.localPosition = new Vector3(0f, waterLevel, 0f);
            surf.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            surf.transform.localScale = new Vector3(len * cell, cell, 1f);
            if (ignore >= 0) surf.layer = ignore;
            var rd = surf.GetComponent<Renderer>();
            if (waterMat) rd.sharedMaterial = waterMat;
            rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            surf.AddComponent<WaterScroll>();
        });

        // 5) objetos celda por celda
        int patrolN = 0;
        var eCells = new List<Vector3>();
        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
            {
                Vector3 p = Pos(r, c);
                switch (rows[r][c])
                {
                    case 'K':
                        Box(geo, PrimitiveType.Cube, $"ParkourBlock_2m_{r}_{c}", p + Vector3.up,
                            new Vector3(cell, 2f, cell), blockMat ? blockMat : wallMat, ground);
                        break;
                    case 'b':
                        Box(geo, PrimitiveType.Cube, $"ParkourBlock_1m_{r}_{c}", p + Vector3.up * 0.5f,
                            new Vector3(cell, 1f, cell), blockMat ? blockMat : wallMat, ground);
                        break;
                    case 'o':
                        Box(geo, PrimitiveType.Cylinder, $"Column_{r}_{c}", p + Vector3.up * wallHeight / 2f,
                            new Vector3(0.9f, wallHeight / 2f, 0.9f), columnMat ? columnMat : wallMat, -1);
                        break;
                    case 'S': Marker(gameplay, "PlayerSpawn", p + Vector3.up * 0.1f); break;
                    case 'B': Marker(gameplay, "BossSpawn", p + Vector3.up * 0.1f); break;
                    case 'p': Marker(patrol, $"Patrol_{++patrolN:00}", p + Vector3.up * 0.1f); break;
                    case 'E': eCells.Add(p); break;
                    case 'H':
                        {
                            GameObject hs = null;
#if UNITY_EDITOR
                            if (hidingSpotPrefab) hs = (GameObject)PrefabUtility.InstantiatePrefab(hidingSpotPrefab);
#else
                            if (hidingSpotPrefab) hs = Instantiate(hidingSpotPrefab);
#endif
                            if (hs != null)
                            {
                                hs.name = $"HidingSpot_{r}_{c}";
                                hs.transform.SetParent(gameplay, false);
                                hs.transform.localPosition = p;
                            }
                            else
                            {
                                Box(gameplay, PrimitiveType.Cube, $"HidingSpot_{r}_{c}", p + Vector3.up * 0.5f,
                                    new Vector3(1.2f, 1f, 1.2f), null, hideLayer, false);
                            }
                        }
                        break;
                }
            }

        // 6) tienda réplica (al norte de la salida)
        if (eCells.Count > 0)
        {
            Vector3 avg = Vector3.zero;
            foreach (var e in eCells) avg += e;
            avg /= eCells.Count;
            avg += Vector3.forward * storeOffsetZ;
            GameObject st = null;
#if UNITY_EDITOR
            if (replicaStorePrefab) st = (GameObject)PrefabUtility.InstantiatePrefab(replicaStorePrefab);
#else
            if (replicaStorePrefab) st = Instantiate(replicaStorePrefab);
#endif
            if (st != null) { st.name = "ReplicaStore"; st.transform.SetParent(gameplay, false); st.transform.localPosition = avg; }
            else Marker(gameplay, "ReplicaStore_Anchor", avg);
        }

        // 7) techo, paneles y luces
        if (addCeiling)
        {
            var ceil = Box(geo, PrimitiveType.Cube, "Ceiling",
                new Vector3(w * cell / 2f, wallHeight + 0.1f, h * cell / 2f),
                new Vector3(w * cell, 0.2f, h * cell), ceilingMat ? ceilingMat : wallMat, -1, false);
            NoShadow(ceil);

            int pe = Mathf.Max(2, panelEvery), panels = 0;
            for (int r = 1; r < h - 1; r++)
                for (int c = 1; c < w - 1; c++)
                {
                    if (r % pe != 1 || c % pe != 1) continue;
                    if (rows[r][c] == '#' || rows[r][c] == 'o') continue;
                    Vector3 p = Pos(r, c);
                    var pn = Box(geo, PrimitiveType.Cube, $"Panel_{r}_{c}",
                        new Vector3(p.x, wallHeight - 0.03f, p.z), new Vector3(1.4f, 0.06f, 0.7f), lightMat, -1, false);
                    NoShadow(pn);
                    if (lightEveryNPanels > 0 && panels++ % lightEveryNPanels == 0)
                    {
                        var lg = new GameObject("PanelLight");
                        lg.transform.SetParent(geo, false);
                        lg.transform.localPosition = new Vector3(p.x, wallHeight - 0.5f, p.z);
                        var L = lg.AddComponent<Light>();
                        L.type = LightType.Point; L.range = 12f; L.intensity = 1.5f;
                        L.color = new Color(0.85f, 0.97f, 1f); L.shadows = LightShadows.None;
                    }
                }
        }

        // 8) NavMesh Links de los bloques de parkour
        if (createLinks) BuildLinks(rows, h, w);

        Debug.Log($"MazeBuilder: {w}x{h} celdas = {w * cell}x{h * cell} m, {patrolN} puntos de patrulla.");
    }

    // Crea 2 links por bloque (piso -> cima, cima -> piso) usando reflexión,
    // así el script compila aunque el paquete AI Navigation no esté instalado.
    void BuildLinks(string[] rows, int h, int w)
    {
        Type linkType = Type.GetType("Unity.AI.Navigation.NavMeshLink, Unity.AI.Navigation");
        if (linkType == null)
        {
            Debug.LogWarning("MazeBuilder: instala el paquete AI Navigation para crear los NavMesh Links.");
            return;
        }
        int agentId = 0; bool found = false;
        for (int i = 0; i < UnityEngine.AI.NavMesh.GetSettingsCount(); i++)
        {
            var s = UnityEngine.AI.NavMesh.GetSettingsByIndex(i);
            if (UnityEngine.AI.NavMesh.GetSettingsNameFromID(s.agentTypeID) == "Boss")
            { agentId = s.agentTypeID; found = true; break; }
        }
        if (!found) Debug.LogWarning("MazeBuilder: no existe el Agent Type 'Boss'; los links usarán el tipo por defecto.");

        Transform grp = Group("Links");
        var seen = new bool[h, w];
        int n = 0;
        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        for (int r0 = 0; r0 < h; r0++)
            for (int c0 = 0; c0 < w; c0++)
            {
                char ch = rows[r0][c0];
                if ((ch != 'K' && ch != 'b') || seen[r0, c0]) continue;

                int minR = r0, maxR = r0, minC = c0, maxC = c0;
                var q = new Queue<Vector2Int>();
                q.Enqueue(new Vector2Int(r0, c0)); seen[r0, c0] = true;
                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    minR = Mathf.Min(minR, p.x); maxR = Mathf.Max(maxR, p.x);
                    minC = Mathf.Min(minC, p.y); maxC = Mathf.Max(maxC, p.y);
                    foreach (var d in dirs)
                    {
                        int rr = p.x + d.x, cc = p.y + d.y;
                        if (rr < 0 || cc < 0 || rr >= h || cc >= w || seen[rr, cc] || rows[rr][cc] != ch) continue;
                        seen[rr, cc] = true; q.Enqueue(new Vector2Int(rr, cc));
                    }
                }

                int cellsR = maxR - minR + 1, cellsC = maxC - minC + 1;
                bool alongX = cellsC <= cellsR;                    // el pasillo corre de este a oeste
                float depth = alongX ? cellsC : cellsR;
                float across = alongX ? cellsR : cellsC;
                Vector3 center = new Vector3(((minC + maxC) / 2f + 0.5f) * cell, 0f,
                                             (h - (minR + maxR) / 2f - 0.5f) * cell);
                Vector3 axis = alongX ? Vector3.right : Vector3.forward;
                float reach = (depth / 2f + 0.5f) * cell;
                Vector3 top = center + Vector3.up * (ch == 'K' ? 2f : 1f);
                float width = Mathf.Min(3f, across * cell - 1f);

                n++;
                MakeLink(grp, $"Link_{ch}_{n}_up", center - axis * reach, top, width, agentId, linkType);
                MakeLink(grp, $"Link_{ch}_{n}_down", top, center + axis * reach, width, agentId, linkType);
            }
    }

    void MakeLink(Transform parent, string n, Vector3 a, Vector3 b, float width, int agentId, Type t)
    {
        var go = new GameObject(n);
        go.transform.SetParent(parent, false);
        var comp = go.AddComponent(t);
        void Set(string prop, object v)
        {
            var pi = t.GetProperty(prop);
            if (pi != null && pi.CanWrite) pi.SetValue(comp, v);
        }
        Set("startPoint", a); Set("endPoint", b); Set("width", width);
        Set("bidirectional", true); Set("agentTypeID", agentId);
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) DestroyImmediate(transform.GetChild(i).gameObject);
    }

    Transform Group(string n)
    {
        var t = new GameObject(n).transform;
        t.SetParent(transform, false);
        return t;
    }

    void Marker(Transform parent, string n, Vector3 pos)
    {
        var go = new GameObject(n);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
    }

    static void NoShadow(GameObject go)
    {
        var rd = go.GetComponent<Renderer>();
        if (rd) rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    GameObject Box(Transform parent, PrimitiveType prim, string n, Vector3 pos, Vector3 scale,
                   Material mat, int layer, bool collider = true)
    {
        var go = GameObject.CreatePrimitive(prim);
        go.name = n;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        if (mat) go.GetComponent<Renderer>().sharedMaterial = mat;
        if (layer >= 0) go.layer = layer;
        if (!collider) { var col = go.GetComponent<Collider>(); if (col) DestroyImmediate(col); }
#if UNITY_EDITOR
        go.isStatic = true;
#endif
        return go;
    }
}