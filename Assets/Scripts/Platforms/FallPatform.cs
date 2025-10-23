using System.Collections;
using UnityEngine;

public class FallPlatform : MonoBehaviour
{
    [Header("Colider")]
    [SerializeField] private GameObject colliderExtra;
    [Header("Tiempos")]
    public float tiempoParaCaer = 2f;
    public float tiempoParaReaparecer = 5f;

    [Header("Efectos")]
    public float intensidadTemblor = 0.1f;

    private Vector3 posicionOriginal;
    private bool jugadorEncima = false;

    // Referencias a los componentes que vamos a desactivar
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;

    void Start()
    {
        posicionOriginal = transform.position;
        
        // Obtenemos los componentes al iniciar
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !jugadorEncima)
        {
            jugadorEncima = true;
            StartCoroutine(SecuenciaDeCaida());
        }
    }

    private IEnumerator SecuenciaDeCaida()
    {
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < tiempoParaCaer)
        {
            float offsetX = Random.Range(-0.5f, 0.5f) * intensidadTemblor;
            float offsetY = Random.Range(-0.5f, 0.5f) * intensidadTemblor;
            transform.position = posicionOriginal + new Vector3(offsetX, offsetY, 0);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transform.position = posicionOriginal;

        // --- CAMBIO CLAVE AQUÍ ---
        // En lugar de desactivar el objeto, desactivamos sus componentes
        spriteRenderer.enabled = false;
        platformCollider.enabled = false;
        colliderExtra.SetActive(false);
        
        Debug.Log("Esperando para reaparecer...");

        // La corutina ahora SÍ continuará
        yield return new WaitForSeconds(tiempoParaReaparecer);
        
        Debug.Log("¡Terminó la espera!");

        Reaparecer();
    }

    private void Reaparecer()
    {
        Debug.Log("Reapareciendo...");
        jugadorEncima = false;
        transform.position = posicionOriginal;

        // --- Y REACTIVAMOS LOS COMPONENTES ---
        spriteRenderer.enabled = true;
        platformCollider.enabled = true;
        colliderExtra.SetActive(true);
    }
}