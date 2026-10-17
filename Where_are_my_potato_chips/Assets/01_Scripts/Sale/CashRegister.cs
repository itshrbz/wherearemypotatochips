using UnityEngine;

/// <summary>Caja registradora. Va en el modelo de la caja (con Collider) y se asigna en SaleSequence.</summary>
public class CashRegister : MonoBehaviour, IInteractable
{
    SaleSequence sale;

    public void Bind(SaleSequence owner) => sale = owner;

    public string Prompt => "Charge";
    public bool CanInteract => sale != null && sale.CurrentStep == SaleSequence.Step.WaitingCharge;
    public void Interact(GameObject interactor) => sale.Charge();
}
