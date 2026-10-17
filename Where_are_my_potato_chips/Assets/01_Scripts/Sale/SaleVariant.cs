using UnityEngine;

/// <summary>
/// Datos de una venta: quién compra, qué dice y qué producto deja en el mostrador.
/// Crear con clic derecho > Create > Potato Chips > Sale Variant.
/// </summary>
[CreateAssetMenu(menuName = "Potato Chips/Sale Variant", fileName = "SaleVariant")]
public class SaleVariant : ScriptableObject
{
    public string buyerName = "Customer";

    [TextArea] public string[] greetingLines = { "Hi. Just this, please." };

    [Tooltip("Si está vacío se crea un cubo como producto.")]
    public GameObject productPrefab;
    public string productName = "Soda";

    [TextArea] public string[] afterSaleLines = { "Thanks." };

    [Header("Comprador raro (\"potatos\")")]
    [Tooltip("Si está activo no hay venta: el comprador pide papas y se dispara OnPotatoRequest.")]
    public bool requestsPotatoes;
    [TextArea] public string[] potatoLines =
    {
        "Potatos.",
        "...You don't have them here.",
        "They are in the back. In the warehouse."
    };
}
