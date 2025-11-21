using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private AbilityUIController abilityUIController; 

    [Header("Movimiento Horizontal")]
    [SerializeField] public float moveForce = 1f;
    [SerializeField] private float moveForceMultiplier = 5000f;

    [Header("Salto")]
    [SerializeField] public float jumpForce = 1f;
    [SerializeField] private float jumpForceMultiplier = 33f;
    [SerializeField] private float variableJumpMultiplier = 0.5f;
    [SerializeField] private float coyoteTime = 0.25f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Salto de Pared")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpHorizontal = 5f;
    [SerializeField] private float wallJumpVertical = 10f;
    [SerializeField] private Vector2 wallJumpAngle = new Vector2(1.0f, 1.2f);

    [Header("Caída Lenta (Glide)")]
    [SerializeField] public float slowFallSpeed = 2f; 

    [Header("Detección")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 2.3f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Habilidades Activas")]
    public bool canUseJump = true;
    public bool canUseDoubleJump = true;
    public bool canUseWallJump = true;
    public bool canUseWallCling = true;
    public bool canUseDash = true;
    public bool canUseSlowFall = true;

    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallClinging;
    private bool canDoubleJumpInternal;
    private bool jumpedThisGrounded;
    private bool isJumpHeld;
    private bool isSlowFallHeld;
    
    private bool lastCanUseJump;
    private bool lastCanUseDoubleJump;
    private bool lastCanUseWallJump;
    private bool lastCanUseWallCling;
    private bool lastCanUseDash;
    private bool lastCanUseSlowFall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (animator == null)
            animator = GetComponent<Animator>(); 
            
        canDoubleJumpInternal = canUseDoubleJump;
        
        UpdateAbilityAvailabilityUI();
        CacheAbilityStates();
        UpdateUIState();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (coyoteTimeCounter > 0)
            coyoteTimeCounter -= Time.deltaTime;
        
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;
            
        CheckForAbilityChanges();
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        CheckSurroundings();
        HandleGroundedState();
        HandleWallClingState(); 
        HandleMovement();
        HandleSlowFall(); 
        HandleVariableJump();

        if (canUseJump)
            HandleJump();

        UpdateAnimator();
        UpdateUIState();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
        {
            horizontalInput = 0;
            return;
        }
        horizontalInput = context.ReadValue<Vector2>().x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f || !canUseJump) return;

        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
            isJumpHeld = true;
        }
        else if (context.canceled)
        {
            isJumpHeld = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context) 
    {
        if (Time.timeScale == 0f || !canUseSlowFall) return;
        
        if (context.started)
            isSlowFallHeld = true;
        else if (context.canceled)
            isSlowFallHeld = false;
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * Mathf.Sign(horizontalInput), wallCheckDistance, wallLayer);
    }

    private void HandleGroundedState()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpedThisGrounded = false;
            
            if (canUseDoubleJump)
                canDoubleJumpInternal = true;
        }
    }

    private void HandleWallClingState()
    {
        isWallClinging = canUseWallCling && isTouchingWall && !isGrounded && horizontalInput != 0;

        if (isWallClinging)
        {
            float limitedVelocity = Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, limitedVelocity);
        }
    }

    private void HandleMovement()
    {
        if (isWallClinging) return; 

        rb.AddForce(Vector2.right * horizontalInput * moveForce * moveForceMultiplier * Time.fixedDeltaTime, ForceMode2D.Force);

        float originalScaleX = Mathf.Abs(transform.localScale.x);
        if (horizontalInput > 0)
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void HandleSlowFall()
    {
        bool isGliding = canUseSlowFall && (isSlowFallHeld || isJumpHeld) && !isGrounded && !isWallClinging && rb.linearVelocity.y < 0;
        
        if (isGliding)
        {
            float limitedVelocity = Mathf.Max(rb.linearVelocity.y, -slowFallSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, limitedVelocity);
        }
    }
    
    private void HandleVariableJump()
    {
        if (!isJumpHeld && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
        }
    }

    private void HandleJump()
    {
        if (jumpBufferCounter <= 0f) return; 

        bool jumped = false;

        if (coyoteTimeCounter > 0f && !jumpedThisGrounded)
        {
            DoJump(Vector2.up);
            if (animator != null) animator.SetTrigger("Jump");
            jumpedThisGrounded = true; 
            jumped = true;
        }
        else if (canUseWallJump && isWallClinging)
        {
            float wallDirection = Mathf.Sign(horizontalInput);
            Vector2 jumpDir = new Vector2(
                -wallDirection * wallJumpHorizontal * wallJumpAngle.x, 
                wallJumpVertical * wallJumpAngle.y
            );
            
            DoJump(jumpDir, false); 
            if (animator != null) animator.SetTrigger("WallJump");
            jumped = true;
        }
        else if (canUseDoubleJump && canDoubleJumpInternal && !isGrounded)
        {
            DoJump(Vector2.up);
            if (animator != null) animator.SetTrigger("DoubleJump");
            canDoubleJumpInternal = false;
            jumped = true;
        }

        if (jumped)
        {
            jumpBufferCounter = 0f; 
            coyoteTimeCounter = 0f;
        }
    }

    private void DoJump(Vector2 direction, bool resetYVelocity = true)
    {
        if (resetYVelocity)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        
        rb.AddForce(direction.normalized * jumpForce * jumpForceMultiplier, ForceMode2D.Impulse);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("Movement", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsWallClinging", isWallClinging);

        bool isFalling = !isGrounded && !isWallClinging && rb.linearVelocity.y < 1f;
        animator.SetBool("IsFalling", isFalling);

        bool isGliding = canUseSlowFall && (isSlowFallHeld || isJumpHeld) && isFalling;
        animator.SetBool("IsPlanning", isGliding);
    }

    private void UpdateUIState()
    {
        if (abilityUIController == null) return;

        abilityUIController.SetClimbColor(isTouchingWall);
        abilityUIController.SetDoubleJumpColor(canDoubleJumpInternal);
        
        bool isGliding = canUseSlowFall && (isSlowFallHeld || isJumpHeld) && !isGrounded && !isWallClinging && rb.linearVelocity.y < 0;
        abilityUIController.SetGlideColor(isGliding);
    } 
    
    private void CacheAbilityStates()
    {
        lastCanUseJump = canUseJump;
        lastCanUseDoubleJump = canUseDoubleJump;
        lastCanUseWallJump = canUseWallJump;
        lastCanUseWallCling = canUseWallCling;
        lastCanUseDash = canUseDash;
        lastCanUseSlowFall = canUseSlowFall;
    }

    private void UpdateAbilityAvailabilityUI()
    {
        if (abilityUIController == null) return;

        abilityUIController.SetDoubleJumpVisible(canUseDoubleJump);
        abilityUIController.SetClimbVisible(canUseWallCling);
        abilityUIController.SetGlideVisible(canUseSlowFall);
    }
    
    private void CheckForAbilityChanges()
    {
        bool changed = false;
        if (lastCanUseJump != canUseJump) changed = true;
        if (lastCanUseDoubleJump != canUseDoubleJump) changed = true;
        if (lastCanUseWallJump != canUseWallJump) changed = true;
        if (lastCanUseWallCling != canUseWallCling) changed = true;
        if (lastCanUseDash != canUseDash) changed = true;
        if (lastCanUseSlowFall != canUseSlowFall) changed = true;

        if (changed)
        {
            UpdateAbilityAvailabilityUI();
            CacheAbilityStates();
        }
    }
}