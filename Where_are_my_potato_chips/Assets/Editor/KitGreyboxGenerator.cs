// =====================================================================
//  KitGreyboxGenerator.cs   (Dev 4 - Entornos)
//  DONDE PONERLO:  Assets/Editor/KitGreyboxGenerator.cs
//  COMO USARLO:    menu superior  Tools > Kit > Generar kit greybox
//
//  QUE HACE:
//  Crea de golpe los prefabs del kit modular (suelo, techo, pared, esquina,
//  puerta, columna) con cubos de Unity, todos con la MISMA medida de
//  cuadricula para que encajen como bloques de Lego.
//  Los guarda en Assets/02_Prefabs/Kit y crea un material gris en
//  Assets/03_Materials. Es seguro ejecutarlo varias veces (sobrescribe).
//
//  REGLA DE ORO DEL PIVOTE:
//  Todas las piezas tienen el pivote en el SUELO y en el CENTRO de su celda
//  (o de su borde, en el caso de las paredes). Suelo y techo miden 4 x 4 m;
//  las paredes miden 2 m de ancho (2 paredes cubren el borde de 1 suelo).
// =====================================================================
using UnityEditor;
using UnityEngine;
using System.IO;

public static class KitGreyboxGenerator
{
    // ================== ZONA 1: MEDIDAS (EDITA AQUI) ==================
    // Si el Dev del jugador usa otra altura, cambia estos numeros y
    // vuelve a ejecutar el menu. Todo el kit se regenera.
    // Segun la guia del equipo (tarjeta A-01): pared de 2 x 3 m.
    const float CELDA   = 2f;    // ancho de cada PARED (metros)
    const float LOSA    = 4f;    // lado de cada pieza de SUELO y TECHO (= 2 paredes)
    const float ALTO    = 3f;    // altura de pared (metros)
    const float GROSOR  = 0.2f;  // grosor de paredes, suelo y techo
    const float PUERTA_ANCHO = 1.2f;  // hueco de la puerta
    const float PUERTA_ALTO  = 2.2f;  // altura del hueco de la puerta

    // ================== ZONA 2: RUTAS (EDITA AQUI) ====================
    const string CARPETA_PREFABS = "Assets/02_Prefabs/Kit";
    const string CARPETA_MATS    = "Assets/03_Materials";
    const string RUTA_MATERIAL   = CARPETA_MATS + "/Greybox_Gris.mat";

    // ================== ZONA 3: MENU PRINCIPAL ========================
    [MenuItem("Tools/Kit/Generar kit greybox")]
    public static void Generar()
    {
        AsegurarCarpeta("Assets/02_Prefabs");
        AsegurarCarpeta(CARPETA_PREFABS);
        AsegurarCarpeta(CARPETA_MATS);

        Material mat = CrearMaterial();

        // Cada llamada crea UNA pieza. Comenta una linea para no generarla.
        GuardarPrefab(CrearSuelo(mat),    "Kit_Suelo");
        GuardarPrefab(CrearTecho(mat),    "Kit_Techo");
        GuardarPrefab(CrearPared(mat),    "Kit_Pared");
        GuardarPrefab(CrearEsquina(mat),  "Kit_Esquina");
        GuardarPrefab(CrearPuerta(mat),   "Kit_Puerta");
        GuardarPrefab(CrearColumna(mat),  "Kit_Columna");
        GuardarPrefab(CrearObstaculo(mat, 1f), "Kit_Obstaculo_1m");   // se puede saltar
        GuardarPrefab(CrearObstaculo(mat, 2f), "Kit_Obstaculo_2m");   // bloquea el paso

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Kit greybox generado en " + CARPETA_PREFABS);
    }

    // ================== ZONA 4: PIEZAS (una funcion por pieza) ========

    // Suelo: una losa de LOSA x LOSA (4 x 4 m). Su cara superior queda en y = 0.
    static GameObject CrearSuelo(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Suelo");
        Cubo(raiz, "Losa", new Vector3(0, -GROSOR / 2f, 0),
             new Vector3(LOSA, GROSOR, LOSA), mat);
        return raiz;
    }

