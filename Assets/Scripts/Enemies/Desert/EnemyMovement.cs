using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 200f;
    [SerializeField] private float jumpForce = 400f;
    
    [Header("Detección de Entorno")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform obstacleCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private int currentFacingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CheckForObstaclesAndJump();
    }

    public void Move(Vector2 direction, float speedMultiplier = 1f)
    {
        if (rb.IsSleeping())
        {
            rb.WakeUp();
        }

        rb.AddForce(new Vector2(direction.x * moveSpeed * speedMultiplier, 0f), ForceMode2D.Force);

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            int newDirection = (int)Mathf.Sign(direction.x);
            if (newDirection != currentFacingDirection)
            {
                Flip(newDirection);
            }
        }
    }
    
    public void Stop()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    private void Flip(int newDirection)
    {
        currentFacingDirection = newDirection;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * currentFacingDirection, transform.localScale.y, transform.localScale.z);
    }
    
    private void CheckForObstaclesAndJump()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        bool isObstacleAhead = Physics2D.OverlapCircle(obstacleCheck.position, 0.2f, obstacleLayer);

        if (isGrounded && isObstacleAhead)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    public int GetFacingDirection()
    {
        return currentFacingDirection;
    }
}