using UnityEngine;
using System.Collections; 

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    // Inspector Variables for Setup and Tuning
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Combat")]
    public Collider2D enemyHitboxCollider; // Legacy attack trigger (can be replaced by AttackPoint)
    public float attackRange = 1.5f;        // Distance to stop and start attacking
    public float attackCooldown = 2f;       // Time between attacks (and the duration the enemy pauses)
    
    [Header("Damage Event Setup")]
    public int attackDamage = 1;              // Damage value dealt to the player
    public LayerMask playerLayer;             // LayerMask for the Player
    public Transform attackPoint;             // Position where the attack check originates
    public float attackRadius = 0.5f;         // Radius of the precise attack check

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float idleTime = 1f;

    [Header("AI / Chasing")]
    public Transform playerTransform;
    public float sightRange = 5f; 
    public float chaseStopDistance = 10f;

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
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentTarget = pointB;

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }
    }

    void Update()
    {
        // 1. Handle attack timer countdown (Used for attack frequency and cooldown pause)
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        // 2. State Check: Sight and Chase Transition (Logic unchanged)
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= sightRange)
            {
                isChasing = true;
            }
            else if (isChasing && distanceToPlayer > chaseStopDistance)
            {
                isChasing = false;
            }
        }
        
        // 3. Handle AI state execution
        if (isChasing && playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Priority 1: Attack (Can only start if cooldown is zero)
            if (distanceToPlayer <= attackRange && attackTimer <= 0f)
            {
                Attack();
                // Timer will be set in FinishAttack() for better sync, but setting it here is also fine.
                RunChaseLogic(true); // Stop movement during attack
            }
            else if (attackTimer > 0f)
            {
                RunChaseLogic(true); // Stop movement while on cooldown
            }
            // Priority 3: Chase (Move towards player)
            else
            {
                RunChaseLogic(false); 
            }
        }
        else
        {
            // Default Priority: Patrol
            RunPatrolLogic();
        }

        // 4. Handle animations 
        animator.SetFloat("Speed", Mathf.Abs(moveDirection));
    }

    void FixedUpdate()
    {
        // Only move if not in the middle of an attack or cooldown
        // The Attack/Cooldown stop is handled by moveDirection = 0 in Update
        
        if (animator.GetBool("isAttacking"))
        {
            // Stop movement during active animation
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            // Apply movement direction calculated in Update
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
            float directionToTarget = currentTarget.position.x > transform.position.x ? 1 : -1;
            moveDirection = directionToTarget;

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

    void RunChaseLogic(bool isExecutingAttackOrCooldown)
    {
        idleTimer = 0f; 

        // 1. Face the player (Logic unchanged)
        float directionToPlayer = playerTransform.position.x > transform.position.x ? 1 : -1;
        bool shouldBeFacingRight = directionToPlayer > 0;
        
        if (shouldBeFacingRight != isFacingRight)
        {
            Flip(); 
        }

        if (isExecutingAttackOrCooldown)
        {
            moveDirection = 0f; 
        }
        else
        {
            moveDirection = directionToPlayer; 
        }
    }

    void Attack()
    {
        animator.SetBool("isAttacking", true);
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

    public void FinishAttack()
    {
        animator.SetBool("isAttacking", false);
        attackTimer = attackCooldown; 
        Debug.Log("Attack finished. Starting cooldown.");
    }
    
    // This function is triggered by an Animation Event at the exact moment of impact.
    public void DealDamageToPlayer()
    {
        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint not assigned! Cannot deal damage precisely. Check the Inspector.");
            return;
        }
        
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hitPlayers)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // This call triggers the player's 'isHurt' animation
                playerHealth.TakeDamage(attackDamage, gameObject);
                Debug.Log($"Enemy hit {hit.name} for {attackDamage} damage on Animation Event frame.");
                return; 
            }
        }
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


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = true;
        }
    }

private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}