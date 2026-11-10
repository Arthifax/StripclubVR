using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace KarmaVR
{
    public sealed class LolaGameManager : MonoBehaviour
    {
        private enum Stage
        {
            Intro,
            Ready,
            PlayingLongClip,
            Finished
        }

        [Serializable]
        private sealed class TimedReveal
        {
            [Min(0f)] public float timeSeconds;
            public GameObject target;

            [NonSerialized] public bool revealed;
        }

        [SerializeField] private VideoManager videoManager;
        [SerializeField] private VideoClip intro;
        [SerializeField] private VideoClip longClip;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TimedReveal[] reveals =
        {
            new TimedReveal { timeSeconds = 28f },
            new TimedReveal { timeSeconds = 45f },
            new TimedReveal { timeSeconds = 94f }
        };
        [SerializeField] private GameObject nextLevelButton;
        [SerializeField] private UnityEvent gameFinished = new UnityEvent();

        private VideoPlayer player;
        private Stage stage;

        private void Start()
        {
            if (continueButton != null)
                continueButton.SetActive(false);
            if (infoPanel != null)
                infoPanel.SetActive(false);
            if (nextLevelButton != null)
                nextLevelButton.SetActive(false);
            ResetReveals();

            if (videoManager == null || intro == null)
            {
                Debug.LogError("Assign Lola's Video Manager and Intro clip.", this);
                return;
            }

            player = videoManager.GetComponent<VideoPlayer>();
            if (!videoManager.Play(intro, ShowContinueButton, LogPlaybackError))
                LogPlaybackError("Could not start the intro.");
        }

        private void ShowContinueButton()
        {
            stage = Stage.Ready;
            if (continueButton != null)
                continueButton.SetActive(true);
        }

        public void Continue()
        {
            if (stage != Stage.Ready)
                return;

            if (longClip == null)
            {
                LogPlaybackError("Assign Lola's Long Clip.");
                return;
            }

            stage = Stage.PlayingLongClip;
            if (continueButton != null)
                continueButton.SetActive(false);

            if (!videoManager.Play(longClip, FinishLongClip, RestoreContinueButton))
                RestoreContinueButton("Could not start the long clip.");
        }

        private void Update()
        {
            if (stage == Stage.PlayingLongClip && player != null && player.clip == longClip && player.isPlaying)
            {
                if (infoPanel != null && !infoPanel.activeSelf)
                    infoPanel.SetActive(true);
                RevealThrough(player.time);
            }
        }

        private void RevealThrough(double seconds)
        {
            foreach (TimedReveal reveal in reveals)
            {
                if (reveal == null || reveal.revealed || reveal.target == null || seconds < reveal.timeSeconds)
                    continue;

                reveal.revealed = true;
                reveal.target.SetActive(true);
            }
        }

        private void FinishLongClip()
        {
            stage = Stage.Finished;
            RevealThrough(longClip.length);

            if (nextLevelButton != null)
                nextLevelButton.SetActive(true);
            gameFinished.Invoke();
        }

        private void ResetReveals()
        {
            foreach (TimedReveal reveal in reveals)
            {
                if (reveal == null)
                    continue;

                reveal.revealed = false;
                if (reveal.target != null)
                    reveal.target.SetActive(false);
            }
        }

        private void RestoreContinueButton(string reason)
        {
            LogPlaybackError(reason);
            if (infoPanel != null)
                infoPanel.SetActive(false);
            ResetReveals();
            ShowContinueButton();
        }

        private void LogPlaybackError(string reason)
        {
            Debug.LogError("Lola video failed: " + reason, this);
        }
    }
}
