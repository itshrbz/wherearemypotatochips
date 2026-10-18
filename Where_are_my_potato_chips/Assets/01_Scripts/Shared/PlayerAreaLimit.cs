using UnityEngine;

/// <summary>
/// Mantiene al jugador dentro de un área en el plano XZ. Se usa en la tienda real para que no salga por la entrada.
/// Solo afecta al jugador: los compradores se mueven por transform y pasan igual.
/// </summary>
public class PlayerAreaLimit : MonoBehaviour
{
    const float Margin = 0.6f;

    Bounds area;
    CharacterController body;

    /// <summary>Limita al jugador al interior de la tienda generada, tomando el techo como referencia.</summary>
    public static void ForStore()
    {
        var ceiling = GameObject.Find("Tienda_Generada/PisoYTecho/Techo") ?? FindByName("Techo");
        if (ceiling == null || !ceiling.TryGetComponent(out Renderer shape))
        {
            Debug.LogWarning("PlayerAreaLimit: no se encontró el techo de la tienda; el jugador no queda limitado.");
            return;
        }

        Bounds bounds = shape.bounds;
        bounds.Expand(new Vector3(-Margin * 2f, 0f, -Margin * 2f));
        new GameObject("PlayerAreaLimit").AddComponent<PlayerAreaLimit>().area = bounds;
    }

    void LateUpdate()
    {
        if (body == null)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) return;
            body = player.GetComponent<CharacterController>();
        }

        Vector3 position = body.transform.position;
        Vector3 inside = new Vector3(
            Mathf.Clamp(position.x, area.min.x, area.max.x),
            position.y,
            Mathf.Clamp(position.z, area.min.z, area.max.z));

        if ((inside - position).sqrMagnitude > 0.000001f && body.enabled) body.Move(inside - position);
    }

    static GameObject FindByName(string objectName)
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.name == objectName) return t.gameObject;
        return null;
    }
}
