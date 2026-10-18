// =====================================================================
//  ParqueBuilder.cs  v2   (Dev 4 - Entornos)   tarjeta N-04
//  DONDE PONERLO:  Assets/Editor/ParqueBuilder.cs   (reemplaza el anterior)
//  ANTES DE USARLO: Tools > Kit > Generar kit y tienda (crea ReplicaStore).
//  COMO USARLO:    Tools > Kit > Construir parque (N-04)   y despues, Bake.
//
//  DISENO DEL PARQUE (70 x 70 m, el jugador entra por el SUR y la meta,
//  ReplicaStore, esta al NORTE):
//   - Camino claro del spawn a la tienda: es la ruta rapida y OBVIA.
//   - Plaza central abierta con piscina de pelotas grande: zona de riesgo.
//   - Al sur: toboganes, casitas y columpios (zona de juego, escondites).
//   - Al oeste: LABERINTO de setos (se edita con el mapa de la ZONA 2).
//   - Al este: red de tubos y piscinas de pelotas.
//   - Bosque con arboles y ARBUSTOS escondite repartidos al azar.
//   Hay ~27 escondites: casi siempre hay uno a menos de 12 m.
//  Todo queda en "Parque_Generado". Al repetir el script, se borra y se
//  vuelve a crear. Suelo_Parque se agranda y la cerca antigua se DESACTIVA.
// =====================================================================
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ParqueBuilder
{
    // ================== ZONA 1: AJUSTES (EDITA AQUI) ==================
    const string RUTA_ESCENA  = "Assets/00_Scenes/12_Playground.unity";
    const string RUTA_REPLICA = "Assets/02_Prefabs/ReplicaStore.prefab";
    const string CARPETA_MATS = "Assets/03_Materials";
    const string NOMBRE_RAIZ  = "Parque_Generado";

    const float TAM_PARQUE = 70f;               // lado del parque (de -35 a 35)
    const bool  DESACTIVAR_CERCA_ANTIGUA = true; // desactiva los muros viejos de 40 m

    static readonly Vector3 POS_SPAWN = new Vector3(0f, 0.1f, -31f);
    static readonly Vector3 POS_TIENDA = new Vector3(0f, 0f, 30f);

    // BossSpawn en la esquina noreste (lejos del spawn del jugador) y puntos de patrulla
    static readonly Vector3 POS_BOSS_SPAWN = new Vector3(30f, 0.1f, 30f);
    static readonly Vector3[] PUNTOS_PATRULLA =
    {
        new Vector3(1f, 0.1f, -20f), new Vector3(1f, 0.1f, -5f), new Vector3(1f, 0.1f, 10f),
        new Vector3(1f, 0.1f, 22f),  new Vector3(14f, 0.1f, -14f), new Vector3(-15f, 0.1f, -17f)
    };

    // Toboganes (la rampa baja hacia +Z con giro 0)
    static readonly Vector3[] POS_TOBOGANES =
        { new Vector3(-24f, 0f, -22f), new Vector3(22f, 0f, -22f), new Vector3(-9f, 0f, -14f) };
    const float ALTURA_TOBOGAN = 2f;

    // Casitas: la entrada mira hacia el camino central
    static readonly Vector3[] POS_CASITAS =
        { new Vector3(-14f, 0f, -27f), new Vector3(14f, 0f, -27f), new Vector3(30f, 0f, -12f), new Vector3(28f, 0f, 22f) };
    static readonly float[] GIRO_CASITAS = { -90f, 90f, 90f, 90f };

    static readonly Vector3[] POS_COLUMPIOS = { new Vector3(-20f, 0f, -31f), new Vector3(20f, 0f, -31f) };

    // Tubos: posicion, giro y si tiene escondite dentro
    static readonly Vector3[] POS_TUBOS = { new Vector3(20f, 0f, 6f), new Vector3(25f, 0f, 12f), new Vector3(12f, 0f, 15f) };
    static readonly float[] GIRO_TUBOS = { 0f, 90f, 0f };
    static readonly bool[] ESCONDITE_TUBOS = { false, true, false };
    const float LARGO_TUBO = 10f;

    // Piscinas de pelotas: posicion y lado
    static readonly Vector3[] POS_PISCINAS = { new Vector3(9f, 0f, 0f), new Vector3(-26f, 0f, 26f), new Vector3(22f, 0f, -6f) };
    static readonly float[] TAM_PISCINAS = { 10f, 8f, 6f };
    const float ALTO_MURO_PISCINA = 1.2f;

    // Laberinto
    static readonly Vector3 POS_LABERINTO = new Vector3(-19f, 0f, 4f);
    const float CELDA_LAB = 3f;     // lado de cada celda (m)
    const float ALTO_SETO = 2.4f;   // altura de los setos (m)

    // Bosque
    const int CANT_ARBOLES  = 24;
    const int CANT_ARBUSTOS = 14;

    // ================== ZONA 2: MAPA DEL LABERINTO (EDITA AQUI) =======
    //  # = seto (pared)   . = pasillo   H = escondite (arbusto en el pasillo)
    //  Arriba = NORTE. Salidas: este (fila 5) y sur (fila 9). 10 x 10 celdas.
    static readonly string[] MAPA_LABERINTO =
    {
        "##########",
        "#...H#...#",
        "#.##.#.#.#",
        "#.#H.#.#.#",
        "#.#.##.#.#",
        "#.#....#..",
        "#.####.#.#",
        "#..H...#.#",
        "#..###H#H#",
        "##.#######"
    };

    // Zonas ocupadas (xmin, xmax, zmin, zmax): el bosque no planta ahi
    static List<Vector4> zonas = new List<Vector4>();

    // ================== ZONA 3: MENU PRINCIPAL ========================
    [MenuItem("Tools/Kit/Construir parque (N-04)")]
    public static void Construir()
    {
        GameObject replica = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_REPLICA);
        if (replica == null)
        {
            EditorUtility.DisplayDialog("Falta ReplicaStore",
                "Primero ejecuta Tools > Kit > Generar kit y tienda (A-01 + A-02).", "Ok");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene escena = EditorSceneManager.OpenScene(RUTA_ESCENA, OpenSceneMode.Single);
        GameObject viejo = GameObject.Find(NOMBRE_RAIZ);
        if (viejo != null) Object.DestroyImmediate(viejo);
        zonas.Clear();

        GameObject raiz = new GameObject(NOMBRE_RAIZ);

        // Materiales (se guardan como archivos en 03_Materials)
        Material rojo     = Mat("Parque_Rojo",     new Color(0.85f, 0.2f, 0.2f));
        Material azul     = Mat("Parque_Azul",     new Color(0.2f, 0.4f, 0.85f));
        Material amarillo = Mat("Parque_Amarillo", new Color(0.95f, 0.8f, 0.2f));
        Material verde    = Mat("Parque_Verde",    new Color(0.2f, 0.6f, 0.25f));
        Material naranja  = Mat("Parque_Naranja",  new Color(0.95f, 0.5f, 0.15f));
        Material marron   = Mat("Parque_Marron",   new Color(0.4f, 0.25f, 0.12f));
        Material seto     = Mat("Parque_Seto",     new Color(0.1f, 0.35f, 0.15f));
        Material cerca    = Mat("Parque_Cerca",    new Color(0.35f, 0.35f, 0.38f));
        Material camino   = Mat("Parque_Camino",   new Color(0.8f, 0.72f, 0.5f));
        Material[] pelotas = { rojo, azul, amarillo, verde, naranja };

        // Cada linea es una zona. Comenta una para no construirla.
        PrepararSuelo();
        CrearCerca(raiz.transform, cerca);
        CrearEntrada(raiz.transform, camino);
        CrearToboganes(raiz.transform, azul, rojo, amarillo, verde);
        CrearCasitas(raiz.transform, amarillo, rojo);
        CrearColumpios(raiz.transform, azul, naranja);
        CrearLaberinto(raiz.transform, seto, verde);
        CrearTubos(raiz.transform, naranja, azul);
        CrearPiscinas(raiz.transform, rojo, pelotas);
        CrearTienda(raiz.transform, replica);
        CrearBossYPatrulla(raiz.transform);           // antes del bosque: reserva su zona
        CrearBosque(raiz.transform, marron, verde);   // al final: usa las zonas ocupadas
        CrearNavMesh(raiz.transform);

        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            t.gameObject.isStatic = true;

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = raiz;
        Debug.Log("Parque construido. Selecciona el objeto NavMesh y presiona Bake.");
    }

    // ================== ZONA 4: ZONAS DEL PARQUE ======================

    // Agranda Suelo_Parque al tamano nuevo (o lo crea si no existe).
    static void PrepararSuelo()
    {
        GameObject suelo = GameObject.Find("Suelo_Parque");
        if (suelo == null)
        {
            suelo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            suelo.name = "Suelo_Parque";
        }
        suelo.transform.position = new Vector3(0f, -0.1f, 0f);
        suelo.transform.rotation = Quaternion.identity;
        suelo.transform.localScale = new Vector3(TAM_PARQUE, 0.2f, TAM_PARQUE);
        int capa = LayerMask.NameToLayer("Ground");
        if (capa >= 0) suelo.layer = capa;
    }

    // Cerca nueva de 3 m y desactivacion de los muros viejos (se pueden reactivar).
    static void CrearCerca(Transform padre, Material mat)
    {
        GameObject raiz = Vacio("Cerca", padre, Vector3.zero);
        float m = TAM_PARQUE / 2f;
        Caja(raiz.transform, "Norte", new Vector3(0, 1.5f, m),  new Vector3(TAM_PARQUE + 0.6f, 3f, 0.6f), mat);
        Caja(raiz.transform, "Sur",   new Vector3(0, 1.5f, -m), new Vector3(TAM_PARQUE + 0.6f, 3f, 0.6f), mat);
        Caja(raiz.transform, "Este",  new Vector3(m, 1.5f, 0),  new Vector3(0.6f, 3f, TAM_PARQUE + 0.6f), mat);
        Caja(raiz.transform, "Oeste", new Vector3(-m, 1.5f, 0), new Vector3(0.6f, 3f, TAM_PARQUE + 0.6f), mat);

        if (!DESACTIVAR_CERCA_ANTIGUA) return;
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            string n = go.name.ToLower();
            if (go.name == NOMBRE_RAIZ || go.name == "Suelo_Parque") continue;
            if (n.Contains("muro") || n.Contains("pared"))
            {
                go.SetActive(false);
                Debug.Log("Cerca antigua desactivada: " + go.name);
            }
        }
    }

    // Spawn del jugador y camino claro hasta la tienda.
    static void CrearEntrada(Transform padre, Material matCamino)
    {
        Vacio("PlayerSpawn", padre, POS_SPAWN);
        // Camino: franja baja y sin collider (solo guia visual)
        Caja(padre, "Camino", new Vector3(0f, 0.02f, -2f), new Vector3(3f, 0.04f, 52f), matCamino, false);
        AddZona(-8f, 8f, -35f, -27f);     // area del spawn
        AddZona(-3f, 3f, -30f, 24f);      // franja del camino
    }

    static void CrearToboganes(Transform padre, Material torre1, Material rampa1, Material torre2, Material rampa2)
    {
        GameObject g = Vacio("Toboganes", padre, Vector3.zero);
        for (int i = 0; i < POS_TOBOGANES.Length; i++)
        {
            bool par = (i % 2 == 0);
            CrearTobogan(g.transform, POS_TOBOGANES[i], 0f, ALTURA_TOBOGAN,
                         par ? torre1 : torre2, par ? rampa1 : rampa2);
            Vector3 p = POS_TOBOGANES[i];
            AddZona(p.x - 4f, p.x + 4f, p.z - 5f, p.z + 9f);
        }
    }

    static void CrearCasitas(Transform padre, Material matPared, Material matTecho)
    {
        GameObject g = Vacio("Casitas", padre, Vector3.zero);
        for (int i = 0; i < POS_CASITAS.Length; i++)
        {
            CrearCasita(g.transform, POS_CASITAS[i], GIRO_CASITAS[i], matPared, matTecho);
            Vector3 p = POS_CASITAS[i];
            AddZona(p.x - 3.5f, p.x + 3.5f, p.z - 3.5f, p.z + 3.5f);
        }
    }

    static void CrearColumpios(Transform padre, Material matMarco, Material matAsiento)
    {
        GameObject g = Vacio("Columpios", padre, Vector3.zero);
        foreach (Vector3 p in POS_COLUMPIOS)
        {
            CrearColumpio(g.transform, p, 0f, matMarco, matAsiento);
            AddZona(p.x - 4f, p.x + 4f, p.z - 2.5f, p.z + 2.5f);
        }
    }

    // Laberinto de setos a partir del mapa de la ZONA 2.
    static void CrearLaberinto(Transform padre, Material matSeto, Material matArbusto)
    {
        GameObject raiz = Vacio("Laberinto", padre, POS_LABERINTO);
        int filas = MAPA_LABERINTO.Length;
        int cols = MAPA_LABERINTO[0].Length;
        for (int r = 0; r < filas; r++)
            for (int c = 0; c < cols; c++)
            {
                char ch = MAPA_LABERINTO[r][c];
                float x = (c - (cols - 1) / 2f) * CELDA_LAB;
                float z = ((filas - 1) / 2f - r) * CELDA_LAB;     // fila 0 = norte
                if (ch == '#')
                    Caja(raiz.transform, "Seto_" + r + "_" + c, new Vector3(x, ALTO_SETO / 2f, z),
                         new Vector3(CELDA_LAB, ALTO_SETO, CELDA_LAB), matSeto);
                else if (ch == 'H')
                    CrearArbusto(raiz.transform, new Vector3(x, 0f, z), matArbusto, 2.6f);
            }
        float mx = cols * CELDA_LAB / 2f + 1.5f, mz = filas * CELDA_LAB / 2f + 1.5f;
        AddZona(POS_LABERINTO.x - mx, POS_LABERINTO.x + mx, POS_LABERINTO.z - mz, POS_LABERINTO.z + mz);
    }

    static void CrearTubos(Transform padre, Material mat1, Material mat2)
    {
        GameObject g = Vacio("Tubos", padre, Vector3.zero);
        for (int i = 0; i < POS_TUBOS.Length; i++)
        {
            Vector3 p = POS_TUBOS[i];
            CrearTubo(g.transform, p, GIRO_TUBOS[i], LARGO_TUBO, i % 2 == 0 ? mat1 : mat2);
            if (ESCONDITE_TUBOS[i])
            {
                // El trigger se coloca dentro del tubo, girado igual que el tubo
                GameObject s = CrearEscondite(g.transform, "HidingSpot_Tubo_" + i,
                                              p + new Vector3(0, 1f, 0), new Vector3(1.4f, 1.8f, LARGO_TUBO - 1f));
                s.transform.rotation = Quaternion.Euler(0f, GIRO_TUBOS[i], 0f);
            }
            float a = (GIRO_TUBOS[i] == 0f) ? 2f : LARGO_TUBO / 2f + 1f;
            float b = (GIRO_TUBOS[i] == 0f) ? LARGO_TUBO / 2f + 1f : 2f;
            AddZona(p.x - a, p.x + a, p.z - b, p.z + b);
        }
    }

    static void CrearPiscinas(Transform padre, Material matMuro, Material[] pelotas)
    {
        GameObject g = Vacio("Piscinas", padre, Vector3.zero);
        for (int i = 0; i < POS_PISCINAS.Length; i++)
        {
            float tam = TAM_PISCINAS[i];
            CrearPiscina(g.transform, POS_PISCINAS[i], tam, ALTO_MURO_PISCINA, matMuro, pelotas, (int)(tam * tam * 1.1f));
            Vector3 p = POS_PISCINAS[i];
            float m = tam / 2f + 1.5f;
            AddZona(p.x - m, p.x + m, p.z - m, p.z + m);
        }
    }

    static void CrearTienda(Transform padre, GameObject replica)
    {
        GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(replica);
        t.transform.SetParent(padre, false);
        t.transform.position = POS_TIENDA;
        AddZona(POS_TIENDA.x - 8f, POS_TIENDA.x + 8f, POS_TIENDA.z - 6f, POS_TIENDA.z + 6f);
    }

    // BossSpawn y PatrolPoints (los pide el checklist del equipo).
    static void CrearBossYPatrulla(Transform padre)
    {
        Vacio("BossSpawn", padre, POS_BOSS_SPAWN);
        AddZona(POS_BOSS_SPAWN.x - 4f, POS_BOSS_SPAWN.x + 4f, POS_BOSS_SPAWN.z - 4f, POS_BOSS_SPAWN.z + 4f);
        GameObject pp = Vacio("PatrolPoints", padre, Vector3.zero);
        for (int i = 0; i < PUNTOS_PATRULLA.Length; i++)
        {
            Vacio("P_" + i, pp.transform, PUNTOS_PATRULLA[i]);
            AddZona(PUNTOS_PATRULLA[i].x - 1f, PUNTOS_PATRULLA[i].x + 1f, PUNTOS_PATRULLA[i].z - 1f, PUNTOS_PATRULLA[i].z + 1f);
        }
    }

    // Arboles y arbustos-escondite repartidos al azar (semilla fija).
    static void CrearBosque(Transform padre, Material matTronco, Material matCopa)
    {
        Random.InitState(11);
        GameObject arboles = Vacio("Arboles", padre, Vector3.zero);
        GameObject arbustos = Vacio("Arbustos_Escondite", padre, Vector3.zero);
        List<Vector3> usados = new List<Vector3>();
        Vector3 p;

        for (int i = 0; i < CANT_ARBOLES; i++)
            if (BuscarPunto(usados, 4f, out p)) { CrearArbol(arboles.transform, p, matTronco, matCopa); usados.Add(p); }

        for (int i = 0; i < CANT_ARBUSTOS; i++)
            if (BuscarPunto(usados, 6f, out p)) { CrearArbusto(arbustos.transform, p, matCopa, 2.4f); usados.Add(p); }
    }

    // Objeto "NavMesh": agrega NavMeshSurface y lo configura para usar colliders.
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
        SerializedProperty geo = so.FindProperty("m_UseGeometry");   // 0 = Render Meshes, 1 = Physics Colliders
        if (geo != null) { geo.intValue = 1; so.ApplyModifiedProperties(); }
    }

    // ================== ZONA 5: PIEZAS ================================

    // Tobogan: torre 2x2 con escalones por detras y rampa hacia +Z local.
    static void CrearTobogan(Transform padre, Vector3 pos, float giroY, float altura, Material matTorre, Material matRampa)
    {
        GameObject raiz = Vacio("Tobogan", padre, pos);
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        Caja(raiz.transform, "Torre", new Vector3(0, altura / 2f, 0), new Vector3(2f, altura, 2f), matTorre);

        float paso = 0.5f;
        int n = Mathf.RoundToInt(altura / paso) - 1;
        for (int j = 0; j < n; j++)
        {
            float h = altura - paso * (j + 1);
            Caja(raiz.transform, "Escalon_" + j, new Vector3(0, h / 2f, -(1.3f + 0.6f * j)), new Vector3(1.6f, h, 0.6f), matTorre);
        }

        float largo = 6.5f;
        float ang = Mathf.Asin(altura / largo) * Mathf.Rad2Deg;
        float horiz = largo * Mathf.Cos(ang * Mathf.Deg2Rad);
        GameObject rampa = Caja(raiz.transform, "Rampa", new Vector3(0, altura / 2f, 1f + horiz / 2f), new Vector3(1.4f, 0.15f, largo), matRampa);
        rampa.transform.localRotation = Quaternion.Euler(ang, 0f, 0f);
    }

    // Casita: 3 muros, techo y entrada abierta hacia -Z local. Es un escondite.
    static void CrearCasita(Transform padre, Vector3 pos, float giroY, Material matPared, Material matTecho)
    {
        GameObject r = Vacio("Casita", padre, pos);
        r.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        Caja(r.transform, "Fondo", new Vector3(0, 1.1f, 1.4f),  new Vector3(3f, 2.2f, 0.2f), matPared);
        Caja(r.transform, "Izq",   new Vector3(-1.4f, 1.1f, 0), new Vector3(0.2f, 2.2f, 3f), matPared);
        Caja(r.transform, "Der",   new Vector3(1.4f, 1.1f, 0),  new Vector3(0.2f, 2.2f, 3f), matPared);
        Caja(r.transform, "Techo", new Vector3(0, 2.3f, 0),     new Vector3(3.6f, 0.2f, 3.6f), matTecho);
        CrearEscondite(r.transform, "HidingSpot_Casita", r.transform.position + new Vector3(0, 1f, 0), new Vector3(2.4f, 2f, 2.4f));
    }

    // Columpio: marco con barra y dos asientos colgados (decoracion / punto de referencia).
    static void CrearColumpio(Transform padre, Vector3 pos, float giroY, Material matMarco, Material matAsiento)
    {
        GameObject r = Vacio("Columpio", padre, pos);
        r.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        Caja(r.transform, "PosteIzq", new Vector3(-2f, 1.25f, 0), new Vector3(0.2f, 2.5f, 0.2f), matMarco);
        Caja(r.transform, "PosteDer", new Vector3(2f, 1.25f, 0),  new Vector3(0.2f, 2.5f, 0.2f), matMarco);
        Caja(r.transform, "Barra",    new Vector3(0, 2.5f, 0),    new Vector3(4.2f, 0.2f, 0.2f), matMarco);
        float[] xs = { -0.8f, 0.8f };
        foreach (float x in xs)
        {
            Caja(r.transform, "Cadena", new Vector3(x, 1.55f, 0), new Vector3(0.04f, 1.9f, 0.04f), matMarco, false);
            Caja(r.transform, "Asiento", new Vector3(x, 0.55f, 0), new Vector3(0.6f, 0.08f, 0.3f), matAsiento, false);
        }
    }

    // Tubo: tunel de 1.6 m de ancho interior y 2 m de alto, a lo largo de Z local.
    static void CrearTubo(Transform padre, Vector3 pos, float giroY, float largo, Material mat)
    {
        GameObject raiz = Vacio("Tubo", padre, pos);
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);
        Caja(raiz.transform, "ParedIzq", new Vector3(-0.9f, 1f, 0), new Vector3(0.2f, 2f, largo), mat);
        Caja(raiz.transform, "ParedDer", new Vector3(0.9f, 1f, 0),  new Vector3(0.2f, 2f, largo), mat);
        Caja(raiz.transform, "Techo",    new Vector3(0, 2.1f, 0),   new Vector3(2f, 0.2f, largo), mat);
    }

    // Piscina de pelotas: muros bajos, entrada de 2 m al sur, pelotas sin collider
    // y una zona HidingSpot que cubre toda la piscina.
    static void CrearPiscina(Transform padre, Vector3 pos, float tam, float alto, Material matMuro, Material[] pelotas, int cantidad)
    {
        GameObject raiz = Vacio("PiscinaPelotas", padre, pos);
        float m = tam / 2f;
        float seg = (tam - 2f) / 2f;
        float cSeg = 1f + seg / 2f;
        Caja(raiz.transform, "MuroSurIzq", new Vector3(-cSeg, alto / 2f, -m), new Vector3(seg, alto, 0.2f), matMuro);
        Caja(raiz.transform, "MuroSurDer", new Vector3(cSeg, alto / 2f, -m),  new Vector3(seg, alto, 0.2f), matMuro);
        Caja(raiz.transform, "MuroNorte",  new Vector3(0, alto / 2f, m),      new Vector3(tam, alto, 0.2f), matMuro);
        Caja(raiz.transform, "MuroEste",   new Vector3(m, alto / 2f, 0),      new Vector3(0.2f, alto, tam), matMuro);
        Caja(raiz.transform, "MuroOeste",  new Vector3(-m, alto / 2f, 0),     new Vector3(0.2f, alto, tam), matMuro);

        GameObject grupo = Vacio("Pelotas", raiz.transform, Vector3.zero);
        Random.InitState(7 + (int)tam);
        for (int i = 0; i < cantidad; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.name = "Pelota_" + i;
            p.transform.SetParent(grupo.transform, false);
            p.transform.localPosition = new Vector3(Random.Range(-m + 0.6f, m - 0.6f), 0.3f, Random.Range(-m + 0.6f, m - 0.6f));
            p.transform.localScale = Vector3.one * 0.6f;
            p.GetComponent<MeshRenderer>().sharedMaterial = pelotas[i % pelotas.Length];
            Object.DestroyImmediate(p.GetComponent<Collider>());
        }
        CrearEscondite(raiz.transform, "HidingSpot_Piscina", pos + new Vector3(0, alto / 2f, 0), new Vector3(tam - 0.6f, alto, tam - 0.6f));
    }

    // Arbol: tronco (cilindro) + copa (esfera sin collider).
    static void CrearArbol(Transform padre, Vector3 pos, Material matTronco, Material matCopa)
    {
        GameObject raiz = Vacio("Arbol", padre, pos);
        GameObject tronco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tronco.name = "Tronco";
        tronco.transform.SetParent(raiz.transform, false);
        tronco.transform.localPosition = new Vector3(0, 1.5f, 0);
        tronco.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
        tronco.GetComponent<MeshRenderer>().sharedMaterial = matTronco;

        GameObject copa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        copa.name = "Copa";
        copa.transform.SetParent(raiz.transform, false);
        copa.transform.localPosition = new Vector3(0, 3.6f, 0);
        copa.transform.localScale = Vector3.one * 3f;
        copa.GetComponent<MeshRenderer>().sharedMaterial = matCopa;
        Object.DestroyImmediate(copa.GetComponent<Collider>());
    }

    // Arbusto escondite: 4 esferas bajas sin collider + zona HidingSpot de 'lado' x 'lado'.
    static void CrearArbusto(Transform padre, Vector3 pos, Material mat, float lado)
    {
        GameObject raiz = Vacio("Arbusto", padre, pos);
        Vector3[] offs = { new Vector3(-0.6f, 0, -0.4f), new Vector3(0.6f, 0, -0.3f), new Vector3(0f, 0, 0.6f), new Vector3(0f, 0.2f, 0f) };
        foreach (Vector3 o in offs)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "Hojas";
            s.transform.SetParent(raiz.transform, false);
            s.transform.localPosition = o + new Vector3(0, 0.6f, 0);
            s.transform.localScale = Vector3.one * 1.5f;
            s.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(s.GetComponent<Collider>());
        }
        CrearEscondite(raiz.transform, "HidingSpot_Arbusto", raiz.transform.position + new Vector3(0, 0.7f, 0), new Vector3(lado, 1.4f, lado));
    }

    // Zona de escondite: trigger en la capa HidingSpot + componente HidingSpot (de Rafael).
    // 'posMundo' es la posicion en el mundo del centro de la zona.
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

    // ================== ZONA 6: AYUDAS (no hace falta tocar) ==========

    static void AddZona(float xmin, float xmax, float zmin, float zmax)
    {
        zonas.Add(new Vector4(xmin, xmax, zmin, zmax));
    }

    static bool EnZonaOcupada(Vector3 p)
    {
        foreach (Vector4 z in zonas)
            if (p.x > z.x - 1.5f && p.x < z.y + 1.5f && p.z > z.z - 1.5f && p.z < z.w + 1.5f) return true;
        return false;
    }

    // Busca un punto libre dentro del parque, lejos de zonas ocupadas y de otros puntos.
    static bool BuscarPunto(List<Vector3> usados, float distMin, out Vector3 p)
    {
        float lim = TAM_PARQUE / 2f - 3f;
        for (int intento = 0; intento < 300; intento++)
        {
            p = new Vector3(Random.Range(-lim, lim), 0f, Random.Range(-lim, lim));
            if (EnZonaOcupada(p)) continue;
            bool lejos = true;
            foreach (Vector3 u in usados)
                if ((u - p).sqrMagnitude < distMin * distMin) { lejos = false; break; }
            if (lejos) return true;
        }
        p = Vector3.zero;
        return false;
    }

    static GameObject Vacio(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        return go;
    }

    static GameObject Caja(Transform padre, string nombre, Vector3 pos, Vector3 escala, Material mat, bool conCollider = true)
    {
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.name = nombre;
        c.transform.SetParent(padre, false);
        c.transform.localPosition = pos;
        c.transform.localScale = escala;
        c.GetComponent<MeshRenderer>().sharedMaterial = mat;
        if (!conCollider) Object.DestroyImmediate(c.GetComponent<Collider>());
        return c;
    }

    // Material de color guardado como archivo (si fuera temporal, la escena lo perderia).
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
