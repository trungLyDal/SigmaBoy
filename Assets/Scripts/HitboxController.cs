using UnityEngine;

public class HitboxController : MonoBehaviour
{
    // This function is called by Unity's physics engine
    // whenever something *enters* this trigger.
    void OnTriggerEnter2D(Collider2D other)
    {
        // We check if the object we hit has the "Enemy" tag.
        if (other.CompareTag("Enemy"))
        {
            // If it's an enemy, print a "Hit!" message.
            Debug.Log("Hit the enemy!");
            
            // Later, we can replace this with:
            // other.GetComponent<EnemyHealth>().TakeDamage(10);
        }
    }
}