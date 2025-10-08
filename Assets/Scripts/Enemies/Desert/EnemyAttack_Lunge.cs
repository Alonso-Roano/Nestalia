using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAttack_Lunge : MonoBehaviour
{
    // Evento para notificar a la IA que el ataque ha terminado
    public event Action OnAttackFinished;

    [Header("Configuración de Embestida")]
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private float chargeSpeed = 100f;
    [SerializeField] private float attackLungeForce = 250f;
    [SerializeField] private float recoilForce = 150f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    private Rigidbody2D rb;
    private bool isReadyToAttack = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // La IA llama a este método para iniciar el ataque
    public bool CanAttack()
    {
        return isReadyToAttack;
    }

    public void PerformAttack(Transform target)
    {
        if (!isReadyToAttack) return;
        
        StartCoroutine(AttackSequence(target));
    }

    private IEnumerator AttackSequence(Transform target)
    {
        isReadyToAttack = false;
        
        // Detener movimiento
        rb.linearVelocity = Vector2.zero;

        // Fase 1: Cargar hacia atrás
        float chargeDirection = Mathf.Sign(transform.position.x - target.position.x);
        rb.linearVelocity = new Vector2(chargeDirection * chargeSpeed, 0);
        yield return new WaitForSeconds(chargeTime);

        // Detenerse brevemente
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        // Fase 2: Embestida
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        rb.AddForce(directionToTarget * attackLungeForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);

        // Fase 3: Retroceso
        rb.linearVelocity = Vector2.zero;
        Vector2 recoilDirection = -directionToTarget;
        rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
        
        // Notificar que la secuencia física del ataque terminó
        OnAttackFinished?.Invoke();

        // Fase 4: Enfriamiento
        yield return new WaitForSeconds(attackCooldown);
        isReadyToAttack = true;
    }
}