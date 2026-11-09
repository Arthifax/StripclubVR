using System.Collections;
using UnityEngine;

namespace KarmaVR
{
    [RequireComponent(typeof(Rigidbody), typeof(FeatherFall))]
    public sealed class MoneyBillVisual : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float flightSeconds = 0.35f;
        [SerializeField, Min(0.1f)] private float fallLifetime = 5f;

        private Rigidbody body;
        private FeatherFall featherFall;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            featherFall = GetComponent<FeatherFall>();
            featherFall.enabled = false;
            body.isKinematic = true;
        }

        internal void FlyTo(Vector3 destination)
        {
            StartCoroutine(FlyAndFall(destination));
        }

        private IEnumerator FlyAndFall(Vector3 destination)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;

            while (elapsed < flightSeconds)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, destination, elapsed / flightSeconds);
                yield return null;
            }

            body.position = destination;
            body.isKinematic = false;
            body.useGravity = true;
            featherFall.enabled = true;
            Destroy(gameObject, fallLifetime);
        }
    }
}
