using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float deathAnimationTime = 1f; // How long is the death animation?
    
    private int currentHealth;
    private Animator animator; // NEW: A reference to the Animator
    private bool isDead = false; // NEW: A flag to stop multiple deaths

    void Start()
    {
        currentHealth = maxHealth;
        
        // NEW: Get the Animator component on this enemy
        animator = GetComponent<Animator>(); 
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log("Enemy took damage! Current health: " + currentHealth);

        // This is your new logic
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // --- NEW LINE ---
            // If we didn't die, play the take hit animation
            animator.SetTrigger("TakeHit");
            // --- END NEW LINE ---
        }
    }

    void Die()
    {
        // NEW: This function is now completely different

        // 1. Set the isDead flag so this can't be called again
        isDead = true;
        Debug.Log("Enemy has been defeated!");

        // 2. Tell the Animator to play the "Death" animation
        animator.SetTrigger("Death");

        // 3. Disable the enemy's collider so it can't be hit anymore
        GetComponent<Collider2D>().enabled = false;
        
        // (Optional) If you add a movement script, you'd disable it here too
        // GetComponent<EnemyMovement>().enabled = false;

        // 4. Destroy this enemy GameObject, but *wait* for the animation to finish
        // The 'deathAnimationTime' should be the length (in seconds) of your death clip
        Destroy(gameObject, deathAnimationTime);
    }
}