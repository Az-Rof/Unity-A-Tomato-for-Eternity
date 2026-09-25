using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController))]
public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Trajectory Settings")]
    [Range(5, 30)] public int trajectorySteps = 15;
    public GameObject dotPrefab;
    public Transform dotsParent;

    float throwForce;
    public float timeStep = 0.1f;

    private GameObject[] dots;
    private Camera mainCamera;
    private PlayerController player;

    void Awake()
    {
        // Now directly reference the new PlayerController
        player = GetComponent<PlayerController>();

    }

    void Start()
    {
        mainCamera = Camera.main;
        InitializeTrajectoryDots();
    }

    void Update()
    {
        // Check if the player's hand is currently holding a physical object
        if (player != null && player.currentCarriedObject != null)
        {
            DrawTrajectory();
        }
        else
        {
            HideTrajectory();
        }
    }

    public void InitializeTrajectoryDots()
    {
        if (dotPrefab == null || dotsParent == null) return;

        // Clean up old dots if any
        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null) Destroy(dots[i]);
            }
        }

        dots = new GameObject[trajectorySteps];
        for (int i = 0; i < trajectorySteps; i++)
        {
            dots[i] = Instantiate(dotPrefab, dotsParent);
            dots[i].SetActive(false);
        }
    }

    private void DrawTrajectory()
    {
        if (dots == null || dots.Length == 0) return;

        // FIX: Get the absolute position from the center of the currently carried object
        Vector2 startPos;
        if (player.currentCarriedObject != null)
        {
            startPos = player.currentCarriedObject.transform.position;
        }
        else
        {
            startPos = player.carryAnchor != null ? (Vector2)player.carryAnchor.position : (Vector2)transform.position;
        }

        Vector2 targetPos = GetMouseWorldPosition2D();
        Vector2 throwDirection = (targetPos - startPos).normalized;

        Vector2 velocity = throwDirection * player.Stats.throwForce;

        for (int i = 0; i < trajectorySteps; i++)
        {
            if (i >= dots.Length || dots[i] == null) continue;

            float t = i * timeStep;
            // Standard Unity parabolic trajectory physics formula
            Vector2 pointPos = startPos + velocity * t + 0.5f * Physics2D.gravity * (t * t);

            dots[i].transform.position = pointPos;
            dots[i].SetActive(true);
        }
    }

    public void HideTrajectory()
    {
        if (dots == null) return;
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null && dots[i].activeSelf)
            {
                dots[i].SetActive(false);
            }
        }
    }

    private Vector2 GetMouseWorldPosition2D()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || Mouse.current == null) return transform.position;

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
    }
}