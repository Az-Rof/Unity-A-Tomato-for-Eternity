using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionPlant : MonoBehaviour
{
    Score score;
    PromptHolder pH;

    private bool playerNear = false;
    private PlayerController player;

    SpriteRenderer sr;

    [Header("Harvest Sequence")]
    public GameObject Platforms;

    public GameObject GrownTomatoPrefab;

    [Header("Base of the plant")]
    [SerializeField] public float maxHP = 100;
    [SerializeField] public float currentHP = 100;
    [SerializeField] public float currentWaterLevel;
    [SerializeField] public float currentFertileLevel;
    [SerializeField] public float maxWaterLevel = 100;
    [SerializeField] public float maxFertileLevel = 100;

    [Header("Decay Settings")]
    public float waterDecayRate = 5f;
    public float fertileDecayRate = 10f;
    public float starveDamage = 5f;
    public float regenRate = 1f;
    public float wateringEffect = 30f;
    public float fertilizingEffect = 30f;

    [Header("Growth")]
    public Sprite[] Stages;
    [SerializeField] public float currentGrowth = 0f;
    public float maxGrowth = 100f;
    public float baseGrowthRate = 1f;
    public float waterGrowthBonus = 0.5f;
    public float fertileGrowthBonus = 0.5f;
    public bool isReadyToHarvest = false;
    int currentStageIndex;

    [Header("Enemy Attack")]
    [Tooltip("HP lost when the plant is hit by an enemy")]
    public float plantDamaged = 10f;
    [Tooltip("Cooldown in seconds between enemy hits")]
    public float hitCooldown = 1f;
    private float lastHitTime = 0f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        score = GameObject.Find("Score").GetComponent<Score>();
        currentWaterLevel = 0f;
        currentFertileLevel = 0f;
        currentGrowth = 0f;
        isReadyToHarvest = false;
        pH = GameObject.Find("Prompts").GetComponent<PromptHolder>();
        AudioManager.Instance.PlayMusic("farm"); // plays farm track while farming
    }


    void Update()
    {

        Watering();
        Fertilizing();
        Harvest();
        currentGrowth = Mathf.Clamp(currentGrowth, 0, maxGrowth);
        if (!isReadyToHarvest)
        {
            PlantDecay();
        }
    }

    void Watering()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (player != null && player.currentCarriedObject != null)
            {
                InteractionBucket bucket = player.currentCarriedObject.GetComponent<InteractionBucket>();
                if (bucket != null && bucket.isFull)
                {
                    bucket.WaterPlant(); // Empty the bucket
                    currentWaterLevel = Mathf.Min(currentWaterLevel + wateringEffect, maxWaterLevel);
                    Debug.Log("Plant watered! Water level: " + currentWaterLevel);
                }
                else if (bucket != null && !bucket.isFull)
                {
                    Debug.Log("Bucket is empty, cannot water the plant.");
                }
            }
        }
    }

    void Fertilizing()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (player != null && player.currentCarriedObject != null)
            {
                InteractionFertilizer fertilizer = player.currentCarriedObject.GetComponent<InteractionFertilizer>();
                if (fertilizer != null)
                {
                    fertilizer.Fertilizing(); // Empty the bucket
                    currentFertileLevel = Mathf.Min(currentFertileLevel + fertilizingEffect, maxFertileLevel);
                    Debug.Log("Plant fertilized! Fertile level: " + currentFertileLevel);

                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = true;
            player = collision.GetComponent<PlayerController>();
            if (isReadyToHarvest)
            {
                pH.Interact.SetActive(true);
            }
        }

        // Enemy hits the plant on contact — use enemy's attackPower from CharacterStats
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
                TakeDamage(enemyStats.attackPower);
            else
                TakeDamage(plantDamaged); // Fallback to default if no CharacterStats found
        }
        if (collision.gameObject.name == "Fertilizer")
        {
            Destroy(collision.gameObject);
            if (currentFertileLevel + 20 > maxFertileLevel)
            {
                score.score += 50;
            }
            currentFertileLevel += 20;
        }
        if (collision.gameObject.name == "Bucket")
        {
            InteractionBucket iB = collision.gameObject.GetComponent<InteractionBucket>();
            if (iB)
            {
                if (iB.isFull)
                {
                    iB.isFull = false;
                    if (currentWaterLevel + 30 > maxWaterLevel)
                    {
                        score.score += 5000;
                    }
                    currentWaterLevel += 30;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = false;
            player = null;
            pH.Interact.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Enemy keeps hitting the plant while in contact (respecting cooldown)
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
                TakeDamage(enemyStats.attackPower);
            else
                TakeDamage(plantDamaged); // Fallback to default if no CharacterStats found
        }
    }

    // Called when an enemy attacks the plant
    public void TakeDamage(float damage)
    {
        // Respect hit cooldown to prevent damage every frame
        if (Time.time - lastHitTime < hitCooldown)
            return;

        lastHitTime = Time.time;
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        Debug.Log("Plant took damage from enemy! HP: " + currentHP);

        // TODO: Trigger death event when HP reaches 0
    }

    // Harvest the plant when fully grown
    void Harvest()
    {
        if (!isReadyToHarvest && currentGrowth >= maxGrowth)
        {
            isReadyToHarvest = true;
            Debug.Log("Plant is fully grown! Ready to harvest.");
        }

        if (playerNear && isReadyToHarvest && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // TODO: Give item to player or trigger harvest event
            Debug.Log("Plant harvested!");
            GameObject.Find("TomatoUI").SetActive(false);
            Instantiate(GrownTomatoPrefab, GameObject.Find("TomatoPos").transform.position, GameObject.Find("TomatoPos").transform.rotation);
            transform.Translate(Vector3.down * 1000);
            score.score += currentFertileLevel * 1000;
            score.score += currentWaterLevel * 1000;
            StartCoroutine(HarvestSequence());
        }

        int newStageIndex = currentStageIndex;
        if (currentGrowth < maxGrowth / 7f)
        {
            newStageIndex = 0;
        }
        else if (currentGrowth < maxGrowth * (2f / 7f))
        {
            newStageIndex = 1;
        }
        else if (currentGrowth < maxGrowth * (3f / 7f))
        {
            newStageIndex = 2;
        }
        else if (currentGrowth < maxGrowth * (4f / 7f))
        {
            newStageIndex = 3;
        }
        else if (currentGrowth < maxGrowth * (5f / 7f))
        {
            newStageIndex = 4;
        }
        else if (currentGrowth < maxGrowth * (6f / 7f))
        {
            newStageIndex = 5;
        }
        else if (isReadyToHarvest)
        {
            newStageIndex = 6;
        }

        if (newStageIndex > currentStageIndex)
        {
            AudioManager.Instance.PlaySFX("tomatogrow"); // ganti dengan nama Audio element di sfxSounds
        }
        currentStageIndex = newStageIndex;
        sr.sprite = Stages[newStageIndex];

        // if (currentGrowth < maxGrowth / 7f)
        // {
        //     sr.sprite = Stages[0];
        // }
        // else if (currentGrowth < maxGrowth * (2f / 7f))
        // {
        //     sr.sprite = Stages[1];
        // }
        // else if (currentGrowth < maxGrowth * (3f / 7f))
        // {
        //     sr.sprite = Stages[2];
        // }
        // else if (currentGrowth < maxGrowth * (4f / 7f))
        // {
        //     sr.sprite = Stages[3];
        // }
        // else if (currentGrowth < maxGrowth * (5f / 7f))
        // {
        //     sr.sprite = Stages[4];
        // }
        // else if (currentGrowth < maxGrowth * (6f / 7f))
        // {
        //     sr.sprite = Stages[5];
        // }
        // else if (isReadyToHarvest)
        // {
        //     sr.sprite = Stages[6];
        // }

    }

    IEnumerator HarvestSequence()
    {
        yield return null;
        AudioManager.Instance.PlayMusic("parkour");
        Platforms.SetActive(true);
    }

    void PlantDecay()
    {
        // Gradually reduce water and fertility over time
        currentWaterLevel -= waterDecayRate * Time.deltaTime;
        currentFertileLevel -= fertileDecayRate * Time.deltaTime;

        // Clamp to zero so they don't go negative
        currentWaterLevel = Mathf.Clamp(currentWaterLevel, 0, maxWaterLevel);
        currentFertileLevel = Mathf.Clamp(currentFertileLevel, 0, maxFertileLevel);

        // If either water or fertility is depleted, the plant takes damage fast
        if (currentWaterLevel <= 0 || currentFertileLevel <= 0 && !isReadyToHarvest)
        {
            currentGrowth -= Time.deltaTime * 0.25f;
        }
        // If both water and fertility are above 0, regenerate HP slowly and grow
        else
        {
            currentHP += regenRate * Time.deltaTime;

            // Growth scales with how well nourished the plant is
            float growthThisFrame = ((0.5f * (currentFertileLevel / maxFertileLevel)) + (0.5f * (currentWaterLevel / maxWaterLevel))) * baseGrowthRate * Time.deltaTime;
            currentGrowth += growthThisFrame;
        }

        // Clamp HP between 0 and maxHP
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // Clamp growth between 0 and maxGrowth
        currentGrowth = Mathf.Clamp(currentGrowth, 0, maxGrowth);

        // TODO: Trigger death event when HP reaches 0
    }
}