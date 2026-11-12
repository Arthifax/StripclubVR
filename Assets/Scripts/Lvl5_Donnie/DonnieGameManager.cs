using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace KarmaVR
{
    public sealed class DonnieGameManager : MonoBehaviour
    {
        private enum Stage
        {
            Intro,
            ReadyToContinue,
            SecondVideo,
            Pills,
            Hallucination,
            Complete
        }

        [SerializeField] private VideoManager videoManager;
        [SerializeField] private VideoClip intro;
        [SerializeField] private VideoClip secondVideo;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject pills;
        [SerializeField] private GameObject hallucinationRoot;
        [SerializeField] private HallucinationEffects effects;
        [SerializeField] private HallucinationTarget[] targets = new HallucinationTarget[0];
        [SerializeField] private AudioSource gameEffectsAudioSource;
        [SerializeField] private AudioClip[] hallucinationSounds = new AudioClip[0];
        [SerializeField] private GameObject exitToMenuButton;
        [SerializeField] private string menuScene;
        [SerializeField] private UnityEvent gameFinished = new UnityEvent();

        [Header("Live progress")]
        [SerializeField] private int totalTargets;
        [SerializeField] private int clickedTargets;
        [SerializeField] private int targetsLeft;

        private Stage stage;
        private int targetsStillVisible;
        private Coroutine hallucinationAudio;

        private void Start()
        {
            SetVisible(continueButton, false);
            SetVisible(pills, false);
            SetVisible(hallucinationRoot, false);
            SetVisible(exitToMenuButton, false);

            totalTargets = targets.Length;
            clickedTargets = 0;
            targetsLeft = totalTargets;

            if (videoManager == null || intro == null)
            {
                LogPlaybackError("Assign the Video Manager and Intro clip.");
                return;
            }

            if (!videoManager.Play(intro, ShowContinue, LogPlaybackError))
                LogPlaybackError("Could not start the intro.");
        }

        public void Continue()
        {
            if (stage != Stage.ReadyToContinue)
                return;

            stage = Stage.SecondVideo;
            SetVisible(continueButton, false);
            if (!videoManager.Play(secondVideo, ShowPills, RestoreContinue))
                RestoreContinue("Could not start the second video. Check the Second Video assignment.");
        }

        public void StartHallucination()
        {
            if (stage != Stage.Pills || !ValidateTargets())
                return;

            stage = Stage.Hallucination;
            targetsStillVisible = totalTargets;
            foreach (HallucinationTarget target in targets)
                target.Begin(this);

            SetVisible(pills, false);
            SetVisible(hallucinationRoot, true);
            if (effects != null)
                effects.FadeIn();

            if (gameEffectsAudioSource != null)
            {
                AudioClip[] clips = System.Array.FindAll(hallucinationSounds, clip => clip != null);
                if (clips.Length > 0)
                    hallucinationAudio = StartCoroutine(PlayHallucinationSounds(clips));
            }
        }

        private IEnumerator PlayHallucinationSounds(AudioClip[] clips)
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(7f, 10f));
                if (gameEffectsAudioSource == null)
                    break;

                AudioClip clip = clips[Random.Range(0, clips.Length)];
                gameEffectsAudioSource.PlayOneShot(clip);
            }

            hallucinationAudio = null;
        }

        private void OnDisable()
        {
            StopHallucinationSounds();
        }

        private void StopHallucinationSounds()
        {
            if (hallucinationAudio == null)
                return;

            StopCoroutine(hallucinationAudio);
            hallucinationAudio = null;
        }

        internal void RegisterClick()
        {
            clickedTargets++;
            targetsLeft--;
        }

        internal void RegisterDisappearance()
        {
            targetsStillVisible--;
            if (targetsStillVisible != 0)
                return;

            stage = Stage.Complete;
            StopHallucinationSounds();
            if (effects != null)
                effects.FadeOut(() => SetVisible(hallucinationRoot, false));
            else
                SetVisible(hallucinationRoot, false);

            SetVisible(exitToMenuButton, true);
            gameFinished.Invoke();
        }

        public void ExitToMenu()
        {
            if (stage != Stage.Complete)
                return;

            if (string.IsNullOrWhiteSpace(menuScene) || !Application.CanStreamedLevelBeLoaded(menuScene))
            {
                Debug.LogError("Assign a Menu Scene included in the build.", this);
                return;
            }

            SceneManager.LoadScene(menuScene);
        }

        private bool ValidateTargets()
        {
            if (hallucinationRoot == null || targets.Length == 0)
            {
                Debug.LogError("Assign the Hallucination Root and at least one target.", this);
                return false;
            }

            var uniqueTargets = new HashSet<HallucinationTarget>();
            foreach (HallucinationTarget target in targets)
            {
                if (target == null || !target.enabled || target.transform == hallucinationRoot.transform ||
                    !target.transform.IsChildOf(hallucinationRoot.transform) || !uniqueTargets.Add(target))
                {
                    Debug.LogError("Each target must be an enabled, distinct component on a child of Hallucination Root.", this);
                    return false;
                }
            }
            return true;
        }

        private void ShowContinue()
        {
            stage = Stage.ReadyToContinue;
            SetVisible(continueButton, true);
        }

        private void ShowPills()
        {
            stage = Stage.Pills;
            SetVisible(pills, true);
        }

        private void RestoreContinue(string reason)
        {
            LogPlaybackError(reason);
            ShowContinue();
        }

        private void LogPlaybackError(string reason)
        {
            Debug.LogError("Donnie video failed: " + reason, this);
        }

        private static void SetVisible(GameObject target, bool visible)
        {
            if (target != null)
                target.SetActive(visible);
        }
    }
}
