using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // Aseguramos que siempre tenga un collider
public class SlowFallPower : MonoBehaviour
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
    [SerializeField] private GameObject slowFall;
    public bool canUseJump = true;
    public bool canUseDoubleJump = true;
    public bool canUseWallJump = true;
    public bool canUseWallCling = true;
    public bool canUseDash = true;
    public bool canUseSlowFall = true;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Guardamos la posición inicial en el primer frame.
        initialPosition = transform.position;
    }

    // --- NUEVO: Lógica del movimiento en Update ---
    void Update()
    {
        // Calculamos la nueva posición Y usando una onda sinusoidal para un movimiento suave.
        float newY = initialPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

        // Aplicamos la nueva posición. Mantenemos la X y Z originales.
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController controller))
            {
                slowFall.SetActive(true);
                controller.Movement.canUseWallCling = canUseWallCling;
                controller.Movement.canUseWallJump = canUseWallJump;
                controller.Movement.canUseDash = canUseDash;
                controller.Movement.canUseSlowFall = canUseSlowFall;
                controller.Movement.canUseJump = canUseJump;
                controller.Movement.canUseDoubleJump = canUseDoubleJump;
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("El objeto con tag 'Player' no tiene un componente inventory.");
            }
        }
    }
}