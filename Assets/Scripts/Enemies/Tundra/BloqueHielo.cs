using UnityEngine;
using System.Collections;

public class BloqueHielo : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoParaRomper = 1.0f;     // Tiempo total desde que se pisa hasta que se rompe (incluye temblor)
    public float duracionTemblor = 0.5f;     // Cuánto tiempo tiembla
    public float magnitudTemblor = 0.05f;    // Qué tan fuerte tiembla
    public float tiempoDeRegeneracion = 5.0f; // Tiempo hasta que reaparece

    // Referencias a los componentes
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Vector3 posicionOriginal;

    // Lista para guardar las referencias a los SpriteRenderer de los hijos
    private SpriteRenderer[] renderizadoresHijos;

    private bool estaRoto = false;

    void Start()
    {
        // Guardamos los componentes y la posición inicial
        spriteRenderer = GetComponent<SpriteRenderer>(); // El SpriteRenderer del objeto padre (si lo tiene)
        boxCollider = GetComponent<BoxCollider2D>();
        posicionOriginal = transform.position;

        // ¡NUEVO! Cargar todos los SpriteRenderer en los objetos hijos
        // (true incluye el SpriteRenderer del padre, pero los gestionaremos por separado)
        // Usamos GetComponentsInChildren para asegurarnos de obtener todos.
        renderizadoresHijos = GetComponentsInChildren<SpriteRenderer>(true);
    }

    // Esto se activa cuando algo colisiona con el bloque
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos si el objeto que colisiona tiene el tag "Player"
        // y si el bloque no está ya roto o en proceso de romperse.
        if (collision.gameObject.CompareTag("Player") && !estaRoto)
        {
            // Verificamos si el jugador está pisando desde arriba
            if (ElJugadorEstaEncima(collision))
            {
                StartCoroutine(SecuenciaDeRotura());
            }
        }
    }

    private bool ElJugadorEstaEncima(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // El vector normal del punto de contacto apunta "hacia afuera" del bloque de hielo.
            // Si el jugador está encima, la normal apuntará hacia abajo (aprox. -1 en Y).
            if (contact.normal.y < -0.5f)
            {
                return true;
            }
        }
        return false;
    }

    // --- NUEVO MÉTODO: Desactivar/Activar Renderizadores Hijos ---

    private void EstablecerVisibilidadHijos(bool activo)
    {
        // Iteramos sobre todos los SpriteRenderer que encontramos en Start()
        foreach (SpriteRenderer sr in renderizadoresHijos)
        {
            // Verificamos que no sea el renderizador del padre
            // si el padre tiene uno y lo estamos gestionando aparte (con spriteRenderer.enabled)
            if (sr.gameObject != gameObject)
            {
                 sr.enabled = activo;
            }
            // Si el padre no tiene SpriteRenderer, la línea sr.gameObject != gameObject 
            // no es estrictamente necesaria si solo quieres desactivar los de los hijos,
            // pero es una buena práctica para ser explícito.
        }
    }

    // -------------------------------------------------------------------

    private IEnumerator SecuenciaDeRotura()
    {
        estaRoto = true; // Marcamos como roto para no volver a activar la corrutina

        // --- 1. Temblor ---
        float temporizadorTemblor = 0f;
        while (temporizadorTemblor < tiempoParaRomper)
        {
            // Solo tiembla durante la última parte del tiempo
            if (temporizadorTemblor > (tiempoParaRomper - duracionTemblor))
            {
                float x = Random.Range(-1f, 1f) * magnitudTemblor;
                float y = Random.Range(-1f, 1f) * magnitudTemblor;
                transform.position = posicionOriginal + new Vector3(x, y, 0);
            }
            
            temporizadorTemblor += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        // --- 2. Romper ---
        transform.position = posicionOriginal; // Vuelve a la posición original antes de desaparecer
        
        // Desactivar el renderizador del padre (si existe) y el colisionador
        if(spriteRenderer != null)
        {
             spriteRenderer.enabled = false;
        }
        boxCollider.enabled = false;

        // ¡NUEVO! Desactivar los renderizadores de los hijos
        EstablecerVisibilidadHijos(false); 
        
        // Opcional: Instanciar un efecto de partículas de rotura aquí
        // Instantiate(efectoParticulasRotura, transform.position, Quaternion.identity);

        // --- 3. Esperar Regeneración ---
        yield return new WaitForSeconds(tiempoDeRegeneracion);

        // --- 4. Regenerar ---
        if(spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        boxCollider.enabled = true;

        // ¡NUEVO! Activar los renderizadores de los hijos
        EstablecerVisibilidadHijos(true); 

        estaRoto = false; // El bloque está listo para romperse de nuevo
        
        // Opcional: Instanciar un efecto de partículas de regeneración aquí
        // Instantiate(efectoParticulasRegeneracion, transform.position, Quaternion.identity);
    }
}