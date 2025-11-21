using UnityEngine;

public class Proyectil_Rayo : MonoBehaviour
{
    // Variables que el jefe llenará
    private int dañoAlJugador;
    private float velocidadProyectil;
    private Vector2 direccion;

    private Rigidbody2D rb;

    void Awake()
    {
        // Obtenemos el Rigidbody al nacer
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Esta función es llamada por el Jefe (BossPlanta_AI)
    /// justo después de instanciar (crear) la bola.
    /// </summary>
    public void Inicializar(Vector2 dir, int daño, float velocidad)
    {
        direccion = dir.normalized;
        dañoAlJugador = daño;
        velocidadProyectil = velocidad;

        // ¡Le damos movimiento!
        if (rb != null)
        {
            rb.linearVelocity = direccion * velocidadProyectil; // Corregido de linearVelocity a velocity
        }

        // Autodestrucción después de 5s si no golpea nada
        Destroy(gameObject, 5f); 
    }

    /// <summary>
    /// Se llama cuando este proyectil toca cualquier otro collider.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Si golpeamos al jugador
        if (collision.CompareTag("Player"))
        {
            // Buscamos su script de vida (¡como en el caimán!)
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dañoAlJugador);
            }
            
            // Nos destruimos al golpear al jugador
            Destroy(gameObject);
        }
        
        // (Si choca con "Plataforma" o cualquier otra cosa, lo ignora)
    }
}