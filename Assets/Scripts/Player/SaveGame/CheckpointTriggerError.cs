using UnityEngine;

public class CheckpointTriggerError : MonoBehaviour
{ 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointErrorManager.Instance.SetErrorCheckpoint(transform.position);
        }
    }
}