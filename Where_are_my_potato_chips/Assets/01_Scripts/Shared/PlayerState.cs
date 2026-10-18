using UnityEngine;

/// <summary>
/// Estado compartido del jugador. Va en el mismo GameObject que el PlayerController.
/// - El controlador del jugador escribe IsCrouching y respeta InputLocked.
/// - HidingSpot escribe IsHidden.
/// - El jefe lee IsHidden e IsCrouching.
/// </summary>
public class PlayerState : MonoBehaviour
{
    public bool IsCrouching { get; set; }
    public bool IsHidden { get; set; }
    public bool InHidingSpot { get; set; }

    /// <summary>True dentro de una tienda réplica: el jefe no puede atraparlo.</summary>
    public bool IsSafe { get; set; }

    /// <summary>True durante transiciones o escenas en las que el jugador no debe moverse.</summary>
    public bool InputLocked { get; set; }
}