    // Techo: igual que el suelo pero a la altura ALTO.
    static GameObject CrearTecho(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Techo");
        Cubo(raiz, "Losa", new Vector3(0, ALTO + GROSOR / 2f, 0),
             new Vector3(LOSA, GROSOR, LOSA), mat);
        return raiz;
    }

    // Pared: una pared completa de CELDA de ancho. Pivote en el centro
    // de su base. Mira hacia +Z (rotala 90 grados para el otro eje).
    static GameObject CrearPared(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Pared");
        Cubo(raiz, "Muro", new Vector3(0, ALTO / 2f, 0),
             new Vector3(CELDA, ALTO, GROSOR), mat);
        return raiz;
    }

    // Esquina: dos paredes en L (para cerrar una esquina de la celda).
    static GameObject CrearEsquina(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Esquina");
        float m = CELDA / 2f;
        // Pared a lo largo de X, en el borde z = -m
        Cubo(raiz, "MuroX", new Vector3(0, ALTO / 2f, -m),
             new Vector3(CELDA, ALTO, GROSOR), mat);
        // Pared a lo largo de Z, en el borde x = -m
        Cubo(raiz, "MuroZ", new Vector3(-m, ALTO / 2f, 0),
             new Vector3(GROSOR, ALTO, CELDA), mat);
        return raiz;
    }

    // Puerta: pared con un hueco en el centro (dos laterales + dintel).
    static GameObject CrearPuerta(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Puerta");
        float lado = (CELDA - PUERTA_ANCHO) / 2f;           // ancho de cada lateral
        float xLado = PUERTA_ANCHO / 2f + lado / 2f;        // posicion de cada lateral
        float altoDintel = ALTO - PUERTA_ALTO;              // altura sobre la puerta

        Cubo(raiz, "LateralIzq", new Vector3(-xLado, ALTO / 2f, 0),
             new Vector3(lado, ALTO, GROSOR), mat);
        Cubo(raiz, "LateralDer", new Vector3(xLado, ALTO / 2f, 0),
             new Vector3(lado, ALTO, GROSOR), mat);
        Cubo(raiz, "Dintel", new Vector3(0, PUERTA_ALTO + altoDintel / 2f, 0),
             new Vector3(PUERTA_ANCHO, altoDintel, GROSOR), mat);
        return raiz;
    }

    // Columna: para romper pasillos y esconder esquinas feas.
    static GameObject CrearColumna(Material mat)
    {
        GameObject raiz = new GameObject("Kit_Columna");
        Cubo(raiz, "Pilar", new Vector3(0, ALTO / 2f, 0),
             new Vector3(0.5f, ALTO, 0.5f), mat);
        return raiz;
    }

    // Obstaculo: caja de 1 x 1 m de base y 'alto' metros de altura
    // (1 m = el jugador puede saltarla, 2 m = no). Util para parkour.
    static GameObject CrearObstaculo(Material mat, float alto)
    {
        GameObject raiz = new GameObject("Kit_Obstaculo_" + alto + "m");
        Cubo(raiz, "Caja", new Vector3(0, alto / 2f, 0), new Vector3(1f, alto, 1f), mat);
        return raiz;
    }

    // ================== ZONA 5: AYUDAS (no hace falta tocar) ==========

    // Crea un cubo hijo de 'padre' con posicion/tamano local y material.
    static void Cubo(GameObject padre, string nombre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.name = nombre;
        c.transform.SetParent(padre.transform, false);
        c.transform.localPosition = pos;
        c.transform.localScale = escala;
        c.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // Guarda la pieza como prefab, la marca estatica (para NavMesh y
    // lightmaps) y borra la copia temporal de la escena.
    static void GuardarPrefab(GameObject go, string nombre)
    {
        go.isStatic = true;
        foreach (Transform hijo in go.transform) hijo.gameObject.isStatic = true;

        string ruta = CARPETA_PREFABS + "/" + nombre + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
    }

    // Material gris con el shader de URP (el proyecto usa URP).
    static Material CrearMaterial()
    {
        Material existente = AssetDatabase.LoadAssetAtPath<Material>(RUTA_MATERIAL);
        if (existente != null) return existente;

        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        Material m = new Material(s);
        m.SetColor("_BaseColor", new Color(0.6f, 0.6f, 0.6f));
        AssetDatabase.CreateAsset(m, RUTA_MATERIAL);
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
