using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace KarmaVR
{
    public sealed class HallucinationEffects : MonoBehaviour
    {
        [SerializeField, Tooltip("Drag the Volume or PostProcessVolume component here.")]
        private Behaviour volume;
        [SerializeField, Range(0f, 1f)] private float peakWeight = 1f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 1f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.5f;

        private FieldInfo weightField;
        private Coroutine fade;

        private void Awake()
        {
            if (volume == null)
                return;

            // Both volume components expose a public float weight field.
            weightField = volume.GetType().GetField("weight", BindingFlags.Instance | BindingFlags.Public);
            if (weightField == null || weightField.FieldType != typeof(float))
            {
                weightField = null;
                Debug.LogError("Hallucination Effects needs a volume component with a float weight.", this);
                return;
            }

            weightField.SetValue(volume, 0f);
        }

        public void FadeIn()
        {
            StartFade(peakWeight, fadeInSeconds, null);
        }

        internal void FadeOut(Action completed)
        {
            StartFade(0f, fadeOutSeconds, completed);
        }

        private void StartFade(float target, float seconds, Action completed)
        {
            if (fade != null)
            {
                StopCoroutine(fade);
                fade = null;
            }

            if (weightField == null || volume == null || !isActiveAndEnabled)
            {
                completed?.Invoke();
                return;
            }

            if (seconds <= 0f)
            {
                weightField.SetValue(volume, target);
                completed?.Invoke();
                return;
            }

            fade = StartCoroutine(FadeWeight(target, seconds, completed));
        }

        private IEnumerator FadeWeight(float target, float seconds, Action completed)
        {
            float start = (float)weightField.GetValue(volume);
            float elapsed = 0f;
            while (elapsed < seconds && volume != null)
            {
                elapsed += Time.deltaTime;
                weightField.SetValue(volume, Mathf.Lerp(start, target, elapsed / seconds));
                yield return null;
            }

            fade = null;
            completed?.Invoke();
        }
    }
}
