using UnityEngine;

public class MosquitoAtaque : MonoBehaviour
{
    // Arrastra el objeto principal del Mosquito (que tiene MosquitoAI.cs) aquí
    [SerializeField] private MosquitoAI aiCerebro;

    void Start()
    {
        if (aiCerebro == null)
        {
            // Se busca a sí mismo en el objeto
            aiCerebro = GetComponent<MosquitoAI>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("MosquitoAtaque ha colisionado con: " + collision.gameObject.name);
        // Asegúrate de que tu jugador tenga el Tag "Player"
        // y que el jugador también tenga un Rigidbody2D y un Collider2D
        if (collision.gameObject.CompareTag("Player"))
        {
            // Avisa al cerebro que golpeamos al jugador
            aiCerebro.OnGolpearJugador();

            // --- Opcional: Hacer daño al jugador ---
            // PlayerHealth healthScript = collision.gameObject.GetComponent<PlayerHealth>();
            // if (healthScript != null)
            // {
            //     healthScript.RecibirDaño(10);
            // }
        }
    }
}