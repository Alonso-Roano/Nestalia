using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CaracolPatrulla : MonoBehaviour
{
    [Header("Configuración de Patrulla")]
    public float velocidadMovimiento = 2f;
    [Tooltip("Distancia total que se moverá a la derecha desde su punto inicial.")]
    public float distanciaPatrulla = 5f; 
    public float toleranciaLlegada = 0.1f; // Qué tan cerca debe estar para cambiar de objetivo

    [HideInInspector]
    public bool estaPatrullando = true; // El script de ataque controlará esto

    private Rigidbody2D rb;
    private float limiteIzquierdo; // El punto de origen
    private float limiteDerecho;   // El punto de origen + distancia
    private float objetivoActual;  // La coordenada X del objetivo
    private bool mirandoDerecha = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Guarda el punto de origen y calcula los límites
        limiteIzquierdo = transform.position.x;
        limiteDerecho = limiteIzquierdo + distanciaPatrulla;

        // Empieza moviéndose hacia el límite derecho
        objetivoActual = limiteDerecho;
        mirandoDerecha = true;

        if (distanciaPatrulla <= 0)
        {
            Debug.LogWarning("La distancia de patrulla es 0 o negativa. El caracol no se moverá.");
            estaPatrullando = false;
        }
    }

    void FixedUpdate()
    {
        // Si no está patrullando (porque está atacando), se detiene.
        if (!estaPatrullando)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 1. Determina la velocidad actual basada en el objetivo
        float velocidadActual;
        if (objetivoActual == limiteDerecho)
        {
            velocidadActual = velocidadMovimiento; // Mover a la derecha
        }
        else
        {
            velocidadActual = -velocidadMovimiento; // Mover a la izquierda
        }

        // 2. Mueve el Rigidbody
        rb.linearVelocity = new Vector2(velocidadActual, rb.linearVelocity.y);

        // 3. Comprueba la dirección y gira si es necesario
        if (velocidadActual > 0 && !mirandoDerecha) Girar();
        else if (velocidadActual < 0 && mirandoDerecha) Girar();

        // 4. Comprueba si ha llegado al objetivo (solo en el eje X)
        if (Mathf.Abs(transform.position.x - objetivoActual) < toleranciaLlegada)
        {
            // Cambia de objetivo
            if (objetivoActual == limiteDerecho)
            {
                objetivoActual = limiteIzquierdo;
            }
            else
            {
                objetivoActual = limiteDerecho;
            }
        }
    }

    private void Girar()
    {
        mirandoDerecha = !mirandoDerecha;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
}