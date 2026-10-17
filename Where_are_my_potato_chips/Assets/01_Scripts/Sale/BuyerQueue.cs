using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hace pasar a los compradores uno tras otro: entran, caminan al mostrador, hablan,
/// esperan la venta y se van. Si un comprador tiene requestsPotatoes, no hay venta:
/// pide papas, se queda mirando y se dispara OnPotatoRequest (abrir la puerta del almacén).
/// En la tienda real lleva 3 compradores; en cada réplica, 1.
/// </summary>
public class BuyerQueue : MonoBehaviour
{
    [Serializable]
    public class Buyer
    {
        [Tooltip("Objeto del comprador ya puesto en la escena (puede estar desactivado).")]
        public GameObject body;
        public SaleVariant variant;
    }

    [SerializeField] List<Buyer> buyers = new List<Buyer>();
    [SerializeField] SaleSequence sale;
    [SerializeField] Transform entrance;
    [SerializeField] Transform counterSpot;
    [SerializeField] Transform exitSpot;
    [SerializeField] float walkSpeed = 1.6f;
    [SerializeField] float startDelay = 2f;
    [SerializeField] float delayBetweenBuyers = 2f;
    [Tooltip("Si está vacío, busca el objeto con tag Player.")]
    [SerializeField] Transform player;

    /// <summary>El comprador raro pidió papas. Conectar aquí la puerta del almacén.</summary>
    public UnityEvent OnPotatoRequest;
    public UnityEvent OnAllBuyersServed;

    public int CurrentIndex { get; private set; } = -1;

    IEnumerator Start()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        foreach (var b in buyers)
            if (b.body != null) b.body.SetActive(false);

        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < buyers.Count; i++)
        {
            CurrentIndex = i;
            var b = buyers[i];
            if (b.body == null || b.variant == null) continue;

            b.body.transform.position = entrance.position;
            b.body.SetActive(true);
            yield return WalkTo(b.body.transform, counterSpot.position);
            FacePlayer(b.body.transform);

            yield return DialogueBox.SayAll(b.variant.buyerName, b.variant.greetingLines);

            if (b.variant.requestsPotatoes)
            {
                yield return DialogueBox.SayAll(b.variant.buyerName, b.variant.potatoLines);
                OnPotatoRequest?.Invoke();
                // Se queda en el mostrador mirando al jugador.
                while (true)
                {
                    FacePlayer(b.body.transform);
                    yield return null;
                }
            }

            bool done = false;
            Action<SaleVariant> onDone = _ => done = true;
            sale.SaleCompleted += onDone;
            sale.StartSale(b.variant);
            while (!done)
            {
                FacePlayer(b.body.transform);
                yield return null;
            }
            sale.SaleCompleted -= onDone;

            yield return DialogueBox.SayAll(b.variant.buyerName, b.variant.afterSaleLines);

            if (exitSpot != null) yield return WalkTo(b.body.transform, exitSpot.position);
            b.body.SetActive(false);
            yield return new WaitForSeconds(delayBetweenBuyers);
        }

        OnAllBuyersServed?.Invoke();
    }

    IEnumerator WalkTo(Transform body, Vector3 target)
    {
        target.y = body.position.y;
        while ((body.position - target).sqrMagnitude > 0.01f)
        {
            Vector3 dir = target - body.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
            body.position = Vector3.MoveTowards(body.position, target, walkSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void FacePlayer(Transform body)
    {
        if (player == null) return;
        Vector3 dir = player.position - body.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
    }
}
