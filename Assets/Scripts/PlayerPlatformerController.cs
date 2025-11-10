using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerPlatformerController : MonoBehaviour
{
    public HitboxController hitbox;
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
    private TrailRenderer trailRenderer; 

    private float horizontalInput;
    private bool isGrounded;

    [Header("Dashing")]
    [SerializeField] private float dashVelocity = 10f;
    [SerializeField] private float dashDuration = 1f;
    private Vector2 dashDirection;
    private bool isDashing;
    private bool canDash = true;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>(); 

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
        
        if (!isDashing) 
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        
        Flip();
        jumpBufferCounter -= Time.deltaTime;

        var dashInput = Input.GetKeyDown(KeyCode.LeftShift); 

        if (dashInput && canDash && !isDashing)
        {
            isDashing = true;
            canDash = false;
            dashDirection = new Vector2(horizontalInput, Input.GetAxisRaw("Vertical")).normalized; 
            trailRenderer.emitting = true;
            if (dashDirection == Vector2.zero)
            {
                dashDirection = new Vector2(transform.localScale.x, 0); 
            }
            StartCoroutine(StopDashing());
        }

        animator.SetBool("isDashing", isDashing);


        if (isGrounded)
        {
            canDash = true;
        }

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
        
        if (Input.GetKeyDown(KeyCode.R))
        {
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

        if (isDashing)
        {
            rb.velocity = dashDirection * dashVelocity;
            return; // Dashing overrides all other movement
        }

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

    private IEnumerator StopDashing() 
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        trailRenderer.emitting = false;
    }

    public void TriggerKnockback(Vector2 direction, float force, float duration)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

    public void EnableAttack1Hitbox()
    {
        if (hitbox != null)
        {
            hitbox.damageAmount = 1;
            hitbox.GetComponent<Collider2D>().enabled = true;
        }
    }

    public void EnableAttack2Hitbox()
    {
        if (hitbox != null)
        {
            hitbox.damageAmount = 2;
            hitbox.GetComponent<Collider2D>().enabled = true;
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
        if (hitbox != null)
        {
            hitbox.GetComponent<Collider2D>().enabled = false;
        }
    }

    private void Flip()
    {
        // Don't flip while attacking or dashing
        if (isAttacking || isDashing) return; 

        if (isFacingRight && horizontalInput < 0f)
        {
            isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
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