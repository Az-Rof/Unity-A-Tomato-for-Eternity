using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    // State machine for AI — enemy seeks and attacks plants, falls back to player
    private enum AIState { Idle, SeekingPlant, AttackingPlant, ChasingPlayer, CombatPlayer }
    private AIState currentState;

    [Header("AI Components")]
    private CharacterStats stats;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Detection Range")]
    [Tooltip("How far the enemy can sense a plant")]
    public float plantDetectionRange = 15f;
    [Tooltip("How far the enemy will chase the player after being provoked")]
    public float chaseRange = 10f;
    [Tooltip("How often to re-scan for targets (seconds)")]
    public float scanInterval = 0.5f;
    private float scanTimer;

    [Header("Aggro")]
    [Tooltip("Whether the enemy is currently provoked and targeting the player")]
    public bool isProvoked = false;
    [Tooltip("How long the enemy stays aggro on the player after losing sight")]
    public float aggroTimeout = 5f;
    private float aggroTimer;

    [Header("Idle Settings")]
    private Vector3 startingPosition;
    [Tooltip("How far the enemy wanders from start when idle")]
    public float idleWanderRadius = 3f;
    private float idleWaitTimer;
    public float idleWaitTime = 2f;
    private Vector3 wanderTarget;

    [Header("Player Reference")]
    public Transform player;

    [Header("Combat Tracking")]
    private float lastAttackTime;
    private Transform currentTarget;

    [Header("Attack Hitbox")]
    [Tooltip("Width of the attack hitbox (perpendicular to facing direction)")]
    public float attackWidth = 1.5f;
    [Tooltip("Height of the attack hitbox (along facing direction)")]
    public float attackHeight = 1.5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundRaycastDistance = 1f;
    public float ledgeCheckDistance = 0.5f;
    private bool isGrounded;

    private Vector2 moveDirection;
    private bool isWaiting = false;

    void Start()
    {
        stats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        startingPosition = transform.position;
        wanderTarget = startingPosition;
        SwitchState(AIState.Idle);
    }

    void Update()
    {
        CheckIfGrounded();
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0)
        {
            scanTimer = scanInterval;
            ScanForTargets();
        }
        HandleStateMachine();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    void HandleStateMachine()
    {
        switch (currentState)
        {
            case AIState.Idle: Idle(); break;
            case AIState.SeekingPlant: SeekPlant(); break;
            case AIState.AttackingPlant: AttackPlant(); break;
            case AIState.ChasingPlayer: ChasePlayer(); break;
            case AIState.CombatPlayer: CombatPlayer(); break;
        }
    }

    public void TakeDamage(float damage)
    {
        stats.TakeDamage(damage);

        // Getting hit by the player provokes the enemy
        isProvoked = true;
        aggroTimer = aggroTimeout;
    }
    // Find the nearest plant tagged "Plant" within detection range
    Transform FindNearestPlant()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        Transform nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (GameObject plant in plants)
        {
            float dist = Vector2.Distance(transform.position, plant.transform.position);
            if (dist <= plantDetectionRange && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = plant.transform;
            }
        }
        return nearest;
    }

    bool IsPlayerInChaseRange()
    {
        return player != null && Vector2.Distance(transform.position, player.position) <= chaseRange;
    }

    // Decide which target to go after
    // Enemy attacks plants by default. Only chases player if provoked (hit by player) and player within chaseRange.
    void ScanForTargets()
    {
        Transform nearestPlant = FindNearestPlant();

        // If provoked and player is within chase range — go after player
        if (isProvoked && IsPlayerInChaseRange())
        {
            aggroTimer = aggroTimeout; // refresh timer
            currentTarget = player;
            if (currentState != AIState.ChasingPlayer && currentState != AIState.CombatPlayer)
                SwitchState(AIState.ChasingPlayer);
            return;
        }

        // If provoked but player is out of chase range — start aggro countdown
        if (isProvoked && !IsPlayerInChaseRange())
        {
            aggroTimer -= scanInterval;
            if (aggroTimer <= 0)
            {
                isProvoked = false;
                // Go back to attacking plants
                if (nearestPlant != null)
                {
                    currentTarget = nearestPlant;
                    SwitchState(AIState.SeekingPlant);
                    return;
                }
                currentTarget = null;
                SwitchState(AIState.Idle);
                return;
            }
            // Still aggro but player out of range — keep chasing last known direction
            currentTarget = player;
            if (currentState != AIState.ChasingPlayer)
                SwitchState(AIState.ChasingPlayer);
            return;
        }

        // Not provoked — attack plants by default
        if (nearestPlant != null)
        {
            currentTarget = nearestPlant;
            if (currentState != AIState.SeekingPlant && currentState != AIState.AttackingPlant)
                SwitchState(AIState.SeekingPlant);
            return;
        }

        // No plant and not provoked — go idle
        currentTarget = null;
        if (currentState != AIState.Idle)
            SwitchState(AIState.Idle);
    }


    void Idle()
    {
        float distToStart = Mathf.Abs(transform.position.x - startingPosition.x);

        // Wander around starting position
        if (distToStart > idleWanderRadius)
        {
            MoveTowards(startingPosition, false);
        }
        else if (isGrounded && moveDirection.x != 0 && IsNearLedge())
        {
            moveDirection = Vector2.zero;
            Flip(-transform.localScale.x);
            return;
        }
        else
        {
            idleWaitTimer -= Time.deltaTime;
            if (idleWaitTimer <= 0)
            {
                idleWaitTimer = idleWaitTime;
                // Pick a random wander target within radius
                float randomX = startingPosition.x + Random.Range(-idleWanderRadius, idleWanderRadius);
                wanderTarget = new Vector3(randomX, startingPosition.y, startingPosition.z);
            }
            MoveTowards(wanderTarget, false);
        }
    }

    void SeekPlant()
    {
        if (currentTarget == null)
        {
            SwitchState(AIState.Idle);
            return;
        }

        // Stop ledge detection if the plant is on the same platform
        if (isGrounded && IsNearLedge() && (currentTarget.position - transform.position).normalized.x == Mathf.Sign(transform.localScale.x))
        {
            moveDirection = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // Reached the plant — start attacking
        if (distance <= stats.attackRange)
        {
            SwitchState(AIState.AttackingPlant);
            return;
        }

        MoveTowards(currentTarget.position, true);
    }

    void AttackPlant()
    {
        if (currentTarget == null)
        {
            SwitchState(AIState.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // Plant moved away — go back to seeking
        if (distance > stats.attackRange)
        {
            SwitchState(AIState.SeekingPlant);
            return;
        }

        // Stop moving and attack
        moveDirection = Vector2.zero;

        // Face the plant
        float directionToTarget = currentTarget.position.x - transform.position.x;
        Flip(directionToTarget);

        if (Time.time > lastAttackTime + stats.attackCooldown)
        {
            Attack();
        }
    }

    void ChasePlayer()
    {
        if (currentTarget == null)
        {
            SwitchState(AIState.Idle);
            return;
        }

        // Player out of chase range — let aggro timer count down in ScanForTargets
        if (!IsPlayerInChaseRange())
        {
            // Keep moving toward last known position but aggro will fade
            return;
        }

        if (isGrounded && IsNearLedge())
        {
            SwitchState(AIState.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        if (distance <= stats.attackRange)
        {
            SwitchState(AIState.CombatPlayer);
            return;
        }

        MoveTowards(currentTarget.position, true);
    }

    void CombatPlayer()
    {
        if (currentTarget == null)
        {
            SwitchState(AIState.Idle);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // Player out of attack range but still detected — back to chasing
        if (distance > stats.attackRange)
        {
            SwitchState(AIState.ChasingPlayer);
            return;
        }
        // Player out of chase range — drop aggro, return to plants
        if (!IsPlayerInChaseRange())
        {
            isProvoked = false;
            SwitchState(AIState.Idle);
            return;
        }
        // Stop moving and attack
        moveDirection = Vector2.zero;

        float directionToTarget = currentTarget.position.x - transform.position.x;
        Flip(directionToTarget);

        if (Time.time > lastAttackTime + stats.attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
    }

    // Called by Animation Event during attack animation
    public void DealDamageToTarget()
    {
        // Determine which direction the enemy is facing (sprite faces left by default)
        float facingDirection = transform.localScale.x > 0 ? -1f : 1f;

        // Box-shaped hitbox in front of the enemy
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(facingDirection * (attackHeight * 0.5f), 0f);
        Vector2 boxSize = new Vector2(attackHeight, attackWidth);

        // Detect all targets within attack box (plants, player, etc.)
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        foreach (Collider2D hit in hits)
        {
            // Skip self
            if (hit.transform == transform) continue;

            // Try to damage a plant
            InteractionPlant plant = hit.GetComponent<InteractionPlant>();
            if (plant != null)
            {
                plant.TakeDamage(stats.attackPower);
                continue;
            }

            // Try to damage the player
            CharacterStats targetStats = hit.GetComponent<CharacterStats>();
            if (targetStats != null && targetStats != stats)
            {
                targetStats.TakeDamage(stats.attackPower);
            }
        }
    }


    void MoveTowards(Vector3 target, bool faceTarget)
    {
        moveDirection = (target - transform.position).normalized;
        if (faceTarget)
        {
            float directionToTarget = target.x - transform.position.x;
            Flip(directionToTarget);
        }
    }

    void ApplyMovement()
    {
        if (currentState != AIState.CombatPlayer && currentState != AIState.AttackingPlant && moveDirection.x != 0)
        {
            Flip(moveDirection.x);
        }
        rb.velocity = new Vector2(moveDirection.x * stats.speed, rb.velocity.y);
    }

    void Flip(float directionX)
    {
        // For sprite which is facing left
        if (directionX > 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (directionX < 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void UpdateAnimator()
    {
        float horizontalMove = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("hMove", horizontalMove);

        bool isIdle = isGrounded && horizontalMove <= 0.01f;
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isGrounded", isGrounded);
    }

    void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private bool IsNearLedge()
    {
        // Sprite faces left by default, so scale.x > 0 means facing left
        float direction = transform.localScale.x > 0 ? -1f : 1f;
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(direction * ledgeCheckDistance, 0);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundRaycastDistance, groundLayer);
        return hit.collider == null;
    }

    private void SwitchState(AIState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        isWaiting = false;
    }

    void OnDrawGizmosSelected()
    {
        // Plant detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, plantDetectionRange);

        // Chase range (aggro range)
        Gizmos.color = new Color(1f, 0.5f, 0f); // orange
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Attack hitbox (box in front of enemy)
        Gizmos.color = Color.red;
        float faceDir = transform.localScale.x > 0 ? -1f : 1f;
        Vector2 atkBoxCenter = (Vector2)transform.position + new Vector2(faceDir * (attackHeight * 0.5f), 0f);
        Gizmos.DrawWireCube(atkBoxCenter, new Vector2(attackHeight, attackWidth));

        // Ground check
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastDistance);

        // Ledge check
        float direction = transform.localScale.x > 0 ? -1f : 1f;
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(direction * ledgeCheckDistance, 0);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.down * groundRaycastDistance);

        // Current target line
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
