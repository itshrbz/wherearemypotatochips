using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Look de cámara de video vieja para todo el juego. Crea un Volume global persistente con el perfil
/// de <see cref="GameFlowSettings.retroLookProfile"/> y ajusta su peso según <see cref="GameSettings.RetroLook"/>.
/// También enciende el post-proceso en la cámara activa, porque el prefab del jugador lo trae apagado.
/// </summary>
public class RetroLook : MonoBehaviour
{
    // Por encima de los perfiles de cada espacio (prioridad 0-2) para que el look gane donde coinciden
    const float Priority = 50f;

    static RetroLook instance;

    Volume volume;
    Camera[] cameras = new Camera[4];
    float nextCameraCheck;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateOnPlay()
    {
        if (instance != null) return;
        var settings = GameFlowSettings.Load();
        if (settings.retroLookProfile == null) return;

        var go = new GameObject("RetroLook");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<RetroLook>();
        instance.Setup(settings);
    }

    /// <summary>Vuelve a leer la opción del jugador. Lo llama <see cref="GameSettings.RetroLook"/> al cambiarla.</summary>
    public static void Refresh()
    {
        if (instance != null) instance.volume.weight = GameSettings.RetroLook;
    }

    void Setup(GameFlowSettings settings)
    {
        volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = Priority;

        bool lighter = settings.lighterLookOnWeb && Application.platform == RuntimePlatform.WebGLPlayer;
        if (lighter)
        {
            // Copia del perfil para no tocar el asset
            volume.profile = Instantiate(settings.retroLookProfile);
            if (volume.profile.TryGet(out ChromaticAberration aberration)) aberration.active = false;
        }
        else
        {
            volume.sharedProfile = settings.retroLookProfile;
        }

        Refresh();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void LateUpdate()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Atajo de prueba hasta que exista el control en opciones (E2): F4 recorre 1 -> 0.5 -> 0
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.f4Key.wasPressedThisFrame)
        {
            float current = GameSettings.RetroLook;
            GameSettings.RetroLook = current > 0.75f ? 0.5f : current > 0.25f ? 0f : 1f;
            Debug.Log("RetroLook = " + GameSettings.RetroLook);
        }
#endif

        // La cámara del jugador no tiene la etiqueta MainCamera, así que se revisan las cámaras activas
        // cada medio segundo en vez de usar Camera.main
        if (Time.unscaledTime < nextCameraCheck) return;
        nextCameraCheck = Time.unscaledTime + 0.5f;

        if (Camera.allCamerasCount > cameras.Length) cameras = new Camera[Camera.allCamerasCount];
        int count = Camera.GetAllCameras(cameras);
        for (int i = 0; i < count; i++)
        {
            if (cameras[i].TryGetComponent(out UniversalAdditionalCameraData data)
                && data.renderType == CameraRenderType.Base)
                data.renderPostProcessing = true;
        }
    }
}
