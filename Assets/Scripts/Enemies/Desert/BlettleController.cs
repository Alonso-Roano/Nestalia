using UnityEngine;
using System.Collections;

public class BlettleController : MonoBehaviour
{
    // --- ESTADOS DE LA INTELIGENCIA ARTIFICIAL ---
    private enum EnemyState { Patrol, Chase }
    private EnemyState currentState = EnemyState.Patrol;

    // --- PROPIEDADES DEL ENEMIGO ---
    [Header("Estadísticas")]
    [Tooltip("Vida máxima del enemigo.")]
    public int maxHealth = 3;
    private int currentHealth;
    [Tooltip("Velocidad de movimiento base.")]
    public float moveSpeed = 1f;
    [Tooltip("Daño que inflige al jugador al tocarlo.")]
    public int attackDamage = 1;
    [Tooltip("Tiempo (en segundos) que el enemigo es invulnerable tras recibir daño.")]
    public float invulnerabilityTime = 0.5f;
    private bool isInvulnerable = false;

    // --- PROPIEDADES DE MOVIMIENTO / IA ---
    [Header("Patrulla")]
    [Tooltip("Distancia que el enemigo recorrerá antes de cambiar de dirección.")]
    public float patrolDistance = 1f;
    private Vector3 startPosition;
    private int moveDirection = 1; // 1 para derecha, -1 para izquierda
    private Rigidbody2D rb; 
    private SpriteRenderer spriteRenderer; 

    [Header("Detección y Persecución")]
    [Tooltip("Distancia a la que el enemigo detecta al jugador.")]
    public float detectionRange = 1f;
    [Tooltip("Multiplicador de velocidad al perseguir al jugador.")]
    public float chaseSpeedMultiplier = 1.5f;
    private Transform target; // El transform del jugador

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Busca el objeto del jugador por la etiqueta "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    void Update()
    {
        HandleDetection();
    }

    void FixedUpdate()
    {
        // Lógica de movimiento en FixedUpdate para cálculos físicos
        HandleAI();
    }
    
    // --- LÓGICA DE DETECCIÓN Y CAMBIO DE ESTADO ---

    private void HandleDetection()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= (detectionRange*200))
        {
            currentState = EnemyState.Chase;
        }
        else if (currentState == EnemyState.Chase)
        {
            // Vuelve a Patrulla si el jugador sale del rango
            currentState = EnemyState.Patrol;
        }
    }

    // --- MANEJADOR PRINCIPAL DE LA IA ---

    private void HandleAI()
    {
        if (currentState == EnemyState.Patrol)
        {
            PatrolMovement();
        }
        else if (currentState == EnemyState.Chase)
        {
            ChaseMovement();
        }
    }

    // --- PATRÓN 1: PERSECUCIÓN ---

    private void ChaseMovement()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        
        float currentSpeed = moveSpeed * 200 * chaseSpeedMultiplier;
        rb.linearVelocity = new Vector2(direction.x * currentSpeed, rb.linearVelocityY);

        // Voltear el sprite según la dirección
        if (direction.x > 0.01f)
        {
            FlipSprite(1); // Derecha
        }
        else if (direction.x < -0.01f)
        {
            FlipSprite(-1); // Izquierda
        }
    }

    //PATRÓN 2: PATRULLA
    private void PatrolMovement()
    {
        Vector2 movement = new Vector2(moveDirection * (moveSpeed * 200), rb.linearVelocityY);
        rb.linearVelocity = movement;

        // Comprobar límites de patrulla
        float currentDistance = transform.position.x - startPosition.x;

        if (moveDirection == 1 && currentDistance >= (patrolDistance*100))
        {
            FlipSprite(-1);
        }
        else if (moveDirection == -1 && currentDistance <= -(patrolDistance*100))
        {
            FlipSprite(1);
        }
    }

    // --- FUNCIONES AUXILIARES DE MOVIMIENTO ---

    private void FlipSprite(int newDirection)
    {
        // Actualiza la dirección de patrulla
        moveDirection = newDirection; 
        
        // Voltear el sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (moveDirection == 1); 
        }
    }

    // --- LÓGICA DE COMBATE ---
    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvulnerable());
        }
    }

    private void Die()
    {
        Destroy(gameObject); 
    }

    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        
        // Efecto visual de invulnerabilidad
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red; 
            yield return new WaitForSeconds(invulnerabilityTime / 3);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(invulnerabilityTime / 3);
            spriteRenderer.color = Color.red; 
            yield return new WaitForSeconds(invulnerabilityTime / 3);
            spriteRenderer.color = originalColor; 
        }
        
        yield return new WaitForSeconds(invulnerabilityTime); 
        isInvulnerable = false;
    }


    // --- LÓGICA DE COLISIÓN / ATAQUE ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Llama a la función de daño del jugador (si existe su script PlayerHealth)
            // other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }
    }
}