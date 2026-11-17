using System.Collections;
using UnityEngine;

/// <summary>
/// Vida en la calle de la tienda: cada tanto pasa un auto cuyos faros barren la vitrina, y un peatón
/// cruza la vereda (a veces se para a mirar hacia adentro). Va en un objeto vacío de 02_Store.
/// </summary>
public class StoreLife : MonoBehaviour
{
    [Header("Autos")]
    [Tooltip("Z del carril, del otro lado de la vereda.")]
    [SerializeField] float laneZ = 20f;
    [Tooltip("El auto aparece y desaparece a esta distancia en X, fuera de la vista.")]
    [SerializeField] float roadHalfLength = 45f;
    [Tooltip("Segundos entre autos (mínimo, máximo).")]
    [SerializeField] Vector2 carInterval = new Vector2(12f, 30f);
    [SerializeField] float firstCarDelay = 6f;
    [SerializeField] float carSpeed = 11f;
    [SerializeField] Material carBodyMaterial;
    [Tooltip("Material emisivo para faros y luces traseras (se tiñe por código).")]
    [SerializeField] Material carLampMaterial;
    [SerializeField] Color headlightColor = new Color(1f, 0.9f, 0.72f);
    [SerializeField] Color taillightColor = new Color(1f, 0.08f, 0.05f);
    [SerializeField] float headlightIntensity = 9f;
    [SerializeField] float headlightRange = 28f;
    [Tooltip("Grados que los faros giran hacia la tienda para que la luz entre por la vitrina.")]
    [SerializeField] float headlightTowardStore = 22f;
    [Tooltip("Id del SfxBank. Si no existe todavía, el auto pasa en silencio.")]
    [SerializeField] string carSfx = "car_pass";

    [Header("Peatón")]
    [Tooltip("Comprador de la escena que se copia como peatón (no se toca el original).")]
    [SerializeField] GameObject pedestrianSource;
    [SerializeField] float sidewalkZ = 12.6f;
    [SerializeField] float sidewalkHalfLength = 22f;
    [SerializeField] Vector2 pedestrianInterval = new Vector2(35f, 70f);
    [SerializeField] float firstPedestrianDelay = 25f;
    [SerializeField] float pedestrianSpeed = 1.2f;
    [Tooltip("Probabilidad de que se pare frente a la vitrina a mirar hacia adentro.")]
    [SerializeField, Range(0f, 1f)] float stareChance = 0.5f;
    [SerializeField] float stareSeconds = 4f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    Transform car;
    Light headlight;
    GameObject pedestrian;

    void Start()
    {
        if (carBodyMaterial != null && carLampMaterial != null)
        {
            car = BuildCar();
            car.gameObject.SetActive(false);
            StartCoroutine(Traffic());
        }

        if (pedestrianSource != null)
        {
            pedestrian = Instantiate(pedestrianSource, transform);
            pedestrian.name = "Peaton";
            pedestrian.SetActive(false);
            StartCoroutine(Pedestrians());
        }
    }

    IEnumerator Traffic()
    {
        yield return new WaitForSeconds(firstCarDelay);
        while (true)
        {
            float dir = Random.value < 0.5f ? 1f : -1f;
            Vector3 from = new Vector3(-dir * roadHalfLength, 0f, laneZ);
            Vector3 to = new Vector3(dir * roadHalfLength, 0f, laneZ);
            car.SetPositionAndRotation(from, Quaternion.LookRotation(Vector3.right * dir));
            // Los faros miran un poco hacia la tienda (-Z) para que la luz cruce el local al pasar
            headlight.transform.localRotation = Quaternion.Euler(4f, -dir * headlightTowardStore, 0f);
            car.gameObject.SetActive(true);

            bool sounded = false;
            while ((car.position - to).sqrMagnitude > 1f)
            {
                car.position = Vector3.MoveTowards(car.position, to, carSpeed * Time.deltaTime);
                // El sonido sale cerca de la vitrina un poco antes de que el auto pase por el frente
                if (!sounded && Mathf.Abs(car.position.x) < carSpeed * 1.2f)
                {
                    sounded = true;
                    if (!string.IsNullOrEmpty(carSfx) && Sfx.Has(carSfx)) Sfx.Play(carSfx, new Vector3(0f, 0.8f, laneZ));
                }
                yield return null;
            }

            car.gameObject.SetActive(false);
            yield return new WaitForSeconds(Random.Range(carInterval.x, carInterval.y));
        }
    }

