using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Primer susto de la partida, sin el jefe. Cuando el cliente raro pide papas, la tienda parpadea y se apaga;
/// al volver la luz el cliente está pegado al jugador, mirándolo fijo, y después vuelve al mostrador.
/// Si el pedido tarda, la tienda parpadea igual antes de los 3 minutos. Solo en la primera visita a 02_Store.
/// </summary>
public class StoreScare : MonoBehaviour
{
    [SerializeField] BuyerQueue queue;
    [Tooltip("Segundos desde que carga la tienda: si el cliente raro todavía no pidió papas, el apagón pasa igual.")]
    [SerializeField] float fallbackSeconds = 140f;
    [SerializeField] float delayAfterRequest = 0.6f;
    [SerializeField] float flickerSeconds = 1.4f;
    [SerializeField] float darkSeconds = 1.1f;
    [SerializeField] float stareSeconds = 1.8f;
    [Tooltip("A cuántos metros del jugador aparece el cliente al volver la luz.")]
    [SerializeField] float stareDistance = 0.85f;
    [Tooltip("Ids del SfxBank. El de susto todavía no existe: pedido a Alan (scare_sting).")]
    [SerializeField] string flickerSfx = "bulb_burst";
    [SerializeField] string stingSfx = "scare_sting";

    readonly List<Light> lights = new List<Light>();
    readonly List<float> intensities = new List<float>();
    readonly List<LightFlicker> flickers = new List<LightFlicker>();
    Transform player;
    bool blackoutDone;
    bool stareDone;
    bool running;

    void Start()
    {
        if (LevelFlowManager.IsEnding) { enabled = false; return; }
        if (queue == null) queue = FindAnyObjectByType<BuyerQueue>();
        if (queue != null) queue.OnPotatoRequest.AddListener(OnPotatoRequest);
        StartCoroutine(Fallback());
    }

    void OnDestroy()
    {
        if (queue != null) queue.OnPotatoRequest.RemoveListener(OnPotatoRequest);
    }

    void OnPotatoRequest()
    {
        if (stareDone || running) return;
        StartCoroutine(Scare(queue.CurrentBody, delayAfterRequest));
    }

    IEnumerator Fallback()
    {
        yield return new WaitForSeconds(fallbackSeconds);
        if (!blackoutDone && !running) yield return Scare(null, 0f);
    }

    IEnumerator Scare(GameObject buyer, float delay)
    {
        running = true;
        yield return new WaitForSeconds(delay);
        CollectLights();
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 1. Los tubos fallan todos juntos
        Play(flickerSfx);
        for (float t = 0f; t < flickerSeconds; t += Time.deltaTime)
        {
            SetLights(Mathf.PerlinNoise(t * 18f, 0.5f) > 0.45f ? 1f : 0.08f);
            yield return null;
        }

        // 2. Apagón. Mientras no se ve nada, el cliente se acerca
        SetLights(0f);
        Vector3 buyerStart = Vector3.zero;
        bool moved = buyer != null && player != null && PlaceInFront(buyer.transform, out buyerStart);
        yield return new WaitForSeconds(darkSeconds);

        // 3. Vuelve la luz de golpe: si el cliente se movió, está ahí mirando
        SetLights(1f);
        if (moved)
        {
            Play(stingSfx, flickerSfx);
            CameraEffects.Shake(2.2f, 0.5f);
            ScreenEffects.Flash(new Color(0f, 0f, 0f, 0.6f), 0.25f);
            for (float t = 0f; t < stareSeconds; t += Time.deltaTime)
            {
                Face(buyer.transform);
                yield return null;
            }

            // Un parpadeo corto y vuelve a su lugar, como si nada
            SetLights(0.05f);
            yield return new WaitForSeconds(0.15f);
            Teleport(buyer.transform, buyerStart);
            SetLights(1f);
            stareDone = true;
        }

        RestoreLights();
        blackoutDone = true;
        running = false;
    }

    // Pone al cliente frente al jugador, sin meterlo en una pared
    bool PlaceInFront(Transform buyer, out Vector3 previous)
    {
        previous = buyer.position;
        // Hacia donde mira la cámara, no el cuerpo: tiene que aparecer justo en el centro de la vista
        var view = Camera.main != null ? Camera.main.transform : player;
        Vector3 forward = view.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f) return false;
        forward.Normalize();

        float distance = stareDistance;
        Vector3 eye = player.position + Vector3.up * 1.2f;
        if (Physics.Raycast(eye, forward, out var hit, stareDistance + 0.3f, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponentInParent<BuyerAnimator>() == null)
            distance = Mathf.Max(0.45f, hit.distance - 0.3f);

        Vector3 target = player.position + forward * distance;
        target.y = previous.y;
        Teleport(buyer, target);
        buyer.rotation = Quaternion.LookRotation(-forward);
        return true;
    }

    // Apagar y prender el cuerpo reinicia BuyerAnimator: si no, toma el salto como una caminata
    static void Teleport(Transform buyer, Vector3 position)
    {
        buyer.gameObject.SetActive(false);
        buyer.position = position;
        buyer.gameObject.SetActive(true);
    }

    void Face(Transform buyer)
    {
        Vector3 dir = player.position - buyer.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) buyer.rotation = Quaternion.LookRotation(dir);
    }

    void CollectLights()
    {
        lights.Clear();
        intensities.Clear();
        flickers.Clear();
        foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
        {
            // El sol, los faros de los autos y las luces de la UI no cuentan
            if (l.type == LightType.Directional || l.GetComponentInParent<StoreLife>() != null) continue;
            lights.Add(l);
            intensities.Add(l.intensity);
        }
        // LightFlicker reescribe la intensidad cada frame: se pausa durante el susto
        foreach (var f in FindObjectsByType<LightFlicker>(FindObjectsInactive.Exclude))
            if (f.enabled) { f.enabled = false; flickers.Add(f); }
    }

    void SetLights(float level)
    {
        for (int i = 0; i < lights.Count; i++)
            if (lights[i] != null) lights[i].intensity = intensities[i] * level;
    }

    void RestoreLights()
    {
        SetLights(1f);
        foreach (var f in flickers) if (f != null) f.enabled = true;
    }

    static void Play(string id, string fallback = null)
    {
        if (!string.IsNullOrEmpty(id) && Sfx.Has(id)) Sfx.Play(id);
        else if (!string.IsNullOrEmpty(fallback) && Sfx.Has(fallback)) Sfx.Play(fallback);
    }
}
