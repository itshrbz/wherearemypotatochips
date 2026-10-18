using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Puerta del almacén en la tienda real. Se habilita cuando el tercer comprador pide las papas
/// y al usarla hace la transición encubierta hacia el primer espacio liminal.
/// </summary>
public class WarehouseDoor : MonoBehaviour, IInteractable
{
    [SerializeField] Light ceilingLight;
    [SerializeField] Transform doorLeaf;
    [SerializeField] Vector3 closedLocalEuler;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip bulbPopClip;
    [SerializeField] AudioClip doorSlamClip;
    [SerializeField] bool unlockedFromStart;

    bool unlocked;
    bool used;

    public string Prompt => "Look for potato chips";
    public bool CanInteract => enabled && unlocked && !used;

    void Start()
    {
        // El prefab de la puerta también se usa como decoración en los niveles
        bool inStore = SceneManager.GetActiveScene().name == LevelFlowManager.Settings.storeScene;
        if (!inStore || LevelFlowManager.IsEnding)
        {
            enabled = false;
            return;
        }

        unlocked = unlockedFromStart;
        var queue = FindAnyObjectByType<BuyerQueue>();
        if (queue != null) queue.OnPotatoRequest.AddListener(Unlock);
        else unlocked = true;
    }

    public void Unlock() => unlocked = true;

    public void Interact(GameObject interactor)
    {
        used = true;
        StartCoroutine(EnterWarehouse(interactor.GetComponent<PlayerState>()));
    }

    IEnumerator EnterWarehouse(PlayerState player)
    {
        if (player != null) player.InputLocked = true;

        Light[] lights = ceilingLight != null ? new[] { ceilingLight } : SceneLights();
        if (lights.Length > 0)
        {
            float[] intensities = new float[lights.Length];
            for (int l = 0; l < lights.Length; l++)
            {
                intensities[l] = lights[l].intensity;
                if (lights[l].TryGetComponent(out LightFlicker flicker)) flicker.enabled = false;
            }

            for (int i = 0; i < 7; i++)
            {
                for (int l = 0; l < lights.Length; l++)
                    lights[l].intensity = i % 2 == 0 ? intensities[l] * 0.1f : intensities[l];
                yield return new WaitForSeconds(Random.Range(0.04f, 0.16f));
            }
            Play(bulbPopClip);
            foreach (var lamp in lights) lamp.enabled = false;
        }

        if (doorLeaf != null)
        {
            Play(doorSlamClip);
            Quaternion from = doorLeaf.localRotation;
            Quaternion to = Quaternion.Euler(closedLocalEuler);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.15f)
            {
                doorLeaf.localRotation = Quaternion.Slerp(from, to, t);
                yield return null;
            }
            doorLeaf.localRotation = to;
        }

        yield return new WaitForSeconds(0.6f);
        LevelFlowManager.StartRun();
    }

    // Las luces interiores de la tienda (todas menos la del sol), para el apagón al abrir la puerta
    static Light[] SceneLights()
    {
        var found = new System.Collections.Generic.List<Light>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var lamp in root.GetComponentsInChildren<Light>())
                if (lamp.type != LightType.Directional) found.Add(lamp);
        return found.ToArray();
    }

    void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
