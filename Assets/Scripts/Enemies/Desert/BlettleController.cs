using UnityEngine;
using System.Collections;
using System;

public class BlettleController : MonoBehaviour
{
    // --- EVENTOS ---
    public event Action OnAttack;

    // --- ESTADOS DE LA INTELIGENCIA ARTIFICIAL ---
    private enum EnemyState { Patrol, Chase, Attacking }
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
    [Tooltip("Distancia a la que el enemigo se prepara para atacar al jugador.")]
    public float attackRange = 0.8f;
    [Tooltip("Multiplicador de velocidad al perseguir al jugador.")]
    public float chaseSpeedMultiplier = 1.5f;
    private Transform target;

    [Header("Combate")]
    [Tooltip("Tiempo en segundos que el enemigo carga hacia atrás antes de atacar.")]
    public float chargeTime = 0.5f;
    [Tooltip("Velocidad a la que retrocede durante la carga.")]
    public float chargeSpeed = 100f;
    [Tooltip("Fuerza del impulso hacia adelante al atacar.")]
    public float attackLungeForce = 250f;
    [Tooltip("Fuerza del retroceso después de atacar.")]
    public float recoilForce = 150f;
    [Tooltip("Tiempo en segundos antes de poder atacar de nuevo.")]
    public float attackCooldown = 1.5f;
    private bool isAttacking = false;
    
    [Header("Pathfinding y Salto")]
    [Tooltip("La fuerza del salto del enemigo.")]
    public float jumpForce = 400f;
    [Tooltip("Define qué es considerado 'obstáculo'.")]
    public LayerMask obstacleLayer;
    [Tooltip("Define qué es considerado 'suelo'.")]
    public LayerMask groundLayer;
    [Tooltip("Punto de origen para detectar el suelo.")]
    public Transform groundCheck;
    [Tooltip("Punto de origen para detectar el obstaculo.")]
    public Transform obstacleCheker;
    private bool isGrounded;
    private bool isObstacle;
    private bool shouldJump = false;


    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    void Update()
    {
        if (!isAttacking)
        {
            HandleDetection();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        isObstacle = Physics2D.OverlapCircle(obstacleCheker.position, 0.2f, obstacleLayer);

        if (!isAttacking)
        {
            HandleAI();
        }
        
        if (isGrounded && isObstacle)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void HandleDetection()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }

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
    
    private void ChaseMovement()
    {
        if (target == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            float currentSpeed = moveSpeed * 200 * chaseSpeedMultiplier;
            rb.AddForce(new Vector2(direction.x * currentSpeed, 0f), ForceMode2D.Force);

            if (direction.x > 0.01f)
            {
                FlipSprite(1);
            }
            else if (direction.x < -0.01f)
            {
                FlipSprite(-1);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (!isAttacking)
            {
                StartCoroutine(AttackSequence());
            }
        }
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        currentState = EnemyState.Attacking;
        rb.linearVelocity = Vector2.zero;

        float chargeDirection = Mathf.Sign(transform.position.x - target.position.x);
        rb.linearVelocity = new Vector2(chargeDirection * chargeSpeed, 0);
        yield return new WaitForSeconds(chargeTime);

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        OnAttack?.Invoke();
        Vector2 directionToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.AddForce(directionToTarget * attackLungeForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);

        rb.linearVelocity = Vector2.zero;
        Vector2 recoilDirection = -directionToTarget;
        rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
        
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    private void PatrolMovement()
    {
        float horizontalSpeed = moveDirection * (moveSpeed * 200);
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);

        float currentDistance = transform.position.x - startPosition.x;

        if ((moveDirection == 1 && currentDistance >= (patrolDistance * 100)) || 
            (moveDirection == -1 && currentDistance <= -(patrolDistance * 100)))
        {
            FlipSprite(-moveDirection);
        }
    }

    private void FlipSprite(int newDirection)
    {
        moveDirection = newDirection;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * newDirection, transform.localScale.y, transform.localScale.z);
    }
    
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
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(invulnerabilityTime / 6);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(invulnerabilityTime / 6);
            }
        }
        isInvulnerable = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
             // other.gameObject.GetComponent<Controller>()?.TakeDamage(attackDamage);
        }
    }
}