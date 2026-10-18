using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>Piezas de UI armadas por código para el menú principal y la pausa.</summary>
public static class UiBuilder
{
    public static readonly Color Accent = new Color(0.95f, 0.78f, 0.25f);

    public static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    public static Transform CreateCanvas(string name, int sortingOrder, Color background)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = new GameObject("Background", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(go.transform, false);
        Stretch((RectTransform)panel.transform);
        panel.GetComponent<Image>().color = background;
        return go.transform;
    }

    public static TextMeshProUGUI CreateLabel(Transform parent, string text, float size, Vector2 position, TMP_FontAsset font = null)
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

    public static Button CreateButton(Transform parent, string text, Vector2 position, UnityAction onClick)
    {
        var go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(360f, 90f);
        rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = Accent;
        colors.selectedColor = Accent;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        var label = CreateLabel(go.transform, text, 44f, Vector2.zero);
        Stretch(label.rectTransform);
        return button;
    }

    public static Slider CreateSlider(Transform parent, Vector2 position, float min, float max, float value, UnityAction<float> onChange)
    {
        var go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        go.transform.SetParent(parent, false);

        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(520f, 30f);
        rect.anchoredPosition = position;

        foreach (var image in go.GetComponentsInChildren<Image>())
            image.color = image.name == "Fill" ? Accent : new Color(1f, 1f, 1f, image.name == "Handle" ? 1f : 0.15f);

        var slider = go.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    public static RectTransform CreatePanel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        Stretch(rect);
        return rect;
    }

    /// <summary>Etiqueta a la izquierda y control a la derecha, para filas de opciones.</summary>
    public static TextMeshProUGUI CreateOptionLabel(Transform parent, string text, float y)
    {
        var label = CreateLabel(parent, text, 34f, new Vector2(-300f, y));
        label.rectTransform.sizeDelta = new Vector2(520f, 60f);
        label.alignment = TextAlignmentOptions.MidlineRight;
        return label;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
