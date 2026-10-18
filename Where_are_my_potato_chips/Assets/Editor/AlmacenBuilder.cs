// =====================================================================
//  AlmacenBuilder.cs  v3   (Dev 4 - Entornos)   tarjeta N-02 (Almacen)
//  DONDE PONERLO:  Assets/Editor/AlmacenBuilder.cs   (reemplaza el anterior)
//  ANTES DE USARLO: Tools > Kit > Generar kit y tienda (crea las piezas).
//  COMO USARLO:    Tools > Kit > Construir almacen (N-02)   y despues, Bake.
//
//  LO QUE PIDE LA TARJETA N-02 Y COMO SE CUMPLE:
//   - 64 x 64 m ............. 16 x 16 losas de 4 m (v3: el doble de grande que la v2)
//   - pasillos de 2 m minimo . el mapa de la ZONA 2 usa celdas de 2 m: un pasillo
//                              es 1 celda, o sea 2 m exactos
//   - ramas sin salida ...... marcadas con H en el mapa (6), cada una con escondite
//   - zona de parkour con NavMeshLink (max 2,2 m): sala central con plataforma de
//                              2,0 m, escalones de 1 m y 4 NavMeshLink para el jefe
//   - ReplicaStore al final . al norte, con la puerta mirando al pasillo-galeria
//   - NavMeshSurface ........ objeto "NavMesh" listo; solo falta pulsar Bake
//
//  RUTAS DEL JUEGO: spawn al sur -> ReplicaStore al norte. El mapa es un laberinto
//  con varios caminos (la ruta mas corta mide ~115 m) y la sala de parkour al centro.
//  La puerta del sur es solo decorativa: queda cerrada detras del jugador.
//  Todo queda en "Almacen_Generado". Al repetir, se borra y se recrea.
// =====================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AlmacenBuilder
{
    // ================== ZONA 1: AJUSTES (EDITA AQUI) ==================
    const string RUTA_ESCENA     = "Assets/00_Scenes/10_Warehouse.unity";
    const string CARPETA_PREFABS = "Assets/02_Prefabs/Environment";
    const string RUTA_REPLICA    = "Assets/02_Prefabs/ReplicaStore.prefab";
    const string CARPETA_MATS    = "Assets/03_Materials";
    const string NOMBRE_RAIZ     = "Almacen_Generado";

    const int   LOSAS = 16;                // 16 x 16 losas de 4 m = 64 x 64 m
    const float CELDA = 2f;                // lado de cada celda del mapa (= ancho de pasillo)
    const float ALTO_MURO = 3f;            // altura de los bloques interiores
    const float PROB_LUZ_APAGADA = 0.75f;  // 0 = todas encendidas, 1 = todas apagadas (pocas luces = mejor rendimiento en WebGL)

    static readonly Vector3 POS_TIENDA = new Vector3(0f, 0f, 28f);   // ocupa z 24..32
    static readonly Vector3 POS_SPAWN  = new Vector3(1f, 0.1f, -31f);

    // BossSpawn lejos del jugador (noroeste) y puntos de patrulla repartidos por el laberinto
    static readonly Vector3 POS_BOSS_SPAWN = new Vector3(-29f, 0.1f, 17f);
    static readonly Vector3[] PUNTOS_PATRULLA =
    {
        new Vector3(-19f, 0.1f, 29f), new Vector3(19f, 0.1f, 29f),     // norte
        new Vector3(-21f, 0.1f, 9f),  new Vector3(21f, 0.1f, 9f),      // centro
        new Vector3(-21f, 0.1f, -3f), new Vector3(13f, 0.1f, -3f),
        new Vector3(-13f, 0.1f, -11f), new Vector3(9f, 0.1f, -19f),    // sur
        new Vector3(-25f, 0.1f, -23f), new Vector3(17f, 0.1f, -27f)
    };

    // Sala de parkour (centro del almacen). El NavMeshLink salva MAX 2,2 m.
    static readonly Vector3 POS_PARKOUR = Vector3.zero;
    const float ALTO_PLATAFORMA = 2.0f;    // no pasar de 2,2 (limite de la tarjeta)
    const float ANCHO_LINK = 1.5f;

    // ================== ZONA 2: MAPA DEL ALMACEN (EDITA AQUI) =========
    //  32 x 32 celdas de 2 m.  Arriba = NORTE.
    //  #  bloque (pared interior)      .  pasillo de 2 m
    //  H  rama sin salida con escondite   T  espacio de ReplicaStore
    static readonly string[] MAPA =
    {
        "##############TTTT##############",   // r0   T = ReplicaStore (norte)
        "#............#TTTT#...........##",
        "#.#.#.###.####TTTT#.#####.###.##",
        "#.#.#...#....#TTTT#.......#...##",
        "#.#.###.###...........###.#.####",   // r4   galeria frente a la tienda
        "#H#...#.#.#.#...#...#...#.#...##",
        "#####.#.#.#.#.###.#.###.#.#.#.##",
        "#...#.#...#.#.....#.......#...##",
        "#.#.#.###.#.#####.#######.#.#.##",
        "#.#....H#.#...#.............#.H#",
        "#.#.#.###.###.#.#####.#.###.#.##",
        "#.#.#...#.......#.............##",
        "#.#.#.#.#.#.#.#.#.#.#.###.#.#.##",
        "#.#...#...#.#...#.#...#H#.#...##",
        "#.#.###.#.#.........#.#.#.###.##",   // r14  sala de parkour (r14 a r17, c12 a c19)
        "#.#...#...#.............#...#..#",
        "#.#.#.###.#.........#####.#.####",
        "#...#...................#.#...##",
        "#.###.#########.#.###.#.###.#.##",
        "#.............#...#.......#.#.##",
        "###.#.#.#####.#.#.#.#####.###.##",
        "#...#.#.#...#...#.#...........##",
        "#.###.#.#.#.#.#######.#.###.#.##",
        "#.....#.#.#...#.....#H#...#.#.H#",
        "#####.#.#.###.#.###.#####.#.#.##",
        "#.........#...#...#...#...#....#",
        "#.#########.###.#.###.#.###.####",
        "#.......#.....#...#...#...#...##",
        "#.###.#.#####.#...#.#####.###.##",
        "#.............#...........#...##",
        "#####.#####.###..###############",
        "################.###############"    // r31  entrada (spawn)

    };

    // ================== ZONA 3: MENU PRINCIPAL ========================
    [MenuItem("Tools/Kit/Construir almacen (N-02)")]
    public static void Construir()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(CARPETA_PREFABS + "/Kit_Pared.prefab") == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_REPLICA) == null)
        {
            EditorUtility.DisplayDialog("Faltan prefabs",
                "Primero ejecuta Tools > Kit > Generar kit y tienda (A-01 + A-02).", "Ok");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene escena = EditorSceneManager.OpenScene(RUTA_ESCENA, OpenSceneMode.Single);
        GameObject viejo = GameObject.Find(NOMBRE_RAIZ);
        if (viejo != null) Object.DestroyImmediate(viejo);

        GameObject raiz = new GameObject(NOMBRE_RAIZ);
        Material bloque = Mat("Almacen_Bloque", new Color(0.45f, 0.42f, 0.38f));
        Material carton = Mat("Almacen_Carton", new Color(0.55f, 0.4f, 0.22f));
        Material metal  = Mat("Almacen_Metal",  new Color(0.3f, 0.33f, 0.38f));

        // Cada linea es una parte. Comenta una para no construirla.
        CrearSuelosYTechos(raiz.transform);
        CrearParedesPerimetro(raiz.transform);
        CrearInterior(raiz.transform, bloque, carton);
        CrearParkour(raiz.transform, carton, metal);
        CrearTienda(raiz.transform);
        Vacio("PlayerSpawn", raiz.transform, POS_SPAWN);
        CrearBossYPatrulla(raiz.transform);
        CrearNavMesh(raiz.transform);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = raiz;
        Debug.Log("Almacen construido. El NavMesh se esta horneando: cuando termine, guarda la escena (Ctrl+S).");
    }

    // ================== ZONA 4: PARTES DEL ALMACEN ====================

    // Suelos (capa Ground) y techos con luz. La sala de parkour NO lleva techo:
    // el jefe mide 2,2 m y una plataforma de 2 m con techo a 3 m no le dejaria
    // espacio para caminar encima (el NavMesh no la reconoceria).
    static void CrearSuelosYTechos(Transform padre)
    {
        GameObject suelos = Vacio("Suelos", padre, Vector3.zero);
        GameObject techos = Vacio("Techos", padre, Vector3.zero);
        Random.InitState(5);
        int capaGround = LayerMask.NameToLayer("Ground");

        for (int i = 0; i < LOSAS; i++)
            for (int j = 0; j < LOSAS; j++)
            {
                float x = (i - (LOSAS - 1) / 2f) * 4f;
                float z = (j - (LOSAS - 1) / 2f) * 4f;

                GameObject s = Poner(suelos.transform, "Kit_Suelo", new Vector3(x, 0, z), 0f);
                if (s != null && capaGround >= 0) CapaRecursiva(s, capaGround);

                bool enParkour = Mathf.Abs(x - POS_PARKOUR.x) < 8f && Mathf.Abs(z - POS_PARKOUR.z) < 4f;
                if (enParkour) continue;

                GameObject t = Poner(techos.transform, "Kit_Techo", new Vector3(x, 0, z), 0f);
                if (t != null && Random.value < PROB_LUZ_APAGADA)
                {
                    Light l = t.GetComponentInChildren<Light>();
                    if (l != null) l.enabled = false;
                }
            }

        // Luces para la sala de parkour (sin techo): dos puntos altos
        CrearLuz(techos.transform, "Luz_Parkour_O", new Vector3(-4f, 5f, 0f));
        CrearLuz(techos.transform, "Luz_Parkour_E", new Vector3(4f, 5f, 0f));
    }

    // Perimetro con paredes de 2 m. Puerta del almacen al sur (cerrada, decorativa).
    static void CrearParedesPerimetro(Transform padre)
    {
        GameObject g = Vacio("Perimetro", padre, Vector3.zero);
        float medio = LOSAS * 4f / 2f;   // 32

        for (float x = -medio + 1f; x < medio; x += 2f)
        {
            Poner(g.transform, "Kit_Pared", new Vector3(x, 0, medio), 0f);
            if (Mathf.Approximately(x, 1f))
            {
                Poner(g.transform, "Kit_PuertaAlmacen", new Vector3(x, 0, -medio), 0f);
                // Puerta cerrada detras del jugador: es decorativa (WarehouseDoor solo funciona en la tienda)
            }
            else Poner(g.transform, "Kit_Pared", new Vector3(x, 0, -medio), 0f);
        }

        for (float z = -medio + 1f; z < medio; z += 2f)
        {
            Poner(g.transform, "Kit_Pared", new Vector3(medio, 0, z), 90f);
            Poner(g.transform, "Kit_Pared", new Vector3(-medio, 0, z), 90f);
        }
    }

    // Bloques interiores y ramas sin salida, a partir del mapa de la ZONA 2.
    static void CrearInterior(Transform padre, Material matBloque, Material matCaja)
    {
        GameObject g = Vacio("Interior", padre, Vector3.zero);
        int n = MAPA.Length;
        for (int r = 0; r < n; r++)
            for (int c = 0; c < MAPA[r].Length; c++)
            {
                char ch = MAPA[r][c];
                bool borde = (r == 0 || r == n - 1 || c == 0 || c == MAPA[r].Length - 1);
                float x = (c - (MAPA[r].Length - 1) / 2f) * CELDA;
                float z = ((n - 1) / 2f - r) * CELDA;                 // fila 0 = norte

                if (ch == '#' && !borde)
                    Caja(g.transform, "Bloque_" + r + "_" + c, new Vector3(x, ALTO_MURO / 2f, z),
                         new Vector3(CELDA, ALTO_MURO, CELDA), matBloque);
                else if (ch == 'H')
                    CrearRama(g.transform, new Vector3(x, 0f, z), r + "_" + c, matCaja);
            }
    }

    // Final de una rama sin salida: dos cajas en el fondo y zona de escondite en toda la celda.
    static void CrearRama(Transform padre, Vector3 pos, string id, Material matCaja)
    {
        GameObject r = Vacio("Rama_" + id, padre, pos);
        Caja(r.transform, "Caja1", new Vector3(0.5f, 0.5f, 0.5f),   Vector3.one * 1.0f, matCaja);
        Caja(r.transform, "Caja2", new Vector3(0.55f, 0.35f, -0.4f), Vector3.one * 0.7f, matCaja);
        CrearEscondite(r.transform, "HidingSpot_Rama_" + id, pos + new Vector3(0, 0.8f, 0), new Vector3(CELDA, 1.6f, CELDA));
    }

    // Sala de parkour: plataforma de 2 m, escalones de 1 m, cajas bajas y altas,
    // y 4 NavMeshLink para que el jefe suba y baje de la plataforma.
    static void CrearParkour(Transform padre, Material matCaja, Material matMetal)
    {
        GameObject g = Vacio("Parkour", padre, POS_PARKOUR);
        float h = ALTO_PLATAFORMA;

        Caja(g.transform, "Plataforma", new Vector3(0, h / 2f, 0), new Vector3(4f, h, 4f), matMetal);
        Caja(g.transform, "Escalon_O", new Vector3(-3f, 0.5f, -1f), new Vector3(2f, 1f, 2f), matCaja);
        Caja(g.transform, "Escalon_E", new Vector3(3f, 0.5f, 1f),  new Vector3(2f, 1f, 2f), matCaja);

        float[] xs = { -6f, 6f };
        float[] zs = { -2.5f, 2.5f };
        foreach (float x in xs)
        {
            foreach (float z in zs) Caja(g.transform, "CajaBaja", new Vector3(x, 0.6f, z), Vector3.one * 1.2f, matCaja);
            Caja(g.transform, "CajaAlta", new Vector3(x, 1f, 0f), new Vector3(1.2f, 2f, 1.2f), matCaja);
        }

        // Todo lo del parkour cuenta como suelo para el jugador
        int capa = LayerMask.NameToLayer("Ground");
        if (capa >= 0) CapaRecursiva(g, capa);

        // NavMeshLink: inicio en el suelo, final sobre la plataforma (desnivel = h <= 2,2 m)
        CrearLink(g.transform, "Link_Sur",   new Vector3(0, 0, -3f),   new Vector3(0, h, -1f));
        CrearLink(g.transform, "Link_Norte", new Vector3(0, 0, 3f),    new Vector3(0, h, 1f));
        CrearLink(g.transform, "Link_Oeste", new Vector3(-3f, 0, 1.2f), new Vector3(-1f, h, 1.2f));
        CrearLink(g.transform, "Link_Este",  new Vector3(3f, 0, -1.2f), new Vector3(1f, h, -1.2f));
    }

    static void CrearTienda(Transform padre)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_REPLICA);
        GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        t.transform.SetParent(padre, false);
        t.transform.position = POS_TIENDA;
    }

    // Objeto "NavMesh" con NavMeshSurface configurado para usar colliders.
    static void CrearNavMesh(Transform padre)
    {
        GameObject nav = Vacio("NavMesh", padre, Vector3.zero);
        System.Type t = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
        if (t == null)
        {
            Debug.LogWarning("No encontre NavMeshSurface. Agregalo a mano: Add Component > NavMesh Surface.");
            return;
        }
        Component c = nav.AddComponent(t);
        SerializedObject so = new SerializedObject(c);
        SerializedProperty geo = so.FindProperty("m_UseGeometry");   // 1 = Physics Colliders
        if (geo != null) { geo.intValue = 1; so.ApplyModifiedProperties(); }
        NivelBake.Hornear(c);
    }

    // BossSpawn y PatrolPoints (los pide el checklist del equipo).
    static void CrearBossYPatrulla(Transform padre)
    {
        Vacio("BossSpawn", padre, POS_BOSS_SPAWN);
        GameObject pp = Vacio("PatrolPoints", padre, Vector3.zero);
        for (int i = 0; i < PUNTOS_PATRULLA.Length; i++)
            Vacio("P_" + i, pp.transform, PUNTOS_PATRULLA[i]);
    }

    // ================== ZONA 5: AYUDAS (no hace falta tocar) ==========

    // NavMeshLink por reflexion (asi compila aunque cambie la version del paquete).
    // 'inicio' y 'fin' son locales al padre.
    static void CrearLink(Transform padre, string nombre, Vector3 inicio, Vector3 fin)
    {
        GameObject go = Vacio(nombre, padre, Vector3.zero);
        System.Type t = System.Type.GetType("Unity.AI.Navigation.NavMeshLink, Unity.AI.Navigation");
        if (t == null)
        {
            Debug.LogWarning("No encontre NavMeshLink. Agregalo a mano: Add Component > NavMesh Link.");
            return;
        }
        Component c = go.AddComponent(t);
        PonerPropiedad(c, t, "startPoint", inicio);
        PonerPropiedad(c, t, "endPoint", fin);
        PonerPropiedad(c, t, "width", ANCHO_LINK);
        PonerPropiedad(c, t, "bidirectional", true);
    }

    static void PonerPropiedad(object obj, System.Type t, string nombre, object valor)
    {
        System.Reflection.PropertyInfo p = t.GetProperty(nombre);
        if (p != null && p.CanWrite) p.SetValue(obj, valor, null);
        else Debug.LogWarning("NavMeshLink: no pude asignar " + nombre + ". Ajustalo a mano.");
    }

    static void CrearLuz(Transform padre, string nombre, Vector3 pos)
    {
        GameObject go = Vacio(nombre, padre, pos);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.93f, 1f, 0.9f);
        l.intensity = 3f;
        l.range = 12f;
        l.shadows = LightShadows.None;
    }

    static GameObject CrearEscondite(Transform padre, string nombre, Vector3 posMundo, Vector3 tamano)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, true);
        go.transform.position = posMundo;
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = tamano;
        int capa = LayerMask.NameToLayer("HidingSpot");
        if (capa >= 0) go.layer = capa;
        go.AddComponent<HidingSpot>();
        return go;
    }

    static void CapaRecursiva(GameObject go, int capa)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = capa;
    }

    static GameObject Vacio(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        return go;
    }

    static GameObject Caja(Transform padre, string nombre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.name = nombre;
        c.transform.SetParent(padre, false);
        c.transform.localPosition = pos;
        c.transform.localScale = escala;
        c.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return c;
    }

    static GameObject Poner(Transform padre, string nombrePrefab, Vector3 pos, float giroY)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(CARPETA_PREFABS + "/" + nombrePrefab + ".prefab");
        if (asset == null) { Debug.LogError("Falta el prefab " + nombrePrefab); return null; }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, giroY, 0f);
        return go;
    }

    static Material Mat(string nombre, Color color)
    {
        if (!AssetDatabase.IsValidFolder(CARPETA_MATS)) AssetDatabase.CreateFolder("Assets", "03_Materials");
        string ruta = CARPETA_MATS + "/" + nombre + ".mat";
        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null) return existente;
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(m, ruta);
        return m;
    }
}
