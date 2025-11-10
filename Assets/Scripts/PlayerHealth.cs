using UnityEngine;
using System.Collections; // Needed for the IEnumerator/Coroutine

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    private UIManager uiManager;

[Header("Knockback Settings")]
    public float knockbackForce = 15f; 
    public float knockbackDuration = 0.2f;

    [Header("Components")]
   
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D mainCollider;
private PlayerPlatformerController playerMovementScript;
    [Header("Damage Settings")]
    private bool isInvulnerable = false;
    public float invulnerabilityTime = 0.5f; 
    public float deathCleanupDelay = 1.5f; 

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>(); 

        playerMovementScript = GetComponent<PlayerPlatformerController>();

        uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SetMaxHealth(maxHealth);
        }

        // Fail-safe check
        if (animator == null || rb == null || mainCollider == null)
        {
             Debug.LogError("PlayerHealth is missing a required component (Animator, Rigidbody2D, or Collider2D) on the GameObject!");
        }
    }

public void TakeDamage(int damage, GameObject damageSource)
{
    if (isInvulnerable || currentHealth <= 0)
    {
        return;
    }

    currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage! Remaining Health: " + currentHealth);
    
    if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth);
        }

    // Trigger the isHurt animation
    if (animator != null)
    {
        animator.SetTrigger("isHurt");
    }

    if (playerMovementScript != null && damageSource != null)
    {
        // Calculate direction away from the enemy
        Vector2 knockbackDirection = (transform.position - damageSource.transform.position).normalized;
        
        // Add a slight upward lift
        knockbackDirection.y = (knockbackDirection.y * 0.5f) + 0.5f; 

        // Call the controller's knockback function
        playerMovementScript.TriggerKnockback(knockbackDirection, knockbackForce, knockbackDuration);
    }

    if (currentHealth <= 0)
    {
        Die();
    }
    else
    {
        // Start invulnerability frames
        StartCoroutine(BecomeTemporarilyInvulnerable());
    }
}
    


    void Die()
{
    currentHealth = 0;
    Debug.Log("Game Over! Player died.");
    
    if (playerMovementScript != null) 
    {
        playerMovementScript.enabled = false;
    }
    // --- END OF CHANGE ---

    // Stop all Rigidbody movement and freeze the body's position
    if (rb != null) 
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // Lock physics
    }

        // 2. Trigger the Death Animation
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    
    if (uiManager != null)
    {
        uiManager.ShowGameOverScreen();
    }

    StartCoroutine(HandleDeathCleanup());
}

// Updated Coroutine to include the single-frame wait
// Coroutine to handle waiting for the animation before scene cleanup
IEnumerator HandleDeathCleanup()
{
    // Wait for the duration of your death animation (1.067s, 1.2s is a safe value)
    yield return new WaitForSeconds(deathCleanupDelay); 
    
    // --- FINAL FIX FOR CUT-OFF ANIMATION ---
    if (animator != null)
    {
        // 1. Manually turn OFF the isDead parameter. 
        // This forces the Animator to take the Death -> Exit transition.
        animator.SetBool("isDead", false);
    }
    
    // 2. Wait one final frame for the Animator to process the exit transition.
    yield return null; 
    
    // --- Cleanup ---
    // ONLY NOW, after the animation exit is complete, disable components and the GameObject.
    if (mainCollider != null) mainCollider.enabled = false;
    if (rb != null) rb.isKinematic = false;

    // Final action: Hide player object and/or trigger game over
    gameObject.SetActive(false); 
}
    
    // Coroutine for invulnerability
    IEnumerator BecomeTemporarilyInvulnerable()
    {
        isInvulnerable = true;

        yield return new WaitForSeconds(invulnerabilityTime);

        isInvulnerable = false;
    }
}