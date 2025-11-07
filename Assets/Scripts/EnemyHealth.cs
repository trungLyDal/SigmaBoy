using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // Public variable to set in the Inspector
    public int maxHealth = 3;
    
    // Private variable to track current health
    private int currentHealth;

    // Start is called when the game begins
    void Start()
    {
        // Set the health to full when the enemy spawns
        currentHealth = maxHealth;
    }

    // This is a public function that our hitbox can call
    public void TakeDamage(int damage)
    {
        // Subtract the damage from our health
        currentHealth -= damage;

        Debug.Log("Enemy took damage! Current health: " + currentHealth);

        // Check if the enemy should be destroyed
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // For now, we just print a message and destroy the GameObject
        Debug.Log("Enemy has been defeated!");
        
        // This removes the enemy from the game scene
        Destroy(gameObject);
    }
}