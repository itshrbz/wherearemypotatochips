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
        go.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        return go;
    }

    void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
