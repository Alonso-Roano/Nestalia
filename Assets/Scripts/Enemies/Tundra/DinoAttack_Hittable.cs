using UnityEngine;


// 🔹 Renombrado de Serpent_Hittable
public class DinoAttack_Hittable : MonoBehaviour, AttackHitbox.IEnemyDamageable
{
    // Este script necesita saber quién es el "cerebro" del proyectil
    private DinoAttack_Movement rootScript;

    void Start()
    {
        // 🔹 Busca el nuevo script principal en el padre
        rootScript = GetComponentInParent<DinoAttack_Movement>();

        if (rootScript == null)
        {
            Debug.LogError("El punto débil no pudo encontrar el script DinoAttack_Movement en su padre.");
        }
    }

    // Cuando el jugador golpea este objeto...
    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        // ...le baja la vida al jefe principal.
        if (rootScript != null && rootScript.bossController != null)
        {
            rootScript.bossController.TakeDamage(damage);
        }
    }
}