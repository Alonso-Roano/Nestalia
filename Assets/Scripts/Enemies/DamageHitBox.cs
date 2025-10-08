using UnityEngine;

public class DamageHitBox : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private int attackDamage = 10;
    private float lastDamageTime = -999f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerHitbox")) return;

        if (Time.time >= lastDamageTime + damageInterval)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                lastDamageTime = Time.time;
                player.Health.TakeDamage(attackDamage, transform);
                Debug.Log($"Jugador recibió {attackDamage} de daño");
            }
        }
    }
}
