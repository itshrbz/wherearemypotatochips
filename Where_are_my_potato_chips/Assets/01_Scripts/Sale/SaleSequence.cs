using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Venta en el mostrador: aparece el producto, el jugador lo escanea y luego cobra en la caja.
/// Va en el mostrador. Lo usan la tienda real y las tiendas réplica.
/// </summary>
public class SaleSequence : MonoBehaviour
{
    public enum Step { Idle, WaitingScan, WaitingCharge }

    [SerializeField] Transform productSpot;
    [SerializeField] CashRegister register;

    [Header("Sonidos (opcionales)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip scanClip;
    [SerializeField] AudioClip chargeClip;

    /// <summary>Para conectar desde el Inspector.</summary>
    public UnityEvent OnSaleCompleted;

    /// <summary>Para conectar desde código. Recibe la venta que terminó.</summary>
    public event Action<SaleVariant> SaleCompleted;

    public Step CurrentStep { get; private set; } = Step.Idle;
    public SaleVariant CurrentVariant { get; private set; }

    GameObject product;

    void Awake()
    {
        if (productSpot == null) productSpot = transform;
        if (register != null) register.Bind(this);
    }

    public void Configure(Transform spot, CashRegister cashRegister)
    {
        productSpot = spot;
        register = cashRegister;
        if (register != null) register.Bind(this);
    }

    public void StartSale(SaleVariant variant)
    {
        if (CurrentStep != Step.Idle || variant == null) return;
        CurrentVariant = variant;

        product = variant.productPrefab != null
            ? Instantiate(variant.productPrefab, productSpot.position, productSpot.rotation)
            : CreatePlaceholderProduct();

        if (product.GetComponentInChildren<Collider>() == null)
            product.AddComponent<BoxCollider>();
        PlaceOnSurface(product);

        var saleProduct = product.GetComponent<SaleProduct>();
        if (saleProduct == null) saleProduct = product.AddComponent<SaleProduct>();
        saleProduct.Bind(this, variant.productName);

        CurrentStep = Step.WaitingScan;
    }

    internal void Scan()
    {
        if (CurrentStep != Step.WaitingScan) return;
        Play(scanClip);
        if (product != null) product.transform.position += productSpot.right * 0.3f;
        CurrentStep = Step.WaitingCharge;
    }

    internal void Charge()
    {
        if (CurrentStep != Step.WaitingCharge) return;
        Play(chargeClip);
        if (product != null) Destroy(product);

        var finished = CurrentVariant;
        CurrentVariant = null;
        CurrentStep = Step.Idle;

        OnSaleCompleted?.Invoke();
        SaleCompleted?.Invoke(finished);
    }

    GameObject CreatePlaceholderProduct()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Product";
        go.transform.SetPositionAndRotation(productSpot.position, productSpot.rotation);
        go.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
        go.GetComponent<Renderer>().material.color = new Color(0.85f, 0.2f, 0.15f);
        return go;
    }

    // El punto del producto suele quedar dentro del mostrador: lo apoya encima de lo que haya debajo.
    void PlaceOnSurface(GameObject item)
    {
        var renderers = item.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        float pivotAboveBottom = item.transform.position.y - bounds.min.y;

        Vector3 origin = productSpot.position + Vector3.up * 2f;
        float nearest = float.MaxValue;
        Vector3 surface = productSpot.position;
        foreach (var hit in Physics.RaycastAll(origin, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.IsChildOf(item.transform) || hit.distance >= nearest) continue;
            if (hit.collider.GetComponentInParent<PlayerState>() != null) continue;
            nearest = hit.distance;
            surface = hit.point;
        }

        Vector3 position = item.transform.position;
        position.y = surface.y + pivotAboveBottom + 0.01f;
        item.transform.position = position;
    }

    void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
