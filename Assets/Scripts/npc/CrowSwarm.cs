using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CrowSwarm : MonoBehaviour
{
    public InteractionPlant tomato;
    public PlayerAnimator pA;
    public SpriteRenderer[] sirs;
    public bool there;
    AudioSource crowAudio;
    bool cawStarted;

    void Start()
    {
        tomato = GameObject.Find("Plant").GetComponent<InteractionPlant>();
        there = false;
        StartCoroutine(Sched());
    }

    IEnumerator Sched()
    {
        while (true)
        {
            yield return new WaitForSeconds(30);
            if (!there)
            {
                there = Random.Range(0, 2) == 0;
            }
        }
    }

    void Update()
    {
        if (there)
        {
            if (!cawStarted)
            {
                crowAudio = AudioManager.Instance.PlaySFXLoop("dragon-studio-crow-caw-halloween-sound-effect-322990", 0, 4.6f);
                cawStarted = true;
            }

            if (Vector3.Distance(pA.transform.position, transform.position) < 2 && pA.cAnim == "shovel")
            {
                there = false;
                AudioManager.Instance.StopSFX(crowAudio);
                crowAudio = null;
                cawStarted = false;
            }

            else
            {
                foreach (SpriteRenderer sr in sirs) sr.enabled = true;
                tomato.currentGrowth -= Time.deltaTime * 0.5f;
            }
        }
        else
        {
            if (cawStarted)
            {
                AudioManager.Instance.StopSFX(crowAudio);
                crowAudio = null;
                cawStarted = false;
            }
            foreach (SpriteRenderer sr in sirs) sr.enabled = false;
        }
    }
}