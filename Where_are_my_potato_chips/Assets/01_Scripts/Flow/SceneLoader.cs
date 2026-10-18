using System.Collections;
using TMPro;
using UnityEngine;
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
        SetPlayerLocked(true);

        if (reveal == Reveal.WakeUp) yield return WakeUp();
        else yield return Fade(0f);

        SetPlayerLocked(false);
        IsBusy = false;
    }

    IEnumerator PrintTicket(string message)
    {
        float startedAt = Time.unscaledTime;
        string[] lines =
        {
            "POTATO MART   #0047",
            "------------------------------",
            "1 x POTATO CHIPS        ---.--",
            "------------------------------",
            message,
            "------------------------------",
            "THANK YOU. PLEASE COME BACK."
        };

        centerText.text = "";
        foreach (string line in lines)
        {
            centerText.text += line + "\n";
            if (settings.printerClip != null) sfx.PlayOneShot(settings.printerClip, 0.6f);
            yield return new WaitForSecondsRealtime(0.22f);
        }

        while (Time.unscaledTime - startedAt < settings.minTicketTime) yield return null;
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
