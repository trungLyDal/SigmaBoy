using UnityEngine;

// These 'RequireComponent' lines automatically add components
// to your GameObject if they are missing. It's good practice.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerPlatformerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Header("Ground Check Settings")]
    public Transform groundCheckPoint;   // A reference to the "GroundCheck" object
    public LayerMask groundLayer;        // A reference to the "Ground" layer
    public float groundCheckRadius = 0.2f;

    // Private variables for internal logic
    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;
    private bool jumpRequested = false;

    // Start() is called once when the game begins
    void Start()
    {
        // Get references to our components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // A quick check to make sure you assigned everything in the editor
        if (groundCheckPoint == null)
        {
            Debug.LogError("Ground Check Point not assigned!");
        }
        if (groundLayer == 0)
        {
            Debug.LogWarning("Ground Layer not set!");
        }
    }

    // Update() is called once per frame. Best for checking inputs.
    void Update()
    {
        // --- 1. Get Player Input ---
        horizontalInput = Input.GetAxisRaw("Horizontal"); // -1 for Left, 1 for Right

        // We check for "Jump" input (defaults to Spacebar)
        if (Input.GetButtonDown("Jump"))
        {
            // We set a "request" flag. We don't jump here
            // because physics should only be handled in FixedUpdate.
            jumpRequested = true;
        }

        // --- 2. Flip Player Sprite ---
        // This flips the character's facing direction
        if (horizontalInput > 0f) // Moving Right
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if (horizontalInput < 0f) // Moving Left
        {
            transform.localScale = new Vector3(-1, 1, 1); // Flips the X-axis
        }

        // --- 3. Send Data to the Animator ---
        // We send our movement data to the Animator component
        // so it knows which animation to play.
        
        // "Speed" will be 0 when idle and 1 when moving.
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        
        // We'll use these for Jump/Fall animations later
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isGrounded", isGrounded);
    }

    // FixedUpdate() is called on a fixed timer. Best for all physics.
    void FixedUpdate()
    {
        // --- 1. Check if Grounded ---
        // We draw an invisible circle at the groundCheckPoint's position.
        // If that circle overlaps with *anything* on the "groundLayer"...
        // ...then isGrounded becomes true.
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // --- 2. Handle Movement ---
        // We set the Rigidbody's velocity.
        // We set 'x' based on our input and moveSpeed.
        // We *must* keep the current 'y' velocity (rb.velocity.y),
        // otherwise gravity and jumping will break.
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // --- 3. Handle Jump ---
        if (jumpRequested && isGrounded)
        {
            // We are jumping, so we apply an *instant* upward force.
            // ForceMode2D.Impulse is like a single explosion or push.
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        
        // We reset the jump request flag so we don't jump every frame.
        jumpRequested = false; 
    }

    // This is a helper function so we can see the "foot sensor" in the editor
    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        
        // Draw a red wire sphere at the ground check position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}