using UnityEngine;
using UnityEngine.InputSystem.Utilities;

[ExecuteAlways]
public class AdaptablePlatform : MonoBehaviour
{
    [SerializeField] float length;
    [SerializeField] SpriteRenderer mid;
    [SerializeField] BoxCollider2D bc;
    [SerializeField] Transform leftEdge;
    [SerializeField] Transform rightEdge;
    void Update()
    {
        if (!Application.isPlaying)
        {
            if (mid != null)
            {
                mid.size = new Vector2(length * 1.6f, 1.6f);
            }
            if (bc != null)
            {
                bc.size = new Vector2((length * 1.6f) + 3f, 0.6f);
            }
            if (leftEdge != null && rightEdge != null)
            {
                leftEdge.localPosition = new Vector3((length * -1.6f)/2 - 0.75f, 0.038f, 0);
                rightEdge.localPosition = new Vector3((length * 1.6f)/2 + 0.55f, 0.038f, 0);
            }
        }
    }
}
