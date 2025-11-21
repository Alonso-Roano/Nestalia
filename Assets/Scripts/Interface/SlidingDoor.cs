using UnityEngine;
public class SlidingDoor : MonoBehaviour
{
    public static event System.Action<int> OnDoorUnlocked;

    [Header("Configuración de la Puerta")]
    [Tooltip("El ID del ítem (llave) que abre esta puerta.")]
    public int requiredItemID = 24;
    
    [Tooltip("Cuánto se moverá la puerta al abrirse (ej: Y=5 para subir 5 unidades)")]
    public Vector3 openPositionOffset;
    
    [Tooltip("Velocidad a la que se abre la puerta.")]
    public float openSpeed = 2f;

    [Header("Configuración del Trigger")]
    [Tooltip("La etiqueta (Tag) del objeto del jugador.")]
    public string playerTag = "Player";
    
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpening = false;
    private bool playerIsNearby = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openPositionOffset;

        bool hasTrigger = false;
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }

        if (!hasTrigger)
        {
            Debug.LogWarning("SlidingDoor: Este objeto no tiene ningún Collider2D marcado como 'Is Trigger'. La puerta no podrá activarse.", this);
        }
    }

    void OnEnable()
    {
        MenuCarrusel.OnKeyItemUsed += HandleItemUse;
    }

    void OnDisable()
    {
        MenuCarrusel.OnKeyItemUsed -= HandleItemUse;
    }

    private void HandleItemUse(int itemID)
    {
        if (isOpening) return;

        if (itemID == requiredItemID && playerIsNearby)
        {
            Debug.Log("¡La llave correcta y el jugador está cerca! Abriendo la puerta...");
            isOpening = true;
            
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            OnDoorUnlocked?.Invoke(itemID);

            MenuCarrusel.OnKeyItemUsed -= HandleItemUse;
        }
        else if (itemID == requiredItemID && !playerIsNearby)
        {
            Debug.Log("Tienes la llave, pero necesitas acercarte a la puerta para usarla.");
        }
    }

    void Update()
    {
        if (isOpening)
        {
            transform.position = Vector3.Lerp(transform.position, openPosition, Time.deltaTime * openSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("El jugador ha entrado en la zona de la puerta.");
            playerIsNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("El jugador ha salido de la zona de la puerta.");
            playerIsNearby = false;
        }
    }
}