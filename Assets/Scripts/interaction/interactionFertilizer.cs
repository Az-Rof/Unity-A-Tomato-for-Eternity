using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class InteractionFertilizer : MonoBehaviour
{
    public bool isCarried = false;

    private bool canInteract;
    private PlayerController player;
    private BoxCollider2D col;
    private Rigidbody2D rb;
    private Camera mainCamera;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Block all interactions during minigames
        if (PlayerController.inputLocked) return;

        // If currently carried -> Right-click to Throw
        if (isCarried)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Throw();
                player.Carrying = false;
            }
        }
        // If on the ground -> Right-click to Pick up
        else if (canInteract && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (player != null && player.currentCarriedObject == null)
            {
                Pickup(player);
                player.Carrying = true;
            }
        }
    }

    // Special function so the Shed can directly give the item to the player's hand
    // Does not call Pickup() because the spawned fertilizer hasn't gone through OnTriggerEnter2D,
    // so canInteract=false and player=null which would cause Pickup() to fail.
    public void ForcePickup(PlayerController p)
    {
        if (p == null) return;
        if (p.currentCarriedObject != null) return;

        player = p;
        isCarried = true;
        p.currentCarriedObject = this.gameObject;

        // Disable physics so it follows the Transform
        rb.simulated = false;
        col.enabled = false;

        // 1. Make the Player the direct Parent
        transform.SetParent(p.transform);

        // 2. Use carryOffset from PlayerController as the local position
        //    Match the pickup position from Pickup()
        transform.localPosition = p.carryOffset;

        transform.localRotation = Quaternion.identity;
    }

    // ... scroll down to the Pickup(PlayerController p) function ...

    void Pickup(PlayerController p)
    {
        // Input validation
        if (!canInteract || player == null || player.currentCarriedObject != null) return;
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        isCarried = true;
        player.currentCarriedObject = this.gameObject;

        // Disable physics so it follows the Transform
        rb.simulated = false;
        col.enabled = false;

        // Make the Player the direct Parent
        transform.SetParent(player.transform);

        // Use carryOffset from PlayerController as the local position
        transform.localPosition = player.carryOffset;

        transform.localRotation = Quaternion.identity;
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
        col.enabled = true;

        Vector2 targetPos = GetMouseWorldPosition2D();
        Vector2 throwDirection = (targetPos - (Vector2)transform.position).normalized;
        rb.velocity = throwDirection * player.Stats.throwForce;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCarried && collision.CompareTag("Player"))
        {
            player = collision.GetComponent<PlayerController>();
            canInteract = true;
        }
        if (collision.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isCarried && collision.CompareTag("Player"))
        {
            canInteract = false;
            player = null;
        }
    }

    public void Fertilizing()
    {
        Destroy(this.gameObject);
    }


    private Vector2 GetMouseWorldPosition2D()
    {
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
}