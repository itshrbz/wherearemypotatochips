// =====================================================================
//  KitGreyboxGenerator.cs   (Dev 4 - Entornos)   tarjetas A-01 + A-02
//  DONDE PONERLO:  Assets/Editor/KitGreyboxGenerator.cs  (reemplaza el anterior)
//  COMO USARLO:    menu  Tools > Kit > Generar kit y tienda (A-01 + A-02)
//
//  QUE HACE (todo de un solo clic, se puede ejecutar varias veces):
//   A-01  Piezas del kit: suelo, techo CON LUZ FLUORESCENTE, pared, esquina,
//         puerta, columna y obstaculos de 1 m y 2 m.
//   A-02  Piezas de tienda: mostrador, caja, gondola y puerta del almacen.
//   Arma ReplicaStore.prefab (8 x 8 m, con puerta) usando esas piezas.
//
//  CARPETAS: si existe Assets/02_Prefabs/Kit, el script la RENOMBRA a
//  Assets/02_Prefabs/Environment (como pide la tarjeta). Al renombrar, los
//  prefabs conservan su identidad y nada se rompe en las escenas.
//  Todas las piezas tienen el pivote en el suelo.
// =====================================================================
using UnityEditor;
using UnityEngine;
using System.IO;

public static class KitGreyboxGenerator
{
    // ================== ZONA 1: MEDIDAS (EDITA AQUI) ==================
    const float CELDA   = 2f;    // ancho de cada PARED (metros)
    const float LOSA    = 4f;    // lado de SUELO y TECHO (= 2 paredes)
    const float ALTO    = 3f;    // altura de pared
    const float GROSOR  = 0.2f;  // grosor de paredes, suelo y techo
    const float PUERTA_ANCHO = 1.2f;  // hueco de la puerta normal
    const float PUERTA_ALTO  = 2.2f;
    const float PUERTA_ALM_ANCHO = 1.4f;  // hueco de la puerta del almacen
    const float PUERTA_ALM_ALTO  = 2.4f;

    // Luz fluorescente del techo
    const float LUZ_INTENSIDAD = 2f;
    const float LUZ_ALCANCE    = 7f;
    static readonly Color LUZ_COLOR = new Color(0.93f, 1f, 0.9f);

    // ================== ZONA 2: RUTAS (EDITA AQUI) ====================
    const string CARPETA_ANTIGUA = "Assets/02_Prefabs/Kit";
    const string CARPETA_PREFABS = "Assets/02_Prefabs/Environment";
    const string RUTA_REPLICA    = "Assets/02_Prefabs/ReplicaStore.prefab";
    const string CARPETA_MATS    = "Assets/03_Materials";

    // ================== ZONA 3: MENU PRINCIPAL ========================
    [MenuItem("Tools/Kit/Generar kit y tienda (A-01 + A-02)")]
    public static void Generar()
    {
        AsegurarCarpeta("Assets/02_Prefabs");

        // Renombrar Kit -> Environment conservando los prefabs existentes
        if (AssetDatabase.IsValidFolder(CARPETA_ANTIGUA) && !AssetDatabase.IsValidFolder(CARPETA_PREFABS))
        {
            string error = AssetDatabase.MoveAsset(CARPETA_ANTIGUA, CARPETA_PREFABS);
            if (!string.IsNullOrEmpty(error)) Debug.LogWarning("No se pudo renombrar Kit: " + error);
        }
        AsegurarCarpeta(CARPETA_PREFABS);
        AsegurarCarpeta(CARPETA_MATS);

        Material gris   = Mat("Greybox_Gris",        new Color(0.6f, 0.6f, 0.6f), false);
        Material oscuro = Mat("Greybox_Oscuro",      new Color(0.25f, 0.25f, 0.28f), false);
        Material luz    = Mat("Luz_Fluorescente",    Color.white, true);

        // ---- A-01: kit (comenta una linea para no generar esa pieza) ----
        GuardarPrefab(CrearSuelo(gris),            "Kit_Suelo");
        GuardarPrefab(CrearTecho(gris, luz),       "Kit_Techo");
        GuardarPrefab(CrearPared(gris),            "Kit_Pared");
        GuardarPrefab(CrearEsquina(gris),          "Kit_Esquina");
        GuardarPrefab(CrearPuerta(gris),           "Kit_Puerta");
        GuardarPrefab(CrearColumna(gris),          "Kit_Columna");
        GuardarPrefab(CrearObstaculo(gris, 1f),    "Kit_Obstaculo_1m");
        GuardarPrefab(CrearObstaculo(gris, 2f),    "Kit_Obstaculo_2m");

        // ---- A-02: piezas de tienda ----
        GuardarPrefab(CrearMostrador(gris),                "Kit_Mostrador");
        GuardarPrefab(CrearCaja(oscuro),                   "Kit_Caja");
        GuardarPrefab(CrearGondola(gris),                  "Kit_Gondola");
        GuardarPrefab(CrearPuertaAlmacen(gris, oscuro),    "Kit_PuertaAlmacen");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ---- ReplicaStore (usa las piezas de arriba) ----
        CrearReplicaStore();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Kit y tienda generados en " + CARPETA_PREFABS);
    }

