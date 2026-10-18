using UnityEngine;

/// <summary>
/// Zona de escondite. Si el jugador está dentro del trigger y agachado,
/// PlayerState.IsHidden pasa a true y el jefe deja de verlo.
/// Requiere un Collider marcado como trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HidingSpot : MonoBehaviour
{
    PlayerState inside;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        int layer = LayerMask.NameToLayer("HidingSpot");
        if (layer >= 0) gameObject.layer = layer;
    }

    void OnTriggerEnter(Collider other)
    {
        var s = other.GetComponentInParent<PlayerState>();
        if (s == null) return;
        inside = s;
        inside.InHidingSpot = true;
    }

    void OnTriggerExit(Collider other)
    {
        var s = other.GetComponentInParent<PlayerState>();
        if (s != null && s == inside)
        {
            s.IsHidden = false;
            s.InHidingSpot = false;
            inside = null;
        }
    }

    void Update()
    {
        if (inside != null) inside.IsHidden = inside.IsCrouching;
    }

    void OnDisable()
    {
        if (inside != null)
        {
            inside.IsHidden = false;
            inside.InHidingSpot = false;
        }
        inside = null;
    }
}
