using UnityEngine;

// Coloca este script en tu Tilemap de Agua (el que tiene el CompositeCollider2D)
[RequireComponent(typeof(Collider2D))]
public class WaterZonePhysics : MonoBehaviour
{
    [Header("Física de la Zona")]
    [Tooltip("La FUERZA MÁXIMA que empuja los objetos hacia arriba (cuando están 100% sumergidos).")]
    public float buoyancyForce = 30f;

    [Tooltip("La RESISTENCIA MÁXIMA que aplica el agua. Frena el movimiento en todas direcciones.")]
    public float waterResistance = 5f;

    [Tooltip("Velocidad máxima de subida. Evita el efecto 'yoyó' en la superficie.")]
    public float maxBuoyancySpeed = 1.5f;

    [Tooltip("La etiqueta de los objetos que deben flotar (ej: 'Player').")]
    public string targetTag = "Player";

    [Tooltip("Un pequeño offset para que el objeto flote 'un poco por debajo' de la superficie. Ajusta esto para que se vea bien.")]
    public float surfaceLevelOffset = 0.1f;

    private Collider2D waterCollider;

    private void Start()
    {
        // Obtenemos el collider de la zona de agua al inicio
        waterCollider = GetComponent<Collider2D>();
    }

    // Se ejecuta continuamente mientras un objeto está DENTRO del trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        // 1. Comprobar si es el objeto que queremos afectar
        if (other.CompareTag(targetTag))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            Collider2D objectCollider = other.GetComponent<Collider2D>();

            // 2. Si el objeto tiene un Rigidbody y un Collider
            if (rb != null && objectCollider != null)
            {
                // ---- CÁLCULO DE FLOTABILIDAD PROPORCIONAL ----
                
                // 3. Encontrar el "nivel del agua" (la parte superior del trigger, con el offset)
                float waterLevel = waterCollider.bounds.max.y - surfaceLevelOffset;

                // 4. Encontrar la altura del objeto
                float objectHeight = objectCollider.bounds.size.y;
                
                // 5. Encontrar qué tan profundo está el objeto
                // (Distancia desde el nivel del agua hasta la parte INFERIOR del objeto)
                float submergedAmount = waterLevel - objectCollider.bounds.min.y;

                // 6. Calcular el porcentaje de hundimiento (de 0.0 a 1.0)
                // Mathf.Clamp01 asegura que el valor nunca sea menor a 0 o mayor a 1
                float submergedPercentage = Mathf.Clamp01(submergedAmount / objectHeight);

                // Si el porcentaje es 0 (apenas tocando), no hacemos nada
                if (submergedPercentage <= 0f)
                {
                    return;
                }

                // ---- APLICAR FLOTABILIDAD (Proporcional) ----
                // La fuerza de flotabilidad es ahora (Fuerza Máxima * Porcentaje Hundido)
                float proportionalBuoyancy = buoyancyForce * submergedPercentage;
                rb.AddForce(Vector2.up * proportionalBuoyancy, ForceMode2D.Force);

                // ---- APLICAR RESISTENCIA (Proporcional) ----
                // También hacemos la resistencia proporcional. Más hundido = más resistencia.
                float proportionalResistance = waterResistance * submergedPercentage;
                Vector2 resistance = -rb.linearVelocity * proportionalResistance;
                rb.AddForce(resistance, ForceMode2D.Force);

                // ---- LIMITAR VELOCIDAD DE SUBIDA ----
                // Esto sigue siendo una buena idea para que se "asiente" en la superficie.
                if (rb.linearVelocity.y > maxBuoyancySpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxBuoyancySpeed);
                }
            }
        }
    }
}