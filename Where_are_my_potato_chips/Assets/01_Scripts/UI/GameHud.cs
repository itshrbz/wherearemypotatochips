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
    TMP_Text objectiveText;
    SaleSequence[] sales = new SaleSequence[0];
    WarehouseDoor door;
    float nextLookup;
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
        objectiveText = CreateObjectiveLabel();
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

        if (objectiveText != null) objectiveText.text = CurrentObjective();

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

    // Guía de qué hacer, arriba de la pantalla. Lee el estado del juego en vez de que cada sistema la avise.
    string CurrentObjective()
    {
        if (LevelFlowManager.IsLoading || LevelFlowManager.IsEnding) return "";

        if (Time.time >= nextLookup)
        {
            nextLookup = Time.time + 1f;
            sales = FindObjectsByType<SaleSequence>(FindObjectsSortMode.None);
            door = FindAnyObjectByType<WarehouseDoor>();
        }

        foreach (var sale in sales)
        {
            if (sale == null) continue;
            if (sale.CurrentStep == SaleSequence.Step.WaitingScan) return "Scan the product: look at it and press E";
            if (sale.CurrentStep == SaleSequence.Step.WaitingCharge) return "Charge it: look at the register and press E";
        }

        if (LevelFlowManager.CurrentLevel < 0)
        {
            if (door != null && door.CanInteract) return "No potato chips on the shelves. Check the warehouse: the door behind the counter";
            return "Wait behind the counter and serve the customers";
        }

        if (playerState != null && playerState.IsSafe) return "Wait for the customer at the counter";
        return LevelContext.BossEnabled
            ? "Find the store. Something hunts you: crouch (Ctrl) inside hiding spots and don't run near it"
            : "Something's off. Find the store hidden somewhere in this place";
    }

    TMP_Text CreateObjectiveLabel()
    {
        var label = CreateLabel("Objective", 0f, 0.6f);
        if (label == null) return null;

        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(1400f, 80f);
        rect.anchoredPosition = new Vector2(0f, -30f);
        label.color = new Color(1f, 1f, 1f, 0.85f);
        return label;
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
