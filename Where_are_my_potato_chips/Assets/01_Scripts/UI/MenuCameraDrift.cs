using UnityEngine;

/// <summary>
/// Avance lento de la cámara por el pasillo del menú. El pasillo se repite cada <see cref="loopLength"/> metros,
/// así que al llegar al final del tramo la cámara vuelve atrás y parece un pasillo sin fin.
/// </summary>
public class MenuCameraDrift : MonoBehaviour
{
    [Tooltip("Metros por segundo hacia adelante.")]
    [SerializeField] float speed = 0.55f;
    [Tooltip("Largo del patrón que se repite en el pasillo (debe coincidir con el fondo del menú).")]
    [SerializeField] float loopLength = 8f;
    [Tooltip("Grados de balanceo, como una cámara en mano.")]
    [SerializeField] float swayDegrees = 1.2f;
    [SerializeField] float swaySpeed = 0.25f;

    Vector3 start;
    Quaternion baseRotation;
    float travelled;

    void Awake()
    {
        start = transform.position;
        baseRotation = transform.rotation;
    }

    void Update()
    {
        travelled = Mathf.Repeat(travelled + speed * Time.unscaledDeltaTime, loopLength);
        transform.position = start + baseRotation * Vector3.forward * travelled;

        // El balanceo puede marear; se apaga con la opción de efectos de cámara
        if (!GameSettings.CameraEffects)
        {
            transform.rotation = baseRotation;
            return;
        }
        float t = Time.unscaledTime * swaySpeed;
        float yaw = (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * 2f * swayDegrees;
        float pitch = (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f * swayDegrees * 0.5f;
        transform.rotation = baseRotation * Quaternion.Euler(pitch, yaw, 0f);
    }
}
