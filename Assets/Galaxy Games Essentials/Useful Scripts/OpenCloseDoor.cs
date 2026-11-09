using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseDoor : MonoBehaviour
{
    [SerializeField] bool _isOpen = false;
    [SerializeField] Vector3 targetRotation;
    [SerializeField] private float rotSpeed = 5f;
    Vector3 startRotation;

    private bool isRunning = false; 

    public bool IsOpen
    {
        get { return _isOpen; }
        set { _isOpen = value; }
    }
    public float RotSpeed
    {
        get { return rotSpeed; }
        set { rotSpeed = value; }
    }

    // Add any references to the target rotation here, if needed.
    // For example, you can use Quaternion.Euler or store a target rotation variable.

    private void Start()
    {
        // Store the start rotation of the GameObject.
        startRotation = transform.eulerAngles;
    }

    public void ToggleRotation()
    {
        if (isRunning) return; // Prevent running the coroutine multiple times.
        // Rotate the GameObject based on the 'isOpen' state.
        if (_isOpen)
        {
            // Add code to get the target rotation, if needed.
            // For example, you can use Quaternion.Euler or another variable holding the target rotation.

            // Example: Rotate towards a target rotation over a duration.
            // Vector3 targetRotation = new Vector3(0f, 90f, 0f);
            // float rotationSpeed = 5f;
            // StartCoroutine(RotateOverTime(targetRotation, rotationSpeed));

            // Or, simply use the start rotation as the target rotation.
            // RotateTo(startRotation);
            StartCoroutine(RotateOverTime(startRotation, rotSpeed));
        }
        else
        {
            // RotateTo(targetRotation); // Rotate back to the start rotation.
            StartCoroutine(RotateOverTime(targetRotation, rotSpeed));
        }

        // Toggle the 'isOpen' boolean.
        _isOpen = !_isOpen;
    }

    private void RotateTo(Vector3 targetRotation)
    {
        transform.rotation = Quaternion.Euler(targetRotation);
    }

    // If you want to use Lerp to rotate over time, you can use this coroutine:
    private IEnumerator RotateOverTime(Vector3 targetRotation, float rotationSpeed)
    {
        isRunning = true;
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);

        while (elapsedTime < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetQuaternion, elapsedTime);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.rotation = targetQuaternion; // Ensure we reach the exact target rotation.
        isRunning = false;
    }
}
