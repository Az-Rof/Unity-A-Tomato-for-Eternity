using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    SpriteRenderer sr;
    public Sprite[] Jump;
    public Sprite[] Idle;
    public Sprite[] sIdle;
    public Sprite[] sRun;
    public Sprite[] cRun;
    public Sprite[] cJump;
    public Sprite[] cIdle;

    public Sprite[] Run;
    public Sprite[] Fall;
    public Sprite[] Carry;
    public Sprite[] Shovel;

    public string cAnim;
    public bool looping = false;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cAnim = "";
    }

    public void PlayAnimation(Sprite[] Animation, float fps, bool looped, string name)
    {
        if (name == "shovel")
        {
            print(name);
        }
        if (cAnim != name && cAnim != "shovel")
        {
            StartCoroutine(Animator(Animation, fps, looped,name));
        }
    }

    IEnumerator Animator(Sprite[] Animation, float fps, bool looped, string name)
    {
        while(cAnim == "shovel")
        {
            yield return null;
        }
        cAnim = name;
        looping = looped;
        if (looped)
        {
            while (looping && cAnim == name)
            {
                for (int i = 0; i < Animation.Length; i++)
                {
                    yield return new WaitForSeconds(1f / fps);
                    if (cAnim == "shovel" && name != "shovel") {
                        break;
                    }
                    else
                    {
                        sr.sprite = Animation[i];
                    }
                    if (!looping || cAnim != name)
                    {
                        cAnim = "";
                        break;
                    }
                }
                yield return null;
            }
        } else
        {
            for (int i = 0; i < Animation.Length; i++)
            {
                if (name == "shovel")
                {
                    cAnim = "shovel";
                }
                if ( cAnim != name)
                {
                    cAnim = "";
                    break;
                }
                
                yield return new WaitForSeconds(1f / fps);
                sr.sprite = Animation[i];

            }
        }
        cAnim = "";
    }
}
