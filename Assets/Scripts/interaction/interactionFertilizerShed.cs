using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionFertilizerShed : MonoBehaviour
{
    [Header("Shed Settings")]
    [Tooltip("Insert the physical Fertilizer Prefab that has the InteractionFertilizer.cs script attached")]
    public GameObject fertilizerPrefab; 
    
    private bool playerNear = false;
    private PlayerController player;
    PromptHolder pH;

    void Start()
    {
        pH = GameObject.Find("Prompts").GetComponent<PromptHolder>();
    }

    void Update()
    {
        // Check the playerNear condition
        if (playerNear && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("[Shed] Right-click pressed near the Shed.");
            Interact();
        }
    }

    void Interact()
    {
        if (player == null)
        {
            Debug.LogError("[Shed] Interact failed: Player reference is null!");
            return;
        }

        if (player.currentCarriedObject == null)
        {
            if (fertilizerPrefab == null)
            {
                Debug.LogError("[Shed] Interact failed: Fertilizer Prefab has not been assigned to the Shed Inspector!");
                return;
            }

            GameObject newFertilizer = Instantiate(fertilizerPrefab, transform.position, Quaternion.identity);
            newFertilizer.name = fertilizerPrefab.name;
            InteractionFertilizer fertilizerScript = newFertilizer.GetComponent<InteractionFertilizer>();

            if (fertilizerScript != null)
            {
                fertilizerScript.ForcePickup(player);
                Debug.Log("[Shed] Success: Fertilizer spawned and picked up!");
            }
            else
            {
                Debug.LogError("[Shed] Interact failed: Fertilizer Prefab does not have the InteractionFertilizer.cs script!");
            }
        }
        else
        {
            Debug.Log("[Shed] Cancelled: Player's hands are full, carrying " + player.currentCarriedObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = true;
            // Use InParent to ensure detection even if the colliding object is a child collider
            player = collision.GetComponentInParent<PlayerController>(); 
            Debug.Log($"[Shed] Player entered the area. PlayerController script found: {player != null}");
            pH.Pickup.SetActive(true);
        }
        if (collision.gameObject.name == "Bucket")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.right * 5;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = false;
            player = null;
            Debug.Log("[Shed] Player left the area.");
            pH.Pickup.SetActive(false);
        }
    }
}