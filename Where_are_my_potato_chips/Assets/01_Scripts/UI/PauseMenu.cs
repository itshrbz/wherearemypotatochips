using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pausa con Esc (o Start del mando): continuar, configuración, controles, volver al menú o salir.
/// LevelFlowManager agrega este componente en cada escena jugable.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    GameObject canvas;
    GameObject mainPanel;
    SettingsScreens screens;

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
            else if (!mainPanel.activeSelf) ShowMain();
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
        canvas.SetActive(true);
        ShowMain();
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

    // Vuelve a empezar el espacio actual (por si el jugador queda atascado)
    void Restart()
    {
        Resume();
        LevelFlowManager.RestartLevel(false);
    }

    void BackToMenu()
    {
        Resume();
        LevelFlowManager.BackToMenu();
    }

    void ShowMain()
    {
        mainPanel.SetActive(true);
        screens.Settings.SetActive(false);
        screens.Controls.SetActive(false);
    }

    void Show(GameObject panel)
    {
        mainPanel.SetActive(false);
        screens.Refresh();
        panel.SetActive(true);
    }

    void BuildUi()
    {
        Transform root = UiBuilder.CreateCanvas("PauseCanvas", 900, new Color(0f, 0f, 0f, 0.85f));
        canvas = root.gameObject;
        canvas.transform.SetParent(transform, false);

        var panel = UiBuilder.CreatePanel(root, "Main");
        UiBuilder.CreateLabel(panel, "PAUSED", 90f, new Vector2(0f, 360f)).color = UiBuilder.Accent;
        UiBuilder.CreateButton(panel, "Resume", new Vector2(0f, 220f), Resume);
        UiBuilder.CreateButton(panel, "Restart", new Vector2(0f, 115f), Restart);
        UiBuilder.CreateButton(panel, "Settings", new Vector2(0f, 10f), () => Show(screens.Settings));
        UiBuilder.CreateButton(panel, "Controls", new Vector2(0f, -95f), () => Show(screens.Controls));
        UiBuilder.CreateButton(panel, "Main menu", new Vector2(0f, -200f), BackToMenu);
        UiBuilder.CreateButton(panel, "Quit", new Vector2(0f, -305f), UiBuilder.Quit);
        mainPanel = panel.gameObject;

        screens = new SettingsScreens(root, ShowMain);
    }
}
