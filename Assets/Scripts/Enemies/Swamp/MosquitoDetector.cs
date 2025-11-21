using UnityEngine;

public class MosquitoDetector : MonoBehaviour
{
    [SerializeField] private MosquitoAI mosquitoAI; // Referencia al script principal

    void Start()
    {
        if (mosquitoAI == null)
        {
            // Intentar encontrar el MosquitoAI en el padre
            mosquitoAI = GetComponentInParent<MosquitoAI>();
            
            if (mosquitoAI == null)
            {
                Debug.LogError("¡Falta asignar el MosquitoAI en el detector!");
                enabled = false;
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mosquitoAI.OnDetectarJugador(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mosquitoAI.OnPerderJugador();
        }
    }
}