using UnityEngine;

[RequireComponent(typeof(EnemyHealth), typeof(EnemyMovement), typeof(EnemyAttack_Lunge))]
public class BlettleAI : MonoBehaviour
{
    private enum EnemyState { Patrol, Chase, Attacking }
    private EnemyState currentState = EnemyState.Patrol;

    [Header("Detección y Rangos")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Patrulla")]
    [SerializeField] private float patrolDistance = 5f;

    private EnemyMovement movement;
    private EnemyAttack_Lunge attack;

    private Transform playerTransform;
    private Transform target;

    private Vector3 startPosition;
    private int patrolDirection = 1;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        attack = GetComponent<EnemyAttack_Lunge>();
    }

    private void Start()
    {
        startPosition = transform.position;

        // --- CAMBIO: Buscamos al jugador pero no lo asignamos como objetivo activo ---
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            // Guardamos la referencia, pero 'target' sigue siendo null
            playerTransform = playerObject.transform;
        }
    }

    // Nos suscribimos al evento para saber cuándo termina un ataque
    private void OnEnable() => attack.OnAttackFinished += HandleAttackFinished;
    private void OnDisable() => attack.OnAttackFinished -= HandleAttackFinished;

    void Update()
    {
        // La máquina de estados solo se actualiza si no estamos en medio de un ataque
        if (currentState != EnemyState.Attacking)
        {
            HandleStateTransitions();
        }

        HandleAI();
    }

    // --- CAMBIO: Lógica de transición de estados reestructurada ---
    private void HandleStateTransitions()
    {
        // Si no hay jugador en la escena, no hacemos nada más que patrullar
        if (playerTransform == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Decidir si tenemos un objetivo activo basado en el rango de detección
        if (distanceToPlayer <= detectionRange)
        {
            target = playerTransform; // El jugador está cerca, es nuestro objetivo
        }
        else
        {
            target = null; // El jugador está lejos, lo perdemos de vista
        }

        // 2. Decidir el estado basado en si tenemos un objetivo
        if (target != null)
        {
            // Si tenemos objetivo y está en rango de ataque, atacamos
            if (distanceToPlayer <= attackRange && attack.CanAttack())
            {
                currentState = EnemyState.Attacking;
            }
            // Si no, lo perseguimos
            else
            {
                currentState = EnemyState.Chase;
            }
        }
        else
        {
            // Si no tenemos objetivo, patrullamos
            currentState = EnemyState.Patrol;
        }
    }

    private void HandleAI()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attacking:
                movement.Stop();
                attack.PerformAttack(target);
                break;
        }
    }

    private void Patrol()
    {
        movement.Move(new Vector2(patrolDirection, 0));

        // Lógica para cambiar de dirección
        if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance)
        {
            patrolDirection *= -1; // Invertir dirección
            // Reseteamos la posición de referencia para medir la distancia desde el nuevo punto
            startPosition = transform.position;
        }
    }

    private void Chase()
    {
        if (target == null) return;

        // Le decimos al componente de movimiento que se mueva hacia el jugador
        Vector2 direction = (target.position - transform.position).normalized;
        movement.Move(direction, 1.5f); // Usamos un multiplicador para ir más rápido
    }

    // Este método se ejecuta cuando el componente de ataque nos avisa que ha terminado
    private void HandleAttackFinished()
    {
        // --- CAMBIO: En lugar de solo volver a Chase, re-evaluamos la situación ---
        HandleStateTransitions();
    }
}