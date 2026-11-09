using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace KarmaVR
{
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class VideoManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField, Min(0)] private float fadeSeconds = 0.35f;
        [SerializeField, Min(1)] private float startupTimeout = 15f;

        private VideoPlayer player;
        private bool busy;
        private bool firstFrame;
        private bool ended;
        private string error;

        private void Awake()
        {
            player = GetComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.isLooping = false;
            player.waitForFirstFrame = true;
            player.source = VideoSource.VideoClip;
        }

        private void OnEnable()
        {
            player.frameReady += OnFrame;
            player.loopPointReached += OnEnd;
            player.errorReceived += OnError;
        }

        private void OnDisable()
        {
            player.frameReady -= OnFrame;
            player.loopPointReached -= OnEnd;
            player.errorReceived -= OnError;
            StopAllCoroutines();
            player.sendFrameReadyEvents = false;
            player.Stop();
            if (fadeOverlay != null)
                fadeOverlay.alpha = 0;
            busy = false;
        }

        // The scene VideoPlayer owns the RenderTexture assignment.
        public bool Play(VideoClip clip, Action completed, Action<string> failed)
        {
            return Play(clip, completed, failed, interruptCurrent: false);
        }

        public bool Play(VideoClip clip, Action completed, Action<string> failed, bool interruptCurrent)
        {
            if (!isActiveAndEnabled || clip == null || (busy && !interruptCurrent))
                return false;

            if (busy)
            {
                StopAllCoroutines();
                player.sendFrameReadyEvents = false;
                player.Pause();
            }

            busy = true;
            StartCoroutine(PlayClip(clip, completed, failed));
            return true;
        }

        private IEnumerator PlayClip(VideoClip clip, Action completed, Action<string> failed)
        {
            yield return FadeTo(1);
            player.Stop();
            firstFrame = false;
            ended = false;
            error = null;
            player.clip = clip;
            player.Prepare();

            float deadline = Time.realtimeSinceStartup + startupTimeout;
            while (!player.isPrepared && error == null && Time.realtimeSinceStartup < deadline)
                yield return null;

            if (error == null && player.isPrepared)
            {
                player.sendFrameReadyEvents = true;
                player.Play();
                deadline = Time.realtimeSinceStartup + startupTimeout;
                while (!firstFrame && !ended && error == null && Time.realtimeSinceStartup < deadline)
                    yield return null;

                player.sendFrameReadyEvents = false;
                if (firstFrame && error == null)
                {
                    yield return FadeTo(0);
                    while (!ended && error == null)
                        yield return null;
                }
                else if (error == null)
                    error = "The video did not produce a frame.";
            }
            else if (error == null)
                error = "Video preparation timed out.";

            // Pause preserves the rendered frame while the puzzle is open.
            // Stop is reserved for replacing the clip or disabling this component.
            player.Pause();
            yield return FadeTo(0);

            busy = false;
            if (error == null)
                completed?.Invoke();
            else
                failed?.Invoke(error);
        }

        private IEnumerator FadeTo(float target)
        {
            if (fadeOverlay == null)
                yield break;

            if (fadeSeconds > 0)
            {
                while (fadeOverlay.alpha != target)
                {
                    fadeOverlay.alpha = Mathf.MoveTowards(
                        fadeOverlay.alpha, target, Time.unscaledDeltaTime / fadeSeconds);
                    yield return null;
                }
            }
            fadeOverlay.alpha = target;
        }

        private void OnFrame(VideoPlayer source, long frame) => firstFrame = true;
        private void OnEnd(VideoPlayer source)
        {
            source.Pause();
            ended = true;
        }
        private void OnError(VideoPlayer source, string message) => error = message;
    }
}
