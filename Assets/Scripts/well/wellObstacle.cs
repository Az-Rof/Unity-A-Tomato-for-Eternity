using System.IO;
using UnityEngine;


public class wellObstacle : MonoBehaviour
{
    private WellSequence wS;
    public Sprite[] rocks;

    public Vector2[] offsets;
    public Vector2[] sizes;
    BoxCollider2D bc;

    void Start()
    {
        int i = Random.Range(0, rocks.Length);
        GetComponent<SpriteRenderer>().sprite = rocks[i];
        bc = GetComponent<BoxCollider2D>();
        bc.offset = offsets[i];
        bc.size = sizes[i];
        transform.Rotate(new Vector3(0, 0, Random.Range(0, 360f)));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Bucket")
        {
            // Lazy lookup: WellSequence GameObject is inactive at Start(),
            // so we find it at collision time when it's already active.
            if (wS == null)
                wS = FindObjectOfType<WellSequence>();

            if (wS != null)
            {
                Debug.Log("Bucket Hit Rock");
                wS.bump();
                AudioManager.Instance.PlaySFX("buckethit");
            }
        }
    }
}