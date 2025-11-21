using UnityEngine;

public class Serpent_DamagePlayer : MonoBehaviour
{
    public int damageToPlayer = 1; // Daño que hace al tocarlo

    // Este collider NO es "Is Trigger"
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobar si ha chocado con el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageToPlayer);
            Debug.Log("JUGADOR GOLPEADO POR SERPIENTE");
        }
    }
}