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
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void OnEnable() => attack.OnAttackFinished += HandleAttackFinished;
    private void OnDisable() => attack.OnAttackFinished -= HandleAttackFinished;

    // Update ahora es mucho más simple.
    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (movement != null)
            {
                movement.Stop();
            }
            return;
        }
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                break;
            case EnemyState.Chase:
                ChaseBehavior();
                break;
            case EnemyState.Attacking:
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    private void PatrolBehavior()
    {
        // 1. Lógica de transición
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 2. Lógica de acción
        movement.Move(new Vector2(patrolDirection, 0));
        if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance)
        {
            patrolDirection *= -1;
            startPosition = transform.position;
        }
    }

    private void ChaseBehavior()
    {
        if (playerTransform == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Lógica de transición
        if (distanceToPlayer <= attackRange && attack.CanAttack())
        {
            AttackBehavior(); // Llamamos directamente a la acción de atacar
            return;
        }
        
        if (distanceToPlayer > detectionRange)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // 2. Lógica de acción
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        movement.Move(direction, 1.5f);
    }

    private void AttackBehavior()
    {
        ChangeState(EnemyState.Attacking);
        movement.Stop();
        attack.PerformAttack(playerTransform);
    }

    private void HandleAttackFinished()
    {
        // Cuando el ataque termina, decidimos qué hacer a continuación.
        // Volver a Chase es una opción segura si el jugador sigue cerca.
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else
        {
            ChangeState(EnemyState.Patrol);
        }
    }
}