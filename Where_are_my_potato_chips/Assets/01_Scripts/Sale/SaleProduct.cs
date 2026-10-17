using UnityEngine;

/// <summary>Producto en el mostrador. SaleSequence lo agrega solo; no hace falta ponerlo a mano.</summary>
public class SaleProduct : MonoBehaviour, IInteractable
{
    SaleSequence sale;
    string productName = "item";

    public void Bind(SaleSequence owner, string name)
    {
        sale = owner;
        if (!string.IsNullOrEmpty(name)) productName = name;
    }

    public string Prompt => "Scan " + productName;
    public bool CanInteract => sale != null && sale.CurrentStep == SaleSequence.Step.WaitingScan;
    public void Interact(GameObject interactor) => sale.Scan();
}
