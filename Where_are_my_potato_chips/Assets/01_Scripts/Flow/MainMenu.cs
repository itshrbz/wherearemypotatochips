using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú principal sobre el pasillo 3D de 01_Menu. Se navega con mouse, teclado o mando:
/// siempre hay un botón seleccionado y Esc o el botón B vuelven atrás.
/// </summary>
public class MainMenu : MonoBehaviour
{
    enum MenuStyle { Original, Chips }

    const string ChipsTitle = "WHERE ARE MY\nPOTATO CHIPS?";
    const float ChipButtonWidth = 330f;
    const float ChipPlayScale = 1.15f;
    const float ChipButtonTop = 306f;
    const float ChipButtonGap = 33f;
    const float ChipQuitAudioTail = 0.025f;
    const float ChipQuitMaxDelay = 0.18f;
    static readonly Color32 ChipsTitleColor = new Color32(255, 239, 190, 255);
    static readonly Color32 ChipsTextColor = new Color32(65, 36, 17, 255);

    [SerializeField] string title = "Where Are My\nPotato Chips?";
    [SerializeField] TMP_FontAsset titleFont;
    [Tooltip("Fecha que muestra el rótulo de videocámara.")]
    [SerializeField] string camcorderDate = "NOV. 14 1997";

    [Header("Apariencia del menú")]
    [Tooltip("Estilo visual que se aplica al iniciar el menú.")]
    [SerializeField] MenuStyle menuStyle;
    [Tooltip("Sprite que se usa como fondo de los botones en Chips.")]
    [SerializeField] Sprite chipButtonSprite;
    [Tooltip("Fuente del título y los botones en Chips.")]
    [SerializeField] TMP_FontAsset chipsFont;
    [Header("Sonido de Chips")]
    [Tooltip("Crujido que suena al activar un botón.")]
    [SerializeField] AudioClip chipCrunch;
    [Tooltip("Volumen del crujido respecto al volumen general del juego.")]
    [SerializeField, Range(0f, 1f)] float chipCrunchVolume = 0.45f;

    GameObject mainPanel;
    GameObject creditsPanel;
    SettingsScreens screens;
    GameObject currentPanel;
    Button firstMainButton;
    TextMeshProUGUI titleLabel;
    TextMeshProUGUI recLabel;
    TextMeshProUGUI clockLabel;
    Material chipsButtonTextMaterial;
    bool useChips;
    bool chipActivationPending;
    float clockStart;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UiBuilder.EnsureEventSystem();

        // Fondo casi transparente: se ve el pasillo de atrás
        Transform canvas = UiBuilder.CreateCanvas("MenuCanvas", 0, new Color(0f, 0f, 0f, 0.3f));

        var panel = UiBuilder.CreatePanel(canvas, "Main");
        useChips = menuStyle == MenuStyle.Chips && chipButtonSprite != null && chipsFont != null;
        if (menuStyle == MenuStyle.Chips && !useChips)
            Debug.LogWarning("Asigna el sprite y la fuente de Chips en MainMenu. Se usará el estilo Original.", this);

        titleLabel = CreateMainTitle(panel, useChips);
        firstMainButton = AddButton("Play", 0, LevelFlowManager.StartNewGame);
        var settingsButton = AddButton("Settings", 1, () => Show(screens.Settings));
        var controlsButton = AddButton("Controls", 2, () => Show(screens.Controls));
        var creditsButton = AddButton("Credits", 3, () => Show(creditsPanel));
        Button quitButton = null;
        // En el navegador no se puede cerrar la pestaña desde el juego
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            quitButton = AddButton("Quit", 4, UiBuilder.Quit, isQuit: true);

        if (useChips)
        {
            SetChipNavigation(firstMainButton, down: settingsButton);
            SetChipNavigation(settingsButton, up: firstMainButton, down: controlsButton);
            SetChipNavigation(controlsButton, up: settingsButton, down: creditsButton);
            SetChipNavigation(creditsButton, up: controlsButton, down: quitButton);
            SetChipNavigation(quitButton, up: creditsButton);
        }
        mainPanel = panel.gameObject;

        screens = new SettingsScreens(canvas, ShowMain);
        creditsPanel = BuildCredits(canvas);
        BuildCamcorderOverlay(canvas);

        ShowMain();
        if (!useChips)
            StartCoroutine(TitleFlicker());

