using UnityEngine;
using System.Collections;

public class CaimanDeFangoAI : MonoBehaviour
{
    [Header("Configuración de Emboscada")]
    [Tooltip("El tiempo (en segundos) que el caimán se queda visible (idle)")]
    [SerializeField] private float duracionIdleVisible = 5f;

    [Tooltip("Tiempo mínimo que espera oculto antes de intentar aparecer")]
    [SerializeField] private float tiempoMinEsperaOculto = 3f;

    [Tooltip("Tiempo máximo que espera oculto antes de intentar aparecer")]
    [SerializeField] private float tiempoMaxEsperaOculto = 8f;

    [Tooltip("Probabilidad (de 0.0 a 1.0) de aparecer después de esperar")]
    [Range(0, 1)]
    [SerializeField] private float probabilidadAparecer = 0.5f;

    [Header("Configuración de Ataque")]
    [Tooltip("El daño que hace la mordida al jugador")]
    [SerializeField] private int dañoAlJugador = 20;

    [Tooltip("El tiempo (en segundos) que dura la animación de 'mordida'")]
    [SerializeField] private float duracionAtaque = 0.5f;

    [Header("Referencias")]
    [Tooltip("Asigna el Animator del Caimán aquí")]
    [SerializeField] private Animator animator;

    [Tooltip("Asigna el SpriteRenderer del Caimán aquí")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Nombres de las animaciones
    private const string ANIM_IDLE = "cocoidle";
    private const string ANIM_ATACAR = "cocoatack";

    private enum Estado { Oculto, IdleVisible, Atacando }
    private Estado estadoActual;

    private bool jugadorEstaEnElTrigger = false;
    private Transform jugador;

    private Coroutine rutinaCiclo;
    private bool esVisible = false;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Comienza oculto
        spriteRenderer.enabled = false;
        estadoActual = Estado.Oculto;

        rutinaCiclo = StartCoroutine(RutinaCicloDeVida());
    }

    private IEnumerator RutinaCicloDeVida()
    {
        while (true)
        {
            // --- 1. FASE OCULTO ---
            estadoActual = Estado.Oculto;
            spriteRenderer.enabled = false;

            float tiempoEspera = Random.Range(tiempoMinEsperaOculto, tiempoMaxEsperaOculto);
            yield return new WaitForSeconds(tiempoEspera);

            // --- 2. FASE DE DECISIÓN ---
            if (Random.value <= probabilidadAparecer)
            {
                // Aparece en modo idle
                estadoActual = Estado.IdleVisible;
                spriteRenderer.enabled = true;
                animator.Play(ANIM_IDLE);

                // Si el jugador ya está en el trigger, ataca al instante
                if (jugadorEstaEnElTrigger)
                {
                    StartCoroutine(RutinaDeAtaque());
                    yield break;
                }

                // Espera visible unos segundos
                yield return new WaitForSeconds(duracionIdleVisible);
            }
        }
    }

    private IEnumerator RutinaDeAtaque()
    {
        // Detenemos la rutina de vida, no todas las corutinas
        if (rutinaCiclo != null)
        {
            StopCoroutine(rutinaCiclo);
            rutinaCiclo = null;
        }

        estadoActual = Estado.Atacando;
        Debug.Log("¡Caimán: Muerde!");
        animator.Play(ANIM_ATACAR);

        // --- Lógica de Daño ---
        if (jugador != null)
        {
            var playerHealth = jugador.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dañoAlJugador);
            }
        }

        // Esperar la duración del ataque
        yield return new WaitForSeconds(duracionAtaque);

        // 🔹 Restablecer animación e invisibilidad
        animator.Play(ANIM_IDLE);
        yield return new WaitForSeconds(0.1f); // Pequeña pausa visual
        spriteRenderer.enabled = false;

        // Reinicia el ciclo de vida
        rutinaCiclo = StartCoroutine(RutinaCicloDeVida());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEstaEnElTrigger = true;
            jugador = other.transform;

            if (estadoActual == Estado.IdleVisible)
            {
                StartCoroutine(RutinaDeAtaque());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEstaEnElTrigger = false;
        }
    }
}
