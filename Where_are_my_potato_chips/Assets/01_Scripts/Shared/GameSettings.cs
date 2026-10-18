using UnityEngine;

/// <summary>Opciones del jugador guardadas entre partidas (PlayerPrefs).</summary>
public static class GameSettings
{
    const string SensitivityKey = "MouseSensitivity";
    const string InvertYKey = "InvertY";
    const string FovKey = "FieldOfView";
    const string VolumeKey = "MasterVolume";
    const string CameraEffectsKey = "CameraEffects";

    public static float Sensitivity
    {
        get => PlayerPrefs.GetFloat(SensitivityKey, 0.5f);
        set => PlayerPrefs.SetFloat(SensitivityKey, value);
    }

    public static bool InvertY
    {
        get => PlayerPrefs.GetInt(InvertYKey, 0) == 1;
        set => PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0);
    }

    public static float FieldOfView
    {
        get => PlayerPrefs.GetFloat(FovKey, 70f);
        set => PlayerPrefs.SetFloat(FovKey, value);
    }

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(VolumeKey, value);
            AudioListener.volume = value;
        }
    }

    /// <summary>Balanceo y sacudidas de cámara. Se puede apagar si marea.</summary>
    public static bool CameraEffects
    {
        get => PlayerPrefs.GetInt(CameraEffectsKey, 1) == 1;
        set => PlayerPrefs.SetInt(CameraEffectsKey, value ? 1 : 0);
    }

    public static void ApplyTo(PlayerController player)
    {
        if (player == null) return;
        player.Sensibility = Sensitivity;
        player.invertY = InvertY;

        var cam = player.GetComponentInChildren<Camera>();
        if (cam != null) cam.fieldOfView = FieldOfView;
    }

    public static void ApplyAudio() => AudioListener.volume = Volume;
}
