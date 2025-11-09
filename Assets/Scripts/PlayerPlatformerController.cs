using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerPlatformerController : MonoBehaviour
{
    public Collider2D hitboxCollider;
    public float moveSpeed = 7f;
    private bool isAttacking = false;
    public float jumpForce = 14f;

    private bool isKnockedBack = false;

    private Coroutine knockbackCoroutine;

private bool isFacingRight = true;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.1f; 

    public float jumpBufferTime = 0.1f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (groundCheckPoint == null)
        {
            Debug.LogError("Ground Check Point not assigned!");
        }
        if (groundLayer == 0)
        {
            Debug.LogWarning("Ground Layer not set!");
        }
    }

    void Update()
    {
        if (isKnockedBack) return;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        Flip();
        jumpBufferCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Input.GetButtonUp("Jump"))
        {
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("Attack");
        }

        // Use KeyDown with KeyCode instead of GetButtonDown with the literal string
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Send the "Attack2" signal to the Animator
            animator.SetTrigger("Attack2");
            if (!isAttacking)
            {
                if (horizontalInput > 0f)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                else if (horizontalInput < 0f)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        // Update animator parameters every frame
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        if (Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer))
        {
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
        }
        if (isKnockedBack) return;
        if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }
    }

    public void TriggerKnockback(Vector2 direction, float force, float duration)
    {
        // Stop any previous knockback to prevent conflicts
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        // Start the new knockback
        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force, duration));
    }
    
    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        // 1. Set the state
        isKnockedBack = true;

        // 2. Clear current velocity to make the knockback force consistent
        rb.velocity = Vector2.zero;

        // 3. Apply the knockback force
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // 4. Wait for the knockback duration to end
        yield return new WaitForSeconds(duration);

        // 5. Reset the state
        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

    public void EnableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
        }
    }

    public void AttackLock()
    {
        isAttacking = true;
    }

    public void AttackUnlock()
    {
        isAttacking = false;
    }

    public void DisableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    private void Flip()
    {
        // Check if the input is moving left, but we are facing right
        if (isFacingRight && horizontalInput < 0f)
        {
            isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        // Check if the input is moving right, but we are facing left
        else if (!isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }


public void SetKnockedBack(bool isKnockedBackState)
{
    isKnockedBack = isKnockedBackState;
}

public void ApplyKnockback(Vector2 direction, float force)
{
    rb.AddForce(direction * force, ForceMode2D.Impulse);
}
}