    // ================== ZONA 4: PIEZAS DEL KIT (A-01) =================

    // Suelo: losa de LOSA x LOSA. Cara superior en y = 0.
    static GameObject CrearSuelo(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Suelo");
        Cubo(raiz, "Losa", new Vector3(0, -GROSOR / 2f, 0), new Vector3(LOSA, GROSOR, LOSA), mat);
        return raiz;
    }

    // Techo: losa a la altura ALTO + panel fluorescente + luz.
    static GameObject CrearTecho(Material mat, Material matLuz)
    {
        GameObject raiz = new GameObject("Kit_Techo");
        Cubo(raiz, "Losa", new Vector3(0, ALTO + GROSOR / 2f, 0), new Vector3(LOSA, GROSOR, LOSA), mat);

        // Panel emisivo (solo visual: se le quita el collider)
        GameObject panel = Cubo(raiz, "PanelFluorescente", new Vector3(0, ALTO - 0.03f, 0),
                                new Vector3(1.4f, 0.06f, 0.35f), matLuz);
        Object.DestroyImmediate(panel.GetComponent<Collider>());

        // Luz puntual suave debajo del panel
        GameObject objLuz = new GameObject("LuzFluorescente");
        objLuz.transform.SetParent(raiz.transform, false);
        objLuz.transform.localPosition = new Vector3(0, ALTO - 0.4f, 0);
        Light l = objLuz.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = LUZ_COLOR;
        l.intensity = LUZ_INTENSIDAD;
        l.range = LUZ_ALCANCE;
        l.shadows = LightShadows.None;   // sin sombras: mas rapido
        return raiz;
    }

    // Pared: CELDA de ancho, pivote en el centro de la base. Corre a lo largo de X.
    static GameObject CrearPared(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Pared");
        Cubo(raiz, "Muro", new Vector3(0, ALTO / 2f, 0), new Vector3(CELDA, ALTO, GROSOR), mat);
        return raiz;
    }

    // Esquina: dos paredes en L.
    static GameObject CrearEsquina(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Esquina");
        float m = CELDA / 2f;
        Cubo(raiz, "MuroX", new Vector3(0, ALTO / 2f, -m), new Vector3(CELDA, ALTO, GROSOR), mat);
        Cubo(raiz, "MuroZ", new Vector3(-m, ALTO / 2f, 0), new Vector3(GROSOR, ALTO, CELDA), mat);
        return raiz;
    }

    // Puerta: pared con hueco.
    static GameObject CrearPuerta(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Puerta");
        MarcoPuerta(raiz, mat, PUERTA_ANCHO, PUERTA_ALTO);
        return raiz;
    }

    // Columna de 1 x 1 m.
    static GameObject CrearColumna(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Columna");
        Cubo(raiz, "Pilar", new Vector3(0, ALTO / 2f, 0), new Vector3(1f, ALTO, 1f), mat);
        return raiz;
    }

    // Obstaculo: base 1 x 1 m, 'alto' metros (1 m se salta, 2 m no).
    static GameObject CrearObstaculo(Material mat, float alto)
    {
        GameObject raiz = new GameObject("Kit_Obstaculo_" + alto + "m");
        Cubo(raiz, "Caja", new Vector3(0, alto / 2f, 0), new Vector3(1f, alto, 1f), mat);
        return raiz;
    }

    // ================== ZONA 5: PIEZAS DE TIENDA (A-02) ===============

    // Mostrador: 3 m de largo (eje X), 1 m de alto, 0.8 m de fondo.
    static GameObject CrearMostrador(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Mostrador");
        Cubo(raiz, "Cuerpo", new Vector3(0, 0.5f, 0), new Vector3(3f, 1f, 0.8f), mat);
        Cubo(raiz, "Tapa",   new Vector3(0, 1.04f, 0), new Vector3(3.1f, 0.08f, 0.9f), mat);
        return raiz;
    }

    // Caja registradora: se coloca sobre el mostrador (a y = 1.08).
    static GameObject CrearCaja(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Caja");
        Cubo(raiz, "Base",     new Vector3(0, 0.175f, 0), new Vector3(0.5f, 0.35f, 0.4f), mat);
        Cubo(raiz, "Pantalla", new Vector3(0, 0.5f, -0.15f), new Vector3(0.4f, 0.3f, 0.05f), mat);
        return raiz;
    }

    // Gondola (estante de supermercado): 0.6 de ancho, 1.8 de alto, 3 de largo (eje Z).
    static GameObject CrearGondola(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Gondola");
        Cubo(raiz, "Cuerpo", new Vector3(0, 0.9f, 0), new Vector3(0.6f, 1.8f, 3f), mat);
        return raiz;
    }

