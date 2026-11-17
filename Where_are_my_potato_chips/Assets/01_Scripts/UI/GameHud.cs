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
    TMP_Text guideText;
    SaleSequence[] sales = new SaleSequence[0];
    WarehouseDoor door;
    ReplicaCheckout replica;
    float nextLookup;
    float levelStartTime;
    float startHeight;
    bool fell;

    const float TutorialStepSeconds = 6f;
    const float FallDistance = 20f;
    static bool tutorialDone;

    static readonly string[] TutorialSteps =
    {
        "Move with <b>W A S D</b> and look around with the <b>mouse</b>",
        "Hold <b>SHIFT</b> to run. It uses stamina, and running makes noise",
        "Hold <b>CTRL</b> to crouch. Crouching inside a hiding spot (the cardboard boxes) hides you",
        "Press <b>SPACE</b> to jump onto boxes and ledges",
    };
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
        objectiveText = CreateTopLabel("Objective", -30f, 0.8f, Color.white);
        guideText = CreateTopLabel("Guide", -95f, 0.6f, UiBuilder.Accent);
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
            levelStartTime = Time.time;
            startHeight = player.transform.position.y;
        }

        CheckFall();

        if (statusText != null && playerState != null)
        {
            if (playerState.IsHidden) statusText.text = "HIDDEN";
            else if (playerState.InHidingSpot) statusText.text = "Crouch to hide (Ctrl)";
            else statusText.text = "";
        }

        if (objectiveText != null) objectiveText.text = CurrentObjective();
        if (guideText != null) guideText.text = StoreDirection();

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
            replica = FindAnyObjectByType<ReplicaCheckout>();
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

        if (playerState != null && playerState.IsSafe) return "You made it. Wait for the customer at the counter";

        // Primer espacio: enseña los controles uno por uno antes de dar el objetivo
        if (LevelFlowManager.CurrentLevel == 0 && !tutorialDone)
        {
            int step = Mathf.FloorToInt((Time.time - levelStartTime) / TutorialStepSeconds);
            if (step < TutorialSteps.Length) return TutorialSteps[step];
            tutorialDone = true;
        }

        return LevelContext.BossEnabled
            ? "Reach the store. Something hunts you: if it sees you, break its line of sight and crouch in a hiding spot"
            : "This is not your warehouse. Find the copy of your store somewhere in this place";
    }

    // Pequeña brújula hacia la tienda réplica, para no perderse
    string StoreDirection()
    {
        if (LevelFlowManager.IsLoading || LevelFlowManager.CurrentLevel < 0 || replica == null || player == null) return "";
        if (playerState != null && playerState.IsSafe) return "";
        if (LevelFlowManager.CurrentLevel == 0 && !tutorialDone) return "";

        Vector3 toStore = replica.transform.position - player.transform.position;
        toStore.y = 0f;
        float distance = toStore.magnitude;
        if (distance < 4f) return "";

        Vector3 forward = player.transform.forward;
        forward.y = 0f;
        float angle = Vector3.SignedAngle(forward, toStore, Vector3.up);
        string where = Mathf.Abs(angle) < 35f ? "ahead of you"
            : Mathf.Abs(angle) > 145f ? "behind you"
            : angle > 0f ? "to your right" : "to your left";
        return $"The store is about {Mathf.RoundToInt(distance)} m {where}";
    }

    // Si el jugador se cae del mapa, el espacio vuelve a empezar
    void CheckFall()
    {
        if (fell || LevelFlowManager.CurrentLevel < 0 || LevelFlowManager.IsLoading) return;
        if (player.transform.position.y > startHeight - FallDistance) return;

        fell = true;
        LevelFlowManager.RestartLevel(false);
    }

    TMP_Text CreateTopLabel(string labelName, float offsetY, float sizeFactor, Color color)
    {
        var label = CreateLabel(labelName, 0f, sizeFactor);
        if (label == null) return null;

        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(1600f, 80f);
        rect.anchoredPosition = new Vector2(0f, offsetY);
        label.color = color;
        label.outlineWidth = 0.2f;
        label.outlineColor = new Color32(0, 0, 0, 220);
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
