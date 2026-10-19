using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Cambios de escena sin cortes: fundido a negro, ticket de caja mientras carga
/// y "despertar" (párpados que se abren) al aparecer en el nuevo espacio.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public enum Reveal { Fade, WakeUp }

    GameFlowSettings settings;
    CanvasGroup blackout;
    RectTransform topLid;
    RectTransform bottomLid;
    TextMeshProUGUI centerText;
    AudioSource sfx;

    // Ticket de caja
    RectTransform ticket;
    TextMeshProUGUI ticketText;
    static readonly Color PaperColor = new Color(0.95f, 0.93f, 0.85f);
    static readonly Color InkColor = new Color(0.13f, 0.12f, 0.11f);
    const string MonoOpen = "<mspace=0.6em>";
    const float TicketWidth = 620f;
    const float TicketHeight = 860f;

    public bool IsBusy { get; private set; }

    public void Setup(GameFlowSettings flowSettings)
    {
        settings = flowSettings;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        BuildOverlay();
    }

    public void Load(string scene, Reveal reveal, string ticketMessage = null, AudioClip sting = null)
    {
        if (IsBusy) return;
        StartCoroutine(LoadRoutine(scene, reveal, ticketMessage, sting));
    }

    public IEnumerator ShowCredits(string text, float seconds, AudioClip sting)
    {
        IsBusy = true;
        if (sting != null) sfx.PlayOneShot(sting);
        yield return Fade(1f);
        centerText.text = text;
        yield return new WaitForSecondsRealtime(seconds);
        centerText.text = "";
        IsBusy = false;
    }

    /// <summary>Pantalla negra con la historia, línea por línea. Cualquier tecla o clic la salta.</summary>
    public IEnumerator ShowStory(string[] lines, float secondsPerLine)
    {
        IsBusy = true;
        yield return Fade(1f);

        string shown = "";
        foreach (var line in lines)
        {
            shown += line + "\n\n";
            centerText.text = shown + "<size=55%><color=#8C8C8C>Press any key to skip</color></size>";

            bool skipped = false;
            for (float t = 0f; t < secondsPerLine; t += Time.unscaledDeltaTime)
            {
                if (SkipPressed()) { skipped = true; break; }
                yield return null;
            }
            if (skipped) break;
        }

        centerText.text = "";
        IsBusy = false;
    }

    static bool SkipPressed() =>
        (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

    IEnumerator LoadRoutine(string scene, Reveal reveal, string ticketMessage, AudioClip sting)
    {
        IsBusy = true;
        SetPlayerLocked(true);
        if (sting != null) sfx.PlayOneShot(sting);
        yield return Fade(1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
        operation.allowSceneActivation = false;

        if (!string.IsNullOrEmpty(ticketMessage))
            yield return PrintTicket(ticketMessage);

        while (operation.progress < 0.9f) yield return null;
        operation.allowSceneActivation = true;
        while (!operation.isDone) yield return null;

        centerText.text = "";
        ticket.gameObject.SetActive(false);
        SetPlayerLocked(true);

        if (reveal == Reveal.WakeUp) yield return WakeUp();
        else yield return Fade(0f);

        SetPlayerLocked(false);
        IsBusy = false;
    }

    IEnumerator PrintTicket(string message)
    {
        float startedAt = Time.unscaledTime;
        System.DateTime now = System.DateTime.Now;

        string[] lines =
        {
            "<align=center><b>POTATO MART</b>",
            "<align=center>STORE #0047",
            "<align=center>OPEN 24 HOURS (ALWAYS)",
            "",
            "<align=left>DATE  " + now.ToString("yyyy-MM-dd") + "   " + now.ToString("HH:mm"),
            "<align=left>REG   03        CASHIER  ???",
            "------------------------------",
            "<align=left>1 x POTATO CHIPS        ---.--",
            "<align=left>    (CLASSIC SALTED)",
            "------------------------------",
            "<align=left>SUBTOTAL               ---.--",
            "<align=left>TOTAL                  ---.--",
            "------------------------------",
            "<align=center>" + message.Replace("\n", "\n<align=center>"),
            "------------------------------",
            "<align=center>THANK YOU.",
            "<align=center>PLEASE COME BACK."
        };

        ticketText.text = MonoOpen;
        ticket.gameObject.SetActive(true);
        ticket.localRotation = Quaternion.Euler(0f, 0f, -1.6f);
        ticket.anchoredPosition = new Vector2(0f, -TicketHeight - 200f);
        foreach (Transform child in ticket) child.gameObject.SetActive(child.name != "Barcode");

        // el papel sube desde abajo
        yield return SlideTicket(-TicketHeight - 200f, 0f, 0.55f);

        // se imprime línea a línea
        foreach (string line in lines)
        {
            ticketText.text += line + "\n";
            if (settings.printerClip != null) sfx.PlayOneShot(settings.printerClip, 0.6f);
            yield return new WaitForSecondsRealtime(0.22f);
        }

        ticket.Find("Barcode").gameObject.SetActive(true);
        if (settings.printerClip != null) sfx.PlayOneShot(settings.printerClip, 0.6f);

        // pequeño temblor mientras espera (papel colgando)
        while (Time.unscaledTime - startedAt < settings.minTicketTime)
        {
            float sway = Mathf.Sin(Time.unscaledTime * 1.7f) * 0.35f;
            ticket.localRotation = Quaternion.Euler(0f, 0f, -1.6f + sway);
            yield return null;
        }
    }

    IEnumerator SlideTicket(float fromY, float toY, float duration)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            ticket.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromY, toY, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
        ticket.anchoredPosition = new Vector2(0f, toY);
    }

    IEnumerator WakeUp()
    {
        SetLids(0f);
        blackout.alpha = 0f;
        yield return MoveLids(0f, 0.3f, 0.5f);
        yield return MoveLids(0.3f, 0f, 0.15f);
        yield return new WaitForSecondsRealtime(0.35f);
        yield return MoveLids(0f, 1f, 0.9f);
    }

    IEnumerator MoveLids(float from, float to, float duration)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            SetLids(Mathf.SmoothStep(from, to, t));
            yield return null;
        }
        SetLids(to);
    }

    IEnumerator Fade(float target)
    {
        float start = blackout.alpha;
        float duration = Mathf.Max(0.01f, settings.fadeDuration);
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            blackout.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        blackout.alpha = target;
    }

    // 0 = ojos cerrados, 1 = abiertos
    void SetLids(float open)
    {
        float cover = 0.5f * (1f - open);
        topLid.anchorMin = new Vector2(0f, 1f - cover);
        topLid.anchorMax = Vector2.one;
        bottomLid.anchorMin = Vector2.zero;
        bottomLid.anchorMax = new Vector2(1f, cover);
    }

    static void SetPlayerLocked(bool locked)
    {
        var state = FindAnyObjectByType<PlayerState>();
        if (state != null) state.InputLocked = locked;
    }

    void BuildOverlay()
    {
        var root = new GameObject("TransitionCanvas", typeof(Canvas), typeof(CanvasScaler));
        root.transform.SetParent(transform, false);

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        blackout = CreatePanel("Blackout", root.transform).gameObject.AddComponent<CanvasGroup>();
        blackout.alpha = 0f;
        blackout.blocksRaycasts = false;

        topLid = CreatePanel("TopLid", root.transform).rectTransform;
        bottomLid = CreatePanel("BottomLid", root.transform).rectTransform;
        SetLids(1f);

        BuildTicket(root.transform);

        var textObject = new GameObject("CenterText", typeof(RectTransform));
        textObject.transform.SetParent(root.transform, false);
        centerText = textObject.AddComponent<TextMeshProUGUI>();
        centerText.alignment = TextAlignmentOptions.Center;
        centerText.fontSize = 38f;
        centerText.color = new Color(0.93f, 0.91f, 0.84f);
        centerText.raycastTarget = false;
        var rect = centerText.rectTransform;
        rect.anchorMin = new Vector2(0.2f, 0.1f);
        rect.anchorMax = new Vector2(0.8f, 0.9f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    void BuildTicket(Transform parent)
    {
        var paper = new GameObject("Ticket", typeof(RectTransform), typeof(Image));
        paper.transform.SetParent(parent, false);
        ticket = (RectTransform)paper.transform;
        ticket.anchorMin = ticket.anchorMax = new Vector2(0.5f, 0.5f);
        ticket.pivot = new Vector2(0.5f, 0.5f);
        ticket.sizeDelta = new Vector2(TicketWidth, TicketHeight);
        var paperImage = paper.GetComponent<Image>();
        paperImage.color = PaperColor;
        paperImage.raycastTarget = false;

        // sombra suave
        var shadow = paper.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(10f, -12f);

        // bordes dentados (arriba y abajo), cuadrados girados del color del fondo
        for (int i = 0; i < 2; i++)
        {
            float y = i == 0 ? TicketHeight * 0.5f : -TicketHeight * 0.5f;
            for (float x = -TicketWidth * 0.5f; x <= TicketWidth * 0.5f + 1f; x += 28f)
            {
                var tooth = new GameObject("Tooth", typeof(RectTransform), typeof(Image));
                tooth.transform.SetParent(ticket, false);
                var r = (RectTransform)tooth.transform;
                r.sizeDelta = new Vector2(20f, 20f);
                r.anchoredPosition = new Vector2(x, y);
                r.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var img = tooth.GetComponent<Image>();
                img.color = Color.black;
                img.raycastTarget = false;
            }
        }

        // texto monoespaciado con tinta oscura
        var textObject = new GameObject("TicketText", typeof(RectTransform));
        textObject.transform.SetParent(ticket, false);
        ticketText = textObject.AddComponent<TextMeshProUGUI>();
        ticketText.text = "";
        ticketText.richText = true;
        ticketText.fontSize = 27f;
        ticketText.color = InkColor;
        ticketText.alignment = TextAlignmentOptions.TopLeft;
        ticketText.textWrappingMode = TextWrappingModes.NoWrap;
        ticketText.lineSpacing = 8f;
        ticketText.raycastTarget = false;
        var tr = ticketText.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(36f, 130f);
        tr.offsetMax = new Vector2(-36f, -46f);

        // código de barras
        var barcode = new GameObject("Barcode", typeof(RectTransform));
        barcode.transform.SetParent(ticket, false);
        var br = (RectTransform)barcode.transform;
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0f);
        br.pivot = new Vector2(0.5f, 0f);
        br.sizeDelta = new Vector2(420f, 70f);
        br.anchoredPosition = new Vector2(0f, 40f);
        var rng = new System.Random(4700);
        float bx = 0f;
        while (bx < 420f)
        {
            float w = rng.Next(1, 4) * 3f;
            var bar = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(br, false);
            var rr = (RectTransform)bar.transform;
            rr.anchorMin = Vector2.zero;
            rr.anchorMax = new Vector2(0f, 1f);
            rr.pivot = new Vector2(0f, 0.5f);
            rr.sizeDelta = new Vector2(w, 0f);
            rr.anchoredPosition = new Vector2(bx, 0f);
            var bi = bar.GetComponent<Image>();
            bi.color = InkColor;
            bi.raycastTarget = false;
            bx += w + rng.Next(1, 4) * 3f;
        }

        ticket.gameObject.SetActive(false);
    }

    static Image CreatePanel(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
        return image;
    }
}
