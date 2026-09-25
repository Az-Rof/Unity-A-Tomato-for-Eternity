using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goddess : MonoBehaviour
{

    public Sprite[] sprites;
    SpriteRenderer sr;
    GameObject Tomato;
    public CharacterStats pc;
    bool HasTomato = false;
    Score sc;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sc = GameObject.Find("Score").GetComponent<Score>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Plant" && !HasTomato)
        {
            Tomato = collision.gameObject;
            StartCoroutine(ImmortalitySequence());
        }
    }

    IEnumerator ImmortalitySequence()
    {
        yield return new WaitForSeconds(0.125f);
        sr.sprite = sprites[1];
        Destroy(Tomato);
        yield return new WaitForSeconds(2f);
        sr.sprite = sprites[2];
        yield return new WaitForSeconds(1f);
        sc.score += 250000;
        pc.speed *= 2;
        pc.jumpPower *= 2;
        
        GameObject.Find("Main Camera").GetComponent<CameraTuning>().Rampage = true;
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(2);
    }
}
