using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Se entro en el endpoint "+ other);
        if (other.CompareTag("Player"))
        {
            PlayerRespawn player = other.GetComponent<PlayerRespawn>();
            PlayerController controller = other.GetComponent<PlayerController>();
            if (player != null)
            {
                controller.SavePlayerData();
                player.SetCheckpoint(transform.position);
                controller.SetHealth(controller.maxHealth);
            }
        }
    }
}
