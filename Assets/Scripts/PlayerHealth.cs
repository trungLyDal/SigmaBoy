using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    // 💡 NEW: Animator Component Reference
    private Animator animator;
    
    // 💡 NEW: Optional - A way to prevent spamming damage/hurt animation
    private bool isInvulnerable = false;
    public float invulnerabilityTime = 0.5f; // Time player flashes/can't take damage

    void Start()
    {
        currentHealth = maxHealth;
        // 💡 NEW: Get the Animator component
        animator = GetComponent<Animator>();

        // Fail-safe check
        if (animator == null)
        {
            Debug.LogError("PlayerHealth requires an Animator component on the GameObject!");
        }
    }

    public void TakeDamage(int damage)
    {
        // 💡 NEW: Check if player can take damage
        if (isInvulnerable || currentHealth <= 0)
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage! Remaining Health: " + currentHealth);

        // 💡 NEW: Trigger the isHurt animation
        if (animator != null)
        {
            // Use SetTrigger if you set 'isHurt' as a Trigger parameter in the Animator
            animator.SetTrigger("isHurt"); 
            
            // OR use SetBool if you set 'isHurt' as a Bool, but you must reset it later (see notes below)
            // animator.SetBool("isHurt", true);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 💡 NEW: Start invulnerability frames
            StartCoroutine(BecomeTemporarilyInvulnerable());
        }
    }

    void Die()
    {
        Debug.Log("Game Over!");
        // 💡 Optional: Trigger Death animation before disabling
        if (animator != null)
        {
            animator.SetBool("isDead", true); 
        }
        
        // For now, we'll just disable the player components
        // Wait a moment for the death animation to play, then disable
        // Invoke("DisableGameObject", 1.0f); 
        gameObject.SetActive(false); 
    }
    
    // 💡 NEW: Coroutine for invulnerability (recommended for platformers)
    System.Collections.IEnumerator BecomeTemporarilyInvulnerable()
    {
        isInvulnerable = true;

        // Optionally, add visual feedback here (like flashing the sprite)
        // SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // for (int i = 0; i < 5; i++)
        // {
        //     sr.enabled = !sr.enabled;
        //     yield return new WaitForSeconds(invulnerabilityTime / 10);
        // }
        // sr.enabled = true;

        yield return new WaitForSeconds(invulnerabilityTime);

        isInvulnerable = false;
    }
}