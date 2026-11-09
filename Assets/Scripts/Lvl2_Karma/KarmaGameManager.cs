using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;

namespace KarmaVR
{
    public sealed class KarmaGameManager : MonoBehaviour
    {
        [Serializable]
        private sealed class Act
        {
            [Min(1)] public int price;
            public VideoClip clip;
            public GameObject infoPanel;
        }

        [SerializeField] private VideoManager videoManager;
        [SerializeField] private MoneyPuzzle money;
        [SerializeField] private VideoClip intro;
        [SerializeField] private VideoClip wrongPayment;
        [SerializeField] private Act[] acts = new Act[0];
        [SerializeField] private GameObject puzzleControls;
        [SerializeField] private GameObject nextLevelButton;
        [SerializeField] private TextMeshProUGUI walletText;
        [SerializeField] private TextMeshProUGUI paymentText;
        [SerializeField] private string nextScene;
        [SerializeField] private UnityEvent paymentRejected = new UnityEvent();
        [SerializeField] private UnityEvent levelCompleted = new UnityEvent();
        [SerializeField] private UnityEvent playbackFailed = new UnityEvent();

        private readonly HashSet<Act> completed = new HashSet<Act>();
        private VideoClip replayClip;
        private bool playing;
        private bool ready;
        private bool finished;

        private void Start()
        {
            if (!ValidateSetup())
                return;

            if (money != null)
            {
                money.Changed += RefreshMoney;
                RefreshMoney();
            }
            else
                Debug.LogWarning("Assign MoneyPuzzle to enable Karma's cash interactions.", this);

            if (acts != null)
                foreach (Act act in acts)
                    if (act != null && act.infoPanel != null)
                        act.infoPanel.SetActive(false);

            if (nextLevelButton != null)
                nextLevelButton.SetActive(false);
            ready = true;
            replayClip = intro;
            Play(intro);
        }

        private void OnDestroy()
        {
            if (money != null)
                money.Changed -= RefreshMoney;
        }

        public void SubmitPayment()
        {
            if (!ready || playing || finished || money == null || !money.IsOpen)
                return;
            if (!ValidateActs())
                return;

            int amount = money.Payment;
            if (amount == 0)
            {
                paymentRejected.Invoke();
                return;
            }

            foreach (Act act in acts)
            {
                if (act.price == amount && !completed.Contains(act))
                {
                    Play(act.clip, act);
                    return;
                }
            }

            money.RefundPayment();
            if (wrongPayment != null)
                Play(wrongPayment);
            paymentRejected.Invoke();
        }

        public void Replay()
        {
            if (ready && !playing)
                Play(replayClip);
        }

        public void LoadNextLevel()
        {
            if (!finished || playing)
                return;
            if (string.IsNullOrWhiteSpace(nextScene) ||
                !Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError("Assign a next scene included in the build.", this);
                return;
            }
            SceneManager.LoadScene(nextScene);
        }

        private void Play(VideoClip clip, Act act = null)
        {
            playing = true;
            SetPuzzleVisible(false);

            bool started = videoManager.Play(clip,
                () =>
                {
                    if (act != null)
                    {
                        completed.Add(act);
                        replayClip = act.clip;
                        money.SpendPayment();
                        if (act.infoPanel != null)
                            act.infoPanel.SetActive(true);
                        finished = completed.Count == acts.Length;
                    }

                    ResumePuzzle();
                    if (act != null && finished)
                        levelCompleted.Invoke();
                },
                reason => PlaybackError(clip, reason));

            if (!started)
                PlaybackError(clip, "The video player could not accept the request.");
        }

        private void PlaybackError(VideoClip clip, string reason)
        {
            Debug.LogError("Karma video failed (" + clip.name + "): " + reason, this);
            ResumePuzzle();
            playbackFailed.Invoke();
        }

        private void ResumePuzzle()
        {
            playing = false;
            SetPuzzleVisible(true);
        }

        private void SetPuzzleVisible(bool visible)
        {
            if (money != null)
                money.SetOpen(visible && !finished);
            if (puzzleControls != null)
                puzzleControls.SetActive(visible && !finished);
            if (nextLevelButton != null)
                nextLevelButton.SetActive(visible && finished);
        }

        private void RefreshMoney()
        {
            if (walletText != null)
                walletText.text = "Geld Over: " + money.Balance;
            if (paymentText != null)
                paymentText.text = "Geld Gegeven: " + money.Payment;
        }

        private bool ValidateSetup()
        {
            if (videoManager == null || intro == null)
            {
                Debug.LogError("Assign Karma's VideoManager and Intro clip to start playback.", this);
                return false;
            }
            return true;
        }

        private bool ValidateActs()
        {
            if (acts == null || acts.Length == 0)
            {
                Debug.LogError("Configure at least one act before submitting payments.", this);
                return false;
            }

            var prices = new HashSet<int>();
            foreach (Act act in acts)
            {
                if (act == null || act.clip == null || act.price <= 0 || !prices.Add(act.price))
                {
                    Debug.LogError("Each Karma act needs a clip and a unique positive price.", this);
                    return false;
                }
            }
            return true;
        }
    }
}
