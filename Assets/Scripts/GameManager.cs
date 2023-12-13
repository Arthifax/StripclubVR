using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public bool openShaq = false;

    public bool isLicenseVersion = false;

    private bool[] levelsCompleted = new bool[0];

    void Awake()
    {
        //Make Singleton
        if (gameManager == null)
        {
            gameManager = new GameManager();
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);

        levelsCompleted = new bool[7];

        for (int i = 0; i < levelsCompleted.Length; i++)
            levelsCompleted[i] = false;
    }

    private void Update()
    {
        /*if (OVRInput.Get(OVRInput.RawButton.B))
            BackToHub();

        if (OVRInput.Get(OVRInput.RawButton.A))
            VideoManager.ResetVideo();

        if (openShaq)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1 &&
                !FindObjectOfType<HubController>().GetBtn().IsInteractable())
                FindObjectOfType<HubController>().EnableChaq();
        }*/
    }

    public static void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void BackToHub()
    {
        if (isLicense())
        {
            SceneManager.LoadScene(3);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public static void EnableShaq() { FindObjectOfType<GameManager>().openShaq = true; }

    public static bool isLicense() { return FindObjectOfType<GameManager>().isLicenseVersion; }

    //public static void OnLevelCompleted(int level) { FindObjectOfType<GameManager>().levelsCompleted[level - 1] = true; }

    //public static bool GetLevelCompleted(int level) { return FindObjectOfType<GameManager>().levelsCompleted[level - 1]; }

    public AudioSource GetBackgroundMusicSource() { return GetComponent<AudioSource>(); }
}
