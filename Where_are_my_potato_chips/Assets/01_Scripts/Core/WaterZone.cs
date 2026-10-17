using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    public static bool PlayerInWater { get; private set; }
    public static event Action<Vector3> PlayerEntered, PlayerExited;

    static int count;
    bool inside;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { count = 0; PlayerInWater = false; PlayerEntered = null; PlayerExited = null; }

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider o)
    {
        if (inside || !o.CompareTag("Player")) return;
        inside = true; count++; PlayerInWater = true;
        PlayerEntered?.Invoke(o.transform.position);
    }

    void OnTriggerExit(Collider o)
    {
        if (!inside || !o.CompareTag("Player")) return;
        Leave(o.transform.position);
    }

    void OnDisable() { if (inside) Leave(transform.position); }

    void Leave(Vector3 p)
    {
        inside = false; count = Mathf.Max(0, count - 1);
        PlayerInWater = count > 0;
        PlayerExited?.Invoke(p);
    }
}