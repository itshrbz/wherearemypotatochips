// =====================================================================
//  AmbienteTools.cs   (Dev 4 - Entornos)   tarjeta A-03 (iluminacion y niebla)
//  DONDE PONERLO:  Assets/Editor/AmbienteTools.cs
//  COMO USARLO:    abre una escena (02_Store, 10_Warehouse, 11_Pools o 12_Playground)
//                  y elige  Tools > Kit > Ambiente > Aplicar a la escena abierta
//
//  QUE HACE EN LA ESCENA ABIERTA (no borra nada de lo tuyo):
//   - Niebla y luz ambiental segun el espacio (y el cielo, si el preset trae uno)
//   - Intensidad del "sol" (Directional Light), si el preset lo pide
//   - Un Global Volume "Volumen_Ambiente" con el COLOR del espacio:
//     balance de blancos, filtro de color, exposicion y split toning.
//  El grano, la vineta y la saturacion baja NO van aqui: son del look global
//  (Assets/Settings/Look_Global.asset, tarea A2), que el jugador puede bajar en opciones.
//  Los valores estan en la ZONA 1: cambialos y vuelve a ejecutar el menu.
//  El perfil se guarda en Assets/Settings/Ambiente_<escena>.asset y se actualiza
//  en su lugar (no se borra), para no romper referencias.
// =====================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class AmbienteTools
{
    // ================== ZONA 1: PRESETS (EDITA AQUI) ==================
    class Preset
    {
        public Color niebla;          // color de la niebla
        public float densidad;        // 0 = sin niebla, 0.01 = leve, 0.05 = espesa
        public Color ambiente;        // luz ambiental (muy baja = oscuro)
        public float sol = -1f;       // intensidad del Directional Light (-1 = no tocarlo)
        public float temperatura;     // -100 frio (azul) a 100 calido (naranja)
        public float tinte;           // -100 verde a 100 magenta
        public Color filtro = Color.white;   // multiplica toda la imagen: el color que domina
        public float exposicion;      // en pasos: -1 = la mitad de luz
        public Color sombras = Gray;  // split toning: tono de las sombras (gris = neutro)
        public Color luces = Gray;    // split toning: tono de las luces altas
        public string cielo;          // ruta de un material de skybox (null = no tocarlo)
    }

    static readonly Color Gray = new Color(0.5f, 0.5f, 0.5f);
    // Espacio = 1, Volume local de la replica = 2, look global (RetroLook) = 50
    const float PrioridadEspacio = 1f;
    public const string CieloParque = "Assets/Settings/Cielo_Parque.mat";
    public const string PerfilReplica = "Assets/Settings/Ambiente_Replica.asset";

    // Tienda: calida, naranja de foco. Es el lugar seguro.
    static readonly Preset TIENDA = new Preset
    {
        densidad = 0f, ambiente = new Color(0.24f, 0.20f, 0.16f),
        temperatura = 18f, tinte = 2f, filtro = new Color(1f, 0.93f, 0.82f), exposicion = 0.05f,
        sombras = new Color(0.50f, 0.47f, 0.45f), luces = new Color(0.62f, 0.53f, 0.42f),
    };

    // Replica: casi igual a la tienda, un poco mas fria. Se aplica con un Volume local en el prefab.
    static readonly Preset REPLICA = new Preset
    {
        temperatura = 2f, tinte = -4f, filtro = new Color(0.95f, 0.95f, 0.93f), exposicion = -0.1f,
        sombras = new Color(0.46f, 0.49f, 0.52f), luces = new Color(0.55f, 0.52f, 0.48f),
    };

    // Almacen (backrooms): amarillo enfermo y parejo. El generador deja prendida 1 de cada 3 luces,
    // asi que la luz pareja sale del ambiente (gratis) y las fluorescentes solo suman manchas y parpadeos.
    static readonly Preset ALMACEN = new Preset
    {
        niebla = new Color(0.44f, 0.41f, 0.24f), densidad = 0.032f, ambiente = new Color(0.40f, 0.36f, 0.19f), sol = 0f,
        temperatura = 28f, tinte = -10f, filtro = new Color(1f, 0.94f, 0.72f), exposicion = 0.1f,
        sombras = new Color(0.45f, 0.52f, 0.40f), luces = new Color(0.64f, 0.58f, 0.36f),
    };

    // Piscinas: cian frio, azulejo blanco
    static readonly Preset PISCINAS = new Preset
    {
        niebla = new Color(0.52f, 0.72f, 0.76f), densidad = 0.018f, ambiente = new Color(0.33f, 0.46f, 0.50f), sol = 0f,
        temperatura = -14f, tinte = -2f, filtro = new Color(0.9f, 1f, 0.98f), exposicion = 0.05f,
        sombras = new Color(0.40f, 0.50f, 0.56f), luces = new Color(0.50f, 0.56f, 0.56f),
    };

    // Parque: atardecer apagado, violeta y gris
    static readonly Preset PARQUE = new Preset
    {
        niebla = new Color(0.24f, 0.21f, 0.28f), densidad = 0.028f, ambiente = new Color(0.18f, 0.15f, 0.21f),
        temperatura = 8f, tinte = 10f, filtro = new Color(0.95f, 0.88f, 1f), exposicion = -0.15f,
        sombras = new Color(0.47f, 0.42f, 0.60f), luces = new Color(0.62f, 0.52f, 0.42f),
        cielo = CieloParque,
    };

    // ================== ZONA 2: MENU ==================================
    [MenuItem("Tools/Kit/Ambiente/Aplicar a la escena abierta (segun su nombre)")]
    static void Auto()
    {
        string n = SceneManager.GetActiveScene().name.ToLower();
        if (n.Contains("store"))           Aplicar(TIENDA);
        else if (n.Contains("warehouse"))  Aplicar(ALMACEN);
        else if (n.Contains("pool"))       Aplicar(PISCINAS);
        else if (n.Contains("playground")) Aplicar(PARQUE);
        else EditorUtility.DisplayDialog("Escena no reconocida",
            "Abre 02_Store, 10_Warehouse, 11_Pools o 12_Playground, o usa una de las opciones de abajo.", "Ok");
    }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Tienda")]
    static void ForzarTienda() { Aplicar(TIENDA); }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Almacen")]
    static void ForzarAlmacen() { Aplicar(ALMACEN); }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Piscinas")]
    static void ForzarPiscinas() { Aplicar(PISCINAS); }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Parque")]
    static void ForzarParque() { Aplicar(PARQUE); }

    // El prefab ReplicaStore ya tiene un Volume local que usa este perfil; esto solo actualiza sus valores.
    [MenuItem("Tools/Kit/Ambiente/Actualizar perfil de la tienda replica")]
    public static void ActualizarReplica()
    {
        GuardarPerfil(PerfilReplica, REPLICA);
        Debug.Log("Perfil de la replica actualizado: " + PerfilReplica);
    }

    // ================== ZONA 3: APLICAR UN PRESET =====================
    static void Aplicar(Preset p)
    {
        Scene escena = SceneManager.GetActiveScene();

        // Niebla y luz ambiental
        RenderSettings.fog = p.densidad > 0f;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = p.niebla;
        RenderSettings.fogDensity = p.densidad;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = p.ambiente;

        if (!string.IsNullOrEmpty(p.cielo))
        {
            Material cielo = AssetDatabase.LoadAssetAtPath<Material>(p.cielo);
            if (cielo != null) RenderSettings.skybox = cielo;
            else Debug.LogWarning("AmbienteTools: no existe el cielo " + p.cielo);
        }

        // "Sol": solo se toca el Directional Light
        if (p.sol >= 0f)
            foreach (Light l in Object.FindObjectsByType<Light>())
                if (l.type == LightType.Directional) l.intensity = p.sol;

        VolumeProfile perfil = GuardarPerfil("Assets/Settings/Ambiente_" + escena.name + ".asset", p);
        CrearVolumen(perfil);

        EditorSceneManager.MarkSceneDirty(escena);
        Debug.Log("Ambiente aplicado a " + escena.name + ". Guarda la escena (Ctrl + S).");
    }

    // Crea o actualiza el perfil de color de un espacio.
    static VolumeProfile GuardarPerfil(string ruta, Preset p)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");

        VolumeProfile perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ruta);
        if (perfil == null)
        {
            perfil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(perfil, ruta);
        }

        // Ahora son del look global; si quedaran aqui se sumarian y no se podrian apagar en opciones
        Quitar<Vignette>(perfil);
        Quitar<FilmGrain>(perfil);

        ColorAdjustments c = Obtener<ColorAdjustments>(perfil);
        c.saturation.overrideState = false;
        c.postExposure.Override(p.exposicion);
        c.colorFilter.Override(p.filtro);

        WhiteBalance wb = Obtener<WhiteBalance>(perfil);
        wb.temperature.Override(p.temperatura);
        wb.tint.Override(p.tinte);

        SplitToning st = Obtener<SplitToning>(perfil);
        st.shadows.Override(p.sombras);
        st.highlights.Override(p.luces);
        st.balance.Override(0f);

        EditorUtility.SetDirty(perfil);
        AssetDatabase.SaveAssets();
        return perfil;
    }

    static T Obtener<T>(VolumeProfile perfil) where T : VolumeComponent
    {
        if (perfil.TryGet(out T componente)) return componente;
        componente = perfil.Add<T>(false);
        componente.name = typeof(T).Name;
        AssetDatabase.AddObjectToAsset(componente, perfil);
        return componente;
    }

    static void Quitar<T>(VolumeProfile perfil) where T : VolumeComponent
    {
        if (!perfil.TryGet(out T componente)) return;
        perfil.Remove<T>();
        AssetDatabase.RemoveObjectFromAsset(componente);
        Object.DestroyImmediate(componente, true);
    }

    // Global Volume de la escena que usa el perfil del espacio.
    static void CrearVolumen(VolumeProfile perfil)
    {
        GameObject go = GameObject.Find("Volumen_Ambiente");
        if (go == null)
        {
            go = new GameObject("Volumen_Ambiente");
            Undo.RegisterCreatedObjectUndo(go, "Volumen_Ambiente");
        }
        Volume vol = go.GetComponent<Volume>();
        if (vol == null) vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = PrioridadEspacio;
        vol.sharedProfile = perfil;
    }
}
