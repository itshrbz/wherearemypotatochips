// =====================================================================
//  ParqueBuilder.cs   (Dev 4 - Entornos)   tarjeta N-04
//  DONDE PONERLO:  Assets/Editor/ParqueBuilder.cs
//  ANTES DE USARLO: ejecuta Tools > Kit > Generar kit y tienda (crea ReplicaStore).
//  COMO USARLO:    menu  Tools > Kit > Construir parque (N-04)
//
//  QUE HACE (sobre la escena 12_Playground, se puede repetir):
//   - 2 toboganes (torre con escalones + rampa)
//   - 2 tubos (tuneles para recorrer)
//   - 1 piscina de pelotas que sirve de ESCONDITE (zona HidingSpot)
//   - arboles de decoracion
//   - ReplicaStore al final del parque
//   - un objeto "NavMesh" con NavMeshSurface (solo falta pulsar Bake)
//  Todo queda dentro de un objeto "Parque_Generado". Al repetirlo, ese
//  objeto se borra y se vuelve a crear. NO toca Suelo_Parque ni las paredes.
// =====================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ParqueBuilder
{
    // ================== ZONA 1: RUTAS Y POSICIONES (EDITA AQUI) =======
    const string RUTA_ESCENA   = "Assets/00_Scenes/12_Playground.unity";
    const string RUTA_REPLICA  = "Assets/02_Prefabs/ReplicaStore.prefab";
    const string CARPETA_MATS  = "Assets/03_Materials";
    const string NOMBRE_RAIZ   = "Parque_Generado";

    // El parque mide 40 x 40 m (de -20 a 20). El jugador entra por el sur (z negativo).
    static readonly Vector3 POS_TOBOGAN_1 = new Vector3(-10f, 0f, -8f);
    const float GIRO_TOBOGAN_1 = 0f;
    static readonly Vector3 POS_TOBOGAN_2 = new Vector3(11f, 0f, -2f);
    const float GIRO_TOBOGAN_2 = -90f;
    const float ALTURA_TOBOGAN = 2f;

    static readonly Vector3 POS_TUBO_1 = new Vector3(-12f, 0f, 2f);
    const float GIRO_TUBO_1 = 90f;
    static readonly Vector3 POS_TUBO_2 = new Vector3(5f, 0f, 6f);
    const float GIRO_TUBO_2 = 0f;
    const float LARGO_TUBO = 8f;

    static readonly Vector3 POS_PISCINA = new Vector3(-12f, 0f, 12f);
    const float TAM_PISCINA = 8f;
    const float ALTO_MURO_PISCINA = 1.2f;
    const int   CANTIDAD_PELOTAS = 70;

    static readonly Vector3 POS_REPLICA = new Vector3(0f, 0f, 14f);

    static readonly Vector3[] POS_ARBOLES =
    {
        new Vector3(-16f, 0f, -14f), new Vector3(16f, 0f, -14f), new Vector3(0f, 0f, -4f),
        new Vector3(15f, 0f, 8f),    new Vector3(-3f, 0f, 3f),   new Vector3(16f, 0f, 16f)
    };

    // ================== ZONA 2: MENU PRINCIPAL ========================
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

        GameObject raiz = new GameObject(NOMBRE_RAIZ);

        Material rojo     = Mat("Parque_Rojo",     new Color(0.85f, 0.2f, 0.2f));
        Material azul     = Mat("Parque_Azul",     new Color(0.2f, 0.4f, 0.85f));
        Material amarillo = Mat("Parque_Amarillo", new Color(0.95f, 0.8f, 0.2f));
        Material verde    = Mat("Parque_Verde",    new Color(0.2f, 0.6f, 0.25f));
        Material naranja  = Mat("Parque_Naranja",  new Color(0.95f, 0.5f, 0.15f));
        Material marron   = Mat("Parque_Marron",   new Color(0.4f, 0.25f, 0.12f));
        Material[] pelotas = { rojo, azul, amarillo, verde, naranja };

        // ---- Toboganes ----
        GameObject toboganes = Vacio("Toboganes", raiz.transform, Vector3.zero);
        CrearTobogan(toboganes.transform, POS_TOBOGAN_1, GIRO_TOBOGAN_1, ALTURA_TOBOGAN, azul, rojo);
        CrearTobogan(toboganes.transform, POS_TOBOGAN_2, GIRO_TOBOGAN_2, ALTURA_TOBOGAN, amarillo, verde);

        // ---- Tubos ----
        GameObject tubos = Vacio("Tubos", raiz.transform, Vector3.zero);
        CrearTubo(tubos.transform, POS_TUBO_1, GIRO_TUBO_1, LARGO_TUBO, naranja);
        CrearTubo(tubos.transform, POS_TUBO_2, GIRO_TUBO_2, LARGO_TUBO, azul);

        // ---- Piscina de pelotas (escondite) ----
        GameObject escondites = Vacio("Escondites", raiz.transform, Vector3.zero);
        CrearPiscina(escondites.transform, POS_PISCINA, TAM_PISCINA, ALTO_MURO_PISCINA, rojo, pelotas);

        // ---- Arboles ----
        GameObject decorado = Vacio("Decorado", raiz.transform, Vector3.zero);
        foreach (Vector3 p in POS_ARBOLES) CrearArbol(decorado.transform, p, marron, verde);

        // ---- ReplicaStore al final ----
        GameObject tienda = (GameObject)PrefabUtility.InstantiatePrefab(replica);
        tienda.transform.SetParent(raiz.transform, false);
        tienda.transform.position = POS_REPLICA;

        // ---- NavMeshSurface ----
        CrearNavMesh(raiz.transform);

        // ---- Todo estatico (ayuda al NavMesh y al rendimiento) ----
        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            t.gameObject.isStatic = true;

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = raiz;
        Debug.Log("Parque construido. Selecciona el objeto NavMesh y presiona Bake.");
    }

    // ================== ZONA 3: PIEZAS DEL PARQUE =====================

    // Tobogan: torre de 2x2 m con escalones por detras y rampa hacia delante (+Z local).
    static void CrearTobogan(Transform padre, Vector3 pos, float giroY, float altura, Material matTorre, Material matRampa)
    {
        GameObject raiz = Vacio("Tobogan", padre, pos);
        raiz.transform.rotation = Quaternion.Euler(0f, giroY, 0f);

        Caja(raiz.transform, "Torre", new Vector3(0, altura / 2f, 0), new Vector3(2f, altura, 2f), matTorre);

        // Escalones de 0.5 m de alto por la espalda (-Z)
        float paso = 0.5f;
        int n = Mathf.RoundToInt(altura / paso) - 1;
        for (int j = 0; j < n; j++)
        {
            float h = altura - paso * (j + 1);
            Caja(raiz.transform, "Escalon_" + j, new Vector3(0, h / 2f, -(1.3f + 0.6f * j)),
                 new Vector3(1.6f, h, 0.6f), matTorre);
        }

        // Rampa: baja desde el borde de la torre hasta el suelo
        float largo = 6.5f;
        float ang = Mathf.Asin(altura / largo) * Mathf.Rad2Deg;
        float horiz = largo * Mathf.Cos(ang * Mathf.Deg2Rad);
        GameObject rampa = Caja(raiz.transform, "Rampa",
            new Vector3(0, altura / 2f, 1f + horiz / 2f), new Vector3(1.4f, 0.15f, largo), matRampa);
        rampa.transform.localRotation = Quaternion.Euler(ang, 0f, 0f);
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

    // Piscina de pelotas: muros bajos con una entrada al sur, pelotas decorativas
    // (sin collider) y una zona HidingSpot (trigger) que cubre toda la piscina.
    static void CrearPiscina(Transform padre, Vector3 pos, float tam, float alto, Material matMuro, Material[] pelotas)
    {
        GameObject raiz = Vacio("PiscinaPelotas", padre, pos);
        float m = tam / 2f;
        float seg = (tam - 2f) / 2f;           // ancho de cada tramo del muro sur (deja 2 m de entrada)
        float cSeg = 1f + seg / 2f;

        Caja(raiz.transform, "MuroSurIzq", new Vector3(-cSeg, alto / 2f, -m), new Vector3(seg, alto, 0.2f), matMuro);
        Caja(raiz.transform, "MuroSurDer", new Vector3(cSeg, alto / 2f, -m),  new Vector3(seg, alto, 0.2f), matMuro);
        Caja(raiz.transform, "MuroNorte",  new Vector3(0, alto / 2f, m),      new Vector3(tam, alto, 0.2f), matMuro);
        Caja(raiz.transform, "MuroEste",   new Vector3(m, alto / 2f, 0),      new Vector3(0.2f, alto, tam), matMuro);
        Caja(raiz.transform, "MuroOeste",  new Vector3(-m, alto / 2f, 0),     new Vector3(0.2f, alto, tam), matMuro);

        // Pelotas decorativas (semilla fija: siempre salen igual)
        GameObject grupo = Vacio("Pelotas", raiz.transform, Vector3.zero);
        Random.InitState(7);
        for (int i = 0; i < CANTIDAD_PELOTAS; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.name = "Pelota_" + i;
            p.transform.SetParent(grupo.transform, false);
            p.transform.localPosition = new Vector3(Random.Range(-m + 0.6f, m - 0.6f), 0.3f, Random.Range(-m + 0.6f, m - 0.6f));
            p.transform.localScale = Vector3.one * 0.6f;
            p.GetComponent<MeshRenderer>().sharedMaterial = pelotas[i % pelotas.Length];
            Object.DestroyImmediate(p.GetComponent<Collider>());   // no estorban al caminar
        }

        // Zona de escondite (trigger + componente HidingSpot de Rafael)
        GameObject zona = Vacio("HidingSpot_Piscina", raiz.transform, new Vector3(0, alto / 2f, 0));
        BoxCollider bc = zona.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(tam - 0.6f, alto, tam - 0.6f);
        int capa = LayerMask.NameToLayer("HidingSpot");
        if (capa >= 0) zona.layer = capa;
        zona.AddComponent<HidingSpot>();
    }

    // Arbol simple: tronco (cilindro) + copa (esfera sin collider).
    static void CrearArbol(Transform padre, Vector3 pos, Material matTronco, Material matCopa)
    {
        GameObject raiz = Vacio("Arbol", padre, pos);
        GameObject tronco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tronco.name = "Tronco";
        tronco.transform.SetParent(raiz.transform, false);
        tronco.transform.localPosition = new Vector3(0, 1.5f, 0);
        tronco.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);   // cilindro: alto real = 2 x escala Y
        tronco.GetComponent<MeshRenderer>().sharedMaterial = matTronco;

        GameObject copa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        copa.name = "Copa";
        copa.transform.SetParent(raiz.transform, false);
        copa.transform.localPosition = new Vector3(0, 3.6f, 0);
        copa.transform.localScale = Vector3.one * 3f;
        copa.GetComponent<MeshRenderer>().sharedMaterial = matCopa;
        Object.DestroyImmediate(copa.GetComponent<Collider>());
    }

    // Objeto "NavMesh" con el componente NavMeshSurface (se agrega por reflexion
    // para que el script compile aunque el paquete cambie de nombre).
    static void CrearNavMesh(Transform padre)
    {
        GameObject nav = Vacio("NavMesh", padre, Vector3.zero);
        System.Type t = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
        if (t != null) nav.AddComponent(t);
        else Debug.LogWarning("No encontre NavMeshSurface. Agregalo a mano: Add Component > NavMesh Surface.");
    }

    // ================== ZONA 4: AYUDAS (no hace falta tocar) ==========

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

    // Material de color como ARCHIVO (si fuera temporal, la escena lo perderia).
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
