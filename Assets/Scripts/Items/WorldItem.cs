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
        Debug.Log("HOla");
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
        // --- MODIFICADO: Guardamos el ID para usarlo después ---
        this.itemID = itemID; 

        ItemBlueprint blueprint = ItemFactory.GetBlueprint(this.itemID);
        if (blueprint == null) 
        {
            Debug.LogError($"No se pudo inicializar WorldItem porque el ID {this.itemID} no es válido.");
            gameObject.SetActive(false);
            return;
        }

        spriteRenderer.sprite = ItemFactory.GetSpriteForItem(this.itemID);

        // Si ya tiene el componente, lo reutilizamos. Si no, lo añadimos.
        if (!TryGetComponent<ItemProperties>(out properties))
        {
            properties = gameObject.AddComponent<ItemProperties>();
        }
        
        properties.itemName = blueprint.itemName;
        properties.healthRestore = blueprint.healthToRestore;
        properties.ability = blueprint.abilityGranted;

        gameObject.name = blueprint.itemName;
    }

    // --- MODIFICADO: Lógica de recolección mejorada ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Intentamos obtener el script de inventario del jugador.
            if (other.TryGetComponent<PlayerController>(out PlayerController controller))
            {
                // 2. Si lo encontramos, añadimos nuestro ID al inventario.
                controller.Inventory.AddItem(this.itemID);
                
                // 3. (Feedback) Instanciamos el efecto de partículas si está asignado.
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }
                
                // 4. (Limpieza) Nos destruimos a nosotros mismos.
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("El objeto con tag 'Player' no tiene un componente inventory.");
            }
        }
    }
}