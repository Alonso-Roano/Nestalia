using UnityEngine;
using System.Collections; // Necesario para las Corutinas

public class MosquitoAI : MonoBehaviour, AttackHitbox.IEnemyDamageable
{
    // --- Configuración General ---
    [Header("Referencias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform spriteTransform; // El transform del Sprite, para voltearlo
    [SerializeField] private GameObject detectorRango; // Objeto que contiene el collider para detectar al jugador
    private Transform jugador;

    [Header("Patrulla")]
    [SerializeField] private float radioDePatrulla = 5f;
    [SerializeField] private float velocidadPatrulla = 2f;
    [SerializeField] private float tiempoEsperaPatrulla = 1.5f; // Tiempo que espera al llegar a un punto
    [SerializeField] private float toleranciaLlegadaPatrulla = 0.3f;

    [Header("Ataque")]
    [SerializeField] private float velocidadAtaque = 6f; // Velocidad de la embestida
    [SerializeField] private float duracionRetroceso = 0.5f;
    [SerializeField] private float cooldownAtaque = 2f; // Tiempo de espera antes de perseguir de nuevo
    [SerializeField] private int dañoAlJugador = 10;

    [Header("Estadísticas")]
    [SerializeField] private int vidaMaxima = 30;
    private int vidaActual;

    [Header("Efectos de Daño")]
    [SerializeField] private float fuerzaKnockback = 5f;
    [SerializeField] private float duracionKnockback = 0.2f;
    [SerializeField] private float duracionFlashDaño = 0.2f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // --- Variables de Estado ---
    private enum Estado { Patrullando, Persiguiendo, Lanzandose, Retrocediendo, Cooldown }
    private Estado estadoActual;

    private Vector2 puntoInicio;
    private Vector2 puntoPatrullaObjetivo;
    private bool mirandoDerecha = true;
    private bool puedeMoverse = true;
    private bool isDead = false;
    private bool landedProcessed = false;
    private float toleranciaPatrullaSqr;
    private bool estaActivo = false;

    void Start()
    {
        // Guardamos dónde "nació" para calcular el radio de patrulla
        puntoInicio = transform.position;
        rb = GetComponent<Rigidbody2D>();
        vidaActual = vidaMaxima; // Inicializar la vida

        // Obtener el SpriteRenderer si no está asignado
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Verificar que tengamos el detector de rango
        if (detectorRango == null)
        {
            Debug.LogError("¡Falta asignar el detector de rango en el Inspector!");
            enabled = false; // Desactivar este script si falta el detector
            return;
        }
        
        // Empezamos patrullando
        CambiarEstado(Estado.Patrullando);
        toleranciaPatrullaSqr = toleranciaLlegadaPatrulla * toleranciaLlegadaPatrulla;
    }

    void Update()
    {
        if (!estaActivo || isDead) return;
        switch (estadoActual)
        {
            case Estado.Persiguiendo:
                if (jugador != null)
                {
                    if ((jugador.position - transform.position).sqrMagnitude < 2.25f) // <-- CAMBIA ESTO
                    {
                        CambiarEstado(Estado.Lanzandose);
                    }
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (!estaActivo || isDead) return;
        // FixedUpdate es para físicas (movimiento)
        if (!puedeMoverse || estadoActual == Estado.Cooldown)
        {
            rb.linearVelocity = Vector2.zero; // Detenerse si no podemos movernos
            return;
        }

        Vector2 direccion;
        float velocidadActual;

        switch (estadoActual)
        {
           case Estado.Patrullando:
                Vector2 direccionHaciaObjetivo = puntoPatrullaObjetivo - (Vector2)transform.position;

                if (direccionHaciaObjetivo.sqrMagnitude < toleranciaPatrullaSqr && puedeMoverse) 
                {
                    StartCoroutine(RutinaNuevoPuntoPatrulla());
                }
                
                if (puedeMoverse)
                {
                    rb.linearVelocity = direccionHaciaObjetivo.normalized * velocidadPatrulla;
                }
                break;

            case Estado.Persiguiendo:
                if (jugador == null) return;
                // Perseguir al jugador
                direccion = (jugador.position - transform.position).normalized;
                velocidadActual = velocidadPatrulla; // Usamos velocidad normal para seguirlo
                rb.linearVelocity = direccion * velocidadActual;
                break;

            case Estado.Lanzandose:
                if (jugador == null) return;
                // Embestir rápido hacia la última posición conocida
                direccion = (jugador.position - transform.position).normalized;
                velocidadActual = velocidadAtaque; // ¡Velocidad de ataque!
                rb.linearVelocity = direccion * velocidadActual;
                // El estado cambia a Retrocediendo cuando colisiona (ver MosquitoAtaque.cs)
                break;

            case Estado.Retrocediendo:
                if (jugador == null) return;
                // Moverse en dirección OPUESTA al jugador
                direccion = (transform.position - jugador.position).normalized;
                velocidadActual = velocidadAtaque * 0.75f; // Retroceder rápido
                rb.linearVelocity = direccion * velocidadActual;
                break;
        }

        // Voltear el sprite
        float velocidadX = rb.linearVelocity.x;

        // Moviéndose a la DERECHA (velocidad > 0) pero mirando IZQUIERDA (!mirandoDerecha)
        if (velocidadX < 0.1f && !mirandoDerecha) 
        {
            Voltear();
        }
        // Moviéndose a la IZQUIERDA (velocidad < 0) pero mirando DERECHA (mirandoDerecha)
        else if (velocidadX > -0.1f && mirandoDerecha) 
        {
            Voltear();
        }
    }

    // --- Gestión de Estados y Corutinas ---

    private void CambiarEstado(Estado nuevoEstado)
    {
        estadoActual = nuevoEstado;
        StopAllCoroutines(); // Detenemos cualquier rutina anterior
        puedeMoverse = true;

        switch (nuevoEstado)
        {
            case Estado.Patrullando:
                StartCoroutine(RutinaNuevoPuntoPatrulla());
                break;
            case Estado.Persiguiendo:
                // No necesita corutina, solo sigue al jugador en FixedUpdate
                break;
            case Estado.Lanzandose:
                // El estado se cambiará por colisión
                break;
            case Estado.Retrocediendo:
                StartCoroutine(RutinaRetroceso());
                break;
            case Estado.Cooldown:
                StartCoroutine(RutinaCooldown());
                break;
        }
    }

 private IEnumerator RutinaNuevoPuntoPatrulla()
    {
        // 1. Detenerse
        puedeMoverse = false;
        yield return new WaitForSeconds(tiempoEsperaPatrulla);

        // 2. Calcular nuevo punto aleatorio DENTRO del radio
        Vector2 puntoAleatorio = Random.insideUnitCircle * radioDePatrulla;
        puntoPatrullaObjetivo = puntoInicio + puntoAleatorio;
        
        // 3. Reanudar movimiento
        puedeMoverse = true;
        
        // ¡¡¡NO LLAMES A CambiarEstado(Estado.Patrullando) AQUÍ!!!
        // Simplemente deja que la corutina termine.
        // FixedUpdate se encargará de mover al mosquito hacia el nuevo
        // puntoPatrullaObjetivo que acabamos de calcular.
    }

    private IEnumerator RutinaRetroceso()
    {
        // Ya nos estamos moviendo hacia atrás (ver FixedUpdate)
        yield return new WaitForSeconds(duracionRetroceso);
        
        // Después de retroceder, entramos en cooldown
        CambiarEstado(Estado.Cooldown);
    }

    private IEnumerator RutinaCooldown()
    {
        puedeMoverse = false;
        yield return new WaitForSeconds(cooldownAtaque);

        // Al terminar el cooldown, decidimos qué hacer
        if (jugador != null)
            CambiarEstado(Estado.Persiguiendo);
        else
            CambiarEstado(Estado.Patrullando);
    }

    // --- Métodos Públicos (llamados por otros scripts) ---

    public void OnDetectarJugador(Transform targetJugador)
    {
        jugador = targetJugador;
        // Solo cambiamos a perseguir si estábamos patrullando
        if (estadoActual == Estado.Patrullando)
        {
            CambiarEstado(Estado.Persiguiendo);
        }
    }

    public void OnPerderJugador()
    {
        jugador = null;
        // Si no estamos en medio de un ataque, volvemos a patrullar
        if (estadoActual == Estado.Persiguiendo)
        {
            CambiarEstado(Estado.Patrullando);
        }
    }

    public void OnGolpearJugador()
    {
        Debug.Log("Mosquito ha golpeado al jugador.");
        // Solo reaccionamos si estábamos embistiendo

            // Aquí puedes añadir lógica de daño
            var playerHealth = jugador.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dañoAlJugador); // Ejemplo de daño
            }
            CambiarEstado(Estado.Retrocediendo);
    
    }

    // --- Utilidad ---

    private void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 laEscala = spriteTransform.localScale;
        laEscala.x *= -1;
        spriteTransform.localScale = laEscala;
    }

    // Implementación de la interfaz IEnemyDamageable
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        RecibirDaño(damage, hitDirection);
    }

