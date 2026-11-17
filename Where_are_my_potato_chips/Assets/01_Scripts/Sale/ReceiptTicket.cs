using UnityEngine;

/// <summary>
/// Ticket de papel que sale de la caja al cobrar: se imprime, queda un momento y se arranca solo.
/// Se crea por código desde SaleSequence; no hace falta ponerlo en la escena.
/// </summary>
public class ReceiptTicket : MonoBehaviour
{
    const float PrintTime = 0.55f;
    const float HoldTime = 1.4f;
    const float TearTime = 0.25f;
    const float Length = 0.16f;

    Vector3 start;
    Vector3 dir;
    float time;

    /// <summary>Imprime un ticket desde lo alto de la caja.</summary>
    public static void Print(Transform register)
    {
        if (register == null) return;
        Vector3 top = register.position;
        var rend = register.GetComponentInChildren<Renderer>();
        if (rend != null) top = new Vector3(rend.bounds.center.x, rend.bounds.max.y, rend.bounds.center.z);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Ticket";
        Destroy(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().material.color = new Color(0.95f, 0.93f, 0.86f);
        // Sale casi vertical, inclinado hacia el jugador; la cara del papel mira al frente de la caja
        Vector3 dir = (register.forward * 0.3f + Vector3.up).normalized;
        go.transform.rotation = Quaternion.LookRotation(dir, register.forward);
        go.transform.localScale = new Vector3(0.075f, 0.002f, 0.002f);

        var t = go.AddComponent<ReceiptTicket>();
        t.start = top;
        t.dir = dir;
        go.transform.position = top;
    }

    void Update()
    {
        time += Time.deltaTime;
        // Sale a tirones, como una impresora térmica
        float printed = Mathf.Clamp01(time / PrintTime);
        float stepped = Mathf.Floor(printed * 7f) / 7f;
        float length = Mathf.Lerp(0.002f, Length, stepped);

        transform.localScale = new Vector3(0.075f, 0.002f, length);
        transform.position = start + dir * length * 0.5f;

        float tear = time - PrintTime - HoldTime;
        if (tear > 0f)
        {
            float k = tear / TearTime;
            transform.position += Vector3.up * k * 0.12f;
            transform.localScale *= 1f - k;
            if (k >= 1f) Destroy(gameObject);
        }
    }
}
