using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Deprecated
{
    public class NumberBoardManager : MonoBehaviour
    {
        string currentSequence = "";
        public string correctSequence = "1234";
        public string wrongSequence = "4321";
        public TextMeshPro currentDisplay;

        public UnityEvent correctEvent, wrongEvent, wrongSequenceEvent;
        //public Material materialForVideo;
        //public VideoPlayer videoPlayer;

        public AudioClip keyPress;
        public AudioClip succesSfx;
        public AudioClip failureSfx;

        bool done;

        // Use this for initialization
        void Start()
        {
            //videoPlayer.Prepare();
        }

        //// Update is called once per frame
        //void Update()
        //{

        //}

        public void AddThis(string received)
        {
            if (!done)
            {
                if (received.Length < 2 && currentSequence.Length < 4)
                {
                    currentSequence += received;
                    currentDisplay.GetComponent<AudioSource>().PlayOneShot(keyPress);
                }
                else if (received == "DONE")
                {
                    if (currentSequence == correctSequence)
                    {
                        TrySequence();
                    }
                    else if (currentSequence == wrongSequence)
                    {
                        wrongSequenceEvent.Invoke();
                    }
                    else
                    {
                        currentDisplay.GetComponent<AudioSource>().PlayOneShot(failureSfx);
                        wrongEvent.Invoke();
                    }
                }
                else if (received == "BACK")
                {
                    currentSequence = currentSequence.Remove(currentSequence.Length - 1);
                    currentDisplay.GetComponent<AudioSource>().PlayOneShot(keyPress);
                }
                else if(received == "Comma")
                {
                    currentSequence += ".";
                }
                else
                {
                    currentDisplay.GetComponent<AudioSource>().PlayOneShot(keyPress);
                }

                currentDisplay.text = currentSequence;
            }
        }

        void TrySequence()
        {
            currentDisplay.GetComponent<AudioSource>().PlayOneShot(succesSfx);
            //if (PlayerPrefs.GetInt("progress") < 1)
            //    PlayerPrefs.SetInt("progress", 1);
            currentSequence = "";
            //videoPlayer.GetComponent<MeshRenderer>().material = materialForVideo;
            //videoPlayer.Play();
            correctEvent.Invoke();
            done = true;
        }

        //private void OnApplicationPause(bool pause)
        //{
        //    if (pause && done)
        //        videoPlayer.Stop();//cbad wanneer uitvalt
        //    else if(!pause && done)
        //        StartCoroutine(StartSoon());
        //}

        //IEnumerator StartSoon()
        //{
        //    yield return new WaitForSeconds(1);
        //    videoPlayer.Play();
        //}
    }
}