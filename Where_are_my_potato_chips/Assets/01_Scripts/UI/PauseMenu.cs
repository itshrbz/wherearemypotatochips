using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Pausa con Esc (o Start del mando). Tiene tres pantallas: la principal, Configuración y Controles.
/// Las opciones se guardan en GameSettings. LevelFlowManager agrega este componente en cada escena jugable.
/// </summary>
public class PauseMenu : MonoBehaviour
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
        "It sees you when you are in front of it, and it hears you run.</color>";

    GameObject canvas;
    GameObject mainPanel;
    GameObject settingsPanel;
    GameObject controlsPanel;

    Slider sensitivitySlider;
    Slider fovSlider;
    Slider volumeSlider;
    TextMeshProUGUI invertYLabel;
    TextMeshProUGUI fullscreenLabel;

    InputAction pauseAction;
    PlayerController player;
    PlayerState playerState;
    bool paused;

    public static bool IsPaused { get; private set; }

    void Awake()
    {
        pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        BuildUi();
        canvas.SetActive(false);
    }

    void OnEnable() => pauseAction.Enable();
    void OnDisable() => pauseAction.Disable();

    void OnDestroy()
    {
        pauseAction.Dispose();
        if (paused) Time.timeScale = 1f;
        IsPaused = false;
    }

    void Update()
    {
        if (player == null && !FindPlayer()) return;

        if (pauseAction.WasPressedThisFrame())
        {
            if (!paused) Pause();
            else if (!mainPanel.activeSelf) ShowPanel(mainPanel);
            else Resume();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // En el navegador, Esc suelta el mouse antes de que el juego lo vea
        if (!paused && !playerState.InputLocked && Cursor.lockState != CursorLockMode.Locked)
            Pause();
#endif
    }

    bool FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player == null) return false;

        playerState = player.GetComponent<PlayerState>();
        GameSettings.ApplyTo(player);
        return true;
    }

    void Pause()
    {
        if (playerState == null || playerState.InputLocked || LevelFlowManager.IsLoading) return;

        paused = IsPaused = true;
        Time.timeScale = 0f;
        playerState.InputLocked = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UiBuilder.EnsureEventSystem();
        RefreshSettings();
        canvas.SetActive(true);
        ShowPanel(mainPanel);
    }

    void Resume()
    {
        paused = IsPaused = false;
        Time.timeScale = 1f;
        canvas.SetActive(false);
        PlayerPrefs.Save();

        if (playerState != null) playerState.InputLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void BackToMenu()
    {
        Resume();
        LevelFlowManager.BackToMenu();
    }

    void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        settingsPanel.SetActive(panel == settingsPanel);
        controlsPanel.SetActive(panel == controlsPanel);
    }

    void RefreshSettings()
    {
        sensitivitySlider.SetValueWithoutNotify(GameSettings.Sensitivity);
        fovSlider.SetValueWithoutNotify(GameSettings.FieldOfView);
        volumeSlider.SetValueWithoutNotify(GameSettings.Volume);
        invertYLabel.text = "Invert Y: " + (GameSettings.InvertY ? "On" : "Off");
        fullscreenLabel.text = "Fullscreen: " + (Screen.fullScreen ? "On" : "Off");
    }

    void ChangeSensitivity(float value)
    {
        GameSettings.Sensitivity = value;
        GameSettings.ApplyTo(player);
    }

    void ChangeFov(float value)
    {
        GameSettings.FieldOfView = value;
        GameSettings.ApplyTo(player);
    }

    void ChangeVolume(float value) => GameSettings.Volume = value;

    void ToggleInvertY()
    {
        GameSettings.InvertY = !GameSettings.InvertY;
        GameSettings.ApplyTo(player);
        RefreshSettings();
    }

    void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        // El cambio se aplica al final del frame; se muestra el valor que va a quedar
        fullscreenLabel.text = "Fullscreen: " + (Screen.fullScreen ? "Off" : "On");
    }

    void BuildUi()
    {
        Transform root = UiBuilder.CreateCanvas("PauseCanvas", 900, new Color(0f, 0f, 0f, 0.85f));
        canvas = root.gameObject;
        canvas.transform.SetParent(transform, false);

        mainPanel = BuildMainPanel(root);
        settingsPanel = BuildSettingsPanel(root);
        controlsPanel = BuildControlsPanel(root);
    }

    GameObject BuildMainPanel(Transform root)
    {
        var panel = UiBuilder.CreatePanel(root, "Main");
        UiBuilder.CreateLabel(panel, "PAUSED", 90f, new Vector2(0f, 330f)).color = UiBuilder.Accent;
        UiBuilder.CreateButton(panel, "Resume", new Vector2(0f, 170f), Resume);
        UiBuilder.CreateButton(panel, "Settings", new Vector2(0f, 60f), () => ShowPanel(settingsPanel));
        UiBuilder.CreateButton(panel, "Controls", new Vector2(0f, -50f), () => ShowPanel(controlsPanel));
        UiBuilder.CreateButton(panel, "Main menu", new Vector2(0f, -160f), BackToMenu);
        UiBuilder.CreateButton(panel, "Quit", new Vector2(0f, -270f), UiBuilder.Quit);
        return panel.gameObject;
    }

    GameObject BuildSettingsPanel(Transform root)
    {
        var panel = UiBuilder.CreatePanel(root, "Settings");
        UiBuilder.CreateLabel(panel, "SETTINGS", 80f, new Vector2(0f, 360f)).color = UiBuilder.Accent;

        UiBuilder.CreateOptionLabel(panel, "Mouse sensitivity", 220f);
        sensitivitySlider = UiBuilder.CreateSlider(panel, new Vector2(240f, 220f), 0.05f, 1.5f, GameSettings.Sensitivity, ChangeSensitivity);

        UiBuilder.CreateOptionLabel(panel, "Field of view", 130f);
        fovSlider = UiBuilder.CreateSlider(panel, new Vector2(240f, 130f), 55f, 100f, GameSettings.FieldOfView, ChangeFov);

        UiBuilder.CreateOptionLabel(panel, "Volume", 40f);
        volumeSlider = UiBuilder.CreateSlider(panel, new Vector2(240f, 40f), 0f, 1f, GameSettings.Volume, ChangeVolume);

        var invert = UiBuilder.CreateButton(panel, "Invert Y", new Vector2(0f, -80f), ToggleInvertY);
        invertYLabel = invert.GetComponentInChildren<TextMeshProUGUI>();
        ((RectTransform)invert.transform).sizeDelta = new Vector2(460f, 90f);

        var fullscreen = UiBuilder.CreateButton(panel, "Fullscreen", new Vector2(0f, -190f), ToggleFullscreen);
        fullscreenLabel = fullscreen.GetComponentInChildren<TextMeshProUGUI>();
        ((RectTransform)fullscreen.transform).sizeDelta = new Vector2(460f, 90f);

        UiBuilder.CreateButton(panel, "Back", new Vector2(0f, -330f), () => ShowPanel(mainPanel));
        return panel.gameObject;
    }

    GameObject BuildControlsPanel(Transform root)
    {
        var panel = UiBuilder.CreatePanel(root, "Controls");
        UiBuilder.CreateLabel(panel, "CONTROLS", 80f, new Vector2(0f, 360f)).color = UiBuilder.Accent;

        var text = UiBuilder.CreateLabel(panel, ControlsText, 34f, new Vector2(0f, 20f));
        text.rectTransform.sizeDelta = new Vector2(1300f, 560f);
        text.lineSpacing = 12f;

        UiBuilder.CreateButton(panel, "Back", new Vector2(0f, -330f), () => ShowPanel(mainPanel));
        return panel.gameObject;
    }
}
