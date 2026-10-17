using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Muestra líneas de diálogo en pantalla. Se crea sola la primera vez que se usa.
/// La UI final puede escuchar LineShown y desactivar drawFallbackUI.
/// </summary>
public class DialogueBox : MonoBehaviour
{
    /// <summary>(quién habla, texto). Texto null = ocultar.</summary>
    public static event Action<string, string> LineShown;

    public static bool drawFallbackUI = true;

    static DialogueBox instance;
    string speaker;
    string text;
    float hideAt;
    GUIStyle style;

    static DialogueBox Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject("DialogueBox").AddComponent<DialogueBox>();
            return instance;
        }
    }

    public static void Say(string who, string line, float seconds = 2.5f)
    {
        var box = Instance;
        box.speaker = who;
        box.text = line;
        box.hideAt = Time.time + seconds;
        LineShown?.Invoke(who, line);
    }

    /// <summary>Muestra las líneas una tras otra. Usar con yield return.</summary>
    public static IEnumerator SayAll(string who, string[] lines, float secondsEach = 2.5f)
    {
        if (lines == null) yield break;
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            Say(who, line, secondsEach);
            yield return new WaitForSeconds(secondsEach);
        }
    }

    void Update()
    {
        if (text != null && Time.time >= hideAt)
        {
            text = null;
            LineShown?.Invoke(speaker, null);
        }
    }

    void OnGUI()
    {
        if (!drawFallbackUI || text == null) return;
        if (style == null)
            style = new GUIStyle(GUI.skin.box) { fontSize = 20, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(Screen.width * 0.2f, Screen.height - 140, Screen.width * 0.6f, 90),
            speaker + ": " + text, style);
    }
}
