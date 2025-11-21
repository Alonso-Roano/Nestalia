using UnityEngine;

public class MosquitoDeteccion : MonoBehaviour
{
    // Arrastra el objeto principal del Mosquito (que tiene MosquitoAI.cs) aquí
    [SerializeField] private MosquitoAI aiCerebro;

    void Start()
    {
        if (aiCerebro == null)
        {
            // Intenta encontrarlo en el padre si no se asignó
            aiCerebro = GetComponentInParent<MosquitoAI>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Asegúrate de que tu jugador tenga el Tag "Player"
        if (other.CompareTag("Player"))
        {
            aiCerebro.OnDetectarJugador(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            aiCerebro.OnPerderJugador();
        }
    }
}