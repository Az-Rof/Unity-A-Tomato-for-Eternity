using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowmation : MonoBehaviour
{
    SpriteRenderer sr;
    public Sprite[] Anim;


    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Animator(Anim, 15));

    }
    IEnumerator Animator(Sprite[] Animation, float fps)
    {
        while (true)
        {
            for (int i = 0; i < Animation.Length; i++)
            {
                yield return new WaitForSeconds(1f / fps);
                sr.sprite = Animation[i];
            }
        }
    }
}
