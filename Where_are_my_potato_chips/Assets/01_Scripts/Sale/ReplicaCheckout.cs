using System.Collections;
using UnityEngine;

/// <summary>
/// Va en la raíz del prefab ReplicaStore. Al entrar, el jugador queda a salvo del jefe,
/// aparece el comprador de las papas y la venta lleva al siguiente espacio.
/// Si no se asigna nada, arma el mostrador y la caja a partir de los objetos del kit.
/// </summary>
public class ReplicaCheckout : MonoBehaviour
{
    [SerializeField] SaleSequence sale;
    [SerializeField] Transform buyerSpot;
    [SerializeField] SaleVariant fallbackVariant;
    [SerializeField] float zonePadding = 0.3f;

    Bounds zone;
    PlayerState player;
    bool served;

    void Start()
    {
        zone = CalculateBounds();
        zone.Expand(zonePadding * 2f);
        if (sale == null) sale = GetComponentInChildren<SaleSequence>();
        if (sale == null) sale = BuildSale();
    }

    void Update()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerState>();
            if (player == null) return;
        }

        bool inside = zone.Contains(player.transform.position);
        player.IsSafe = inside;

        if (inside && !served) StartCoroutine(Serve());
    }

    void OnDisable()
    {
        if (player != null) player.IsSafe = false;
    }

    IEnumerator Serve()
    {
        served = true;
        SaleVariant variant = PickVariant();
        Transform buyer = SpawnBuyer();

        yield return DialogueBox.SayAll(variant.buyerName, variant.greetingLines);

        if (sale != null)
        {
            bool done = false;
            void OnSaleDone(SaleVariant _) => done = true;

            sale.SaleCompleted += OnSaleDone;
            sale.StartSale(variant);
            while (!done)
            {
                Face(buyer, player.transform.position);
                yield return null;
            }
            sale.SaleCompleted -= OnSaleDone;
        }

        yield return DialogueBox.SayAll(variant.buyerName, variant.afterSaleLines);
        LevelFlowManager.GoToNextLevel();
    }

    SaleVariant PickVariant()
    {
        var variants = LevelFlowManager.Settings.replicaVariants;
        int level = LevelFlowManager.CurrentLevel;
        if (variants != null && level >= 0 && level < variants.Length && variants[level] != null)
            return variants[level];
        if (fallbackVariant != null) return fallbackVariant;

        var generic = ScriptableObject.CreateInstance<SaleVariant>();
        generic.buyerName = "???";
        generic.greetingLines = new[] { "Potatos." };
        generic.afterSaleLines = new[] { "These are not my potatos." };
        return generic;
    }

    Transform SpawnBuyer()
    {
        Vector3 position;
        if (buyerSpot != null)
        {
            position = buyerSpot.position;
        }
        else
        {
            // Del lado contrario del mostrador respecto al jugador
            Vector3 counter = sale != null ? sale.transform.position : zone.center;
            Vector3 away = counter - player.transform.position;
            away.y = 0f;
            position = counter + away.normalized * 0.9f;
            position.y = zone.min.y + 1f;
        }

        GameObject prefab = LevelFlowManager.Settings.buyerPrefab;
        GameObject buyer;
        if (prefab != null)
        {
            buyer = Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            buyer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            buyer.transform.position = position;
            Destroy(buyer.GetComponent<Collider>());
        }

        buyer.name = "ReplicaBuyer";
        Face(buyer.transform, player.transform.position);
        return buyer.transform;
    }

    SaleSequence BuildSale()
    {
        Transform counter = FindChildContaining("mostrador");
        if (counter == null) counter = FindChildContaining("counter");
        if (counter == null)
        {
            Debug.LogWarning("ReplicaCheckout: no encontré el mostrador; la réplica avanza sin venta.", this);
            return null;
        }

        Bounds counterBounds = RendererBounds(counter);
        var spot = new GameObject("ProductSpot").transform;
        spot.SetParent(counter, true);
        spot.position = new Vector3(counterBounds.center.x, counterBounds.max.y + 0.05f, counterBounds.center.z);

        Transform registerObject = FindChildContaining("caja");
        if (registerObject == null)
        {
            registerObject = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            registerObject.name = "Register";
            registerObject.SetParent(counter, true);
            registerObject.localScale = new Vector3(0.4f, 0.3f, 0.3f);
            registerObject.position = spot.position + counter.right * 0.6f + Vector3.up * 0.15f;
        }
        if (registerObject.GetComponentInChildren<Collider>() == null) registerObject.gameObject.AddComponent<BoxCollider>();

        var register = registerObject.GetComponent<CashRegister>();
        if (register == null) register = registerObject.gameObject.AddComponent<CashRegister>();

        var newSale = counter.gameObject.AddComponent<SaleSequence>();
        newSale.Configure(spot, register);
        return newSale;
    }

    Transform FindChildContaining(string text)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            if (child != transform && child.name.ToLowerInvariant().Contains(text)) return child;
        return null;
    }

    Bounds CalculateBounds() => RendererBounds(transform);

    static Bounds RendererBounds(Transform root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(root.position, Vector3.one * 4f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static void Face(Transform body, Vector3 target)
    {
        if (body == null) return;
        Vector3 direction = target - body.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f) body.rotation = Quaternion.LookRotation(direction);
    }
}
