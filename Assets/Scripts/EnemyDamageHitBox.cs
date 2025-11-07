using UnityEngine;

public class EnemyDamageHitbox : MonoBehaviour
{
    public int damageAmount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we hit is the Player
        if (other.CompareTag("Player"))
        {
            // Try to get the PlayerHealth component
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
        }
    }
}