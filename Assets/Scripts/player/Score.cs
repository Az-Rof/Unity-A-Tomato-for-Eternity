using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    [Header("References")]
    public float score;
    public TextMeshProUGUI scoreUI;
    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        score = Mathf.Clamp(score, 0, 999999);
        scoreUI.text = "SCORE: " + Mathf.Round(score);
    }
}
