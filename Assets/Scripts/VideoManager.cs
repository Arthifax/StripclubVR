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
        private long latestFrame;
        private bool ended;
        private string error;
        private RenderTexture skyboxTarget;
        private RenderTexture seekTarget;

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
            if (seekTarget != null)
                ReleaseSeekTarget(publishFrame: false);
            else
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
            return StartPlayback(clip, completed, failed, interruptCurrent, holdLastFrame: false);
        }

        public bool HoldFinalFrame(VideoClip clip, Action completed, Action<string> failed)
        {
            return StartPlayback(clip, completed, failed, interruptCurrent: false, holdLastFrame: true);
        }

        private bool StartPlayback(VideoClip clip, Action completed, Action<string> failed,
            bool interruptCurrent, bool holdLastFrame)
        {
            if (!isActiveAndEnabled || clip == null || (busy && !interruptCurrent))
                return false;

            if (busy)
            {
                StopAllCoroutines();
                player.sendFrameReadyEvents = false;
                player.Pause();
                ReleaseSeekTarget(publishFrame: false);
            }

            busy = true;
            StartCoroutine(PlayClip(clip, completed, failed, holdLastFrame));
            return true;
        }

        private IEnumerator PlayClip(VideoClip clip, Action completed, Action<string> failed, bool holdLastFrame)
        {
            if (holdLastFrame && player.renderMode == VideoRenderMode.RenderTexture && player.targetTexture != null)
            {
                // Leave the previous frame on the skybox while the intro decodes and seeks.
                skyboxTarget = player.targetTexture;
                seekTarget = RenderTexture.GetTemporary(skyboxTarget.descriptor);
                player.targetTexture = seekTarget;
            }
            else
                yield return FadeTo(1);

            player.Stop();
            firstFrame = false;
            latestFrame = -1;
            ended = false;
            error = null;
            player.clip = clip;
            player.Prepare();

            float deadline = Time.realtimeSinceStartup + startupTimeout;
            while (!player.isPrepared && error == null && Time.realtimeSinceStartup < deadline)
                yield return null;

            if (error == null && !player.isPrepared)
                error = "Video preparation timed out.";
            if (error == null && holdLastFrame && (!player.canSetTime || player.length <= 0 || player.frameCount == 0))
                error = "This clip does not support seeking to its final frame.";

            if (error == null)
            {
                player.sendFrameReadyEvents = true;
                player.Play();
                deadline = Time.realtimeSinceStartup + startupTimeout;
                while (!firstFrame && !ended && error == null && Time.realtimeSinceStartup < deadline)
                    yield return null;

                if (firstFrame && error == null)
                {
                    if (holdLastFrame)
                    {
                        // Capture before end-of-stream, while the decoded image is still available.
                        long framesBeforeEnd = Math.Max(1L, (long)Math.Ceiling(player.frameRate * 0.12d));
                        long holdFrame = Math.Max(0L, (long)player.frameCount - 1 - framesBeforeEnd);
                        if (!ended && latestFrame < holdFrame)
                            player.time = Math.Max(0d, player.length - 0.25d);

                        deadline = Time.realtimeSinceStartup + startupTimeout;
                        while (latestFrame < holdFrame && !ended && error == null &&
                            Time.realtimeSinceStartup < deadline)
                            yield return null;

                        if (error == null && (ended || latestFrame < holdFrame))
                            error = "Could not capture the intro backdrop before playback ended.";
                    }
                    else
                    {
                        player.sendFrameReadyEvents = false;
                        yield return FadeTo(0);
                        while (!ended && error == null)
                            yield return null;
                    }
                }
                else if (error == null)
                    error = "The video did not produce a frame.";
            }

            // Keep regular playback paused. For backdrop captures, stop only after copying the frame.
            player.sendFrameReadyEvents = false;
            player.Pause();
            ReleaseSeekTarget(publishFrame: error == null);
            yield return FadeTo(0);

            busy = false;
            if (error == null)
                completed?.Invoke();
            else
                failed?.Invoke(error);
        }

        private void ReleaseSeekTarget(bool publishFrame)
        {
            if (seekTarget == null)
                return;

            if (publishFrame && skyboxTarget != null)
            {
                RenderTexture previous = RenderTexture.active;
                Graphics.Blit(seekTarget, skyboxTarget);
                RenderTexture.active = previous;
            }

            // Stop while the player still points at the temporary texture.
            // Reconnecting it first can overwrite the held skybox image.
            player.Stop();
            player.targetTexture = skyboxTarget;
            RenderTexture.ReleaseTemporary(seekTarget);
            seekTarget = null;
            skyboxTarget = null;
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

        private void OnFrame(VideoPlayer source, long frame)
        {
            firstFrame = true;
            latestFrame = frame;
        }
        private void OnEnd(VideoPlayer source)
        {
            source.Pause();
            ended = true;
        }
        private void OnError(VideoPlayer source, string message) => error = message;
    }
}
