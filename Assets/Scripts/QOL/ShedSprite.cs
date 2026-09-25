using UnityEngine;

public class ShedSprite : MonoBehaviour
{
    public Transform player;
    SpriteRenderer sr;
    public float xBound;
    public Sprite[] sheds;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        sr.sprite = player.position.x < xBound ? sheds[0] : sheds[1];
        transform.localPosition = new Vector3(transform.localPosition.x,transform.localPosition.y, player.position.x < xBound ? 1f : 0.35f);
    }
}
