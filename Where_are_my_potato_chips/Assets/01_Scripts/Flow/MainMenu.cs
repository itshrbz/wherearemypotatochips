using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string title = "Where Are My\nPotato Chips?";
    [SerializeField] TMP_FontAsset titleFont;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UiBuilder.EnsureEventSystem();

        Transform canvas = UiBuilder.CreateCanvas("MenuCanvas", 0, new Color(0.05f, 0.05f, 0.06f));
        UiBuilder.CreateLabel(canvas, title, 120f, new Vector2(0f, 200f), titleFont).color = UiBuilder.Accent;
        UiBuilder.CreateButton(canvas, "Play", new Vector2(0f, -60f), LevelFlowManager.StartNewGame);
        UiBuilder.CreateButton(canvas, "Quit", new Vector2(0f, -180f), UiBuilder.Quit);
        UiBuilder.CreateLabel(canvas, "WASD move   ·   Shift run   ·   Ctrl crouch   ·   Space jump   ·   E interact   ·   Esc pause",
            26f, new Vector2(0f, -440f)).color = new Color(1f, 1f, 1f, 0.5f);
    }
}
