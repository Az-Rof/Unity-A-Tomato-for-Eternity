using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxTrack : MonoBehaviour
{
    public float Strength;
    Transform player;
    Vector3 startPos;
    private void Start()
    {
        player = GameObject.Find("Main Camera").transform;
        startPos = transform.position;
    }
    void Update()
    {
        Vector3 tgt = new Vector3(player.position.x,startPos.y,startPos.z);
        transform.position = Vector3.Lerp(startPos, tgt, Strength);
    } 
}
