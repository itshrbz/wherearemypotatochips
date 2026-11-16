using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú principal sobre el pasillo 3D de 01_Menu. Se navega con mouse, teclado o mando:
/// siempre hay un botón seleccionado y Esc o el botón B vuelven atrás.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] string title = "Where Are My\nPotato Chips?";
    [SerializeField] TMP_FontAsset titleFont;
    [Tooltip("Fecha que muestra el rótulo de videocámara.")]
    [SerializeField] string camcorderDate = "NOV. 14 1997";

    GameObject mainPanel;
    GameObject creditsPanel;
    SettingsScreens screens;
    GameObject currentPanel;
    Button firstMainButton;
    TextMeshProUGUI titleLabel;
    TextMeshProUGUI recLabel;
    TextMeshProUGUI clockLabel;
    float clockStart;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UiBuilder.EnsureEventSystem();

        // Fondo casi transparente: se ve el pasillo de atrás
        Transform canvas = UiBuilder.CreateCanvas("MenuCanvas", 0, new Color(0f, 0f, 0f, 0.3f));

        var panel = UiBuilder.CreatePanel(canvas, "Main");
        titleLabel = UiBuilder.CreateLabel(panel, title, 135f, new Vector2(0f, 270f), titleFont);
        titleLabel.color = UiBuilder.Accent;
        titleLabel.rectTransform.sizeDelta = new Vector2(1600f, 360f);

        firstMainButton = UiBuilder.CreateButton(panel, "Play", new Vector2(0f, 20f), LevelFlowManager.StartNewGame);
        UiBuilder.CreateButton(panel, "Settings", new Vector2(0f, -90f), () => Show(screens.Settings));
        UiBuilder.CreateButton(panel, "Controls", new Vector2(0f, -200f), () => Show(screens.Controls));
        UiBuilder.CreateButton(panel, "Credits", new Vector2(0f, -310f), () => Show(creditsPanel));
        // En el navegador no se puede cerrar la pestaña desde el juego
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            UiBuilder.CreateButton(panel, "Quit", new Vector2(0f, -420f), UiBuilder.Quit);
        mainPanel = panel.gameObject;

        screens = new SettingsScreens(canvas, ShowMain);
        creditsPanel = BuildCredits(canvas);
        BuildCamcorderOverlay(canvas);

        ShowMain();
        StartCoroutine(TitleFlicker());
    }

    void Update()
    {
        UpdateCamcorder();

        // Esc o B en el mando vuelven al menú desde cualquier pantalla
        bool back = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                    || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        if (back && currentPanel != mainPanel)
        {
            PlayerPrefs.Save();
            ShowMain();
            return;
        }

        // Si el mouse deseleccionó todo, el teclado o el mando recuperan la selección
        var events = EventSystem.current;
        if (events != null && events.currentSelectedGameObject == null && NavigationPressed())
            SelectFirst(currentPanel);
    }

    void ShowMain()
    {
        mainPanel.SetActive(true);
        screens.Settings.SetActive(false);
        screens.Controls.SetActive(false);
        creditsPanel.SetActive(false);
        currentPanel = mainPanel;
        Select(firstMainButton != null ? firstMainButton.gameObject : null);
    }

    void Show(GameObject screen)
    {
        mainPanel.SetActive(false);
        screens.Refresh();
        screen.SetActive(true);
        currentPanel = screen;
        SelectFirst(screen);
    }

    GameObject BuildCredits(Transform canvas)
    {
        var panel = UiBuilder.CreatePanel(canvas, "Credits");
        UiBuilder.CreateLabel(panel, "CREDITS", 80f, new Vector2(0f, 380f)).color = UiBuilder.Accent;

        var text = UiBuilder.CreateLabel(panel, LevelFlowManager.Settings.credits, 36f, new Vector2(0f, 20f));
        text.rectTransform.sizeDelta = new Vector2(1300f, 600f);
        text.enableAutoSizing = true;
        text.fontSizeMin = 20f;
        text.fontSizeMax = 36f;

        UiBuilder.CreateButton(panel, "Back", new Vector2(0f, -380f), ShowMain);
        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    // Rótulo de videocámara vieja en las esquinas, a juego con el look de A2
    void BuildCamcorderOverlay(Transform canvas)
    {
        // Sin "▶": la fuente por defecto de TMP no trae ese glifo
        recLabel = UiBuilder.CreateLabel(canvas, "PLAY", 40f, Vector2.zero);
        Corner(recLabel, new Vector2(0f, 1f), new Vector2(70f, -60f), TextAlignmentOptions.TopLeft);

        clockLabel = UiBuilder.CreateLabel(canvas, "", 40f, Vector2.zero);
        Corner(clockLabel, new Vector2(1f, 0f), new Vector2(-70f, 60f), TextAlignmentOptions.BottomRight);
        clockStart = Time.unscaledTime;
    }

    static void Corner(TextMeshProUGUI label, Vector2 anchor, Vector2 offset, TextAlignmentOptions alignment)
    {
        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(600f, 120f);
        label.alignment = alignment;
        label.color = new Color(1f, 1f, 1f, 0.75f);
        label.characterSpacing = 6f;
    }

    void UpdateCamcorder()
    {
        if (clockLabel == null) return;
        // Empieza a las 3:07 de la madrugada, la hora del turno de noche
        int seconds = 3 * 3600 + 7 * 60 + Mathf.FloorToInt(Time.unscaledTime - clockStart);
        clockLabel.text = string.Format("AM {0}:{1:00}:{2:00}\n{3}", seconds / 3600, seconds / 60 % 60, seconds % 60, camcorderDate);
        recLabel.enabled = Mathf.Repeat(Time.unscaledTime, 1.4f) < 1.0f;
    }

    // De vez en cuando el título falla como un letrero viejo
    IEnumerator TitleFlicker()
    {
        Color baseColor = titleLabel.color;
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(4f, 9f));
            int flashes = Random.Range(2, 5);
            for (int i = 0; i < flashes; i++)
            {
                titleLabel.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);
                yield return new WaitForSecondsRealtime(Random.Range(0.04f, 0.1f));
                titleLabel.color = baseColor;
                yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.15f));
            }
        }
    }

    static bool NavigationPressed()
    {
        var k = Keyboard.current;
        if (k != null && (k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame
                          || k.wKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame
                          || k.tabKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame))
            return true;
        var g = Gamepad.current;
        return g != null && (g.dpad.up.wasPressedThisFrame || g.dpad.down.wasPressedThisFrame
                             || g.leftStick.ReadValue().sqrMagnitude > 0.25f || g.buttonSouth.wasPressedThisFrame);
    }

    static void SelectFirst(GameObject panel)
    {
        if (panel == null) return;
        var selectable = panel.GetComponentInChildren<Selectable>();
        Select(selectable != null ? selectable.gameObject : null);
    }

    static void Select(GameObject target)
    {
        if (EventSystem.current == null || target == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }
}
