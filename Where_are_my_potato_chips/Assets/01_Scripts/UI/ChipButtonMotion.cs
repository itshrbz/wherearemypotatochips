using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Da respuesta visual al puntero, al foco y a la pulsación sin mover el área clicable.</summary>
public sealed class ChipButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    internal const float PressDuration = 0.085f;
    const float HoverScale = 1.035f;
    const float HoverAngle = -1.5f;
    const float PressScale = 0.949f;
    const float ResponseSpeed = 28f;

    RectTransform visual;
    Outline focusOutline;
    bool hovered;
    bool pointerDown;
    float pressUntil;
    float tilt;

    internal void Initialize(RectTransform content, Outline focus)
    {
        visual = content;
        focusOutline = focus;
    }

    internal void PlayPress() => pressUntil = Time.unscaledTime + PressDuration;

    void Update()
    {
        bool pressed = (hovered && pointerDown) || Time.unscaledTime < pressUntil;
        float scale = pressed ? PressScale : hovered ? HoverScale : 1f;
        float angle = hovered && !pressed ? HoverAngle : 0f;
        float blend = 1f - Mathf.Exp(-ResponseSpeed * Time.unscaledDeltaTime);
        visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one * scale, blend);
        tilt = Mathf.Lerp(tilt, angle, blend);
        visual.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    void OnDisable()
    {
        hovered = false;
        pointerDown = false;
        pressUntil = 0f;
        tilt = 0f;

        if (visual != null)
        {
            visual.localScale = Vector3.one;
            visual.localRotation = Quaternion.identity;
        }
        if (focusOutline != null)
            focusOutline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;
    public void OnPointerExit(PointerEventData eventData) => hovered = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            pointerDown = false;
    }

    public void OnSelect(BaseEventData eventData) => focusOutline.enabled = true;
    public void OnDeselect(BaseEventData eventData) => focusOutline.enabled = false;
}
