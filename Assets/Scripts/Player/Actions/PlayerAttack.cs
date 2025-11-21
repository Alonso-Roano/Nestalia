using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private GameObject playerHurtbox;
    public int attackDamage = 1;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private Vector2 hitboxOffset = new Vector2(0.7f, 0.7f);

    [Header("Configuración Visual")]
    [SerializeField] private GameObject attackIndicatorObject;

    [Header("Ataque Pogo (Hacia Abajo)")]
    public float pogoForce = 10f;

    [Header("Mapeo de Inputs (Unit Events)")]
    [Tooltip("PlayerInput que dispara eventos por acción.")]
    [SerializeField] private PlayerInput playerInput;

    public Animator animator;

    private Rigidbody2D rb;
    private AttackHitbox attackHitboxScript;

    private bool isAttacking = false;
    private Vector2 lastMoveDirection = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (attackHitboxObject != null)
        {
            attackHitboxScript = attackHitboxObject.GetComponent<AttackHitbox>();
            attackHitboxScript.damage = attackDamage;
            attackHitboxObject.SetActive(false);
        }

        if (attackIndicatorObject) attackIndicatorObject.SetActive(false);
        if (playerHurtbox) playerHurtbox.SetActive(true);
    }

    private void Update()
    {
        // El movimiento se lee directamente del PlayerInput
        Vector2 moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.1f)
        {
            lastMoveDirection = moveInput;
        }
    }

    // --- NUEVO: Método llamado por Unity Event del PlayerInput ---
    public void OnAttackEvent(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
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

        if (playerHurtbox != null)
            playerHurtbox.SetActive(false);

        Vector2 newPosition = new Vector2(
            direction.x * hitboxOffset.x,
            direction.y * hitboxOffset.y
        );

        attackHitboxObject.transform.localPosition = newPosition;

        if (attackIndicatorObject != null)
        {
            attackIndicatorObject.transform.localPosition = newPosition;

            float angle = direction == Vector2.left ? 180f :
                          direction == Vector2.up ? 90f :
                          direction == Vector2.down ? -90f : 0f;

            attackIndicatorObject.transform.localRotation =
                Quaternion.Euler(0, 0, angle);

            attackIndicatorObject.SetActive(true);
        }

        attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        // --- POGO ---
        if (direction == Vector2.down && attackHitboxScript.enemyHit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * pogoForce, ForceMode2D.Impulse);
        }

        attackHitboxObject.SetActive(false);
        if (attackIndicatorObject) attackIndicatorObject.SetActive(false);

        if (playerHurtbox) playerHurtbox.SetActive(true);

        isAttacking = false;
    }
}
