using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // Aseguramos que siempre tenga un collider
public class WorldItem : MonoBehaviour
{
    [Header("Id del objeto")]
    [SerializeField] private int idItem = 0;
    // --- NUEVO: Campos para configurar el comportamiento en el Inspector ---
    [Header("Efecto de Flote")]
    [SerializeField] private float hoverSpeed = 2f;    // Velocidad del movimiento vertical
    [SerializeField] private float hoverHeight = 0.15f; // Amplitud del movimiento

    [Header("Efecto al Recoger")]
    [SerializeField] private GameObject pickupEffectPrefab; // Prefab de partículas a instanciar

    // --- Variables internas del script ---
    private ItemProperties properties;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialPosition; // Posición inicial para calcular el flote
    private int itemID; // Guardaremos el ID del ítem para darlo al inventario

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Guardamos la posición inicial en el primer frame.
        initialPosition = transform.position;
        Initialize(idItem);
    }

    // --- NUEVO: Lógica del movimiento en Update ---
    void Update()
    {
        // Calculamos la nueva posición Y usando una onda sinusoidal para un movimiento suave.
        float newY = initialPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

        // Aplicamos la nueva posición. Mantenemos la X y Z originales.
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    /// <summary>
    /// Esta es la función clave. La llamaremos desde fuera para inicializar el ítem.
    /// </summary>
    public void Initialize(int itemID)
    {
        this.itemID = itemID;

        GameData data = DataManager.Instance.LoadGame();
        
        // --- CORRECCIÓN DE LÓGICA ---
        // 1. Revisa si el DataManager (o el PlayerStatsManager) ya tiene este ID
        // Usamos PlayerStatsManager.Instance.fruits que está en memoria
        if (PlayerStatsManager.Instance != null && PlayerStatsManager.Instance.fruits.Contains(this.itemID))
        {
            Debug.Log($"El ítem con ID {this.itemID} ya fue recogido (según StatsManager). Desactivando.");
            gameObject.SetActive(false);
            return;
        }
        
        // 2. ¡NO AÑADIR LA FRUTA AQUÍ!
        // PlayerStatsManager.Instance.AddFruit(this.itemID); // <-- ELIMINA ESTA LÍNEA DE AQUÍ

        // 3. El resto de la inicialización está bien
        ItemBlueprint blueprint = ItemFactory.GetBlueprint(this.itemID);
        if (blueprint == null)
        {
            Debug.LogError($"No se pudo inicializar WorldItem porque el ID {this.itemID} no es válido.");
            gameObject.SetActive(false);
            return;
        }

        spriteRenderer.sprite = ItemFactory.GetSpriteForItem(this.itemID);

        if (!TryGetComponent<ItemProperties>(out properties))
        {
            properties = gameObject.AddComponent<ItemProperties>();
        }

        properties.itemName = blueprint.itemName;
        properties.healthRestore = blueprint.healthToRestore;
        properties.ability = blueprint.abilityGranted;

        gameObject.name = blueprint.itemName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController controller))
            {
                // 1. Añade al inventario actual (para el carrusel)
                controller.Inventory.AddItem(this.itemID);

                // 2. --- AÑADIR ESTA LÍNEA AQUÍ ---
                // Añade a la lista de items ÚNICOS (para que no vuelva a aparecer)
                // Esto también llamará a SaveStats()
                PlayerStatsManager.Instance.AddFruit(this.itemID);

                // 3. (Feedback)
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }

                // 4. (Limpieza)
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("El objeto con tag 'Player' no tiene un componente inventory.");
            }
        }
    }
}