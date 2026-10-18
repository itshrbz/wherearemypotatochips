// =====================================================================
//  MuestrarioBuilder.cs   (Dev 4 - Entornos)   muestra A-01 + A-02
//  DONDE PONERLO:  Assets/Editor/MuestrarioBuilder.cs
//  ANTES DE USARLO: ejecuta Tools > Kit > Generar kit y tienda (A-01 + A-02).
//  COMO USARLO:    menu  Tools > Kit > Construir muestrario (A-01 + A-02)
//
//  QUE HACE (en tu escena de prueba, se puede repetir):
//   1) CATALOGO: una fila con TODAS las piezas del kit y de la tienda, cada
//      una con el nombre de su prefab (miralas en la Hierarchy).
//   2) PASILLO de ejemplo armado con el kit (4 x 8 m, con puerta al fondo).
//   3) ReplicaStore ya colocada, con su puerta mirando al catalogo.
//   4) Un suelo grande para poder caminar, y el Player.prefab si existe.
//  Todo queda dentro de "Muestrario_Generado". Al repetirlo, ese objeto se
//  borra y se vuelve a crear. No toca nada mas de la escena.
// =====================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MuestrarioBuilder
{
    // ================== ZONA 1: AJUSTES (EDITA AQUI) ==================
    // Para usar la escena Cristhian, cambia "Bullain" por "Cristhian".
    const string RUTA_ESCENA    = "Assets/00_Scenes/_Sandbox/Bullain.unity";
    const string CARPETA_PREFABS = "Assets/02_Prefabs/Environment";
    const string RUTA_PLAYER    = "Assets/02_Prefabs/Player.prefab";
    const string NOMBRE_RAIZ    = "Muestrario_Generado";

    const float SEPARACION   = 6f;                              // metros entre piezas del catalogo
    static readonly Vector3 POS_CATALOGO = new Vector3(0f, 0f, -12f);
    static readonly Vector3 POS_PASILLO  = new Vector3(-15f, 0f, 2f);
    static readonly Vector3 POS_TIENDA   = new Vector3(10f, 0f, 6f);
    static readonly Vector3 POS_PLAYER   = new Vector3(0f, 1f, -6f);

    // Piezas del catalogo, en orden de izquierda a derecha
    static readonly string[] PIEZAS =
    {
        "Kit_Suelo", "Kit_Techo", "Kit_Pared", "Kit_Esquina", "Kit_Puerta", "Kit_Columna",
        "Kit_Obstaculo_1m", "Kit_Obstaculo_2m",
        "Kit_Mostrador", "Kit_Caja", "Kit_Gondola", "Kit_PuertaAlmacen"
    };

    // ================== ZONA 2: MENU PRINCIPAL ========================
    [MenuItem("Tools/Kit/Construir muestrario (A-01 + A-02)")]
    public static void Construir()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(CARPETA_PREFABS + "/Kit_Pared.prefab") == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/02_Prefabs/ReplicaStore.prefab") == null)
        {
            EditorUtility.DisplayDialog("Faltan prefabs",
                "Primero ejecuta Tools > Kit > Generar kit y tienda (A-01 + A-02).", "Ok");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RUTA_ESCENA) == null)
        {
            EditorUtility.DisplayDialog("No encuentro la escena",
                "No existe " + RUTA_ESCENA + ". Cambia RUTA_ESCENA en la ZONA 1 del script.", "Ok");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene escena = EditorSceneManager.OpenScene(RUTA_ESCENA, OpenSceneMode.Single);
        GameObject viejo = GameObject.Find(NOMBRE_RAIZ);
        if (viejo != null) Object.DestroyImmediate(viejo);

        GameObject raiz = new GameObject(NOMBRE_RAIZ);

        CrearSueloGrande(raiz.transform);
        CrearCatalogo(raiz.transform);
        CrearPasillo(raiz.transform);
        CrearTienda(raiz.transform);
        PonerPlayer(raiz.transform);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = raiz;
        Debug.Log("Muestrario construido en " + RUTA_ESCENA);
    }

    // ================== ZONA 3: PARTES DEL MUESTRARIO =================

    // Suelo grande para caminar. Queda un poco mas abajo que los suelos del kit
    // para que no se "peleen" visualmente (z-fighting).
    static void CrearSueloGrande(Transform padre)
    {
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = "Suelo_Muestrario";
        s.transform.SetParent(padre, false);
        s.transform.localPosition = new Vector3(0f, -0.35f, 0f);
        s.transform.localScale = new Vector3(90f, 0.2f, 50f);
        int capa = LayerMask.NameToLayer("Ground");
        if (capa >= 0) s.layer = capa;
    }

    // Fila con todas las piezas, centrada en POS_CATALOGO.
    static void CrearCatalogo(Transform padre)
    {
        GameObject grupo = Vacio("Catalogo", padre, POS_CATALOGO);
        float inicio = -(PIEZAS.Length - 1) * SEPARACION / 2f;
        for (int i = 0; i < PIEZAS.Length; i++)
            Poner(grupo.transform, PIEZAS[i], new Vector3(inicio + i * SEPARACION, 0f, 0f), 0f);
    }

    // Pasillo de 4 x 8 m: 2 suelos, 2 techos, paredes laterales (2 m cada una),
    // cierre de inicio y, al fondo, una pared y una puerta.
    static void CrearPasillo(Transform padre)
    {
        GameObject g = Vacio("Pasillo_Ejemplo", padre, POS_PASILLO);

        float[] zSuelos = { 0f, 4f };
        foreach (float z in zSuelos)
        {
            Poner(g.transform, "Kit_Suelo", new Vector3(0f, 0f, z), 0f);
            Poner(g.transform, "Kit_Techo", new Vector3(0f, 0f, z), 0f);
        }

        float[] zParedes = { -1f, 1f, 3f, 5f };
        foreach (float z in zParedes)
        {
            Poner(g.transform, "Kit_Pared", new Vector3(-2f, 0f, z), 90f);
            Poner(g.transform, "Kit_Pared", new Vector3(2f, 0f, z), 90f);
        }

        Poner(g.transform, "Kit_Pared",  new Vector3(-1f, 0f, -2f), 0f);   // cierre de inicio
        Poner(g.transform, "Kit_Pared",  new Vector3(1f, 0f, -2f), 0f);
        Poner(g.transform, "Kit_Pared",  new Vector3(-1f, 0f, 6f), 0f);    // fondo
        Poner(g.transform, "Kit_Puerta", new Vector3(1f, 0f, 6f), 0f);     // puerta
    }

    // ReplicaStore colocada en la escena.
    static void CrearTienda(Transform padre)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/02_Prefabs/ReplicaStore.prefab");
        GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        t.transform.SetParent(padre, false);
        t.transform.localPosition = POS_TIENDA;
    }

    // Si Rafael ya subio Player.prefab, lo pone para que puedas caminar.
    static void PonerPlayer(Transform padre)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PLAYER);
        if (asset == null)
        {
            Debug.Log("No hay Player.prefab todavia: el muestrario queda sin jugador.");
            return;
        }
        GameObject p = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        p.transform.SetParent(padre, false);
        p.transform.localPosition = POS_PLAYER;
    }

    // ================== ZONA 4: AYUDAS (no hace falta tocar) ==========

    static GameObject Vacio(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        return go;
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
}
