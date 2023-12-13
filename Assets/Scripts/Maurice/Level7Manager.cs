using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level7Manager : MonoBehaviour
{
    [SerializeField]
    private GameObject winScreen = null;

    [SerializeField] private AudioSource source = null;
    [SerializeField] private AudioClip _victorySFX = null;
    private bool _victoryPlayed = false;

    private bool check = false;

    [SerializeField] private GameObject _info = null;

    public void StartCheck() { check = true; }

    [SerializeField]
    private GameObject[] objectsToEnable = new GameObject[0];

    private void Start()
    {
        /*if (GameManager.GetLevelCompleted(7))
        {
            foreach (GameObject obj in objectsToEnable)
            {
                obj.SetActive(true);
            }
        }*/
    }

    private void Update()
    {
        if (!check)
            return;

        if (VideoManager.GetPlayer().time >= VideoManager.GetPlayer().clip.length - 4.5f)
        {
            // Show info
            _info.SetActive(true);
        }

        if (VideoManager.GetPlayer().time >= VideoManager.GetPlayer().clip.length - .1f)
        {
            // Play victory sound
            if (_victoryPlayed) { }
            else
            {
                // Show info
                //_info.SetActive(true);

                PlaySound(_victorySFX, .7f);
                _victoryPlayed = true;
            }
            // Show button next level
            winScreen.SetActive(true);
        }
    }
    private void PlaySound(AudioClip clip, float volume)
    {
        source.volume = volume;
        source.PlayOneShot(clip);
    }
}
