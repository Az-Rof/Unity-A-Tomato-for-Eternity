using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class InteractionBucket : MonoBehaviour
{
    [Header("Bucket State")]
    public bool isFull = false;
    public bool isCarried = false;

    public Sprite[] stateSprites;

    SpriteRenderer sr;

    private bool canInteract;
    private PlayerController p2;

    private PlayerController player;
    private BoxCollider2D bucketCollider;
    private Rigidbody2D rb;
    private Camera mainCamera;
    Animator animator;
    PromptHolder pH;


    void Start()
    {
        pH = GameObject.Find("Prompts").GetComponent<PromptHolder>();
        p2 = GameObject.Find("Player").GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
    }
    void Awake()
    {
        // animator = GetComponent<Animator>();
        bucketCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        UpdateVisual();
    }

    void Update()
    {
        // Block all interactions during minigames
        if (PlayerController.inputLocked) return;

        sr.sprite = isFull ? stateSprites[0] : stateSprites[1];

        // Separate the paths. If carried, only process the Throw function.
        // If on the ground, only process the Pickup function.
        if (isCarried)
        {
            Throw();
            p2.Carrying = true;

        }
        else
        {
            Pickup();
            p2.Carrying = false;
        }
        if (transform.position.x < p2.WorldBounds.x || transform.position.x > p2.WorldBounds.y)
        {
            rb.velocity *= -1;
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, p2.WorldBounds.x, p2.WorldBounds.y), transform.position.y, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCarried && collision.CompareTag("Player"))
        {
            player = collision.GetComponent<PlayerController>();
            canInteract = true;
            pH.Pickup.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isCarried && collision.CompareTag("Player"))
        {
            canInteract = false;
            player = null;
            pH.Pickup.SetActive(false);
        }
    }

    void Pickup()
    {

        // Input validation
        if (!canInteract || player == null || player.currentCarriedObject != null) return;
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        isCarried = true;
        pH.Pickup.SetActive(false);
        player.currentCarriedObject = this.gameObject;

        // Disable physics so it follows the Transform
        rb.simulated = false;
        bucketCollider.enabled = false;

        // 1. Make the Player the direct Parent
        transform.SetParent(player.transform);

        // 2. Use carryOffset from PlayerController as the local position
        transform.localPosition = player.carryOffset;

        transform.localRotation = Quaternion.identity;
        AudioManager.Instance.PlaySFX("itempickup");
    }

    void Throw()
    {
        // Input validation
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        rb.simulated = true;
        isCarried = false;
        player.currentCarriedObject = null;

        transform.SetParent(null);

        rb.isKinematic = false;
        bucketCollider.enabled = true;

        Vector2 targetPos = GetMouseWorldPosition2D();
        Vector2 throwDirection = (targetPos - (Vector2)transform.position).normalized;
        rb.velocity = throwDirection * player.Stats.throwForce;
        AudioManager.Instance.PlaySFX("itemthrow");
    }

    public void FillBucket()
    {
        if (isFull) return;
        isFull = true;
        UpdateVisual();
        Debug.Log("Bucket is full of water");
    }

    public void WaterPlant()
    {
        if (!isFull) return;
        isFull = false;
        UpdateVisual();
        Debug.Log("Bucket is empty after watering");
    }

    void UpdateVisual()
    {
        // spriteRenderer.sprite = isFull ? fullSprite : emptySprite;
    }

    private Vector2 GetMouseWorldPosition2D()
    {
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    // Hit Grounds
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 < collision.gameObject.layer)
        {
            AudioManager.Instance.PlaySFX("buckethit",0.01f);
        }
    }
}