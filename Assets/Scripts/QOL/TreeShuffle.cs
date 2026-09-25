using UnityEngine;
public class TreeShuffle : MonoBehaviour
{
    public Sprite[] trees;
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = trees[Random.Range(0, trees.Length)];
        GetComponent<SpriteRenderer>().flipX = Random.Range(0, 2) == 0;
    }
}
