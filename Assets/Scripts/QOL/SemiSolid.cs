using UnityEngine;

public class SemiSolid : MonoBehaviour
{
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    void Update()
    {
        bc.isTrigger = rb.velocity.y > 0;
    }
}
