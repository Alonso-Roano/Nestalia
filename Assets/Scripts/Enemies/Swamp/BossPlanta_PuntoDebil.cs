using UnityEngine;
using System.Collections;

public class BossPlanta_PuntoDebil : MonoBehaviour, AttackHitbox.IEnemyDamageable
{
    [Header("Efectos (Opcional)")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlantBossController cerebroJefe; // Referencia al nuevo controlador
    private int vidaMaxima_Punto;
    private int vidaActual;
    private bool estaDestruido = false;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Llamado por el BossPlanta_Controller durante el Start.
    /// </summary>
    public void Inicializar( PlantBossController controller, int vida)
    {
        cerebroJefe = controller;
        vidaMaxima_Punto = vida;
        ResetPuntoDebil();
    }

    /// <summary>
    /// Restaura este punto débil a su estado inicial (para el Reset de la arena).
    /// </summary>
    public void ResetPuntoDebil()
    {
        vidaActual = vidaMaxima_Punto;
        estaDestruido = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        
        GetComponent<Collider2D>().enabled = true;
    }

    /// <summary>
    /// Esta es la función que llama el ataque de tu jugador.
    /// </summary>
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (estaDestruido || cerebroJefe == null) return;

        vidaActual -= damage;
        
        // ¡IMPORTANTE! Reporta el daño al controlador para la barra de vida global
        cerebroJefe.ReportDamageTaken(damage);

        StartCoroutine(FlashDaño());

        if (vidaActual <= 0)
        {
            estaDestruido = true;

            // Avisar al cerebro que fuimos destruidos
            cerebroJefe.OnPuntoDebilDestruido();

            // Desactivar este punto débil
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            GetComponent<Collider2D>().enabled = false;
        }
    }

    private IEnumerator FlashDaño()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}