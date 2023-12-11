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
    [SerializeField] private int maxSavedPhotos = 5;
    [SerializeField] private List<PhotoInfo> objectsToBePhotographed = new List<PhotoInfo>();

    [Header("Script Stuff")]
    [SerializeField] private GameObject cameraDisplayArea;
    [SerializeField] private RenderTexture cameraTexture;
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject cameraEffect;
    [SerializeField] private GameObject photoGallery;
    [SerializeField] private Text photoIndexText;
    [SerializeField] private Text clueText;
    [SerializeField] private Animator flashEffect;

    [SerializeField] private AudioClip cameraSFX = null;

    private float captureInterval = 0.5f;
    private bool canCapture = true;
    private bool pressed = false;
    private List<Tuple<Texture2D, GameObject>> savedPhotos;
    private int currentPhotoIndex = 0;
    private bool cameraModeActive = true;

    private Camera photoCamera;
    private AudioSource audioSource;

    public UnityEvent photosCaptured;

    private void Awake()
    {
        savedPhotos = new List<Tuple<Texture2D, GameObject>>();
        photoCamera = GetComponent<Camera>();
        audioSource = GetComponent<AudioSource>();
        photoCamera.targetTexture = cameraTexture;
        photoIndexText.text = "1/" + maxSavedPhotos;
    }

    private void Update()
    {

        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && !pressed) //|| Input.GetKeyDown(KeyCode.Space))
        {
            pressed = true;
            if (cameraModeActive) TakePhoto(); else SwitchPhoto();
        }
        else
        {
            pressed = false;
        }
    }

    #region public methods
    public void TakePhoto()
    {
        if (canCapture && cameraModeActive)
        {
            canCapture = false;
            audioSource.PlayOneShot(cameraSFX);
            flashEffect.SetTrigger("Flash");
            StartCoroutine(CapturePhoto());
        }
    }

    // Switches between each taken photo in order
    public void SwitchPhoto(int i = 1)
    {
        // If no photos are taken yet, don't switch
        if (savedPhotos.Count == 0 || cameraModeActive) return;

        if (i == 1) currentPhotoIndex++;
        else currentPhotoIndex--;

        if (currentPhotoIndex >= savedPhotos.Count) currentPhotoIndex = 0;
        else if (currentPhotoIndex < 0) currentPhotoIndex = savedPhotos.Count - 1;

        // Draw the sprite corresponding to the index number onto the camera
        photoDisplayArea.sprite = Sprite.Create(savedPhotos[currentPhotoIndex].Item1, new Rect(0.0f, 0.0f, savedPhotos[currentPhotoIndex].Item1.width, savedPhotos[currentPhotoIndex].Item1.height), new Vector2(0.5f, 0.5f), 100.0f);
        // Update the index number text
        photoIndexText.text = (currentPhotoIndex + 1) + "/" + maxSavedPhotos;
        // Update the clue text
        clueText.text = objectsToBePhotographed.Find(o => o.objectToBePhotographed == savedPhotos[currentPhotoIndex].Item2).clue;
    }

    // Switches between camera mode and gallery mode
    public void SwitchMode()
    {
        if (cameraModeActive)
        {
            // Disable all camera mode objects, enable all gallery mode objects
            cameraModeActive = false;
            photoDisplayArea.gameObject.SetActive(true);
            cameraDisplayArea.SetActive(false);
            cameraEffect.SetActive(false);
            photoGallery.SetActive(true);
        }
        else
        {
            // Vice versa
            cameraModeActive = true;
            cameraDisplayArea.SetActive(true);
            photoDisplayArea.gameObject.SetActive(false);
            photoGallery.SetActive(false);
            cameraEffect.SetActive(true);
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
        //yield return new WaitForEndOfFrame();

        //// Render photo FOTO IS NOT SHOWN ANYMORE SO ALL THIS CODE IS TURNED OFF
        //RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        //photoCamera.targetTexture = renderTexture;
        //photoCamera.Render();
        //RenderTexture.active = renderTexture;
        //Texture2D screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //screenCapture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //screenCapture.Apply();
        //photoCamera.targetTexture = cameraTexture;
        //RenderTexture.active = null;
        //Destroy(renderTexture);

        //// Show photo
        //photoDisplayArea.sprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Save photo and object
        GameObject photographedObject = CheckPhotographedObject();

        savedPhotos.Add(Tuple.Create(new Texture2D(0,0)/*screenCapture*/, photographedObject));
        



        // ------------------------------------------------------------------------------------------------------------------------------------------





        //if (savedPhotos.Count > maxSavedPhotos) savedPhotos.RemoveAt(0);
        currentPhotoIndex = savedPhotos.Count - 1;
        // Update text
        photoIndexText.text = savedPhotos.Count + "/" + maxSavedPhotos;

        if(photographedObject != null)
        {
            //This line makes the cluetext in the scene visible
            photographedObject.GetComponent<Text>().text = objectsToBePhotographed.Find(o => o.objectToBePhotographed == photographedObject).clue;

            clueText.text = objectsToBePhotographed.Find(o => o.objectToBePhotographed == photographedObject).clue;
        }

        // Check if all objects are photographed
        if (AllObjectsPhotographed())
        {
            // Show clue
            clueText.gameObject.SetActive(true);
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
