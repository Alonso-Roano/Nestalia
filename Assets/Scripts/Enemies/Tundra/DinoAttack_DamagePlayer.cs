using UnityEngine;

// 🔹 Renombrado de Serpent_DamagePlayer
public class DinoAttack_DamagePlayer : MonoBehaviour
{
    public int damageToPlayer = 1;

    // Asegúrate de que este collider NO sea "Is Trigger"
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 🔹 Asumo que tu script de vida del jugador se llama 'PlayerHealth'
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageToPlayer, transform);
            Debug.Log("JUGADOR GOLPEADO POR ATAQUE DINO");
        }
    }
    
    // 🔹 Opcional: si usas Triggers en lugar de Collisions
    /*
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damageToPlayer);
            Debug.Log("JUGADOR GOLPEADO POR ATAQUE DINO");
        }
    }
    */
}