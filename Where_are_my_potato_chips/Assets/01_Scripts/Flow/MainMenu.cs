using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string title = "Where Are My\nPotato Chips?";
    [SerializeField] TMP_FontAsset titleFont;

    GameObject mainPanel;
    SettingsScreens screens;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UiBuilder.EnsureEventSystem();

        Transform canvas = UiBuilder.CreateCanvas("MenuCanvas", 0, new Color(0.05f, 0.05f, 0.06f));

        var panel = UiBuilder.CreatePanel(canvas, "Main");
        UiBuilder.CreateLabel(panel, title, 120f, new Vector2(0f, 230f), titleFont).color = UiBuilder.Accent;
        UiBuilder.CreateButton(panel, "Play", new Vector2(0f, 0f), LevelFlowManager.StartNewGame);
        UiBuilder.CreateButton(panel, "Settings", new Vector2(0f, -110f), () => Show(screens.Settings));
        UiBuilder.CreateButton(panel, "Controls", new Vector2(0f, -220f), () => Show(screens.Controls));
        UiBuilder.CreateButton(panel, "Quit", new Vector2(0f, -330f), UiBuilder.Quit);
        mainPanel = panel.gameObject;

        screens = new SettingsScreens(canvas, ShowMain);
    }

    void ShowMain()
    {
        mainPanel.SetActive(true);
        screens.Settings.SetActive(false);
        screens.Controls.SetActive(false);
    }

    void Show(GameObject screen)
    {
        mainPanel.SetActive(false);
        screens.Refresh();
        screen.SetActive(true);
    }
}
