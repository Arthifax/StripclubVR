using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace KarmaVR
{
    public sealed class HallucinationEffects : MonoBehaviour
    {
        [SerializeField] private PostProcessVolume volume;
        [SerializeField, Range(0f, 1f)] private float peakWeight = 1f;
        [SerializeField, UnityEngine.MinAttribute(0f)] private float fadeInSeconds = 1f;
        [SerializeField, UnityEngine.MinAttribute(0f)] private float fadeOutSeconds = 0.5f;
        [SerializeField] private bool animateColors = true;
        [SerializeField, UnityEngine.MinAttribute(0.05f)] private float colorTransitionSeconds = 1f;

        private Coroutine fade;
        private ColorGrading colorGrading;
        private Bloom bloom;
        private bool cyclingColors;
        private float colorElapsed;
        private float fromHue;
        private float toHue;
        private Color fromBloom;
        private Color toBloom;

        private void Awake()
        {
            if (volume == null)
                return;

            volume.weight = 0f;
        }

        public void FadeIn()
        {
            if (animateColors && volume != null && isActiveAndEnabled)
            {
                // Use this volume's runtime profile so the shared asset stays unchanged.
                PostProcessProfile profile = volume.profile;
                colorGrading = profile.GetSetting<ColorGrading>();
                bloom = profile.GetSetting<Bloom>();

                if (colorGrading != null)
                    colorGrading.hueShift.overrideState = true;
                if (bloom != null)
                    bloom.color.overrideState = true;

                cyclingColors = colorGrading != null || bloom != null;
                if (cyclingColors)
                    PickNextColors();
            }

            StartFade(peakWeight, fadeInSeconds, null);
        }

        internal void FadeOut(Action completed)
        {
            cyclingColors = false;
            StartFade(0f, fadeOutSeconds, completed);
        }

        private void Update()
        {
            if (!cyclingColors)
                return;

            colorElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(colorElapsed / Mathf.Max(0.05f, colorTransitionSeconds));
            if (colorGrading != null)
                colorGrading.hueShift.value = Mathf.Lerp(fromHue, toHue, progress);
            if (bloom != null)
                bloom.color.value = Color.Lerp(fromBloom, toBloom, progress);

            if (progress >= 1f)
                PickNextColors();
        }

        private void PickNextColors()
        {
            colorElapsed = 0f;
            if (colorGrading != null)
            {
                fromHue = colorGrading.hueShift.value;
                toHue = UnityEngine.Random.Range(-180f, 180f);
            }
            if (bloom != null)
            {
                fromBloom = bloom.color.value;
                toBloom = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            }
        }

        private void StartFade(float target, float seconds, Action completed)
        {
            if (fade != null)
            {
                StopCoroutine(fade);
                fade = null;
            }

            if (volume == null || !isActiveAndEnabled)
            {
                completed?.Invoke();
                return;
            }

            if (seconds <= 0f)
            {
                volume.weight = target;
                completed?.Invoke();
                return;
            }

            fade = StartCoroutine(FadeWeight(target, seconds, completed));
        }

        private IEnumerator FadeWeight(float target, float seconds, Action completed)
        {
            float start = volume.weight;
            float elapsed = 0f;
            while (elapsed < seconds && volume != null)
            {
                elapsed += Time.deltaTime;
                volume.weight = Mathf.Lerp(start, target, elapsed / seconds);
                yield return null;
            }

            fade = null;
            completed?.Invoke();
        }
    }
}