    // Función interna para recibir daño
    private void RecibirDaño(int cantidad, Vector2 direccionGolpe)
    {
        vidaActual -= cantidad;

        // Aplicar knockback
        StartCoroutine(AplicarKnockback(direccionGolpe));

        // Efecto visual de daño
        StartCoroutine(FlashDaño());

        // Si la vida llega a 0 o menos, el mosquito muere
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private IEnumerator AplicarKnockback(Vector2 direccionGolpe)
    {
        // Guardar el estado de movimiento actual
        bool podiaMoverseAntes = puedeMoverse;
        puedeMoverse = false;

        // Aplicar la fuerza de knockback
        rb.linearVelocity = direccionGolpe.normalized * fuerzaKnockback;

        // Esperar la duración del knockback
        yield return new WaitForSeconds(duracionKnockback);

        // Restaurar el estado de movimiento solo si no ha muerto
        if (vidaActual > 0)
        {
            puedeMoverse = podiaMoverseAntes;
        }
    }

    private IEnumerator FlashDaño()
    {
        if (spriteRenderer != null)
        {
            // Guardar el color original
            Color colorOriginal = spriteRenderer.color;
            
            // Cambiar a rojo
            spriteRenderer.color = Color.red;
            
            // Esperar
            yield return new WaitForSeconds(duracionFlashDaño);
            
            // Volver al color original si no ha muerto
            if (vidaActual > 0)
            {
                spriteRenderer.color = colorOriginal;
            }
        }
    }

    private void Morir()
    {
        isDead = true;
        spriteRenderer.color = Color.white; // Cambiar color para indicar muerte
        // Desactivar todos los scripts
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // No desactivar este script hasta el final
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // Desactivar scripts en el objeto detector de rango si existe
        if (detectorRango != null)
        {
            MonoBehaviour[] detectorScripts = detectorRango.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in detectorScripts)
            {
                script.enabled = false;
            }
        }

        // Eliminar todos los colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            Destroy(collider);
        }

        // Configurar el Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // Voltear el sprite como si estuviera muerto (90 grados)
        if (spriteTransform != null)
        {
            spriteTransform.rotation = Quaternion.Euler(0f, 0f, 180f);
            spriteTransform.position = new Vector3(spriteTransform.position.x, spriteTransform.position.y - 25f, spriteTransform.position.z);
            Animation anim = spriteTransform.GetComponent<Animation>();
            anim.Stop();
        }

        // Desactivar este script al final
        enabled = false;
    }
    private void OnBecameVisible()
    {
        // Cuando el mosquito entra en pantalla, "despierta".
        estaActivo = true;
        
        // Opcional: si quieres que empiecen a patrullar en cuanto se ven
        // if (estadoActual == Estado.Patrullando)
        // {
        //     CambiarEstado(Estado.Patrullando);
        // }
    }

    private void OnBecameInvisible()
    {
        // Cuando el mosquito sale de pantalla, se "duerme".
        estaActivo = false;
        
        // Importante: Detener su movimiento para que no se aleje flotando
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}