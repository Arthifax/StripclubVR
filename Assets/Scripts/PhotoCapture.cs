using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class PhotoCapture : MonoBehaviour
{
    [SerializeField] private AudioSource source = null;
    [SerializeField] private AudioClip _victorySFX = null;

    [System.Serializable]
    protected class PhotoInfo
    {
        public GameObject objectToBePhotographed;
        public string clue;
    }

    [Header("Customizable")]
    [SerializeField] private List<PhotoInfo> objectsToBePhotographed = new List<PhotoInfo>();

    [Header("Script Stuff")]
    [SerializeField] private AudioClip cameraSFX;
    [SerializeField] private GameObject rayOrigin;

    private float captureInterval = 0.5f;
    private bool canCapture = true;
    private bool pressed = false;
    private List<Tuple<Texture2D, GameObject>> savedPhotos;
    private bool cameraModeActive = true;

    private Camera photoCamera;
    private AudioSource audioSource;

    public UnityEvent photosCaptured;

    private void Awake()
    {
        savedPhotos = new List<Tuple<Texture2D, GameObject>>();
        photoCamera = GetComponent<Camera>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        RaycastHit hit;

        var rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
                    

        if (rightHandDevices.Count == 1)
        {
            InputDevice device = rightHandDevices[0];
            bool triggerButtonValue = false;
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonValue) && triggerButtonValue)
            {
                if (!pressed)
                {
                    pressed = true;
                    Debug.Log("Flits");
                    //TakePhoto();

                    canCapture = false;
                    audioSource.PlayOneShot(cameraSFX);
                    //StartCoroutine(CapturePhoto());
                }
            }
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonValue) && !triggerButtonValue)
            {
                pressed = false;
            }
        }
    }

    #region public methods
    public void TakePhoto()
    {
        if (canCapture && cameraModeActive)
        {
            canCapture = false;
            audioSource.PlayOneShot(cameraSFX);
            StartCoroutine(CapturePhoto());
        }
    }
    #endregion

    // Check if one of the objectsToBePhotographed objects is within the photo
    private GameObject CheckPhotographedObject()
    {
        GameObject closestObject = null;
        float closestDistance = 999;

        foreach (PhotoInfo info in objectsToBePhotographed)
        {
            GameObject obj = info.objectToBePhotographed;
            Vector3 viewportPoint = photoCamera.WorldToViewportPoint(obj.transform.position);
            // If the object is within the viewport of the camera
            if (viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1 && viewportPoint.z > 0)
            {
                // Grab the distance from the object to the center of the camera
                float distanceFromCenter = Vector2.Distance(new Vector2(viewportPoint.x, viewportPoint.y), new Vector2(0.5f, 0.5f));

                // A photo will be taken of the object closest to the center
                if (distanceFromCenter < closestDistance)
                {
                    closestDistance = distanceFromCenter;
                    closestObject = obj;
                }
            }
        }

        return closestObject;
    }

    // Check if all objectsToBePhotographed are photographed
    private bool AllObjectsPhotographed()
    {
        // Checklist to keep track of the objects that aren't photographed yet
        List<GameObject> checklist = new List<GameObject>();
        foreach (PhotoInfo info in objectsToBePhotographed) checklist.Add(info.objectToBePhotographed);

        foreach (Tuple<Texture2D, GameObject> data in savedPhotos)
        {
            // Remove the object that is photographed from the checklist
            checklist.Remove(data.Item2);
        }

        // If no objects are left in the checklist then all objects are photographed
        if (checklist.Count == 0)
        {
            PlaySound(_victorySFX, .7f);
            return true;
        }
        return false;
    }

    private IEnumerator CapturePhoto()
    {
        GameObject photographedObject = CheckPhotographedObject();

        if(photographedObject != null)
        {
            //This line makes the cluetext in the scene visible
            photographedObject.GetComponent<Text>().text = objectsToBePhotographed.Find(o => o.objectToBePhotographed == photographedObject).clue;
        }

        // Check if all objects are photographed
        if (AllObjectsPhotographed())
        {
            //PlaySound(_victorySFX, .7f);
            photosCaptured.Invoke();
        }
        else
        {
            // Wait for interval
            yield return new WaitForSeconds(captureInterval);
            canCapture = true;
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        source.volume = volume;
        source.PlayOneShot(clip);
    }
}
