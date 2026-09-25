using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    public int gameSceneIndex;
    public GameObject creditsMenu;
    public GameObject mainMenu;
    public Vector2 cameraMovementRange;
    public Transform Cam;
    int dir = 1;
    public AudioSource[] sfx;

    public void playGame()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        sfx[1].Play();
        yield return new WaitForSeconds(10);
        SceneManager.LoadScene(gameSceneIndex);
    }
    public void toggleCredits()
    {
        if (!creditsMenu.activeInHierarchy)
        {
            sfx[0].Play();
        }
        creditsMenu.SetActive(!creditsMenu.activeInHierarchy);
        mainMenu.SetActive(!mainMenu.activeInHierarchy);
    }
    public void exitGame()
    {
        Application.Quit();
    }
    void Update()
    {
        if (Cam != null)
        {
            if (dir == 1)
            {
                if (Cam.position.x > cameraMovementRange.y)
                {
                    dir = -1;
                }
            }
            else
            {
                if (Cam.position.x < cameraMovementRange.x)
                {
                    dir = 1;
                }
            }
            Cam.Translate(new Vector3(dir * Time.deltaTime, 0, 0));
        }
    }
}
