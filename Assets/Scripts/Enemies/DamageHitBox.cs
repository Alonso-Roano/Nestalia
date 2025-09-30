using UnityEngine;

public class DamageHitBox : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;
    private float lastDamageTime = -999f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerHitbox")) return;

        if (Time.time >= lastDamageTime + damageInterval)
        {
            var controller = GetComponentInParent<BlettleController>();
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                lastDamageTime = Time.time;
                player.TakeDamage(controller.attackDamage, transform);
                Debug.Log($"Jugador recibió {controller.attackDamage} de daño de {controller.name}");
            }
        }
    }
}
