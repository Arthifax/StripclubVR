using System.Collections;
using UnityEngine;

namespace KarmaVR
{
    public sealed class HallucinationTarget : MonoBehaviour
    {
        [SerializeField] private Vector3 driftDistance = new Vector3(0.25f, 0.18f, 0.12f);
        [SerializeField, Min(0f)] private float driftSpeed = 1.2f;
        [SerializeField] private Vector3 rotationPerSecond = new Vector3(0f, 20f, 0f);
        [SerializeField, Min(0f)] private float shrinkSeconds = 0.5f;

        private DonnieGameManager manager;
        private Vector3 startPosition;
        private float driftTime;
        private float phase;
        private bool clicked;

        internal void Begin(DonnieGameManager owner)
        {
            manager = owner;
            startPosition = transform.localPosition;
            phase = Random.Range(0f, 2f * Mathf.PI);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (manager == null)
                return;

            driftTime += Time.deltaTime * driftSpeed;
            var offset = new Vector3(
                Mathf.Sin(driftTime + phase) - Mathf.Sin(phase),
                Mathf.Sin(driftTime * 0.8f + phase) - Mathf.Sin(phase),
                Mathf.Sin(driftTime * 0.6f + phase) - Mathf.Sin(phase));
            transform.localPosition = startPosition + Vector3.Scale(driftDistance, offset) * 0.5f;
            transform.Rotate(rotationPerSecond * Time.deltaTime, Space.Self);
        }

        public void Click()
        {
            if (manager == null || clicked || !isActiveAndEnabled)
                return;

            clicked = true;
            foreach (Collider hitbox in GetComponentsInChildren<Collider>())
                hitbox.enabled = false;

            manager.RegisterClick();
            StartCoroutine(Shrink());
        }

        private IEnumerator Shrink()
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < shrinkSeconds)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / shrinkSeconds);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                yield return null;
            }

            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
            if (manager != null)
                manager.RegisterDisappearance();
        }
    }
}
