using UnityEngine;

public class ProyectilEspina : MonoBehaviour
{
    public float tiempoDeVida = 5f;
    public int danoEspina = 5; // Daño que hace esta espina

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Evita chocar con el enemigo que la disparó
        if (other.CompareTag("Enemy"))
        {
            return;
        }
         if (other.CompareTag("Arrow"))
        { 
            return;
        }

        // --- ¡ACTUALIZADO! ---
        // Si choca con el jugador
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Llama a la función de daño del jugador
                playerHealth.TakeDamage(danoEspina);
            }
            Destroy(gameObject); // Destruye la espina al golpear al jugador
        }

        // Si choca con el escenario
        if (other.CompareTag("Ground")) // Asegúrate de que tu suelo tenga este tag
        {
            Destroy(gameObject);
        }
    }
}