using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conecta el prefab UI_Game (Crosshair, EnergyBar, DialogueBox, DialogueText) con el juego.
/// LevelFlowManager lo agrega solo al instanciar el HUD.
/// </summary>
public class GameHud : MonoBehaviour
{
    Image crosshair;
    Slider energyBar;
    GameObject dialoguePanel;
    TMP_Text dialogueText;
    TMP_Text promptText;
    TMP_Text statusText;
    PlayerState playerState;
    PlayerController player;
    Vector3 crosshairScale = Vector3.one;

    void Awake()
    {
        crosshair = FindChild<Image>("Crosshair");
        energyBar = FindChild<Slider>("EnergyBar");
        dialogueText = FindChild<TMP_Text>("DialogueText");

        var panel = FindChild<Image>("DialogueBox");
        if (panel != null) dialoguePanel = panel.gameObject;

        var logo = FindChild<TMP_Text>("Logo");
        if (logo != null) logo.gameObject.SetActive(false);

        if (crosshair != null) crosshairScale = crosshair.rectTransform.localScale;
        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = 1f;
            energyBar.interactable = false;
        }

        promptText = CreateLabel("Prompt", -70f, 0.75f);
        statusText = CreateLabel("HidingStatus", -120f, 0.6f);
        ShowLine(null, null);
    }

    void OnEnable()
    {
        PlayerInteractor.PromptChanged += ShowPrompt;
        DialogueBox.LineShown += ShowLine;
        DialogueBox.drawFallbackUI = false;
    }

    void OnDisable()
    {
        PlayerInteractor.PromptChanged -= ShowPrompt;
        DialogueBox.LineShown -= ShowLine;
        DialogueBox.drawFallbackUI = true;
    }

    void Update()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
            if (player == null) return;

            player.drawStaminaBar = false;
            playerState = player.GetComponent<PlayerState>();
            var interactor = player.GetComponent<PlayerInteractor>();
            if (interactor != null) interactor.DrawFallbackUI = false;
        }

        if (statusText != null && playerState != null)
        {
            if (playerState.IsHidden) statusText.text = "HIDDEN";
            else if (playerState.InHidingSpot) statusText.text = "Crouch to hide (Ctrl)";
            else statusText.text = "";
        }

        if (energyBar != null)
        {
            float stamina = player.Stamina01;
            energyBar.value = stamina;
            energyBar.gameObject.SetActive(stamina < 0.99f);
        }
    }

    void ShowPrompt(string prompt)
    {
        if (promptText != null) promptText.text = prompt != null ? "[E] " + prompt : "";
        if (crosshair != null)
            crosshair.rectTransform.localScale = prompt != null ? crosshairScale * 1.6f : crosshairScale;
    }

    void ShowLine(string speaker, string line)
    {
        bool visible = !string.IsNullOrEmpty(line);
        if (dialoguePanel != null) dialoguePanel.SetActive(visible);
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(visible);
            dialogueText.text = visible ? $"<b>{speaker}:</b> {line}" : "";
        }
    }

    TMP_Text CreateLabel(string labelName, float offsetY, float sizeFactor)
    {
        if (dialogueText == null) return null;

        var label = Instantiate(dialogueText, transform);
        label.name = labelName;
        label.gameObject.SetActive(true);
        label.text = "";
        label.fontSize = dialogueText.fontSize * sizeFactor;
        label.alignment = TextAlignmentOptions.Center;

        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 60f);
        rect.anchoredPosition = new Vector2(0f, offsetY);
        return label;
    }

    T FindChild<T>(string childName) where T : Component
    {
        foreach (var component in GetComponentsInChildren<T>(true))
            if (component.name == childName) return component;
        return null;
    }
}
