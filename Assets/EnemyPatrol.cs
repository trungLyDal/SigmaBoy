using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float idleTime = 1f;

    [Header("AI / Chasing")]
    public Transform playerTransform; 
    public float attackRange = 1.5f; 
    public float attackCooldown = 2f; 
    
    // --- Private Variables ---
    private Transform currentTarget;
    private Animator animator;
    private Rigidbody2D rb;
    private float idleTimer = 0f;
    private bool isFacingRight = true;

    private bool isChasing = false;
    private float attackTimer = 0f;
    
    // NEW: We use this to tell FixedUpdate what to do
    private float moveDirection = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        currentTarget = pointB;

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Update() is now only for "thinking" and "animating"
        
        // Handle AI state
        if (isChasing)
        {
            RunChaseLogic();
        }
        else
        {
            RunPatrolLogic();
        }
        
        // Handle animations
        if (isChasing || idleTimer <= 0f)
        {
            // We are either chasing or patrolling (not idle)
            animator.SetFloat("Speed", Mathf.Abs(moveDirection));
        }
        else
        {
            // We are idle at a patrol point
            animator.SetFloat("Speed", 0f);
        }

        // Handle attack timer
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // FixedUpdate is for applying physics (moving)
        // We set the vertical velocity to keep gravity
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    void RunPatrolLogic()
    {
        if (idleTimer > 0f)
        {
            // We are waiting, so don't move
            idleTimer -= Time.deltaTime;
            moveDirection = 0f;

            if (idleTimer <= 0f)
            {
                SwitchTarget();
            }
        }
        else // We are moving
        {
            // Get direction to target
            float directionToTarget = currentTarget.position.x > transform.position.x ? 1 : -1;
            moveDirection = directionToTarget;

            // Check if we have reached the target
            if (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(currentTarget.position.x, 0)) < 0.1f)
            {
                // We've reached the point. Start idling.
                idleTimer = idleTime;
            }
        }
    }

    void RunChaseLogic()
    {
        idleTimer = 0f; // Stop any patrol idling

        // 1. Face the player
        if (playerTransform.position.x > transform.position.x && !isFacingRight)
        {
            Flip(); // Face right
        }
        else if (playerTransform.position.x < transform.position.x && isFacingRight)
        {
            Flip(); // Face left
        }

        // 2. Check if player is in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            // --- PLAYER IS IN RANGE ---
            // Stop moving
            moveDirection = 0f;
            
            // Try to attack
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown; // This line is now active
            }
        }
        else
        {
            // --- PLAYER IS OUT OF RANGE ---
            // Move towards them
            float directionToPlayer = playerTransform.position.x > transform.position.x ? 1 : -1;
            moveDirection = directionToPlayer;
        }

        // --- NEW (uncomment) ---
        // Count down attack timer
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void Attack()
    {
        Debug.Log("Enemy is attacking!");
        
        // --- NEW LINE ---
        // Tell the animator to play the attack
animator.SetBool("isAttacking", true);    }

    void SwitchTarget()
    {
        if (currentTarget == pointB)
        {
            currentTarget = pointA;
        }
        else
        {
            currentTarget = pointB;
        }
        Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            moveDirection = 0f; // Stop moving
        }
    }
}