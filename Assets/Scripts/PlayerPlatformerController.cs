using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerPlatformerController : MonoBehaviour
{
    public Collider2D hitboxCollider;
    public float moveSpeed = 7f;
    private bool isAttacking = false;
    public float jumpForce = 14f;

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
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpBufferCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
jumpBufferCounter = jumpBufferTime;        }

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

        if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }
        
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
}