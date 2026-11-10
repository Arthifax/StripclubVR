using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace KarmaVR
{
    public sealed class DaveGameManager : MonoBehaviour
    {
        private enum Stage
        {
            Intro,
            FindCode,
            RevealingCode,
            ReadyForQuestions,
            PlayingQuestionClip,
            WaitingForAnswer,
            Complete
        }

        [Serializable]
        private sealed class AnswerEntry
        {
            public string answer;
            public VideoClip clip;
        }

        [SerializeField] private VideoManager videoManager;
        [SerializeField] private VideoClip intro;
        [SerializeField] private VideoClip firstQuestionClip;

        [SerializeField] private GameObject codeButton;
        [SerializeField] private GameObject pictureHitbox;
        [SerializeField] private GameObject paintingRoot;
        [SerializeField, Min(0f), Tooltip("Time from enabling the painting to showing Questioning Dave.")]
        private float paintingRevealSeconds = 1f;
        [SerializeField] private GameObject questionButton;

        [SerializeField] private GameObject keyboardRoot;
        [SerializeField] private TextMeshProUGUI answerText;
        [SerializeField] private AnswerEntry[] answerList = new AnswerEntry[0];
        [SerializeField] private GameObject[] keyboardToggleObjects = new GameObject[0];
        [SerializeField] private UnityEvent incorrectAnswer = new UnityEvent();
        [SerializeField] private AudioSource gameEffectsAudioSource;
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip victorySound;

        private Stage stage;
        private int answerIndex;

        private void Start()
        {
            codeButton?.SetActive(false);
            pictureHitbox?.SetActive(false);
            paintingRoot?.SetActive(false);
            questionButton?.SetActive(false);
            keyboardRoot?.SetActive(false);
            SetKeyboardToggleObjects(false);

            if (videoManager == null || intro == null)
            {
                Debug.LogError("Assign Dave's Video Manager and Intro clip.", this);
                return;
            }

            if (!videoManager.Play(intro, () =>
                {
                    stage = Stage.FindCode;
                    codeButton?.SetActive(true);
                    pictureHitbox?.SetActive(true);
                }, LogPlaybackError))
            {
                LogPlaybackError("Could not start the intro.");
            }
        }

        public void ShowCode()
        {
            if (stage != Stage.FindCode)
                return;

            stage = Stage.RevealingCode;
            codeButton?.SetActive(false);
            pictureHitbox?.SetActive(false);
            paintingRoot?.SetActive(true);
            StartCoroutine(ShowQuestionButtonAfterReveal());
        }

        private IEnumerator ShowQuestionButtonAfterReveal()
        {
            if (paintingRevealSeconds > 0f)
                yield return new WaitForSeconds(paintingRevealSeconds);

            stage = Stage.ReadyForQuestions;
            questionButton?.SetActive(true);
        }

        public void StartQuestions()
        {
            if (stage != Stage.ReadyForQuestions || !ValidateQuestions())
                return;

            stage = Stage.PlayingQuestionClip;
            keyboardRoot.SetActive(true);
            questionButton?.SetActive(false);

            if (!videoManager.Play(firstQuestionClip, WaitForAnswer, RestoreQuestionButton))
                RestoreQuestionButton("Could not start the first question.");
        }

        public void CheckAnswer()
        {
            if (stage != Stage.WaitingForAnswer)
                return;

            string input = answerText.text.Trim();
            answerText.text = string.Empty;
            if (input.Length == 0)
                return;

            AnswerEntry entry = answerList[answerIndex];
            if (!MatchesAnswer(input, entry.answer.Trim()))
            {
                incorrectAnswer.Invoke();
                return;
            }

            stage = Stage.PlayingQuestionClip;
            if (videoManager.Play(entry.clip, AdvanceQuestion, RestoreCurrentQuestion))
                PlaySound(correctSound);
            else
                RestoreCurrentQuestion("Could not start the answer clip.");
        }

        private static bool MatchesAnswer(string input, string expected)
        {
            if (string.Equals(input, expected, StringComparison.OrdinalIgnoreCase))
                return true;

            // Preserve the original whole-word matching for single-word answers.
            foreach (string word in input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                if (string.Equals(word, expected, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private void AdvanceQuestion()
        {
            answerIndex++;
            if (answerIndex < answerList.Length)
            {
                WaitForAnswer();
                return;
            }

            stage = Stage.Complete;
            keyboardRoot.SetActive(false);
            SetKeyboardToggleObjects(true);
            PlaySound(victorySound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (gameEffectsAudioSource != null && clip != null)
                gameEffectsAudioSource.PlayOneShot(clip);
        }

        private void WaitForAnswer()
        {
            stage = Stage.WaitingForAnswer;
        }

        public void ShowKeyboard()
        {
            if (stage == Stage.Complete)
                keyboardRoot.SetActive(true);
        }

        public void HideKeyboard()
        {
            if (stage == Stage.Complete)
                keyboardRoot.SetActive(false);
        }

        private void SetKeyboardToggleObjects(bool visible)
        {
            foreach (GameObject toggle in keyboardToggleObjects)
                if (toggle != null)
                    toggle.SetActive(visible);
        }

        private bool ValidateQuestions()
        {
            if (firstQuestionClip == null || keyboardRoot == null || answerText == null || answerList.Length == 0)
            {
                Debug.LogError("Assign Dave's first question clip, keyboard root, answer text, and answer list.", this);
                return false;
            }

            for (int i = 0; i < answerList.Length; i++)
            {
                AnswerEntry entry = answerList[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.answer) || entry.clip == null)
                {
                    Debug.LogError($"Dave's Answer List element {i} needs answer text and a clip.", this);
                    return false;
                }
            }

            return true;
        }

        private void RestoreQuestionButton(string reason)
        {
            LogPlaybackError(reason);
            stage = Stage.ReadyForQuestions;
            keyboardRoot.SetActive(false);
            questionButton?.SetActive(true);
        }

        private void RestoreCurrentQuestion(string reason)
        {
            LogPlaybackError(reason);
            WaitForAnswer();
        }

        private void LogPlaybackError(string reason)
        {
            Debug.LogError("Dave video failed: " + reason, this);
        }
    }
}
