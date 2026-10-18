// =====================================================================
//  NivelTools.cs   (Dev 4 - Entornos)   para Piscinas y cualquier escena
//  DONDE PONERLO:  Assets/Editor/NivelTools.cs
//  COMO USARLO:    menu  Tools > Kit > Nivel > ...
//
//  Sirve para dejar CUALQUIER nivel listo segun el checklist del equipo sin
//  tocar lo que ya tiene (no borra nada, solo agrega o mueve estos objetos):
//    PlayerSpawn, BossSpawn, PatrolPoints (con puntos hijos), ReplicaStore y
//    un objeto NavMesh con NavMeshSurface.
//  Cada opcion pone el objeto en el CENTRO DE LA VISTA Scene: mueve la camara
//  de la vista Scene hasta el lugar donde lo quieres y elige la opcion.
//  "Revisar checklist" cuenta lo que tiene la escena abierta.
// =====================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NivelTools
{
    // ================== ZONA 1: AJUSTES (EDITA AQUI) ==================
    const string RUTA_REPLICA = "Assets/02_Prefabs/ReplicaStore.prefab";
    const float  ALTURA_SPAWN = 0.1f;   // los spawns quedan un poco sobre el suelo
    const int    MIN_ESCONDITES = 3;    // minimo que pide el checklist

    // ================== ZONA 2: MENU ==================================
    [MenuItem("Tools/Kit/Nivel/1. Poner PlayerSpawn aqui")]
    static void PonerPlayerSpawn() { PonerUnico("PlayerSpawn"); }

    [MenuItem("Tools/Kit/Nivel/2. Poner BossSpawn aqui")]
    static void PonerBossSpawn() { PonerUnico("BossSpawn"); }

    [MenuItem("Tools/Kit/Nivel/3. Agregar punto de patrulla aqui")]
    static void AgregarPatrulla()
    {
        GameObject raiz = BuscarOCrear("PatrolPoints", Vector3.zero);
        int n = raiz.transform.childCount;
        GameObject p = new GameObject("P_" + n);
        Undo.RegisterCreatedObjectUndo(p, "Punto de patrulla");
        p.transform.SetParent(raiz.transform, false);
        p.transform.position = PuntoVista(ALTURA_SPAWN);
        Terminar(p);
    }

    [MenuItem("Tools/Kit/Nivel/4. Poner ReplicaStore aqui")]
    static void PonerReplica()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_REPLICA);
        if (prefab == null) { Aviso("No encuentro " + RUTA_REPLICA); return; }
        GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(t, "ReplicaStore");
        t.transform.position = PuntoVista(0f);
        Terminar(t);
    }

    [MenuItem("Tools/Kit/Nivel/5. Agregar NavMesh Surface")]
    static void AgregarNavMesh()
    {
        System.Type t = TipoSurface();
        if (t == null) { Aviso("No encuentro NavMeshSurface. Agregalo a mano: Add Component > NavMesh Surface."); return; }
        GameObject nav = GameObject.Find("NavMesh");
        if (nav == null)
        {
            nav = new GameObject("NavMesh");
            Undo.RegisterCreatedObjectUndo(nav, "NavMesh");
        }
        if (nav.GetComponent(t) == null) Undo.AddComponent(nav, t);
        Terminar(nav);
        Debug.Log("Listo. Selecciona el objeto NavMesh y presiona Bake.");
    }

    [MenuItem("Tools/Kit/Nivel/6. Revisar checklist de la escena")]
    static void RevisarChecklist()
    {
        int escondites = Object.FindObjectsByType<HidingSpot>(FindObjectsSortMode.None).Length;
        bool replica = false;
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (BuscaReplica(go.transform)) { replica = true; break; }
        System.Type ts = TipoSurface();
        bool surface = ts != null && Object.FindObjectsByType(ts, FindObjectsSortMode.None).Length > 0;
        GameObject pp = GameObject.Find("PatrolPoints");

        string msg = "Escena: " + SceneManager.GetActiveScene().name + "\n\n"
            + Marca(GameObject.Find("PlayerSpawn") != null) + " PlayerSpawn\n"
            + Marca(GameObject.Find("BossSpawn") != null) + " BossSpawn\n"
            + Marca(pp != null && pp.transform.childCount > 0) + " PatrolPoints (opcional)\n"
            + Marca(escondites >= MIN_ESCONDITES) + " HidingSpot: " + escondites + " (minimo " + MIN_ESCONDITES + ")\n"
            + Marca(replica) + " ReplicaStore\n"
            + Marca(surface) + " NavMeshSurface (recuerda hornear con Bake)";
        Debug.Log(msg);
        EditorUtility.DisplayDialog("Checklist del nivel", msg, "Ok");
    }

    // ================== ZONA 3: AYUDAS (no hace falta tocar) ==========

    // Centro de la vista Scene, a la altura 'y'.
    static Vector3 PuntoVista(float y)
    {
        SceneView sv = SceneView.lastActiveSceneView;
        Vector3 p = (sv != null) ? sv.pivot : Vector3.zero;
        p.y = y;
        return p;
    }

    // Crea el objeto si no existe; si existe, lo mueve al centro de la vista.
    static void PonerUnico(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        if (go == null)
        {
            go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, nombre);
        }
        else Undo.RecordObject(go.transform, "Mover " + nombre);
        go.transform.position = PuntoVista(ALTURA_SPAWN);
        Terminar(go);
    }

    static GameObject BuscarOCrear(string nombre, Vector3 pos)
    {
        GameObject go = GameObject.Find(nombre);
        if (go != null) return go;
        go = new GameObject(nombre);
        go.transform.position = pos;
        Undo.RegisterCreatedObjectUndo(go, nombre);
        return go;
    }

    static bool BuscaReplica(Transform t)
    {
        if (t.name.StartsWith("ReplicaStore")) return true;
        foreach (Transform hijo in t) if (BuscaReplica(hijo)) return true;
        return false;
    }

    static System.Type TipoSurface()
    {
        return System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
    }

    static string Marca(bool ok) { return ok ? "[OK]" : "[FALTA]"; }

    static void Terminar(GameObject go)
    {
        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void Aviso(string texto)
    {
        EditorUtility.DisplayDialog("Aviso", texto, "Ok");
    }
}
