using UnityEngine;

public class Serpent_Hittable : MonoBehaviour, AttackHitbox.IEnemyDamageable
{
    // Este script necesita saber quién es el "cerebro"
    private Serpent_Attack rootScript;

    void Start()
    {
        // Encontrar el script principal en el objeto padre
        rootScript = GetComponentInParent<Serpent_Attack>();

        if (rootScript == null)
        {
            Debug.LogError("La cabeza no pudo encontrar el script Serpent_Attack en su padre.");
        }
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        rootScript.bossController.TakeDamage(damage);
    }
}