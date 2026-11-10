using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace KarmaVR
{
    public sealed class MauriceGameManager : MonoBehaviour
    {
        private enum Stage
        {
            Intro,
            Objectives,
            SecondVideo,
            Complete
        }

        [SerializeField] private VideoManager videoManager;
        [SerializeField] private VideoClip intro;
        [SerializeField] private VideoClip secondVideo;
        [SerializeField] private GameObject controllerCameraRoot;
        [SerializeField] private GameObject keyboardRoot;
        [SerializeField] private GameObject infoCanvasObject;
        [SerializeField] private GameObject infoObject;
        [SerializeField] private TextMeshProUGUI answerText;
        [SerializeField] private GameObject[] clueTextGameObjects = new GameObject[4];
        [SerializeField, Tooltip("Enable the camera and keyboard at scene start for testing.")]
        private bool activateObjectivesImmediately;
        [SerializeField] private AudioClip happySound;
        [SerializeField] private AudioSource gameEffectsAudioSource;
        [SerializeField] private UnityEvent gameFinished = new UnityEvent();

        [SerializeField, Tooltip("Live objective progress. Element 0 is objective 1.")]
        private bool[] objectives = new bool[4];
        private Stage stage;
        private bool AllCluesCollected => Array.TrueForAll(objectives, clicked => clicked);

        private void Start()
        {
            objectives = new bool[4];

            if (controllerCameraRoot != null)
                controllerCameraRoot.SetActive(false);
            if (keyboardRoot != null)
                keyboardRoot.SetActive(false);
            if (infoCanvasObject != null)
                infoCanvasObject.SetActive(false);
            if (infoObject != null)
                infoObject.SetActive(false);
            foreach (GameObject clue in clueTextGameObjects)
                if (clue != null)
                    clue.SetActive(false);

            if (videoManager == null || intro == null)
            {
                Debug.LogError("Assign Maurice's Video Manager and Intro clip.", this);
                return;
            }

            if (!videoManager.Play(intro, FinishIntro, LogPlaybackError))
                LogPlaybackError("Could not start the intro.");

            if (activateObjectivesImmediately)
                BeginObjectives();
        }

        private void FinishIntro()
        {
            if (stage == Stage.Intro)
                BeginObjectives();
        }

        private void BeginObjectives()
        {
            stage = Stage.Objectives;
            if (answerText != null)
                answerText.text = string.Empty;
            if (controllerCameraRoot != null)
                controllerCameraRoot.SetActive(true);
            if (keyboardRoot != null)
                keyboardRoot.SetActive(true);
        }

        // Assign 1, 2, 3, or 4 in the corresponding target's Inspector event.
        public void ObjectiveClicked(int objectiveNr)
        {
            if (stage != Stage.Objectives || objectiveNr < 1 || objectiveNr > objectives.Length)
                return;

            int index = objectiveNr - 1;
            if (objectives[index])
                return;

            objectives[index] = true;
            bool allCluesCollected = AllCluesCollected;
            if (index < clueTextGameObjects.Length && clueTextGameObjects[index] != null)
                clueTextGameObjects[index].SetActive(true);

            if (!allCluesCollected)
                return;

            if (happySound != null && gameEffectsAudioSource != null)
                gameEffectsAudioSource.PlayOneShot(happySound);
        }

        private bool CanType => stage == Stage.Objectives && answerText != null;

        public void PressKey(string key)
        {
            if (CanType && !string.IsNullOrEmpty(key))
                answerText.text += key;
        }

        public void Backspace()
        {
            if (CanType && answerText.text.Length > 0)
                answerText.text = answerText.text.Substring(0, answerText.text.Length - 1);
        }

        public void CheckAnswer()
        {
            if (stage == Stage.SecondVideo || stage == Stage.Complete || answerText == null || videoManager == null)
                return;

            string answer = answerText.text.Trim();
            answerText.text = string.Empty;
            if (!string.Equals(answer, "Sara", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(answer, "Sarah", StringComparison.OrdinalIgnoreCase))
                return;

            stage = Stage.SecondVideo;
            if (keyboardRoot != null)
                keyboardRoot.SetActive(false);
            if (controllerCameraRoot != null)
                controllerCameraRoot.SetActive(false);

            if (videoManager.Play(secondVideo, FinishGame, RestoreAnswer, interruptCurrent: true))
            {
                if (infoCanvasObject != null)
                    infoCanvasObject.SetActive(true);
            }
            else
                RestoreAnswer("Could not start the second video. Check the Second Video assignment.");
        }

        private void FinishGame()
        {
            stage = Stage.Complete;
            if (infoObject != null)
                infoObject.SetActive(true);
            gameFinished.Invoke();
        }

        private void RestoreAnswer(string reason)
        {
            LogPlaybackError(reason);
            stage = Stage.Objectives;
            if (infoCanvasObject != null)
                infoCanvasObject.SetActive(false);
            if (controllerCameraRoot != null)
                controllerCameraRoot.SetActive(true);
            if (keyboardRoot != null)
                keyboardRoot.SetActive(true);
        }

        private void LogPlaybackError(string reason)
        {
            Debug.LogError("Maurice video failed: " + reason, this);
        }
    }
}
