using UnityEngine;

public class EnemyStatic : MonoBehaviour
{
    public int damage = 10;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other.gameObject);
    }

    private void TryApplyDamage(GameObject hitObject)
    {
        PlayerHealth playerHandler = hitObject.GetComponent<PlayerHealth>();

        if (playerHandler != null)
        {
            playerHandler.TakeDamage(damage, transform); 
        }
    }
}