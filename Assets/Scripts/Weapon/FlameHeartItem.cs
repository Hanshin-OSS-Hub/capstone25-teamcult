
using UnityEngine;

public class FlameHeartItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            
            var playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
               
                playerHealth.GetFlameHeart();
            }

            if (MusicDirector.Instance != null)
            {
                MusicDirector.Instance.SetFlameMode(true);
            }

            Destroy(gameObject);
        }
    }
}