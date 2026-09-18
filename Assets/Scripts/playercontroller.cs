using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class playercontroller : MonoBehaviour
{
    // set-get the character stats
    characterstats stats;
    public characterstats Stats
    {
        get { return stats; }
        set { stats = value; }
    }

    public Slider healthSlider;
    public LayerMask groundLayer;


    // Animator animator;
    Rigidbody2D rigidbody2D;
    BoxCollider2D boxCollider2D;
    [SerializeField]
    float distanceGrounding = 1f;

    [SerializeField] bool onGround;
    // Start is called before the first frame update
    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        // animator = GetComponent<Animator>();

        stats = GetComponent<characterstats>();
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
            Debug.LogWarning(
                "Health or Stamina sliders are not assigned or stats is null. Please assign them in the inspector."
            );
        }


    }

    // Update is called once per frame
    void Update()
    {

        jump();
    }

    // Physics Update
    void FixedUpdate()
    {
        movement();
        isGrounded();
    }

    // Movement
    void movement()
    {
        rigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * stats.speed, rigidbody2D.velocity.y);
        // animator.SetFloat("hMove", Input.GetAxis("Horizontal"));
    }


    void isGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distanceGrounding, groundLayer);
        if (hit.collider != null)
        {
            onGround = true; // Player is on the ground
            // animator.SetBool("onGround", true);
        }
        else
        {
            onGround = false;
            // animator.SetBool("onGround", false);
        }
    }

    void jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, stats.jumpPower);
            onGround = false;
        }
    }

}
