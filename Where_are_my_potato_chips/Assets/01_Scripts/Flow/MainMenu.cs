using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string title = "Where Are My\nPotato Chips?";
    [SerializeField] TMP_FontAsset titleFont;
    [SerializeField] Color accent = new Color(0.95f, 0.78f, 0.25f);

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        Transform canvas = CreateCanvas();
        CreateLabel(canvas, title, 120f, new Vector2(0f, 200f), titleFont).color = accent;
        CreateButton(canvas, "Play", new Vector2(0f, -60f), LevelFlowManager.StartNewGame);
        CreateButton(canvas, "Quit", new Vector2(0f, -180f), Quit);
        CreateLabel(canvas, "WASD move   ·   Shift run   ·   Ctrl crouch   ·   Space jump   ·   E interact",
            26f, new Vector2(0f, -440f), null).color = new Color(1f, 1f, 1f, 0.5f);
    }

    static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    Transform CreateCanvas()
    {
        var go = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(go.transform, false);
        Stretch((RectTransform)background.transform);
        background.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.06f);
        return go.transform;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float size, Vector2 position, TMP_FontAsset font)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        var rect = label.rectTransform;
        rect.sizeDelta = new Vector2(1600f, size * 2.6f);
        rect.anchoredPosition = position;
        return label;
    }

    void CreateButton(Transform parent, string text, Vector2 position, UnityAction onClick)
    {
        var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(360f, 90f);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = accent;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        var label = CreateLabel(go.transform, text, 44f, Vector2.zero, null);
        Stretch(label.rectTransform);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
