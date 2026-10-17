using UnityEngine;

/// <summary>
/// Cualquier objeto con el que el jugador puede interactuar con E.
/// Necesita un Collider que NO sea trigger para que el raycast lo encuentre.
/// </summary>
public interface IInteractable
{
    /// <summary>Texto que se muestra al apuntarlo, por ejemplo "Scan" o "Charge".</summary>
    string Prompt { get; }

    /// <summary>Si es false, el objeto no se resalta ni responde.</summary>
    bool CanInteract { get; }

    void Interact(GameObject interactor);
}
