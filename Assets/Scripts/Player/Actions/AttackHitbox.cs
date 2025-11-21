using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    // El script del jugador asignará el daño aquí
    [HideInInspector] public int damage;

    // Propiedad pública para saber si se ha golpeado a un enemigo
    // El 'private set' permite que solo este script pueda cambiar su valor
    public bool enemyHit { get; private set; }

    // Este método se llama cada vez que el objeto se activa
    private void OnEnable()
    {
        // Reseteamos el estado cada vez que comienza un ataque
        enemyHit = false;
    }

    // Dentro del método OnTriggerEnter2D de tu script AttackHitbox.cs
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemyHealth = other.GetComponent<IEnemyDamageable>();
            if (enemyHealth != null)
            {
                // Pasamos la posición del jugador (el padre de la hitbox)
                enemyHealth.TakeDamage(damage, transform.parent.position);
            }
        }
    }

    public interface IEnemyDamageable
    {
        void TakeDamage(int damage, Vector2 damageSourcePosition);
    }
}