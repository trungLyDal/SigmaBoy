using UnityEngine;

public class HitboxController : MonoBehaviour
{
    // This function is called by Unity's physics engine
    // whenever something *enters* this trigger.
   void OnTriggerEnter2D(Collider2D other)
    {
        // We still check for the "Enemy" tag
        if (other.CompareTag("Enemy"))
        {
            // --- THIS IS THE NEW CODE ---
            
            // Try to get the EnemyHealth script from the object we hit
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            // Check if the enemy actually has the health script
            if (enemy != null)
            {
                // If it does, call its TakeDamage function
                enemy.TakeDamage(1); // We'll say our attack does 1 damage
            }
            
            // --- END NEW CODE ---
        }
    }
}