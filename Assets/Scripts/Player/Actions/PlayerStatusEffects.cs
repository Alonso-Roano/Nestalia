using UnityEngine;
using System.Collections;

public class PlayerStatusEffects : MonoBehaviour
{
    // --- REFERENCIAS A COMPONENTES ---
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private PlayerHealing playerHealing;

    // --- VALORES ORIGINALES ---
    // Movimiento
    private float originalMoveForce;
    private float originalJumpForce;
    private float originalSlowFallMultiplier;
    
    // Combate
    private int originalAttackDamage;
    private float originalPogoForce;

    // Vida y Defensa
    private int originalMaxHealth;
    private float originalKnockForce;

    // Curación
    private float originalHealRate;
    
    // Corutina activa para evitar solapamiento
    private Coroutine activeEffectCoroutine;

    void Awake()
    {
        // Obtenemos las referencias a los otros componentes
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealing = GetComponent<PlayerHealing>();

        // Guardamos los valores originales para poder restaurarlos después
        if (playerMovement != null)
        {
            originalMoveForce = playerMovement.moveForce;
            originalJumpForce = playerMovement.jumpForce;
            originalSlowFallMultiplier = playerMovement.slowFallMultiplier;
        }
        if (playerAttack != null)
        {
            originalAttackDamage = playerAttack.attackDamage;
            originalPogoForce = playerAttack.pogoForce;
        }
        if (playerHealth != null)
        {
            originalMaxHealth = playerHealth.maxHealth;
            originalKnockForce = playerHealth.knockForce;
        }
        if (playerHealing != null)
        {
            originalHealRate = playerHealing.initialHealRate;
        }
    }

    // Método público que es llamado para aplicar un efecto
    public void ApplyEffect(ItemBlueprint blueprint)
    {
        // Si ya hay un efecto, lo detenemos antes de empezar el nuevo
        if (activeEffectCoroutine != null)
        {
            StopCoroutine(activeEffectCoroutine);
            RevertStats(); // Lo restauramos para evitar bugs
        }

        activeEffectCoroutine = StartCoroutine(StatusEffectCoroutine(blueprint));
    }

    private IEnumerator StatusEffectCoroutine(ItemBlueprint blueprint)
    {
        Debug.Log($"Activando efecto '{blueprint.itemName}' ({blueprint.change}) por {blueprint.boostDuration} segundos.");

        // --- APLICAMOS LOS EFECTOS USANDO UN SWITCH ---
        switch (blueprint.change)
        {
            case "SpeedBoost":
                if (playerMovement != null) playerMovement.moveForce = originalMoveForce * blueprint.boostMultiplier;
                break;

            case "JumpBoost":
                if (playerMovement != null) playerMovement.jumpForce = originalJumpForce * blueprint.boostMultiplier;
                break;

            case "DamageUp":
                if (playerAttack != null) playerAttack.attackDamage = (int)(originalAttackDamage * blueprint.boostMultiplier);
                break;

            case "HealthIncrease":
                if (playerHealth != null)
                {
                    playerHealth.maxHealth = (int)(originalMaxHealth * blueprint.boostMultiplier);
                    // Opcional: Curar al jugador al máximo de la nueva vida
                    // playerHealth.Heal(playerHealth.maxHealth); 
                }
                break;
            
            case "FeatherFall":
                if (playerMovement != null) playerMovement.slowFallMultiplier = originalSlowFallMultiplier * blueprint.boostMultiplier;
                break;

            case "KnockbackResist": // Un multiplicador < 1 reduce el knockback
                if (playerHealth != null) playerHealth.knockForce = originalKnockForce * blueprint.boostMultiplier;
                break;

            case "PogoPower":
                if (playerAttack != null) playerAttack.pogoForce = originalPogoForce * blueprint.boostMultiplier;
                break;

            case "HealingSpeed":
                if (playerHealing != null) playerHealing.initialHealRate = originalHealRate * blueprint.boostMultiplier;
                break;
                
            default:
                Debug.LogWarning($"El efecto con el change '{blueprint.change}' no está definido.");
                break;
        }

        // Esperamos la duración del efecto
        yield return new WaitForSeconds(blueprint.boostDuration);

        // --- REVERTIMOS LOS EFECTOS ---
        Debug.Log($"Efecto de '{blueprint.itemName}' ha terminado.");
        RevertStats();
        activeEffectCoroutine = null;
    }

    private void RevertStats()
    {
        // Restauramos todos los valores a su estado original
        if (playerMovement != null)
        {
            playerMovement.moveForce = originalMoveForce;
            playerMovement.jumpForce = originalJumpForce;
            playerMovement.slowFallMultiplier = originalSlowFallMultiplier;
        }
        if (playerAttack != null)
        {
            playerAttack.attackDamage = originalAttackDamage;
            playerAttack.pogoForce = originalPogoForce;
        }
        if (playerHealth != null)
        {
            playerHealth.maxHealth = originalMaxHealth;
            playerHealth.knockForce = originalKnockForce;
        }
        if (playerHealing != null)
        {
            playerHealing.initialHealRate = originalHealRate;
        }
    }
}