        Button AddButton(string text, int index, UnityAction onClick, bool isQuit = false)
        {
            if (!useChips)
                return UiBuilder.CreateButton(panel, text, new Vector2(0f, 20f - index * 110f), onClick);

            ChipButtonMotion motion = null;
            var button = UiBuilder.CreateButton(panel, text, Vector2.zero, () =>
            {
                if (!chipActivationPending)
                    StartCoroutine(ActivateChipButton(motion, onClick, isQuit));
            });
            motion = StyleChipButton(button, index);
            return button;
        }
    }

    IEnumerator ActivateChipButton(ChipButtonMotion motion, UnityAction action, bool isQuit)
    {
        chipActivationPending = true;
        motion.PlayPress();
        Sfx.PlayUi(chipCrunch, chipCrunchVolume);

        float delay = ChipButtonMotion.PressDuration;
        // Al cerrar la aplicación desaparece el emisor persistente. Dejamos una cola breve para el crujido.
        if (isQuit && chipCrunch != null && chipCrunchVolume > 0f)
            delay = Mathf.Clamp(chipCrunch.length + ChipQuitAudioTail, delay, ChipQuitMaxDelay);

        yield return new WaitForSecondsRealtime(delay);
        chipActivationPending = false;
        action();
    }

    TextMeshProUGUI CreateMainTitle(Transform panel, bool useChips)
    {
        if (!useChips)
        {
            var original = UiBuilder.CreateLabel(panel, title, 135f, new Vector2(0f, 270f), titleFont);
            original.color = UiBuilder.Accent;
            original.rectTransform.sizeDelta = new Vector2(1600f, 360f);
            return original;
        }

        var chips = UiBuilder.CreateLabel(panel, ChipsTitle, 92f, Vector2.zero, chipsFont);
        var rect = chips.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -72f);
        rect.sizeDelta = new Vector2(940f, 220f);
        chips.alignment = TextAlignmentOptions.Top;
        chips.lineSpacing = -9f;
        chips.color = ChipsTitleColor;
        chips.outlineColor = new Color32(87, 50, 29, 245);
        chips.outlineWidth = 0.16f;
        return chips;
    }

    ChipButtonMotion StyleChipButton(Button button, int index)
    {
        bool isPlay = index == 0;
        float aspect = chipButtonSprite.rect.height / chipButtonSprite.rect.width;
        float width = ChipButtonWidth * (isPlay ? ChipPlayScale : 1f);
        float height = width * aspect;
        float top = ChipButtonTop;
        if (!isPlay)
            top += ChipButtonWidth * ChipPlayScale * aspect + ChipButtonGap + (index - 1) * (height + ChipButtonGap);

        // Solo se mueve Visual. El rectángulo raíz sigue recibiendo el puntero.
        var hitArea = button.image;
        hitArea.color = Color.clear;
        hitArea.canvasRenderer.cullTransparentMesh = false;
        var rect = hitArea.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(0f, -top - height * 0.5f);

        var label = button.GetComponentInChildren<TextMeshProUGUI>();
        var visual = new GameObject("Visual", typeof(RectTransform)).GetComponent<RectTransform>();
        visual.SetParent(rect, false);
        UiBuilder.Stretch(visual);

        var shadowImage = CreateChipImage(visual, "Shadow");
        shadowImage.color = new Color32(27, 26, 14, 96);
        shadowImage.rectTransform.anchoredPosition = new Vector2(0f, -6f);
        var softEdge = shadowImage.gameObject.AddComponent<Shadow>();
        softEdge.effectColor = new Color32(27, 26, 14, 28);
        softEdge.effectDistance = new Vector2(1f, -5f);
        softEdge.useGraphicAlpha = false;

        var image = CreateChipImage(visual, "Chips");
        button.targetGraphic = image;
        var focus = image.gameObject.AddComponent<Outline>();
        focus.effectColor = new Color32(247, 232, 176, 215);
        focus.effectDistance = new Vector2(3f, 3f);
        focus.enabled = false;

        var colors = button.colors;
        // El tinte nativo multiplica el sprite. Atenuar más el rojo suaviza su naranja.
        colors.normalColor = new Color32(211, 221, 205, 255);
        colors.highlightedColor = new Color32(239, 246, 226, 255);
        colors.selectedColor = new Color32(230, 239, 219, 255);
        colors.pressedColor = new Color32(195, 208, 187, 255);
        colors.fadeDuration = 0.07f;
        button.colors = colors;

        float fontSize = isPlay ? 59f : 50f;
        var textShadow = UiBuilder.CreateLabel(visual, label.text, fontSize, Vector2.zero, chipsFont);
        textShadow.name = "Text Shadow";
        UiBuilder.Stretch(textShadow.rectTransform);
        textShadow.rectTransform.anchoredPosition = new Vector2(0f, -3f);
        textShadow.color = new Color32(74, 39, 19, 110);

        label.rectTransform.SetParent(visual, false);
        UiBuilder.Stretch(label.rectTransform);
        label.font = chipsFont;
        label.fontSharedMaterial = GetChipsButtonTextMaterial();
        label.fontSize = fontSize;
        label.color = ChipsTextColor;

        var motion = button.gameObject.AddComponent<ChipButtonMotion>();
        motion.Initialize(visual, focus);
        return motion;
    }

    Image CreateChipImage(RectTransform parent, string name)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.rectTransform.SetParent(parent, false);
        UiBuilder.Stretch(image.rectTransform);
        image.sprite = chipButtonSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    Material GetChipsButtonTextMaterial()
    {
        if (chipsButtonTextMaterial != null)
            return chipsButtonTextMaterial;

        var material = new Material(chipsFont.material);
        material.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(255, 229, 166, 223));
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.12f);
        chipsButtonTextMaterial = material;
        return material;
    }

    static void SetChipNavigation(Button button, Button up = null, Button down = null)
    {
        if (button == null)
            return;

        button.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = up,
            selectOnDown = down
        };
    }

    void OnDestroy()
    {
        if (chipsButtonTextMaterial != null)
            Destroy(chipsButtonTextMaterial);
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
        if (events != null && events.currentSelectedGameObject == null
            && NavigationPressed(useChips && currentPanel == mainPanel))
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

    static bool NavigationPressed(bool includesHorizontal)
    {
        var k = Keyboard.current;
        if (k != null && (k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame
                          || k.wKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame
                          || k.tabKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame
                          || (includesHorizontal && (k.leftArrowKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame
                              || k.aKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame))))
            return true;
        var g = Gamepad.current;
        return g != null && (g.dpad.up.wasPressedThisFrame || g.dpad.down.wasPressedThisFrame
                             || g.leftStick.ReadValue().sqrMagnitude > 0.25f || g.buttonSouth.wasPressedThisFrame
                             || (includesHorizontal && (g.dpad.left.wasPressedThisFrame || g.dpad.right.wasPressedThisFrame)));
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
