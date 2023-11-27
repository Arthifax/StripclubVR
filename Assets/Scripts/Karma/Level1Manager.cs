using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Level1Manager : MonoBehaviour
{
    [SerializeField]
    private AudioClip correctAnswerSound = null;
    [SerializeField]
    private AudioClip wrongAnswerSound = null;

    [SerializeField]
    private GameObject[] objectsToEnable = new GameObject[0];

    [System.Serializable]
    protected class Answer
    {
        public string answer = "";
        public int reward = 0;
        public bool hasAnswered = false;
    }

    [SerializeField] private List<Answer> answers = new List<Answer>();

    [SerializeField]
    private AudioSource source = null;

    private MoneyController mc = null;

    private VideoClip lastVid = null;

    private void Awake()
    {
        //source = GetComponent<AudioSource>();
        mc = GetComponent<MoneyController>();
    }

    private void Start()
    {
        if (GameManager.GetLevelCompleted(1))
        {
            foreach (GameObject obj in objectsToEnable)
            {
                obj.SetActive(true);
            }
        }
        else
        {
            // Hey future Joey, if the hint system isnt working in game its prob because of this!!!!!!
            // If the init scene doesnt get loaded first this script cant find GameManager, very important!
            //hc.StartTimer();
        }
    }

    public void GetString(string input)
    {
        CheckInput(input);
    }

    public void CheckInput(string input)
    {
        if (input == "")
            return;

        int index = 0;

        for (int i = 0; i < answers.Count; i++)
        {
            if (input.ToLower() == answers[i].answer.ToLower() && !answers[i].hasAnswered)
            {
                mc.PutMoneyBackOnPile(answers[i].reward);
                answers[i].hasAnswered = true;

                //PlaySound(correctAnswerSound, .2f);
                PlaySound(correctAnswerSound, .5f);

                Debug.Log("Elmo very happy :D");
                break;
            }
            else
            {
                index++;

                if (index >= answers.Count)
                    WrongAnswer();
            }
        }
    }

    private void WrongAnswer()
    {
        Debug.Log("Elmo is very sad :(");
        //PlaySound(wrongAnswerSound, .6f);
        PlaySound(wrongAnswerSound, .9f);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        source.volume = volume;
        source.PlayOneShot(clip);
    }

    public void ReplayVideo() { FindObjectOfType<VideoManager>().PlayVideo(lastVid); }

    public void SetLastVideo(VideoClip clip) { lastVid = clip; }
}
