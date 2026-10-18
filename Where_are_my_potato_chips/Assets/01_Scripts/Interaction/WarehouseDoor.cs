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

        if (ceilingLight == null) ceilingLight = FindAnyObjectByType<Light>();
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

        if (ceilingLight != null)
        {
            float intensity = ceilingLight.intensity;
            for (int i = 0; i < 7; i++)
            {
                ceilingLight.intensity = i % 2 == 0 ? intensity * 0.1f : intensity;
                yield return new WaitForSeconds(Random.Range(0.04f, 0.16f));
            }
            Play(bulbPopClip);
            ceilingLight.enabled = false;
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

    void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