    IEnumerator Pedestrians()
    {
        yield return new WaitForSeconds(firstPedestrianDelay);
        while (true)
        {
            float dir = Random.value < 0.5f ? 1f : -1f;
            // y = 1: la raíz del comprador espera estar sobre el piso; BuyerAnimator lo apoya en la vereda
            Vector3 from = new Vector3(-dir * sidewalkHalfLength, 1f, sidewalkZ);
            Vector3 to = new Vector3(dir * sidewalkHalfLength, 1f, sidewalkZ);
            pedestrian.transform.SetPositionAndRotation(from, Quaternion.LookRotation(Vector3.right * dir));
            pedestrian.SetActive(true);

            bool stares = Random.value < stareChance;
            float stareX = Random.Range(-4f, 4f);
            yield return WalkTo(new Vector3(stares ? stareX : to.x, from.y, sidewalkZ));
            if (stares)
            {
                // Se queda quieto mirando hacia adentro, como si buscara a alguien
                for (float t = 0f; t < stareSeconds; t += Time.deltaTime)
                {
                    Turn(Vector3.back);
                    yield return null;
                }
                yield return WalkTo(to);
            }

            pedestrian.SetActive(false);
            yield return new WaitForSeconds(Random.Range(pedestrianInterval.x, pedestrianInterval.y));
        }
    }

    IEnumerator WalkTo(Vector3 target)
    {
        var body = pedestrian.transform;
        target.y = body.position.y;
        while ((body.position - target).sqrMagnitude > 0.01f)
        {
            Vector3 dir = target - body.position;
            dir.y = 0f;
            Turn(dir);
            body.position = Vector3.MoveTowards(body.position, target, pedestrianSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void Turn(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        var body = pedestrian.transform;
        body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
    }

    // Auto armado con cajas: de noche y detrás del vidrio solo se lee la silueta y las luces
    Transform BuildCar()
    {
        var root = new GameObject("Auto").transform;
        root.SetParent(transform, false);

        Box(root, "Carroceria", new Vector3(0f, 0.55f, 0f), new Vector3(1.8f, 0.7f, 4.2f), carBodyMaterial, null);
        Box(root, "Cabina", new Vector3(0f, 1.15f, -0.3f), new Vector3(1.6f, 0.55f, 2.2f), carBodyMaterial, null);
        var lamp = new MaterialPropertyBlock();
        lamp.SetColor(BaseColorId, headlightColor);
        lamp.SetColor(EmissionId, headlightColor * 3f);
        Box(root, "FaroIzq", new Vector3(-0.65f, 0.6f, 2.1f), new Vector3(0.35f, 0.15f, 0.05f), carLampMaterial, lamp);
        Box(root, "FaroDer", new Vector3(0.65f, 0.6f, 2.1f), new Vector3(0.35f, 0.15f, 0.05f), carLampMaterial, lamp);
        var tail = new MaterialPropertyBlock();
        tail.SetColor(BaseColorId, taillightColor);
        tail.SetColor(EmissionId, taillightColor * 2f);
        Box(root, "TraseraIzq", new Vector3(-0.7f, 0.65f, -2.1f), new Vector3(0.3f, 0.12f, 0.05f), carLampMaterial, tail);
        Box(root, "TraseraDer", new Vector3(0.7f, 0.65f, -2.1f), new Vector3(0.3f, 0.12f, 0.05f), carLampMaterial, tail);

        // Una sola luz para los dos faros: la mitad de costo en WebGL y no se nota la diferencia
        var lightGo = new GameObject("Faros");
        lightGo.transform.SetParent(root, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.7f, 2.2f);
        headlight = lightGo.AddComponent<Light>();
        headlight.type = LightType.Spot;
        headlight.color = headlightColor;
        headlight.intensity = headlightIntensity;
        headlight.range = headlightRange;
        headlight.spotAngle = 70f;
        headlight.innerSpotAngle = 30f;
        headlight.shadows = LightShadows.None;
        return root;
    }

    static void Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, MaterialPropertyBlock block)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = material;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (block != null) r.SetPropertyBlock(block);
    }
}
