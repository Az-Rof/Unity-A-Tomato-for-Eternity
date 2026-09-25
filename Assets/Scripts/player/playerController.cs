using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using static UnityEngine.Random;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("Misc")]
    public Vector2 WorldBounds;
    public PlayerAnimator pA;
    public CameraTuning cT;
    public bool Carrying = false;
    public bool Attacking = false;

    [Header("Carry System")]
    public Transform carryAnchor;
    [HideInInspector] public GameObject currentCarriedObject;
    [Header("Carry Settings")]
    public Vector3 carryOffset = new Vector3(0, 1.5f, 0);

    // Lock player input during minigames
    public static bool inputLocked = false;

    // set-get the character stats
    CharacterStats stats;
    public CharacterStats Stats
    {
        get { return stats; }
        set { stats = value; }
    }
    public Slider healthSlider;
    public LayerMask groundLayer;
    public float distanceGrounding = 1f;

    [Header("Attack")]
    [Tooltip("Layer mask for enemy detection")]
    public LayerMask enemyLayer;
    [Tooltip("Width of the attack hitbox (perpendicular to facing direction)")]
    public float attackWidth = 1.5f;
    [Tooltip("Height of the attack hitbox (along facing direction)")]
    public float attackHeight = 1.5f;
    private float lastAttackTime;
    private Animator animator;

    Rigidbody2D rb;
    private bool onGround;
    BoxCollider2D boxCollider2D;

    float footstepInterval = 0.5f;
    float footstepTimer;
    int lastFootstep;

    float attackingInterval = 0.1f, attackingTimer;
    int lastAttacking;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();
    }

    void Start()
    {
        if (healthSlider != null && stats != null)
        {
            healthSlider.maxValue = stats.maxHealth;
            healthSlider.value = stats.currentHealth;
        }
        else
        {
            Debug.LogWarning("Health slider are not asiggned");
        }
    }

    void Update()
    {
        Jump();
        HandleAttack();
        if (pA.cAnim != "shovel")
        {
            UpdateAnimator();
        }
    }

    void FixedUpdate()
    {
        Movement();
        CheckGrounded();
    }

    Vector2 gNorm;

    void Movement()
    {
        if (inputLocked) return;
        float moveX = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
        }
        rb.velocity = new Vector2(moveX * stats.speed, rb.velocity.y);


        // Flip sprite based on movement direction (sprite faces left by default)
        if (moveX > 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveX < 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (transform.position.x < WorldBounds.x || transform.position.x > WorldBounds.y)
        {
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, WorldBounds.x, WorldBounds.y), transform.position.y, transform.position.z);
        }
        audioFootstep();
    }




    public Transform PM;
    bool jFB = false;
    void jF()
    {
        jFB = false;
    }

    void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distanceGrounding, groundLayer);
        onGround = hit.collider != null && !jFB;
        if (onGround)
        {
            PM.rotation = Quaternion.Euler(0, 0, hit.collider.gameObject.transform.rotation.eulerAngles.z);
            gNorm = hit.normal;
        }
        else
        {
            PM.rotation = Quaternion.Euler(0, 0, 0);
        }
    }


    void Jump()
    {
        if (inputLocked) return;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && onGround)
        {
            if (Time.timeScale == 0f) return;
            jFB = true;
            Invoke("jF", 0.25f);
            rb.velocity = new Vector2(rb.velocity.x, stats.jumpPower);
            onGround = false;
            if (Carrying)
            {
                pA.PlayAnimation(pA.cJump, 15, false, "cjump");
            }
            else
            {
                pA.PlayAnimation(pA.Jump, 15, false, "jump");
            }
            AudioManager.Instance.PlaySFX("jump");
        }
    }

    void adb()
    {
        Attacking = false;
        pA.looping = false;
    }
    void HandleAttack()
    {
        if (inputLocked || Carrying) return;
        if (Time.timeScale == 0f) return;
        if (IsPointerOverUI()) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            audioAttacking();
            StartCoroutine(bFA());
            lastAttackTime = Time.time;
            Attacking = true;
        }

    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();
        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {
            GameObject go = result.gameObject;
            if (go.GetComponent<Button>() != null ||
                go.GetComponent<Slider>() != null ||
                go.GetComponent<Toggle>() != null ||
                go.GetComponent<UnityEngine.EventSystems.IPointerClickHandler>() != null ||
                go.GetComponent<UnityEngine.EventSystems.IDragHandler>() != null)
            {
                return true;
            }
        }

        return false;
    }
    IEnumerator bFA()
    {
        while (pA.cAnim != "shovel")
        {
            pA.looping = false;
            pA.cAnim = "";
            pA.PlayAnimation(pA.Shovel, 12, false, "shovel");
            yield return null;
        }
        Invoke("adb", 4 / 12);
    }
    void DealDamage()
    {
        // Determine which direction the player is facing (sprite faces left by default)
        float facingDirection = transform.localScale.x > 0 ? -1f : 1f;

        // Box-shaped hitbox in front of the player
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(facingDirection * (attackHeight * 0.5f), 0f);
        Vector2 boxSize = new Vector2(attackHeight, attackWidth);

        // Detect enemies within attack box
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            // Try to damage an Enemy
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(stats.attackPower);
                Debug.Log("Player attacked " + hit.gameObject.name + " for " + stats.attackPower + " damage!");

                int index;
                index = UnityEngine.Random.Range(1, 4);
                AudioManager.Instance.PlaySFX("shovelhit" + index);
                continue;
            }

            // Fallback: damage any CharacterStats (in case no Enemy script)
            CharacterStats targetStats = hit.GetComponent<CharacterStats>();
            if (targetStats != null && targetStats != stats)
            {
                targetStats.TakeDamage(stats.attackPower);
                Debug.Log("Player attacked " + hit.gameObject.name + " for " + stats.attackPower + " damage!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack hitbox (box in front of player)
        if (stats != null)
        {
            float facingDirection = transform.localScale.x > 0 ? -1f : 1f;
            Vector2 boxCenter = (Vector2)transform.position + new Vector2(facingDirection * (attackHeight * 0.5f), 0f);
            Vector2 boxSize = new Vector2(attackHeight, attackWidth);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }

    void UpdateAnimator()
    {
        if (Attacking || pA.cAnim == "shovel")
        {
            if (pA.cAnim != "shovel")
            {
                pA.PlayAnimation(pA.Shovel, 12, false, "shovel");
                return;
            }
        }
        else
        {
            if (pA.cAnim != "shovel" && !Attacking)
            {

                if (!Carrying && !Attacking)
                {

                    if (onGround && !Attacking)
                    {
                        if (Input.GetAxis("Horizontal") != 0 && !Attacking)
                        {
                            if (pA.cAnim != "run" && !Attacking)
                            {
                                pA.looping = false;
                                pA.PlayAnimation(pA.Run, 8, true, "run");
                            }
                        }
                        else
                        {
                            if (pA.cAnim != "idle" && !Attacking)
                            {
                                pA.looping = false;
                                pA.PlayAnimation(pA.Idle, 8, true, "idle");
                            }
                        }

                    }
                }
                else
                {
                    if (onGround)
                    {
                        if (Input.GetAxis("Horizontal") != 0)
                        {
                            if (pA.cAnim != "crun" && !Attacking)
                            {
                                pA.looping = false;
                                pA.PlayAnimation(pA.cRun, 8, true, "crun");
                            }
                        }
                        else
                        {
                            if (pA.cAnim != "cidle" && !Attacking)
                            {
                                pA.looping = false;
                                pA.PlayAnimation(pA.cIdle, 8, true, "cidle");
                            }
                        }
                    }
                }
            }
        }
    }


    void audioFootstep()
    {
        // Set Audio Footsteps:
        if (AudioManager.Instance == null) return;
        bool moving = MathF.Abs(rb.velocity.x) > 0;
        if (!onGround || !moving)
        {
            footstepTimer = 0f;
            return;
        }
        footstepTimer += Time.deltaTime;
        if (footstepTimer < footstepInterval) return;
        footstepTimer = 0f;

        int index;
        do
        {
            index = UnityEngine.Random.Range(1, 4);
        }
        while (index == lastFootstep);
        lastFootstep = index;

        AudioManager.Instance.PlaySFX("footstep" + index);
    }

    void audioAttacking()
    {
        if (AudioManager.Instance == null) return;
        if (Time.timeScale == 0) return;
        attackingTimer += Time.deltaTime;
        attackingTimer = 0;
        int index;
        do
        {
            index = UnityEngine.Random.Range(1, 4);
        }
        while (index == lastAttacking);
        lastAttacking = index;
        AudioManager.Instance.PlaySFX("shovelswing" + index);
        if (attackingTimer < attackingInterval) return;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 < collision.gameObject.layer)
        {
            AudioManager.Instance.PlaySFX("land");
        }
        // if (collision.gameObject.layer == LayerMask.NameToLayer("groundLayer"))
        // {
        //     AudioManager.Instance.PlaySFX("land");
        // }

    }


}