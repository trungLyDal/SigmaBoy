using UnityEngine;
using System.Collections; // Needed for the Coroutine in TakeHit if you use it later

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    // Inspector Variables for Setup and Tuning
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Combat")]
    public Collider2D enemyHitboxCollider; // Reference to the enemy's attack trigger
    public float attackRange = 1.5f;        // Distance to stop and start attacking
    public float attackCooldown = 2f;       // Time between attacks

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float idleTime = 1f;

    [Header("AI / Chasing")]
    public Transform playerTransform;
    public float sightRange = 5f; // NEW: The distance the enemy can 'see' the player
    public float chaseStopDistance = 10f; // NEW: Distance to stop chasing if player leaves

    // --- Private Components & State ---
    private Transform currentTarget;
    private Animator animator;
    private Rigidbody2D rb;
    private float idleTimer = 0f;
    private bool isFacingRight = true;

    private bool isChasing = false;
    private float attackTimer = 0f;
    private float moveDirection = 0f;

    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentTarget = pointB;

        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        // IMPORTANT: Check Rigidbody2D Body Type in Inspector. It must be Dynamic or Kinematic, NOT Static.
    }

    void Update()
    {
        // 1. Handle attack timer countdown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        // 2. State Check: Sight and Chase Transition
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Start Chasing: If player is within sight range
            if (distanceToPlayer <= sightRange)
            {
                isChasing = true;
            }
            // Stop Chasing: If player leaves the dedicated stop range
            else if (isChasing && distanceToPlayer > chaseStopDistance)
            {
                isChasing = false;
            }
        }
        
        // 3. Handle AI state execution
        if (isChasing && playerTransform != null)
        {
            // Highest Priority: Attack Check
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= attackRange && attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown;
                RunChaseLogic(true); // Stop movement for attack
            }
            else
            {
                // Priority 2: Chase Logic
                RunChaseLogic(false); // Move towards player
            }
        }
        else
        {
            // Default Priority: Patrol
            RunPatrolLogic();
        }

        // 4. Handle animations (Set Speed parameter for Run/Idle animations)
        // Ensure "Speed" parameter is set as a Float in the Animator.
        animator.SetFloat("Speed", Mathf.Abs(moveDirection));
    }

    void FixedUpdate()
    {
        // FixedUpdate is for applying physics (movement)

        // Only move if not in the middle of an attack animation
        if (animator.GetBool("isAttacking"))
        {
            // Stop movement during attack
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            // Otherwise, move horizontally based on the AI's current direction.
            rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        }
    }

    void RunPatrolLogic()
    {
        if (idleTimer > 0f)
        {
            // Idling at point
            idleTimer -= Time.deltaTime;
            moveDirection = 0f;

            if (idleTimer <= 0f)
            {
                SwitchTarget();
            }
        }
        else // Moving
        {
            // Determine direction to the current target point
            float directionToTarget = currentTarget.position.x > transform.position.x ? 1 : -1;
            moveDirection = directionToTarget;

            // NEW: Check if the enemy needs to turn to face the target
            bool shouldBeFacingRight = directionToTarget > 0;
            if (shouldBeFacingRight != isFacingRight)
            {
                Flip(); 
            }

            // Check if point is reached (horizontal check only)
            if (Mathf.Abs(transform.position.x - currentTarget.position.x) < 0.1f)
            {
                idleTimer = idleTime;
            }
        }
    }

    void RunChaseLogic(bool isExecutingAttack)
    {
        idleTimer = 0f; // Stop patrol timer

        // 1. Face the player
        float directionToPlayer = playerTransform.position.x > transform.position.x ? 1 : -1;
        bool shouldBeFacingRight = directionToPlayer > 0;
        
        if (shouldBeFacingRight != isFacingRight)
        {
            Flip(); 
        }

        if (isExecutingAttack)
        {
            moveDirection = 0f; // Stop movement when attacking
        }
        else
        {
            // Chase: Move towards the player
            moveDirection = directionToPlayer;
        }
    }

    void Attack()
    {
        Debug.Log("Enemy is attacking!");

        // 1. Set the Bool for the Animator (enables Attack state transition)
        // Ensure "isAttacking" is a Bool parameter in the Animator.
        animator.SetBool("isAttacking", true);

        // 2. Force stop movement for responsiveness
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void SwitchTarget()
    {
        currentTarget = (currentTarget == pointB) ? pointA : pointB;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    // --- Animation Event Functions (Called by the Attack animation clip) ---
    // !!! IMPORTANT: This MUST be called as an Event near the END of your Attack animation clip !!!
    public void FinishAttack()
    {
        // This is the crucial line to reset the Bool and allow the Animator to leave the Attack state.
        animator.SetBool("isAttacking", false);
        Debug.Log("Attack finished, isAttacking set to false.");
    }
    
    // Hitbox control functions, called from animation events
    public void EnableHitbox()
    {
        if (enemyHitboxCollider != null) { enemyHitboxCollider.enabled = true; }
    }

    public void DisableHitbox()
    {
        if (enemyHitboxCollider != null) { enemyHitboxCollider.enabled = false; }
    }

    // --- Detection Triggers (Optional, can be used for a closer range alert) ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        // This makes the enemy alert even if sight checks miss (e.g., player jumps behind it)
        if (other.CompareTag("Player"))
        {
            isChasing = true;
        }
    }
}