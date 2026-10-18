using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Capa de pantalla de los espacios liminales: viñeta que oscurece los bordes cuando el jefe
/// tiene de objetivo al jugador (más suave si solo sospecha) y destellos de color.
/// Es una imagen de UI, así que funciona igual en WebGL.
/// </summary>
public class ScreenEffects : MonoBehaviour
{
    [SerializeField] float maxAlpha = 0.85f;
    [SerializeField, Range(0f, 1f)] float suspicionStrength = 0.4f;
    [SerializeField] float fadeSpeed = 1.5f;
    [SerializeField] float pulseSpeed = 6f;
    [SerializeField] float pulseAmount = 0.08f;

    static ScreenEffects instance;

    Image vignette;
    Image flash;
    BossAI boss;
    float level;
    Color flashColor;
    float flashTime;
    float flashDuration;
    float nextLookup;

    public static void Flash(Color color, float duration)
    {
        if (instance == null) return;
        if (instance.flashTime > 0f) return;
        instance.flashColor = color;
        instance.flashTime = instance.flashDuration = duration;
    }

    void Awake()
    {
        instance = this;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;

        vignette = CreateLayer("Vignette", Sprite.Create(BuildVignette(256), new Rect(0, 0, 256, 256), Vector2.one * 0.5f));
        vignette.color = new Color(0.04f, 0f, 0f, 0f);
        flash = CreateLayer("Flash", null);
        flash.color = Color.clear;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Update()
    {
        if (boss == null && Time.unscaledTime >= nextLookup)
        {
            nextLookup = Time.unscaledTime + 1f;
            boss = FindAnyObjectByType<BossAI>();
        }

        float target = 0f;
        if (boss != null && boss.isActiveAndEnabled)
            target = boss.IsChasing ? 1f : boss.Awareness01 * suspicionStrength;

        level = Mathf.MoveTowards(level, target, fadeSpeed * Time.deltaTime);
        float pulse = boss != null && boss.IsChasing ? Mathf.Sin(Time.time * pulseSpeed) * pulseAmount : 0f;
        SetAlpha(vignette, Mathf.Clamp01(level * maxAlpha + pulse * level));

        if (flashTime > 0f)
        {
            flashTime -= Time.deltaTime;
            Color c = flashColor;
            c.a *= Mathf.Clamp01(flashTime / flashDuration);
            flash.color = c;
        }
    }

    Image CreateLayer(string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        UiBuilder.Stretch((RectTransform)go.transform);

        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    static void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    // Transparente en el centro y opaco hacia las esquinas
    static Texture2D BuildVignette(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (size - 1f) * 2f - 1f;
                float v = y / (size - 1f) * 2f - 1f;
                float distance = Mathf.Sqrt(u * u + v * v);
                float alpha = Mathf.Pow(Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.35f, distance)), 1.4f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }
}
