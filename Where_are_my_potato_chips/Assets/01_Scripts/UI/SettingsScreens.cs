using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Pantallas de Configuración y Controles. Las usan el menú principal y la pausa.
/// Si hay un jugador en la escena, los cambios se le aplican al momento.
/// </summary>
public class SettingsScreens
{
    const string ControlsText =
        "<b>WASD</b>  Move\n" +
        "<b>Mouse</b>  Look\n" +
        "<b>Shift</b>  Run (uses stamina)\n" +
        "<b>Ctrl / C</b>  Crouch  -  inside a hiding spot, you hide\n" +
        "<b>Space</b>  Jump\n" +
        "<b>E</b>  Interact: scan, charge, open\n" +
        "<b>Esc</b>  Pause\n\n" +
        "<color=#F2C640>Serve customers: scan the product, then charge it at the register.\n" +
        "The replica store at the end of each place is safe.\n" +
        "It sees you when you are in front of it, and it hears you run.\n" +
        "If it sees you hide, it knows where you are.</color>";

    public GameObject Settings { get; }
    public GameObject Controls { get; }

    Slider sensitivitySlider;
    Slider fovSlider;
    Slider volumeSlider;
    TextMeshProUGUI invertYLabel;
    TextMeshProUGUI cameraEffectsLabel;
    TextMeshProUGUI fullscreenLabel;

    public SettingsScreens(Transform root, UnityAction onBack)
    {
        Settings = BuildSettings(root, onBack);
        Controls = BuildControls(root, onBack);
        Settings.SetActive(false);
        Controls.SetActive(false);
    }

    public void Refresh()
    {
        sensitivitySlider.SetValueWithoutNotify(GameSettings.Sensitivity);
        fovSlider.SetValueWithoutNotify(GameSettings.FieldOfView);
        volumeSlider.SetValueWithoutNotify(GameSettings.Volume);
        invertYLabel.text = "Invert Y: " + (GameSettings.InvertY ? "On" : "Off");
        cameraEffectsLabel.text = "Camera effects: " + (GameSettings.CameraEffects ? "On" : "Off");
        fullscreenLabel.text = "Fullscreen: " + (Screen.fullScreen ? "On" : "Off");
    }

    static void ApplyToPlayer() => GameSettings.ApplyTo(Object.FindAnyObjectByType<PlayerController>());

    GameObject BuildSettings(Transform root, UnityAction onBack)
    {
        var panel = UiBuilder.CreatePanel(root, "Settings");
        UiBuilder.CreateLabel(panel, "SETTINGS", 80f, new Vector2(0f, 360f)).color = UiBuilder.Accent;

        UiBuilder.CreateOptionLabel(panel, "Mouse sensitivity", 220f);
        sensitivitySlider = UiBuilder.CreateSlider(panel, new Vector2(240f, 220f), 0.05f, 1.5f, GameSettings.Sensitivity, value =>
        {
            GameSettings.Sensitivity = value;
            ApplyToPlayer();
        });

        UiBuilder.CreateOptionLabel(panel, "Field of view", 130f);
        fovSlider = UiBuilder.CreateSlider(panel, new Vector2(240f, 130f), 55f, 100f, GameSettings.FieldOfView, value =>
        {
            GameSettings.FieldOfView = value;
            ApplyToPlayer();
        });

        UiBuilder.CreateOptionLabel(panel, "Volume", 40f);
        volumeSlider = UiBuilder.CreateSlider(panel, new Vector2(240f, 40f), 0f, 1f, GameSettings.Volume, value => GameSettings.Volume = value);

        invertYLabel = WideButton(panel, "Invert Y", new Vector2(-260f, -80f), () =>
        {
            GameSettings.InvertY = !GameSettings.InvertY;
            ApplyToPlayer();
            Refresh();
        });

        cameraEffectsLabel = WideButton(panel, "Camera effects", new Vector2(260f, -80f), () =>
        {
            GameSettings.CameraEffects = !GameSettings.CameraEffects;
            Refresh();
        });

        fullscreenLabel = WideButton(panel, "Fullscreen", new Vector2(0f, -190f), () =>
        {
            Screen.fullScreen = !Screen.fullScreen;
            // El cambio se aplica al final del frame; se muestra el valor que va a quedar
            fullscreenLabel.text = "Fullscreen: " + (Screen.fullScreen ? "Off" : "On");
        });

        UiBuilder.CreateButton(panel, "Back", new Vector2(0f, -330f), () =>
        {
            PlayerPrefs.Save();
            onBack();
        });
        return panel.gameObject;
    }

    static GameObject BuildControls(Transform root, UnityAction onBack)
    {
        var panel = UiBuilder.CreatePanel(root, "Controls");
        UiBuilder.CreateLabel(panel, "CONTROLS", 80f, new Vector2(0f, 360f)).color = UiBuilder.Accent;

        var text = UiBuilder.CreateLabel(panel, ControlsText, 34f, new Vector2(0f, 20f));
        text.rectTransform.sizeDelta = new Vector2(1300f, 560f);
        text.lineSpacing = 12f;

        UiBuilder.CreateButton(panel, "Back", new Vector2(0f, -330f), onBack);
        return panel.gameObject;
    }

    static TextMeshProUGUI WideButton(Transform panel, string text, Vector2 position, UnityAction onClick)
    {
        var button = UiBuilder.CreateButton(panel, text, position, onClick);
        ((RectTransform)button.transform).sizeDelta = new Vector2(460f, 90f);
        return button.GetComponentInChildren<TextMeshProUGUI>();
    }
}
