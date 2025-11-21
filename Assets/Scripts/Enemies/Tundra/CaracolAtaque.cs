using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CaracolPatrulla))] // Asegura que el script de patrulla exista
public class CaracolAtaque : MonoBehaviour
{
    [Header("Configuración de Sprites")]
    public Sprite spriteOculto;
    private Sprite spritePrincipal;
    private SpriteRenderer spriteRenderer;

    [Header("Configuración de Ataque")]
    public GameObject espinaPrefab;
    public Transform puntoDisparo;
    public float velocidadEspina = 10f;
    public int danoPorToque = 10; // Daño que hace el caracol al tocarte

    [Header("Configuración de Detección")]
    public LayerMask capaJugador;
    public float distanciaVision = 8f;

    [Header("Tiempos y Delays")]
    public float retrasoAtaque = 0.5f;
    public float duracionOculto = 1.5f;
    public float cooldownAtaque = 3.0f;

    private bool puedeAtacar = true;
    private CaracolPatrulla scriptMovimiento; // Referencia al script de patrulla

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        scriptMovimiento = GetComponent<CaracolPatrulla>(); // Obtiene la referencia
        spritePrincipal = spriteRenderer.sprite;
    }

    void Update()
    {
        if (puedeAtacar)
        {
            DetectarJugador();
        }
    }

    void DetectarJugador()
    {
        // La dirección ahora la obtenemos de la escala del transform
        Vector2 direccion = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, distanciaVision, capaJugador);
        Debug.DrawRay(transform.position, direccion * distanciaVision, Color.red);

        if (hit.collider != null)
        {
            puedeAtacar = false;
            StartCoroutine(RutinaAtaque());
        }
    }

    IEnumerator RutinaAtaque()
    {
        // 1. Detiene el movimiento de patrulla INMEDIATAMENTE
        scriptMovimiento.estaPatrullando = false;

        // 2. Espera el delay
        yield return new WaitForSeconds(retrasoAtaque);

        // 3. Cambia al sprite de "oculto"
        spriteRenderer.sprite = spriteOculto;

        // 4. Lanza las espinas
        LanzarEspinas();

        // 5. Espera mientras está oculto
        yield return new WaitForSeconds(duracionOculto);

        // 6. Vuelve al sprite principal
        spriteRenderer.sprite = spritePrincipal;

        // 7. Espera el cooldown
        yield return new WaitForSeconds(cooldownAtaque);

        // 8. Permite que vuelva a patrullar
        scriptMovimiento.estaPatrullando = true;

        // 9. Resetea la habilidad de atacar
        puedeAtacar = true;
    }

    void LanzarEspinas()
    {
        float[] angulos = { 0f, 45f, 90f, 135f, 180f };
        // Define un factor de velocidad para compensar la falta de gravedad
        // Las de 0, 180 (horizontales) necesitan el mayor impulso extra.
        // Las de 45, 135 (diagonales) necesitan un impulso moderado.
        // La de 90 (vertical) no necesita impulso extra (o muy poco, dependiendo de tu distanciaVision)
        float factorVelocidadLateral = 2f; // Ajusta este valor (ej: 1.3 = 30% más rápido)
        float factorVelocidadDiagonal = 1.5f; // Ajusta este valor (ej: 1.15 = 15% más rápido)

        foreach (float angulo in angulos)
        {
            Vector2 direccion = Quaternion.Euler(0, 0, angulo) * Vector2.right;
            GameObject espina = Instantiate(espinaPrefab, puntoDisparo.position, Quaternion.Euler(0, 0, angulo));
            Rigidbody2D rb = espina.GetComponent<Rigidbody2D>();

            // Determina la velocidad final a aplicar
            float velocidadFinal  = velocidadEspina;

            if (angulo == 90f)
            {
                // Espinas horizontales (0 y 180): les damos la velocidad extra
                velocidadFinal *= factorVelocidadLateral;
            }
            else if (angulo == 45f || angulo == 135f)
            {
                // Espinas diagonales (45 y 135): les damos un poco de velocidad extra
                velocidadFinal *= factorVelocidadDiagonal;
            }
            // Nota: La espina de 90 grados usa la velocidad base (velocidadEspina)

            if (rb != null)
            {
                // Aplicamos la velocidad ajustada
                rb.linearVelocity = direccion * velocidadFinal;
            }
        }
    }

    // --- ¡NUEVO! ---
    // Detecta colisiones para hacer daño por toque
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo hace daño por toque si está patrullando (no si está escondido)
        if (scriptMovimiento.estaPatrullando && collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(danoPorToque);
            }
        }
    }
}