using UnityEngine;

[CreateAssetMenu(menuName = "Potato Chips/Game Flow Settings", fileName = "GameFlowSettings")]
public class GameFlowSettings : ScriptableObject
{
    [Header("Escenas")]
    public string menuScene = "01_Menu";
    public string storeScene = "02_Store";
    public string[] liminalScenes = { "10_Warehouse", "11_Pools", "12_Playground" };
    [Tooltip("Qué espacio de la partida simula el juego al darle Play directo a un nivel (1 = ya aparece el jefe).")]
    public int directPlayLevelIndex = 1;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject hudPrefab;
    public GameObject bossPrefab;
    public GameObject buyerPrefab;
    public GameObject replicaStorePrefab;

    [Header("Tiendas réplica (una por espacio)")]
    public SaleVariant[] replicaVariants;

    [Header("Transiciones")]
    public float fadeDuration = 0.6f;
    public float minTicketTime = 4f;
    [TextArea] public string[] ticketMessages =
    {
        "ITEM NOT FOUND: POTATO CHIPS\nREALITY: OUT OF STOCK",
        "AISLE 7 IS STILL NOT\nTHE SNACK AISLE",
        "YOU CAN SMELL SALT AND VINEGAR.\nALMOST HOME. ALMOST."
    };
    [TextArea(4, 10)] public string credits = "WHERE ARE MY POTATO CHIPS?";

    [Header("Audio")]
    public AudioClip tensionMusic;
    public AudioClip printerClip;
    public AudioClip caughtClip;
    public AudioClip registerClip;

    static GameFlowSettings cached;

    public static GameFlowSettings Load()
    {
        if (cached == null) cached = Resources.Load<GameFlowSettings>("GameFlowSettings");
        if (cached == null) cached = CreateInstance<GameFlowSettings>();
        return cached;
    }
}
