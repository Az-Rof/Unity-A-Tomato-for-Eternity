using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionWell : MonoBehaviour
{
    [Header("Minigame")]
    [Tooltip("Reference to the WellSequence minigame object")]
    public WellSequence wellMinigame;
    public Transition transit;

    private bool playerNear = false;
    private PlayerController player;
    private InteractionBucket pendingBucket; // Bucket waiting for minigame result
    PromptHolder pH;

    void Start()
    {
        pH = GameObject.Find("Prompts").GetComponent<PromptHolder>();
    }

    void Update()
    {
        // Environmental interaction using the E key
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        if (player == null) 
        {
            Debug.LogError("[Well] Error: Player reference is missing!");
            return;
        }

        if (player.currentCarriedObject != null)
        {
            InteractionBucket bucket = player.currentCarriedObject.GetComponent<InteractionBucket>();
            
            if (bucket != null)
            {
                if (!bucket.isFull)
                {
                    if (wellMinigame != null)
                    {
                        if (wellMinigame.state != WellSequence.MinigameState.Playing)
                        {
                            pendingBucket = bucket;
                            PlayerController.inputLocked = true;
                            StartCoroutine(stextn());
                        }
                        else
                        {
                            Debug.LogWarning("Player is already playing the minigame.");
                        }
                    }
                    else
                    {
                        Debug.LogError("[Well] WellSequence minigame reference is not assigned!");
                    }
                }
                else
                {
                    Debug.Log("[Well] Cancelled: Bucket is already full of water!");
                }
            }
            else
            {
                Debug.Log($"[Well] Cancelled: The carried object is not a bucket, but {player.currentCarriedObject.name}");
            }
        }
        else
        {
            Debug.Log("[Well] Cancelled: Player's hands are empty, bring a bucket here to fill with water.");
        }
    }

    IEnumerator stextn()
    {
        transit.transit(true);
        while (transit.inTransit)
        {
            yield return null;
        }
        wellMinigame.state = WellSequence.MinigameState.Playing;
        wellMinigame.gameObject.SetActive(true);
        wellMinigame.StartMinigame(OnMinigameComplete);
        Debug.Log("[Well] Minigame started!");
    }

    // Called by WellSequence when the minigame ends
    // won = true: bucket gets filled; won = false: bucket stays empty
    void OnMinigameComplete(bool won)
    {
        if (won)
        {
            if (pendingBucket != null)
            {
                pendingBucket.FillBucket();
                Debug.Log("[Well] Minigame won! Bucket filled with water!");
            }
        }
        else
        {
            Debug.Log("[Well] Minigame lost! Bucket remains empty.");
        }
        PlayerController.inputLocked = false;
        pendingBucket = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Search for PlayerController on the parent object, ignore Tag
        PlayerController p = collision.GetComponentInParent<PlayerController>();
        if (p != null)
        {
            playerNear = true;
            player = p;
            Debug.Log($"[Well] Player entered the well area. Detected: {player != null}");
            pH.Interact.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController p = collision.GetComponentInParent<PlayerController>();
        if (p != null)
        {
            playerNear = false;
            player = null;
            Debug.Log("[Well] Player left the well area.");
            pH.Interact.SetActive(false);
        }
    }
}