    // Puerta del almacen: marco + hoja ("Hoja" se puede desactivar o animar).
    static GameObject CrearPuertaAlmacen(Material mat, Material matHoja)
    {
        GameObject raiz = new GameObject("Kit_PuertaAlmacen");
        MarcoPuerta(raiz, mat, PUERTA_ALM_ANCHO, PUERTA_ALM_ALTO);
        Cubo(raiz, "Hoja", new Vector3(0, PUERTA_ALM_ALTO / 2f, 0),
             new Vector3(PUERTA_ALM_ANCHO, PUERTA_ALM_ALTO, 0.08f), matHoja);
        return raiz;
    }

    // ================== ZONA 6: REPLICASTORE ==========================
    // Cuarto de 8 x 8 m centrado en (0,0,0). Entrada (puerta) en el lado -Z.
    static void CrearReplicaStore()
    {
        GameObject raiz = new GameObject("ReplicaStore");

        // Suelos y techos (4 de cada uno)
        float[] c = { -2f, 2f };
        foreach (float x in c)
            foreach (float z in c)
            {
                Poner(raiz, "Kit_Suelo", new Vector3(x, 0, z), 0);
                Poner(raiz, "Kit_Techo", new Vector3(x, 0, z), 0);
            }

        // Paredes laterales (x = -4 y x = 4), giradas 90 grados
        float[] zs = { -3f, -1f, 1f, 3f };
        foreach (float z in zs)
        {
            Poner(raiz, "Kit_Pared", new Vector3(-4f, 0, z), 90);
            Poner(raiz, "Kit_Pared", new Vector3(4f, 0, z), 90);
        }

        // Pared del fondo (z = 4) y del frente (z = -4, con puerta en x = 1)
        float[] xs = { -3f, -1f, 1f, 3f };
        foreach (float x in xs)
        {
            Poner(raiz, "Kit_Pared", new Vector3(x, 0, 4f), 0);
            Poner(raiz, x == 1f ? "Kit_Puerta" : "Kit_Pared", new Vector3(x, 0, -4f), 0);
        }

        // Muebles
        Poner(raiz, "Kit_Mostrador", new Vector3(-1.6f, 0, -2.6f), 0);
        Poner(raiz, "Kit_Caja",      new Vector3(-2.4f, 1.08f, -2.6f), 0);
        Poner(raiz, "Kit_Gondola",   new Vector3(-2.8f, 0, 1.2f), 0);
        Poner(raiz, "Kit_Gondola",   new Vector3(0f,    0, 1.2f), 0);
        Poner(raiz, "Kit_Gondola",   new Vector3(2.8f,  0, 1.2f), 0);

        PrefabUtility.SaveAsPrefabAsset(raiz, RUTA_REPLICA);
        Object.DestroyImmediate(raiz);
    }

    // ================== ZONA 7: AYUDAS (no hace falta tocar) ==========

    // Pared con hueco: dos laterales + dintel.
    static void MarcoPuerta(GameObject raiz, Material mat, float ancho, float alto)
    {
        float lado = (CELDA - ancho) / 2f;
        float xLado = ancho / 2f + lado / 2f;
        float altoDintel = ALTO - alto;
        Cubo(raiz, "LateralIzq", new Vector3(-xLado, ALTO / 2f, 0), new Vector3(lado, ALTO, GROSOR), mat);
        Cubo(raiz, "LateralDer", new Vector3(xLado, ALTO / 2f, 0), new Vector3(lado, ALTO, GROSOR), mat);
        Cubo(raiz, "Dintel", new Vector3(0, alto + altoDintel / 2f, 0), new Vector3(ancho, altoDintel, GROSOR), mat);
    }

    // Cubo hijo de 'padre' con posicion/tamano local y material.
    static GameObject Cubo(GameObject padre, string nombre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.name = nombre;
        c.transform.SetParent(padre.transform, false);
        c.transform.localPosition = pos;
        c.transform.localScale = escala;
        c.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return c;
    }

    // Instancia un prefab del kit como hijo (mantiene el vinculo con el prefab).
    static GameObject Poner(GameObject padre, string nombrePrefab, Vector3 pos, float giroY)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(CARPETA_PREFABS + "/" + nombrePrefab + ".prefab");
        if (asset == null) { Debug.LogError("Falta el prefab " + nombrePrefab); return null; }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.transform.SetParent(padre.transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0, giroY, 0);
        return go;
    }

    // Guarda como prefab (sobrescribe el existente) y marca estatico.
    static void GuardarPrefab(GameObject go, string nombre)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.isStatic = true;
        PrefabUtility.SaveAsPrefabAsset(go, CARPETA_PREFABS + "/" + nombre + ".prefab");
        Object.DestroyImmediate(go);
    }

    // Material URP: lo busca y, si no existe, lo crea.
    static Material Mat(string nombre, Color color, bool emisivo)
    {
        string ruta = CARPETA_MATS + "/" + nombre + ".mat";
        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null) return existente;

        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", color);
        if (emisivo)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * 2f);
        }
        AssetDatabase.CreateAsset(m, ruta);
        return m;
    }

    static void AsegurarCarpeta(string ruta)
    {
        if (!AssetDatabase.IsValidFolder(ruta))
        {
            string padre = Path.GetDirectoryName(ruta).Replace("\\", "/");
            AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
        }
    }
}
