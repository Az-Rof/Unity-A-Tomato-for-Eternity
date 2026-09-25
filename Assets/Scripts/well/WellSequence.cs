using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class WellSequence : MonoBehaviour
{
    [Header("Reference")]
    public Transform bucket;
    public Transition transit;
    public GameObject controls;
    Score score;

    [Header("Obstacle Spawner")]
    [Tooltip("Reference to the ObstacleSpawner for randomizing obstacle positions each session")]
    public WellObstacleSpawner obstacleSpawner;


    [Header("Fall Settings")]
    public float startSpeed = 2f;
    public float maxSpeed = 50f;
    public float acceleration = 10f;

    [Header("Bucket Movement")]
    public float horizontalSpeed = 5f;
    public Vector2 bucketBounds = new Vector2(-17f, 17f);

    [Header("Well")]
    public float scrollStop = -500f;

    [Header("Speed Control")]
    [Tooltip("How much faster S accelerates compared to normal acceleration")]
    public float boostMultiplier = 3f;
    [Tooltip("How much W reduces speed (higher = stronger brake)")]
    public float brakeMultiplier = 3f;

    [Header("Bounce")]
    public float bounceMultiplier = 1.5f;
    public float maxBounceSpeed = 15f;

    private float speed;
    PromptHolder pH;

    private bool isBouncing;

    [Header("Minigame")]
    public int maxBumps = 3;
    private int currentBumps;

    // Store initial positions for reset
    private Vector3 initialBucketPos;
    private Vector3 initialWellPos;
    bool iFrames = false;
    public enum MinigameState
    {
        Idle,
        Playing,
        Won,
        Lost
    }

    public MinigameState state = MinigameState.Idle;

    private Action<bool> callback;

    void Start()
    {
        pH = GameObject.Find("Prompts").GetComponent<PromptHolder>();
        score = GameObject.Find("Score").GetComponent<Score>();
        speed = startSpeed;
        initialBucketPos = bucket != null ? bucket.localPosition : Vector3.zero;
        initialWellPos = transform.localPosition;
    }

    public void StartMinigame(Action<bool> result = null)
    {
        callback = result;
        StartCoroutine(strtExtn());
    }

    IEnumerator strtExtn()
    {
        transit.transit(true);
        while (transit.inTransit)
        {
            yield return null;
        }
        iFrames = false;
        controls.SetActive(true);
        state = MinigameState.Playing;
        speed = startSpeed;
        currentBumps = 0;
        isBouncing = false;

        // Store initial positions at the start of each minigame
        if (bucket != null)
            initialBucketPos = bucket.localPosition;
        initialWellPos = transform.localPosition;

        // Spawn random obstacles for this session
        if (obstacleSpawner != null)
            obstacleSpawner.SpawnObstacles();
        else
            Debug.LogWarning("[WellSequence] ObstacleSpawner reference is not assigned!");

        transit.transit(false);
    }

    void Update()
    {
        if (state == MinigameState.Playing)
        {
            pH.Interact.SetActive(false);
        }
        if (state != MinigameState.Playing)
            return;
        MoveBucket();
    }

    IEnumerator LoseSequence()
    {
        while (transform.localPosition.y < 10)
        {
            speed -= Time.deltaTime * acceleration* 10;
            transform.Translate(Vector3.down * speed * Time.deltaTime);
            yield return null;
        }
        transit.transit(true);
        while (transit.inTransit)
        { yield return null; }
            yield return new WaitForSeconds(0.5f);
        EndMinigame(false);
    }

    IEnumerator WinSequence()
    {
        iFrames = true;
        score.score += 10;
        while (bucket.localPosition.y > -10)
        {
            bucket.Translate(Vector3.down * 0.125f * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        transit.transit(true);
        while (bucket.localPosition.y < 12)
        {
            bucket.Translate(Vector3.up * 0.25f * Time.deltaTime);
            yield return null;
        }
        EndMinigame(true);
    }

    void MoveBucket()
    {
        if (currentBumps < maxBumps)
        {
            if (!isBouncing)
            {
                if (transform.localPosition.y > scrollStop)
                {
                    // Gravity — always accelerates the fall
                    speed += acceleration * Time.deltaTime;

                    float vInput = GetVerticalInput();
                    if (vInput > 0) // W: brake — actually reduces speed
                        speed -= acceleration * brakeMultiplier * Time.deltaTime;
                    else if (vInput < 0) // S: boost — extra acceleration on top of gravity
                        speed += acceleration * boostMultiplier * Time.deltaTime;

                    speed = Mathf.Clamp(speed, 0, maxSpeed);
                    transform.Translate(Vector3.down * speed * Time.deltaTime);
                    score.score += speed * Time.deltaTime;
                }
            }
            else
            {

                transform.Translate(Vector3.down * speed * Time.deltaTime);
                speed += acceleration * Time.deltaTime * 2;
                if (speed >= 0)
                {
                    speed = startSpeed;
                    isBouncing = false;
                    iFrames = false;
                }

            }
            Vector3 pos = bucket.localPosition;
            pos.x += GetHorizontalInput() * horizontalSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, bucketBounds.x, bucketBounds.y);
            bucket.localPosition = pos;

            // Win condition 
            if (transform.localPosition.y <= scrollStop)
            {
                StartCoroutine(WinSequence());
            }
        }
    }
    public void bump()
    {
        if (!iFrames)
        {
            if (isBouncing)
                return;
            float bounceForce = speed * bounceMultiplier;
            bounceForce = Mathf.Clamp(bounceForce, 3f, maxBounceSpeed);
            speed = -bounceForce;
            isBouncing = true;
            currentBumps++;
            iFrames = true;
            Debug.Log("Bucket Bounce : " + speed);
            if (currentBumps >= maxBumps)
            {
                StartCoroutine(LoseSequence());
            }
        }
    }
    void EndMinigame(bool win)
    {
        state = win ? MinigameState.Won : MinigameState.Lost;

        callback?.Invoke(win);

        // Reset positions back to start
        if (bucket != null)
            bucket.localPosition = initialBucketPos;
        transform.localPosition = initialWellPos;
        speed = startSpeed;
        isBouncing = false;

        // Clear all spawned obstacles
        if (obstacleSpawner != null)
            obstacleSpawner.ClearObstacles();

        // Deactivate this GameObject
        controls.SetActive(false);
        StartCoroutine(te());
    }

    IEnumerator te()
    {
        while (transit.inTransit) { yield return null; }
        transit.transit(false);
        gameObject.SetActive(false);
    }
    float GetVerticalInput()
    {
        float input = 0;

        if (Keyboard.current != null)
        {

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input = 1;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input = -1;
            }
        }
        return input;
    }
    float GetHorizontalInput()
    {
        float input = 0;
        if (Keyboard.current != null)
        {

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input = 1;
            }

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input = -1;
            }
        }
        return input;
    }
}