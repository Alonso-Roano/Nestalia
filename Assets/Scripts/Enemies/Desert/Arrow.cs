using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 15f;
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;
    public int arrowDamage = 10;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isHoming = true;
    private bool hasCollided = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    public void SetTarget(Vector2 target)
    {
        targetPosition = target;
        isHoming = true;
        hasCollided = false;
        rb.gravityScale = 0;
    }

    void FixedUpdate()
    {
        if (!isHoming || hasCollided) return;

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            hasCollided = true;
            StartFalling();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (distanceToTarget <= 5f)
        {
            isHoming = false;
            StartFalling();
        }
        else
        {
            rb.linearVelocity = direction * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void StartFalling()
    {
        if (hasCollided) rb.linearVelocity = Vector2.zero;

        isHoming = false;
        rb.gravityScale = 100f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger)
            {
                col.enabled = false;
            }
        }

        Destroy(gameObject, 2f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCollided) return;

        hasCollided = true;

        StartFalling();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(arrowDamage, transform);

            hasCollided = true;
            StartFalling(); 
        }
    }
}
