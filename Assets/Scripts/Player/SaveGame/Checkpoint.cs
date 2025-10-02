using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Se entro en el endpoint "+ other);
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.State.SavePlayerData();
                controller.RespawnController.SetCheckpoint(transform.position);
                controller.Health.SetHealth(controller.Health.maxHealth);
            }
        }
    }
}
