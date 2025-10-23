using UnityEngine;

public class LevelOneStart : MonoBehaviour
{
    void Start()
    {
        PlayerController playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        Debug.Log(playerController != null);
        if (playerController != null)
        {
            playerController.transform.position = new Vector3(400f, 500f, transform.position.z);
            playerController.Health.SetHealth(20);
        }
    }
}
