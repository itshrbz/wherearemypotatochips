// =====================================================================
//  TiendaBuilder.cs   tarjeta N-01 (tienda real, escena 02_Store)
//  COMO USARLO:  Tools > Kit > Construir tienda real (N-01)  y Ctrl+S.
//
//  Viste la tienda alrededor de lo que ya funciona (mostrador, caja,
//  compradores, puerta del almacen y PlayerSpawn), sin moverlo:
//   - Local de 14 x 13.5 m con piso a cuadros, paredes, zocalo y techo.
//   - Vitrina de vidrio al frente con puertas abiertas: los compradores
//     entran desde la vereda.
//   - Dos gondolas dobles y un estante de pared llenos de productos.
//     En una gondola, el hueco vacio de "POTATO CHIPS" (agotado).
//   - Tres heladeras con botellas, estante detras del mostrador,
//     letrero "POTATO MART", carteles, reloj, camara de seguridad.
//   - Tubos fluorescentes con luz (uno parpadea) y la calle de noche.
//  Todo queda en "Tienda_Generada". Al repetir, se borra y se recrea.
// =====================================================================
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class TiendaBuilder
{
    const string RUTA_ESCENA = "Assets/00_Scenes/02_Store.unity";
    const string CARPETA_MATS = "Assets/03_Materials";
    const string NOMBRE_RAIZ = "Tienda_Generada";

    // Interior: x de -7 a 7, z de -4 (pared del almacen) a 9.5 (vitrina)
    const float X_LADO = 7f;
    const float Z_FONDO = -4f;
    const float Z_FRENTE = 9.5f;
    const float ALTO = 3.2f;
    const float GROSOR = 0.2f;
    const float HUECO_PUERTA_ALMACEN = 1f;   // mitad del hueco de la puerta del fondo
    const float HUECO_ENTRADA = 1.3f;        // mitad del hueco de la entrada

    static readonly Vector3 POS_ENTRADA = new Vector3(0f, 1f, 11.5f);
    static readonly Vector3 POS_SALIDA = new Vector3(0.6f, 1f, 11.5f);

    static Material piso1, piso2, pared, zocalo, techo, metal, madera, vidrio, tubo, negro, calle, carton;
    static Material[] productos;

    [MenuItem("Tools/Kit/Construir tienda real (N-01)")]
    public static void Construir()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene escena = EditorSceneManager.OpenScene(RUTA_ESCENA, OpenSceneMode.Single);
        GameObject viejo = GameObject.Find(NOMBRE_RAIZ);
        if (viejo != null) Object.DestroyImmediate(viejo);

        CrearMateriales();
        GameObject raiz = new GameObject(NOMBRE_RAIZ);
        Transform r = raiz.transform;

        PrepararEscena();
        CrearPisoYTecho(r);
        CrearParedes(r);
        CrearVitrina(r);
        CrearZonaMostrador(r);
        CrearEstanteriaFondo(r);
        CrearGondolas(r);
        CrearHeladeras(r);
        CrearDetalles(r);
        CrearLuces(r);
        CrearCalle(r);

        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            if (t.GetComponent<Light>() == null && t.GetComponent<LightFlicker>() == null) t.gameObject.isStatic = true;

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = raiz;
        Debug.Log("Tienda construida.");
    }

    // ---------------------------------------------------------------- escena

    // Apaga el piso viejo, mueve la entrada de los compradores a la vereda y pone la noche.
    static void PrepararEscena()
    {
        GameObject pisoViejo = GameObject.Find("Floor");
        if (pisoViejo != null) pisoViejo.SetActive(false);

        Mover("Entrance", POS_ENTRADA);
        Mover("Exit", POS_SALIDA);

        GameObject sol = GameObject.Find("Directional Light");
        if (sol != null && sol.TryGetComponent(out Light luzSol))
        {
            luzSol.intensity = 0.08f;
            luzSol.color = new Color(0.55f, 0.6f, 0.8f);
        }

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.16f, 0.17f, 0.19f);
    }

    static void Mover(string nombre, Vector3 pos)
    {
        GameObject go = GameObject.Find(nombre);
        if (go != null) go.transform.position = pos;
    }

    // ---------------------------------------------------------------- estructura

    static void CrearPisoYTecho(Transform padre)
    {
        Transform g = Vacio("PisoYTecho", padre, Vector3.zero).transform;

        // Base con collider y baldosas de 1 m encima (sin collider)
        Caja(g, "Base", new Vector3(0f, -0.1f, 2.75f), new Vector3(X_LADO * 2f, 0.2f, Z_FRENTE - Z_FONDO), piso1);
        Transform baldosas = Vacio("Baldosas", g, Vector3.zero).transform;
        for (float x = -X_LADO + 0.5f; x < X_LADO; x += 1f)
            for (float z = Z_FONDO + 0.5f; z < Z_FRENTE; z += 1f)
            {
                bool par = (Mathf.FloorToInt(x + X_LADO) + Mathf.FloorToInt(z - Z_FONDO)) % 2 == 0;
                if (!par) Caja(baldosas, "Baldosa", new Vector3(x, 0.002f, z), new Vector3(1f, 0.004f, 1f), piso2, false);
            }

        Caja(g, "Techo", new Vector3(0f, ALTO + 0.1f, 2.75f), new Vector3(X_LADO * 2f + 0.4f, 0.2f, Z_FRENTE - Z_FONDO + 0.4f), techo);
    }

    static void CrearParedes(Transform padre)
    {
        Transform g = Vacio("Paredes", padre, Vector3.zero).transform;
        float largoZ = Z_FRENTE - Z_FONDO;
        float centroZ = (Z_FRENTE + Z_FONDO) / 2f;

        ParedConZocalo(g, "Izquierda", new Vector3(-X_LADO, 0f, centroZ), new Vector3(GROSOR, ALTO, largoZ), true);
        ParedConZocalo(g, "Derecha", new Vector3(X_LADO, 0f, centroZ), new Vector3(GROSOR, ALTO, largoZ), true);

        // Fondo con el hueco de la puerta del almacen (la puerta ya esta en la escena)
        float largo = X_LADO - HUECO_PUERTA_ALMACEN;
        float cx = (X_LADO + HUECO_PUERTA_ALMACEN) / 2f;
        ParedConZocalo(g, "FondoIzq", new Vector3(-cx, 0f, Z_FONDO), new Vector3(largo, ALTO, GROSOR), false);
        ParedConZocalo(g, "FondoDer", new Vector3(cx, 0f, Z_FONDO), new Vector3(largo, ALTO, GROSOR), false);
        Caja(g, "FondoDintel", new Vector3(0f, 2.9f, Z_FONDO), new Vector3(HUECO_PUERTA_ALMACEN * 2f, 0.6f, GROSOR), pared);
    }

    static void ParedConZocalo(Transform padre, string nombre, Vector3 basePos, Vector3 tam, bool alongZ)
    {
        Caja(padre, nombre, basePos + Vector3.up * (tam.y / 2f), tam, pared);
        Vector3 tamZocalo = alongZ ? new Vector3(tam.x + 0.06f, 0.25f, tam.z) : new Vector3(tam.x, 0.25f, tam.z + 0.06f);
        Caja(padre, nombre + "_Zocalo", basePos + Vector3.up * 0.125f, tamZocalo, zocalo, false);
        Vector3 tamFranja = alongZ ? new Vector3(tam.x + 0.04f, 0.12f, tam.z) : new Vector3(tam.x, 0.12f, tam.z + 0.04f);
        Caja(padre, nombre + "_Franja", basePos + Vector3.up * 1.3f, tamFranja, zocalo, false);
    }

    // Frente de vidrio: murete, vidrios, cabecera y la entrada con las puertas abiertas.
    static void CrearVitrina(Transform padre)
    {
        Transform g = Vacio("Vitrina", padre, Vector3.zero).transform;
        float largo = X_LADO - HUECO_ENTRADA;
        float cx = (X_LADO + HUECO_ENTRADA) / 2f;

        foreach (float lado in new[] { -1f, 1f })
        {
            float x = cx * lado;
            Caja(g, "Murete", new Vector3(x, 0.3f, Z_FRENTE), new Vector3(largo, 0.6f, GROSOR), zocalo);
            Caja(g, "Vidrio", new Vector3(x, 1.6f, Z_FRENTE), new Vector3(largo, 2f, 0.04f), vidrio);
            for (float m = -largo / 2f; m <= largo / 2f + 0.01f; m += largo / 3f)
                Caja(g, "Marco", new Vector3(x + m, 1.6f, Z_FRENTE), new Vector3(0.08f, 2f, 0.1f), metal, false);
            Caja(g, "PuertaAbierta", new Vector3((HUECO_ENTRADA + 0.6f) * lado, 1.2f, Z_FRENTE + 0.12f), new Vector3(1.2f, 2.3f, 0.04f), vidrio, false);
        }
        Caja(g, "Cabecera", new Vector3(0f, 2.9f, Z_FRENTE), new Vector3(X_LADO * 2f, 0.6f, GROSOR), pared);
        Caja(g, "Felpudo", new Vector3(0f, 0.01f, Z_FRENTE - 0.9f), new Vector3(2.2f, 0.02f, 1.2f), negro, false);

        Texto(g, "Cartel_OPEN", "OPEN 24H", new Vector3(-4.5f, 2.2f, Z_FRENTE - 0.05f), Vector3.forward, 2.4f, new Color(1f, 0.3f, 0.25f));
    }

    // ---------------------------------------------------------------- mostrador

    // El mostrador y la caja de la escena se quedan; aqui se visten y se completan.
    static void CrearZonaMostrador(Transform padre)
    {
        Transform g = Vacio("ZonaMostrador", padre, Vector3.zero).transform;

        Caja(g, "FrenteMadera", new Vector3(0f, 0.5f, 0.32f), new Vector3(2.1f, 0.95f, 0.04f), madera, false);
        Caja(g, "Tapa", new Vector3(0f, 1.02f, 0f), new Vector3(2.2f, 0.05f, 0.7f), metal);
        // Mueble bajo la caja registradora (la caja flota en x 0.7, z -1); deja libre el lugar del jugador
        Caja(g, "MuebleCaja", new Vector3(0.85f, 0.5f, -0.85f), new Vector3(0.5f, 1f, 0.9f), madera);

        // Exhibidor de golosinas sobre el mostrador
        Transform golosinas = Vacio("Golosinas", g, new Vector3(-0.65f, 1.05f, 0.1f)).transform;
        Caja(golosinas, "Rack", new Vector3(0f, 0.15f, 0f), new Vector3(0.5f, 0.3f, 0.3f), metal, false);
        for (int i = 0; i < 6; i++)
            Caja(golosinas, "Golosina", new Vector3(-0.18f + (i % 3) * 0.18f, 0.33f + (i / 3) * 0.1f, -0.05f + (i / 3) * 0.08f),
                 new Vector3(0.14f, 0.08f, 0.06f), productos[i % productos.Length], false);

        // Revistero al costado del mostrador (lado del jugador)
        Transform revistero = Vacio("Revistero", g, new Vector3(-1.6f, 0f, -0.6f)).transform;
        Caja(revistero, "Mueble", new Vector3(0f, 0.6f, 0f), new Vector3(0.6f, 1.2f, 0.5f), metal);
        for (int i = 0; i < 3; i++)
            Caja(revistero, "Revista", new Vector3(0f, 0.4f + i * 0.3f, 0.26f), new Vector3(0.45f, 0.25f, 0.02f), productos[(i + 2) % productos.Length], false);
    }

    // Estante de pared detras del mostrador, a ambos lados de la puerta del almacen.
    static void CrearEstanteriaFondo(Transform padre)
    {
        Transform g = Vacio("EstanteFondo", padre, Vector3.zero).transform;
        Estanteria(g, "Izq", new Vector3(-4f, 0f, Z_FONDO + 0.3f), 0f, 5f, 4, 23);
        Estanteria(g, "Der", new Vector3(4f, 0f, Z_FONDO + 0.3f), 0f, 5f, 4, 29);
    }

    // ---------------------------------------------------------------- salon

    static void CrearGondolas(Transform padre)
    {
        Transform g = Vacio("Gondolas", padre, Vector3.zero).transform;

        // Gondolas dobles a lo largo de Z, a los costados del pasillo central
        GondolaDoble(g, "Gondola_Izq", new Vector3(-4.2f, 0f, 5.2f), 5f, 3, true);
        GondolaDoble(g, "Gondola_Der", new Vector3(3.6f, 0f, 5.2f), 5f, 7, false);

        // Estante de pared a la izquierda
        Estanteria(g, "Pared_Izq", new Vector3(-X_LADO + 0.3f, 0f, 5.2f), 90f, 6f, 5, 13);

        // Cartel colgante de las papas, justo sobre el hueco vacio
        Transform cartel = Vacio("Cartel_Papas", g, new Vector3(-3.75f, 2.55f, 6.4f)).transform;
        Caja(cartel, "Placa", Vector3.zero, new Vector3(0.04f, 0.4f, 1.6f), negro, false);
        Caja(cartel, "Cable", new Vector3(0f, 0.4f, 0f), new Vector3(0.01f, 0.4f, 0.01f), metal, false);
        Texto(cartel, "Texto", "POTATO CHIPS", new Vector3(0.03f, 0f, 0f), Vector3.right, 1.4f, new Color(1f, 0.82f, 0.2f));
        Texto(g, "Agotado", "SOLD OUT", new Vector3(-3.7f, 0.95f, 6.4f), Vector3.right, 0.9f, new Color(1f, 0.25f, 0.2f));
    }

    // Gondola doble: dos caras de estantes. En la izquierda, el tramo de las papas queda vacio.
    static void GondolaDoble(Transform padre, string nombre, Vector3 pos, float largo, int semilla, bool conHueco)
    {
        Transform g = Vacio(nombre, padre, pos).transform;
        Caja(g, "Cuerpo", new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 1.7f, largo), metal);
        Caja(g, "Base", new Vector3(0f, 0.06f, 0f), new Vector3(1.0f, 0.12f, largo), zocalo, false);
        Caja(g, "Remate", new Vector3(0f, 1.72f, 0f), new Vector3(0.5f, 0.05f, largo + 0.1f), metal, false);

        foreach (float lado in new[] { -1f, 1f })
        {
            for (int n = 0; n < 4; n++)
            {
                float y = 0.2f + n * 0.42f;
                Caja(g, "Repisa", new Vector3(0.3f * lado, y, 0f), new Vector3(0.35f, 0.03f, largo), metal, false);
                float zHueco = conHueco && lado > 0f ? 1.2f : 999f;   // tramo vacio de las papas
                LlenarRepisa(g, new Vector3(0.3f * lado, y + 0.015f, 0f), largo, zHueco, semilla + n * 7 + (lado > 0 ? 3 : 0));
            }
        }
    }

    // Estante de pared: un lado, de 'largo' metros, girado 'giroY'.
    static void Estanteria(Transform padre, string nombre, Vector3 pos, float giroY, float largo, int repisas, int semilla)
    {
        Transform g = Vacio("Estante_" + nombre, padre, pos).transform;
        g.localRotation = Quaternion.Euler(0f, giroY, 0f);
        Caja(g, "Fondo", new Vector3(0f, 1.1f, -0.2f), new Vector3(largo, 2.2f, 0.05f), metal);
        Caja(g, "Lado1", new Vector3(-largo / 2f, 1.1f, 0f), new Vector3(0.04f, 2.2f, 0.45f), metal, false);
        Caja(g, "Lado2", new Vector3(largo / 2f, 1.1f, 0f), new Vector3(0.04f, 2.2f, 0.45f), metal, false);
        Caja(g, "Bloqueo", new Vector3(0f, 1.1f, 0f), new Vector3(largo, 2.2f, 0.45f), metal).GetComponent<MeshRenderer>().enabled = false;

        for (int n = 0; n < repisas; n++)
        {
            float y = 0.15f + n * (2f / repisas);
            Caja(g, "Repisa", new Vector3(0f, y, 0f), new Vector3(largo, 0.03f, 0.4f), metal, false);
            Transform fila = Vacio("Fila", g, new Vector3(0f, y + 0.015f, 0f)).transform;
            fila.localRotation = Quaternion.Euler(0f, -90f, 0f);
            LlenarRepisa(fila, Vector3.zero, largo, 999f, semilla + n * 5);
        }
    }

    // Productos en fila a lo largo de Z local. Sin colliders.
    static void LlenarRepisa(Transform padre, Vector3 origen, float largo, float zHueco, int semilla)
    {
        Random.InitState(semilla);
        float z = -largo / 2f + 0.12f;
        while (z < largo / 2f - 0.12f)
        {
            float ancho = Random.Range(0.12f, 0.24f);
            if (Mathf.Abs(z - zHueco) > 0.8f)
            {
                float alto = Random.Range(0.14f, 0.34f);
                Material m = productos[Random.Range(0, productos.Length)];
                bool botella = Random.value < 0.2f;
                GameObject p = botella
                    ? Primitiva(PrimitiveType.Cylinder, padre, "Botella", origen + new Vector3(0f, alto / 2f, z), new Vector3(0.09f, alto / 2f, 0.09f), m)
                    : Caja(padre, "Producto", origen + new Vector3(0f, alto / 2f, z), new Vector3(0.18f, alto, ancho * 0.9f), m, false);
                p.transform.localRotation = Quaternion.Euler(0f, Random.Range(-6f, 6f), 0f);
            }
            z += ancho;
        }
    }

    // Tres heladeras contra la pared derecha, con vidrio, luz y botellas.
    static void CrearHeladeras(Transform padre)
    {
        Transform g = Vacio("Heladeras", padre, Vector3.zero).transform;
        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = new Vector3(X_LADO - 0.45f, 0f, 3.2f + i * 1.7f);
            Transform h = Vacio("Heladera_" + i, g, pos).transform;
            Caja(h, "Cuerpo", new Vector3(0.1f, 1.1f, 0f), new Vector3(0.7f, 2.2f, 1.6f), negro);
            Caja(h, "Interior", new Vector3(-0.26f, 1.15f, 0f), new Vector3(0.02f, 1.8f, 1.4f), tubo, false);
            Caja(h, "Vidrio", new Vector3(-0.27f, 1.15f, 0f), new Vector3(0.02f, 1.9f, 1.5f), vidrio, false);
            Caja(h, "Tirador", new Vector3(-0.3f, 1.15f, 0.6f), new Vector3(0.04f, 0.6f, 0.04f), metal, false);

            for (int n = 0; n < 4; n++)
            {
                float y = 0.35f + n * 0.45f;
                Caja(h, "Repisa", new Vector3(-0.05f, y, 0f), new Vector3(0.45f, 0.02f, 1.4f), metal, false);
                Random.InitState(i * 31 + n);
                Material m = productos[Random.Range(0, productos.Length)];
                for (float z = -0.6f; z <= 0.6f; z += 0.15f)
                    Primitiva(PrimitiveType.Cylinder, h, "Botella", new Vector3(-0.1f, y + 0.14f, z), new Vector3(0.08f, 0.13f, 0.08f), m);
            }

            GameObject luz = Vacio("Luz", h, new Vector3(-0.5f, 1.4f, 0f));
            Light l = luz.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.8f, 0.92f, 1f);
            l.intensity = 0.6f;
            l.range = 2f;
            l.shadows = LightShadows.None;
        }
    }

    // ---------------------------------------------------------------- detalles

    static void CrearDetalles(Transform padre)
    {
        Transform g = Vacio("Detalles", padre, Vector3.zero).transform;

        // Letrero principal: lo ve el jugador desde el mostrador
        Transform letrero = Vacio("Letrero_PotatoMart", g, new Vector3(0f, 2.75f, Z_FRENTE - 0.15f)).transform;
        Caja(letrero, "Placa", Vector3.zero, new Vector3(3.6f, 0.55f, 0.06f), negro, false);
        Texto(letrero, "Texto", "POTATO MART", new Vector3(0f, 0f, -0.04f), Vector3.back, 3.2f, new Color(1f, 0.82f, 0.2f));

        // Carteles en las paredes
        Cartel(g, "2 x 1 SODA", new Vector3(-X_LADO + 0.12f, 2.45f, 1.2f), Vector3.right, productos[0]);
        Cartel(g, "NO LOITERING", new Vector3(X_LADO - 0.12f, 2.45f, 1.2f), Vector3.left, productos[2]);
        Cartel(g, "EMPLOYEES ONLY", new Vector3(-1.8f, 2.45f, Z_FONDO + 0.12f), Vector3.forward, negro);

        // Reloj sobre la puerta del almacen (siempre la misma hora)
        Transform reloj = Vacio("Reloj", g, new Vector3(0f, 2.75f, Z_FONDO + 0.13f)).transform;
        Primitiva(PrimitiveType.Cylinder, reloj, "Cara", Vector3.zero, new Vector3(0.45f, 0.02f, 0.45f), tubo).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Caja(reloj, "Horas", new Vector3(0.05f, 0.04f, 0.03f), new Vector3(0.03f, 0.13f, 0.01f), negro, false).transform.localRotation = Quaternion.Euler(0f, 0f, -50f);
        Caja(reloj, "Minutos", new Vector3(-0.03f, 0.07f, 0.035f), new Vector3(0.02f, 0.19f, 0.01f), negro, false).transform.localRotation = Quaternion.Euler(0f, 0f, 20f);

        // Camara de seguridad en la esquina
        Transform camara = Vacio("CamaraSeguridad", g, new Vector3(X_LADO - 0.4f, ALTO - 0.15f, Z_FRENTE - 0.4f)).transform;
        Caja(camara, "Soporte", Vector3.zero, new Vector3(0.06f, 0.2f, 0.06f), metal, false);
        Caja(camara, "Cuerpo", new Vector3(-0.1f, -0.12f, -0.1f), new Vector3(0.15f, 0.12f, 0.3f), negro, false).transform.localRotation = Quaternion.Euler(20f, 45f, 0f);

        // Tacho de basura y cajas apiladas junto a la puerta del almacen
        Primitiva(PrimitiveType.Cylinder, g, "Tacho", new Vector3(-2.2f, 0.4f, 8.6f), new Vector3(0.45f, 0.4f, 0.45f), metal);
        Caja(g, "CajaCarton1", new Vector3(2f, 0.3f, Z_FONDO + 0.5f), new Vector3(0.7f, 0.6f, 0.6f), carton);
        Caja(g, "CajaCarton2", new Vector3(2.1f, 0.85f, Z_FONDO + 0.45f), new Vector3(0.6f, 0.5f, 0.5f), carton);
    }

    // 'haciaSala' apunta desde la pared hacia el interior de la tienda.
    static void Cartel(Transform padre, string texto, Vector3 pos, Vector3 haciaSala, Material fondo)
    {
        Transform c = Vacio("Cartel", padre, pos).transform;
        c.localRotation = Quaternion.LookRotation(-haciaSala);
        Caja(c, "Fondo", Vector3.zero, new Vector3(1.4f, 0.6f, 0.02f), fondo, false);
        Texto(c, "Texto", texto, new Vector3(0f, 0f, -0.02f), Vector3.back, 1.2f, Color.white);
    }

    // Tubos fluorescentes con luz; el ultimo parpadea.
    static void CrearLuces(Transform padre)
    {
        Transform g = Vacio("Luces", padre, Vector3.zero).transform;
        float[] xs = { -3f, 3f };
        float[] zs = { -1.5f, 3f, 7.5f };
        int i = 0;
        foreach (float z in zs)
            foreach (float x in xs)
            {
                Transform t = Vacio("Fluorescente_" + i, g, new Vector3(x, ALTO - 0.04f, z)).transform;
                Caja(t, "Carcasa", Vector3.zero, new Vector3(1.3f, 0.06f, 0.3f), metal, false);
                Caja(t, "Tubo", new Vector3(0f, -0.035f, 0f), new Vector3(1.2f, 0.02f, 0.2f), tubo, false);

                GameObject luz = Vacio("Luz", t, new Vector3(0f, -0.3f, 0f));
                Light l = luz.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(0.92f, 1f, 0.94f);
                l.intensity = 1.6f;
                l.range = 7.5f;
                l.shadows = i == 2 ? LightShadows.Soft : LightShadows.None;
                if (i == 5) luz.AddComponent<LightFlicker>();
                i++;
            }
    }

    // Vereda, calle oscura y un farol: lo que se ve por la vitrina.
    static void CrearCalle(Transform padre)
    {
        Transform g = Vacio("Calle", padre, Vector3.zero).transform;
        Caja(g, "Vereda", new Vector3(0f, -0.05f, 12.5f), new Vector3(30f, 0.1f, 6f), zocalo);
        Caja(g, "Asfalto", new Vector3(0f, -0.12f, 25f), new Vector3(60f, 0.1f, 20f), calle);
        Caja(g, "Cordon", new Vector3(0f, 0.05f, 15.5f), new Vector3(30f, 0.15f, 0.25f), metal, false);

        Transform farol = Vacio("Farol", g, new Vector3(5f, 0f, 14.8f)).transform;
        Caja(farol, "Poste", new Vector3(0f, 2.2f, 0f), new Vector3(0.12f, 4.4f, 0.12f), negro);
        Caja(farol, "Brazo", new Vector3(0f, 4.4f, -0.5f), new Vector3(0.1f, 0.1f, 1f), negro, false);
        GameObject luz = Vacio("Luz", farol, new Vector3(0f, 4.2f, -0.9f));
        Light l = luz.AddComponent<Light>();
        l.type = LightType.Spot;
        l.spotAngle = 80f;
        l.color = new Color(1f, 0.75f, 0.45f);
        l.intensity = 6f;
        l.range = 9f;
        luz.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // ---------------------------------------------------------------- materiales

    static void CrearMateriales()
    {
        piso1 = Mat("Tienda_Piso1", new Color(0.82f, 0.8f, 0.72f));
        piso2 = Mat("Tienda_Piso2", new Color(0.55f, 0.62f, 0.56f));
        pared = Mat("Tienda_Pared", new Color(0.78f, 0.82f, 0.7f));
        zocalo = Mat("Tienda_Zocalo", new Color(0.18f, 0.32f, 0.24f));
        techo = Mat("Tienda_Techo", new Color(0.88f, 0.88f, 0.86f));
        metal = Mat("Tienda_Metal", new Color(0.62f, 0.64f, 0.66f), smoothness: 0.6f);
        madera = Mat("Tienda_Madera", new Color(0.45f, 0.29f, 0.16f));
        negro = Mat("Tienda_Negro", new Color(0.08f, 0.08f, 0.09f));
        calle = Mat("Tienda_Calle", new Color(0.05f, 0.05f, 0.06f));
        carton = Mat("Tienda_Carton", new Color(0.6f, 0.45f, 0.26f));
        tubo = Mat("Tienda_Tubo", new Color(0.95f, 1f, 0.95f), emision: 1.6f);
        vidrio = Mat("Tienda_Vidrio", new Color(0.7f, 0.85f, 0.9f, 0.18f), smoothness: 0.95f, transparente: true);

        productos = new[]
        {
            Mat("Tienda_ProdRojo", new Color(0.85f, 0.15f, 0.15f)),
            Mat("Tienda_ProdAmarillo", new Color(0.95f, 0.8f, 0.15f)),
            Mat("Tienda_ProdAzul", new Color(0.15f, 0.35f, 0.85f)),
            Mat("Tienda_ProdVerde", new Color(0.2f, 0.65f, 0.3f)),
            Mat("Tienda_ProdNaranja", new Color(0.95f, 0.5f, 0.1f)),
            Mat("Tienda_ProdBlanco", new Color(0.92f, 0.92f, 0.9f)),
            Mat("Tienda_ProdMorado", new Color(0.5f, 0.2f, 0.6f)),
        };
    }

    static Material Mat(string nombre, Color color, float smoothness = 0.25f, float emision = 0f, bool transparente = false)
    {
        if (!AssetDatabase.IsValidFolder(CARPETA_MATS)) AssetDatabase.CreateFolder("Assets", "03_Materials");
        string ruta = CARPETA_MATS + "/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        bool nuevo = m == null;
        if (nuevo) m = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", smoothness);
        if (emision > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * emision);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }
        if (transparente)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetOverrideTag("RenderType", "Transparent");
        }

        if (nuevo) AssetDatabase.CreateAsset(m, ruta);
        else EditorUtility.SetDirty(m);
        return m;
    }

    // ---------------------------------------------------------------- ayudas

    // 'haciaDonde' es la direccion (local al padre) desde la que se lee el texto.
    static void Texto(Transform padre, string nombre, string texto, Vector3 pos, Vector3 haciaDonde, float ancho, Color color)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.LookRotation(-haciaDonde);

        TextMeshPro t = go.AddComponent<TextMeshPro>();
        t.text = texto;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.enableAutoSizing = true;
        t.fontSizeMin = 0.5f;
        t.fontSizeMax = 6f;
        t.fontStyle = FontStyles.Bold;
        t.rectTransform.sizeDelta = new Vector2(ancho, ancho * 0.22f);
    }

    static GameObject Vacio(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        return go;
    }

    static GameObject Caja(Transform padre, string nombre, Vector3 pos, Vector3 escala, Material mat, bool conCollider = true)
        => Primitiva(PrimitiveType.Cube, padre, nombre, pos, escala, mat, conCollider);

    static GameObject Primitiva(PrimitiveType tipo, Transform padre, string nombre, Vector3 pos, Vector3 escala, Material mat, bool conCollider = false)
    {
        GameObject c = GameObject.CreatePrimitive(tipo);
        c.name = nombre;
        c.transform.SetParent(padre, false);
        c.transform.localPosition = pos;
        c.transform.localScale = escala;
        c.GetComponent<MeshRenderer>().sharedMaterial = mat;
        c.GetComponent<MeshRenderer>().shadowCastingMode = escala.magnitude < 0.6f ? ShadowCastingMode.Off : ShadowCastingMode.On;
        if (!conCollider) Object.DestroyImmediate(c.GetComponent<Collider>());
        return c;
    }
}
