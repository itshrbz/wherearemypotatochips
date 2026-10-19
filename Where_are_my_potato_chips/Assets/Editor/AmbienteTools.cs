// =====================================================================
//  AmbienteTools.cs   (Dev 4 - Entornos)   tarjeta A-03 (iluminacion y niebla)
//  DONDE PONERLO:  Assets/Editor/AmbienteTools.cs
//  COMO USARLO:    abre una escena (10_Warehouse, 11_Pools o 12_Playground)
//                  y elige  Tools > Kit > Ambiente > Aplicar a la escena abierta
//
//  QUE HACE EN LA ESCENA ABIERTA (no borra nada de lo tuyo):
//   - Enciende la niebla (color y densidad segun el espacio)
//   - Oscurece la luz ambiental y baja el "sol" (Directional Light)
//   - Crea un Global Volume con vineta, grano y poca saturacion
//  Los valores estan en la ZONA 1: cambialos y vuelve a ejecutar el menu.
//  El perfil del Volume se guarda en Assets/Settings/Ambiente_<escena>.asset
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
        public Color niebla;        // color de la niebla
        public float densidad;      // 0.01 = leve, 0.05 = espesa
        public Color ambiente;      // luz ambiental (muy baja = oscuro)
        public float sol;           // intensidad del Directional Light (0 = apagado)
        public float vineta;        // 0 a 1
        public float grano;         // 0 a 1
        public float saturacion;    // -100 a 100 (negativo = mas gris)
    }

    static readonly Preset ALMACEN   = new Preset { niebla = new Color(0.18f, 0.20f, 0.16f), densidad = 0.045f, ambiente = new Color(0.05f, 0.055f, 0.05f), sol = 0f,    vineta = 0.40f, grano = 0.45f, saturacion = -25f };
    static readonly Preset PISCINAS  = new Preset { niebla = new Color(0.50f, 0.66f, 0.68f), densidad = 0.020f, ambiente = new Color(0.30f, 0.38f, 0.40f), sol = 0f,    vineta = 0.25f, grano = 0.35f, saturacion = -15f };
    static readonly Preset PARQUE    = new Preset { niebla = new Color(0.08f, 0.08f, 0.12f), densidad = 0.032f, ambiente = new Color(0.035f, 0.035f, 0.06f), sol = 0.06f, vineta = 0.45f, grano = 0.40f, saturacion = -20f };

    // ================== ZONA 2: MENU ==================================
    [MenuItem("Tools/Kit/Ambiente/Aplicar a la escena abierta (segun su nombre)")]
    static void Auto()
    {
        string n = SceneManager.GetActiveScene().name.ToLower();
        if (n.Contains("warehouse"))       Aplicar(ALMACEN);
        else if (n.Contains("pool"))       Aplicar(PISCINAS);
        else if (n.Contains("playground")) Aplicar(PARQUE);
        else EditorUtility.DisplayDialog("Escena no reconocida",
            "Abre 10_Warehouse, 11_Pools o 12_Playground, o usa una de las opciones de abajo.", "Ok");
    }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Almacen")]
    static void ForzarAlmacen() { Aplicar(ALMACEN); }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Piscinas")]
    static void ForzarPiscinas() { Aplicar(PISCINAS); }

    [MenuItem("Tools/Kit/Ambiente/Forzar preset Parque")]
    static void ForzarParque() { Aplicar(PARQUE); }

    // ================== ZONA 3: APLICAR UN PRESET =====================
    static void Aplicar(Preset p)
    {
        Scene escena = SceneManager.GetActiveScene();

        // Niebla y luz ambiental
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = p.niebla;
        RenderSettings.fogDensity = p.densidad;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = p.ambiente;

        // "Sol": solo se toca el Directional Light
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) l.intensity = p.sol;

        CrearVolumen(escena, p);

        EditorSceneManager.MarkSceneDirty(escena);
        Debug.Log("Ambiente aplicado a " + escena.name + ". Guarda la escena (Ctrl + S).");
    }

    // Global Volume con perfil nuevo (se reemplaza el anterior de esta escena).
    static void CrearVolumen(Scene escena, Preset p)
    {
        string ruta = "Assets/Settings/Ambiente_" + escena.name + ".asset";
        if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
        AssetDatabase.DeleteAsset(ruta);

        VolumeProfile perfil = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(perfil, ruta);

        Vignette v = perfil.Add<Vignette>(true);
        v.intensity.Override(p.vineta);
        v.smoothness.Override(0.5f);
        AssetDatabase.AddObjectToAsset(v, perfil);

        FilmGrain g = perfil.Add<FilmGrain>(true);
        g.type.Override(FilmGrainLookup.Thin1);
        g.intensity.Override(p.grano);
        AssetDatabase.AddObjectToAsset(g, perfil);

        ColorAdjustments c = perfil.Add<ColorAdjustments>(true);
        c.saturation.Override(p.saturacion);
        c.postExposure.Override(-0.3f);
        AssetDatabase.AddObjectToAsset(c, perfil);

        EditorUtility.SetDirty(perfil);
        AssetDatabase.SaveAssets();

        GameObject go = GameObject.Find("Volumen_Ambiente");
        if (go == null)
        {
            go = new GameObject("Volumen_Ambiente");
            Undo.RegisterCreatedObjectUndo(go, "Volumen_Ambiente");
        }
        Volume vol = go.GetComponent<Volume>();
        if (vol == null) vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.sharedProfile = perfil;
    }
}
