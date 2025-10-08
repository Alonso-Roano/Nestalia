using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    [Tooltip("El objeto hijo que funciona como hitbox de ataque.")]
    [SerializeField] private GameObject attackHitboxObject;
    [Tooltip("La hitbox del jugador que recibe daño. Se desactivará durante el ataque para dar invulnerabilidad.")]
    [SerializeField] private GameObject playerHurtbox; // <-- CAMPO AÑADIDO
    [Tooltip("El daño que inflige cada ataque.")]
    public int attackDamage = 1;
    [Tooltip("Cuánto tiempo (en segundos) permanece activa la hitbox.")]
    [SerializeField] private float attackDuration = 0.2f;
    [Tooltip("Distancia a la que se desplaza la hitbox desde el centro del jugador.")]
    [SerializeField] private Vector2 hitboxOffset = new Vector2(0.7f, 0.7f);

    [Header("Ataque Pogo (Hacia Abajo)")]
    [Tooltip("La fuerza de retroceso/salto al atacar hacia abajo y golpear a un enemigo.")]
    [SerializeField] public float pogoForce = 10f;

    [Header("Mapeo de Inputs")]
    [Tooltip("Referencia a la acción de Ataque del Input Actions asset.")]
    [SerializeField] private InputActionReference attackActionReference;
    [Tooltip("Referencia a la acción de Movimiento (WASD) del Input Actions asset.")]
    [SerializeField] private InputActionReference moveActionReference;

    public Animator animator;

    // Componentes y variables internas
    private Rigidbody2D rb;
    private AttackHitbox attackHitboxScript;
    private InputAction attackAction;
    private InputAction moveAction;
    private Vector2 lastMoveDirection = Vector2.right;
    private bool isAttacking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        attackAction = attackActionReference.action;
        moveAction = moveActionReference.action;

        if (attackHitboxObject != null)
        {
            attackHitboxScript = attackHitboxObject.GetComponent<AttackHitbox>();
            attackHitboxScript.damage = attackDamage;
            attackHitboxObject.SetActive(false);
        }
        else
        {
            Debug.LogError("No se ha asignado un objeto para la AttackHitbox en el inspector.");
        }

        // Aseguramos que la hurtbox del jugador esté activa al empezar
        if (playerHurtbox != null)
        {
            playerHurtbox.SetActive(true);
        }
    }

    private void OnEnable()
    {
        attackAction.Enable();
        moveAction.Enable();
        attackAction.performed += OnAttack;
    }

    private void OnDisable()
    {
        attackAction.Disable();
        moveAction.Disable();
        attackAction.performed -= OnAttack;
    }

    private void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 0.1f)
        {
            lastMoveDirection = moveInput;
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isAttacking) return;

        Vector2 attackDirection;

        if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
        {
            attackDirection = new Vector2(Mathf.Sign(lastMoveDirection.x), 0);
            animator.SetTrigger("Attack");
        }
        else
        {
            attackDirection = new Vector2(0, Mathf.Sign(lastMoveDirection.y));

            if (lastMoveDirection.y > 0)
                animator.SetTrigger("AttackUp");
            else
                animator.SetTrigger("AttackDown");
        }

        if (attackDirection.sqrMagnitude == 0)
        {
            attackDirection = new Vector2(Mathf.Sign(transform.localScale.x), 0);
            animator.SetTrigger("Attack");
        }

        StartCoroutine(AttackCoroutine(attackDirection));
    }


    private IEnumerator AttackCoroutine(Vector2 direction)
    {
        isAttacking = true;

        // --- CAMBIO AÑADIDO: Desactivar la hurtbox del jugador ---
        if (playerHurtbox != null)
        {
            playerHurtbox.SetActive(false);
        }
        // ---------------------------------------------------------

        attackHitboxObject.transform.localPosition = new Vector2(
            direction.x * hitboxOffset.x,
            direction.y * hitboxOffset.y
        );

        attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (direction == Vector2.down && attackHitboxScript.enemyHit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * pogoForce, ForceMode2D.Impulse);
        }

        attackHitboxObject.SetActive(false);

        // --- CAMBIO AÑADIDO: Reactivar la hurtbox del jugador ---
        if (playerHurtbox != null)
        {
            playerHurtbox.SetActive(true);
        }
        // -------------------------------------------------------

        isAttacking = false;
    }
}