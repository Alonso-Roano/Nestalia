using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    [SerializeField] private Vector2 velocidadMovimiento;
    
    private Material material;
    private Rigidbody2D jugadorRB;

    void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
        jugadorRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 offset = (jugadorRB.linearVelocity.x * velocidadMovimiento) * Time.deltaTime * 0.0001f;

        // Aplicamos el desplazamiento a la textura del material.
        material.mainTextureOffset += offset;
    }
}