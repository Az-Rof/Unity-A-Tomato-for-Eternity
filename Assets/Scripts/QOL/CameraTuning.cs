using UnityEngine;

public class CameraTuning : MonoBehaviour
{
    public Vector2 yAxisBounds;
    public Vector2 xAxisBounds;
    Camera c;
    public bool Rampage;
    AudioManager audioManager;

    void Start()
    {
        c = GetComponent<Camera>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
       if (transform.parent.position.y < yAxisBounds.x || transform.parent.position.y > yAxisBounds.y)
        {
            transform.position = new Vector3(transform.position.x,Mathf.Clamp(transform.parent.position.y, yAxisBounds.x, yAxisBounds.y),transform.position.z);
        } 
        if (transform.parent.position.x < xAxisBounds.x || transform.parent.position.x > xAxisBounds.y)
        {
            transform.position = new Vector3(Mathf.Clamp(transform.parent.position.x, xAxisBounds.x, xAxisBounds.y), transform.position.y, transform.position.z);
        }
        
        if (!(transform.parent.position.y < yAxisBounds.x || transform.parent.position.y > yAxisBounds.y) && !(transform.parent.position.x < xAxisBounds.x || transform.parent.position.x > xAxisBounds.y))
        {
            transform.localPosition = new Vector3(0, 0, -10);
        }

        if (transform.position.y == yAxisBounds.y)
        {

            c.orthographicSize = 20;
            audioManager.PlayMusic("goddess");
        } else
        {
            c.orthographicSize = Rampage ? 10:5;
        }
    }
}
