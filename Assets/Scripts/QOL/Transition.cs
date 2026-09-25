using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Transition : MonoBehaviour
{
    Image transition;
    public float tSpeed = 10;
    public bool inTransit = false;
    public bool selfStart = false;
    void Start()
    {
        transition = GetComponent<Image>();
        if (selfStart)
        {
            transition.fillAmount = 1;
            transit(false);
        }
    }

    IEnumerator FadeIn()
    {
        inTransit = true;
        transition.fillClockwise = true;
        float i = 0;
        while (transition.fillAmount != 1)
        {
            i += Time.deltaTime * tSpeed;
            Mathf.Clamp(i, 0, 1);
            transition.fillAmount = Mathf.Lerp(transition.fillAmount, 1, i);
            yield return null;
        }
        inTransit = false;
    }
    IEnumerator FadeOut()
    {
        inTransit = true;
        transition.fillClockwise = false;
        float i = 0;
        while (transition.fillAmount != 0)
        {
            i += Time.deltaTime * tSpeed;
            Mathf.Clamp(i, 0, 1);
            transition.fillAmount = Mathf.Lerp(transition.fillAmount, 0, i);
            yield return null;
        }
        inTransit = false;
    }

    public void transit(bool Visible)
    {
        if (!inTransit)
        {
            if (Visible)
            {
                StartCoroutine(FadeIn());
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }
    }
}
