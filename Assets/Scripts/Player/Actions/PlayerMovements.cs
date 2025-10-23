using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Animation")]
    public Animator animator;

    [Header("Movimiento")]
    public float jumpForce = 1f;
    public float wallSlideSpeed = 80f;
    public float slowFallMultiplier = 1f;
    public float wallJumpHorizontal = 1f;
    public float wallJumpVertical = 1f;
    public float moveForce = 1f;
    public float variableJumpMultiplier = 0.01f;
    public float coyoteTime = 0.25f;
    public float jumpBufferTime = 0.2f;

    [Header("Detección")]
    public Transform groundCheck;
    public float groundCheckRadius = 1f;
    public LayerMask groundLayer;
    public Transform wallCheck;
    public float wallCheckDistance = 2.3f;
    public LayerMask wallLayer;

    [Header("Habilidades activas")]
    // aqui activo/desactivo habilidades
    public bool canUseJump = true;
    public bool canUseDoubleJump = true;
    public bool canUseWallJump = true;
    public bool canUseWallCling = true;
    public bool canUseDash = true;
    public bool canUseSlowFall = true;

    // Variables internas del script
    private bool isGrounded;
    private bool isTouchingWall;
    private bool canDoubleJump;
    private bool isWallClinging;
    private bool wasGrounded;
    private bool jumpedThisGrounded;
    private bool isDashing;
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private AbilityUIController abilityUIController;

    void Awake()
    {
        // aqui guardo la referencia para no estar llamando GetComponent cada rato
        rb = GetComponent<Rigidbody2D>();
        abilityUIController = FindAnyObjectByType<AbilityUIController>();
    }

    private void Update()
    {
        // El Update solo se encarga de lógica no física, como los contadores de tiempo.
        // Es muy ligero y eficiente.
        if (Time.timeScale == 0f) return;

        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;
        
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // FixedUpdate se usa para toda la lógica de físicas para consistencia.
        if (Time.timeScale == 0f) return;

        // 1. Revisamos el entorno
        CheckSurroundings();
        
        // 2. Ejecutamos las habilidades
        MoveHorizontal();
        if (canUseJump) HandleJump();
        if (canUseWallCling) HandleWallCling();
        if (canUseSlowFall) HandleSlowFall();

        // 3. Actualizamos el Animator
        UpdateAnimator();
    }

    // método que viene del sistema de Input para moverme
    public void OnMove(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
        {
            horizontalInput = 0;
            return;
        }
        horizontalInput = context.ReadValue<Vector2>().x;
    }

    // salto con buffer y salto variable
    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;

        if (!canUseJump) return; // si desactivo salto, no hago nada

        if (context.performed)
            jumpBufferCounter = jumpBufferTime;

        // si suelto el botón mientras voy hacia arriba, corto el salto
        if (context.canceled && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
    }

    // dash on/off
    public void OnDash(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;

        if (!canUseDash) return; // si dash está apagado, ignoro la entrada
        isDashing = context.performed;
    }

    private void MoveHorizontal()
    {
        const float forceMultiplier = 5000f;
        rb.AddForce(Vector2.right * horizontalInput * moveForce * forceMultiplier * Time.fixedDeltaTime, ForceMode2D.Force);

        // giro al personaje dependiendo de hacia dónde me muevo
        float originalScaleX = Mathf.Abs(transform.localScale.x);
        if (horizontalInput > 0)
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void HandleSlowFall()
    {
        bool isCurrentlyPlanning = !isGrounded && !isWallClinging && rb.linearVelocity.y < 0 && isDashing;
        if (isCurrentlyPlanning)
        {
            float slowFallForce = Mathf.Abs(rb.linearVelocity.y) * rb.mass * -(1 - (slowFallMultiplier * 10));
            rb.AddForce(Vector2.up * slowFallForce, ForceMode2D.Force);
        }
    }

    private void HandleJump()
    {
        if (jumpBufferCounter <= 0f) return;

        bool jumped = false;

        // coyote time (me da chance de saltar aunque ya no esté tocando piso)
        if (coyoteTimeCounter > 0f && !jumpedThisGrounded)
        {
            DoJump(Vector2.up * jumpForce);
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
            jumpedThisGrounded = true;
            jumped = true;
        }
        // wall jump
        else if (canUseWallJump && isWallClinging)
        {
            float wallDirection = Mathf.Sign(horizontalInput);
            Vector2 jumpDir = new Vector2((-wallDirection * wallJumpHorizontal) / 1.7f, wallJumpVertical * 1.2f);
            DoJump(jumpDir);
            jumped = true;
            if (animator != null)
            {
                animator.SetTrigger("WallJump");
            }
        }
        // double jump
        else if (canUseDoubleJump && canDoubleJump)
        {
            if (animator != null)
            {
                animator.SetTrigger("DoubleJump");
            }
            DoJump(Vector2.up * jumpForce);
            canDoubleJump = false;
            jumped = true;
        }

        if (jumped)
            jumpBufferCounter = 0f;
    }

    private void CheckSurroundings()
    {
        // chequeo si estoy tocando suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius * 4, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            // si tengo el doble salto activo, lo reseteo
            if (canUseDoubleJump) canDoubleJump = true;
            jumpedThisGrounded = false;
        }
        wasGrounded = isGrounded;

        // manejo coyote time
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        // chequeo si estoy tocando pared
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * Mathf.Sign(horizontalInput), wallCheckDistance * 25, wallLayer);
        isWallClinging = canUseWallCling && isTouchingWall && !isGrounded && horizontalInput != 0;

        abilityUIController.SetClimbColor(isTouchingWall);
        abilityUIController.SetDoubleJumpColor(canDoubleJump);

        if (animator != null)
        {
            animator.SetBool("IsWallClinging", isWallClinging);
        }
    }

    private void HandleWallCling()
    {
        // controlo el "slide" cuando me pego a una pared
        float slideLimit = -(wallSlideSpeed * 400);
        if (isWallClinging && rb.linearVelocity.y < slideLimit)
        {
            float slideForce = (slideLimit - rb.linearVelocity.y) * rb.mass;
            rb.AddForce(Vector2.up * slideForce, ForceMode2D.Impulse);
        }
    }

    private void DoJump(Vector2 force)
    {
        // reseteo velocidad vertical antes del salto para que sea más consistente
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(force * 33, ForceMode2D.Impulse);
    }
    
    private void UpdateAnimator()
    {
        animator.SetFloat("Movement", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsWallClinging", isWallClinging);

        bool isFalling = !isGrounded && !isWallClinging && rb.linearVelocity.y < 0;
        animator.SetBool("IsFalling", isFalling);

        bool isGliding = !isGrounded && !isWallClinging && rb.linearVelocity.y < 0 && isDashing;
        animator.SetBool("IsPlanning", isGliding); // "Planning" era tu nombre original, lo mantengo

        // Actualizar UI (si existe)
        if (abilityUIController != null)
        {
            abilityUIController.SetClimbColor(isTouchingWall);
            abilityUIController.SetDoubleJumpColor(canDoubleJump);
            abilityUIController.SetGlideColor(isFalling || isDashing);
        }
    }
}