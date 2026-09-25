using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Info")]
    public string CharacterName;
    public string CharacterType;

    // Basic Stats
    [Header("Basic Stats")]
    public float maxHealth;
    public float attackPower;
    public float attackRange;
    public float attackCooldown;
    public float speed;
    public float jumpPower;
    public float throwForce;

    [Header("Health Regen")]
    [Tooltip("HP regenerated per second")]
    public float healthRegenRate = 2f;

    // Current Stats
    [Header("Current Stats")]
    public float currentHealth;

    private float lastAttackTime = 0f;

    void Awake() { }

    void Start()
    {
        InitiateCharacterStats();
        InitiateEnemy();
        StartCoroutine(RegenerateHealth());
    }

    // void Update()
    // {
    //     // Condition of death will only trigger the event once
    //     if (currentHealth <= 0 && !hasTriggeredDeath)
    //     {
    //         hasTriggeredDeath = true; // Set the flag to true immediately to prevent it from being triggered again

    //         if (tag == "Enemy" && !isthisImportantCharacter)
    //         {
    //             Debug.Log(gameObject.name + " has died. Enemy defeated.");
    //             Die();
    //             Score score = FindObjectOfType<Score>();
    //             if (score != null)
    //             {
    //                 score.IncreaseScore(+100);
    //             }
    //         }
    //         else if (tag == "Player" && isthisImportantCharacter)
    //         {
    //             Debug.Log(gameObject.name + " has died. Game Over.");
    //             // Game Over logic
    //         }
    //         else if (tag == "Enemy" && isthisImportantCharacter)
    //         {
    //             Debug.Log(gameObject.name + " has died. Important Enemy defeated.");

    //             if (enemyScript != null)
    //             {
    //                 enemyScript.enabled = false; // Disable enemy AI script
    //             }

    //             // MAIN CHANGE: Play cutscene from here
    //             if (deathCutscene != null)
    //             {
    //                 deathCutscene.Play();
    //             }
    //             else
    //             {
    //                 Debug.LogWarning(
    //                     "Death cutscene has not been set in Inspector for " + gameObject.name
    //                 );
    //             }
    //         }
    //     }
    // }

    void InitiateCharacterStats()
    {
        // Initialize character name & type
        CharacterName = this.gameObject.name;
        CharacterType = this.gameObject.tag;

        // Initialize Basic Stats
        currentHealth = maxHealth;
    }

    void InitiateEnemy()
    {
        if (tag == "Enemy")
        {
            // Enemy AI is handled by the Enemy script
        }
        else
        {
            return; // If not an enemy, exit the method
        }
    }

    // Health regeneration coroutine
    IEnumerator RegenerateHealth()
    {
        while (true)
        {
            if (currentHealth < maxHealth)
            {
                currentHealth += healthRegenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            yield return null;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log(gameObject.name + " took " + damage + " damage! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
