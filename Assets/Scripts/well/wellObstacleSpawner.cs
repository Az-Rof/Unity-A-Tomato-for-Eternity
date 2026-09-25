using System.Collections.Generic;
using UnityEngine;

public class WellObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [Tooltip("Collider2D that defines the area where obstacles can spawn")]
    public Collider2D wellMiniGamesArea;

    [Header("Obstacle")]
    [SerializeField] GameObject obstaclePrefab;

    [Header("Spawn Settings")]
    [Tooltip("Number of obstacles to spawn each minigame session")]
    public int obstacleCount = 5;
    [Tooltip("Minimum vertical spacing between obstacles (Y axis)")]
    public float minVerticalSpacing = 1.5f;
    [Tooltip("Padding from the edges of the spawn area")]
    public float edgePadding = 0.5f;

    // Track spawned obstacles so we can clean them up
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    public void SpawnObstacles()
    {
        ClearObstacles();

        if (wellMiniGamesArea == null || obstaclePrefab == null)
        {
            Debug.LogError("[ObstacleSpawner] Spawn area or obstacle prefab is not assigned!");
            return;
        }

        Bounds bounds = wellMiniGamesArea.bounds;

        // Min/max X within the collider bounds (with padding)
        float minX = bounds.min.x + edgePadding;
        float maxX = bounds.max.x - edgePadding;
        // Min/max Y within the collider bounds (with padding)
        float minY = bounds.min.y + edgePadding;
        float maxY = bounds.max.y - edgePadding;

        // Track used Y positions to ensure vertical spacing
        List<float> usedY = new List<float>();

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = obstacleCount * 10;

        while (spawned < obstacleCount && attempts < maxAttempts)
        {
            attempts++;

            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);

            // Check vertical spacing against previously spawned obstacles
            bool tooClose = false;
            foreach (float y in usedY)
            {
                if (Mathf.Abs(randomY - y) < minVerticalSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            // Spawn the obstacle
            Vector2 spawnPos = new Vector2(randomX, randomY);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.SetParent(transform);
            spawnedObstacles.Add(obstacle);
            usedY.Add(randomY);
            spawned++;
        }

        Debug.Log($"[ObstacleSpawner] Spawned {spawned} obstacles.");
    }

    public void ClearObstacles()
    {
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle != null)
                Destroy(obstacle);
        }
        spawnedObstacles.Clear();
    }
}