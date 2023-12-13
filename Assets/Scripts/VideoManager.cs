using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public VideoClip[] videos;
    public UnityEvent videoAtEnd;

    [SerializeField] private GameObject panel;

    [SerializeField] private bool playFirstVideoWhenStart = true;

    [SerializeField] private bool playVideo = false;
    [SerializeField] private bool pauseVideo = false;
    [SerializeField] private bool playNextVideo = false;
    [SerializeField] private bool restartVideo = false;

    [HideInInspector]
    public VideoPlayer vp;

    [SerializeField]
    private UnityEvent OnVideoReset;

    int count = 0;

    bool doneFading = true;
    CanvasGroup canvasGroup;

    private void Awake()
    {
        vp = GetComponent<VideoPlayer>();

        if (panel)
            canvasGroup = panel.GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        vp.loopPointReached += VideoDone;

        //StartCoroutine(FadeIn());
        if (playFirstVideoWhenStart)
        {
            PlayVideo();
        }

        if (FindObjectOfType<Level1Manager>())
            FindObjectOfType<Level1Manager>().SetLastVideo(vp.clip);
    }

    private void Update()
    {
        if (playVideo)
        {
            playVideo = false;
            PlayVideo();
        }
        if (pauseVideo)
        {
            pauseVideo = false;
            PauseVideo();
        }
        if (playNextVideo)
        {
            playNextVideo = false;
            PlayNextVideo();
        }
        if (restartVideo)
        {
            restartVideo = false;
            RestartVideo();
        }
    }

    public void PlayVideo()
    {
        //StartCoroutine(FadeOut());
        if (vp.clip == null)
        {
            vp.clip = videos[count];
        }
        vp.Play();
        StartCoroutine(WaitForReady());
    }

    public VideoClip GetCurrentVideo() { return vp.clip; }

    public void PlayNextVideo()
    {
        if (count + 1 < videos.Length)
        {
            //StartCoroutine(FadeOut());
            vp.Stop();
            vp.clip = videos[++count];
            vp.Play();
            StartCoroutine(WaitForReady());
        }
        else Debug.LogError("No next video available");
    }

    public void PlayVideoAtIndex(int index)
    {
        if (index < videos.Length)
        {
            //StartCoroutine(FadeOut());
            vp.Stop();
            vp.clip = videos[index];
            vp.Play();
            StartCoroutine(WaitForReady());
        }
        else Debug.LogError("No video at that index");
    }

    public void PauseVideo()
    {
        vp.Pause();
    }

    public void RestartVideo()
    {
        //StartCoroutine(FadeOut());
        vp.Stop();
        OnVideoReset?.Invoke();
        vp.Play();
        StartCoroutine(WaitForReady());
    }

    public IEnumerator FadeIn()
    {
        //StopCoroutine(FadeOut());
        doneFading = false;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 2;
            yield return null;
        }
        doneFading = true;
    }

    public IEnumerator WaitForReady()
    {
        yield return new WaitUntil(() => vp.isPrepared && doneFading);
        //StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut()
    {
        //StopCoroutine(FadeIn());
        doneFading = false;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * 2;
            yield return null;
        }
        doneFading = true;
    }

    public void PlayVideo(VideoClip clip)
    {
        if (count + 1 < videos.Length)
        {
            //StartCoroutine(FadeOut());
            vp.Stop();
            vp.clip = clip;
            vp.Play();
            StartCoroutine(WaitForReady());
        }
        else Debug.LogError("No next video available");
    }

    void VideoDone(VideoPlayer vp)
    {
        videoAtEnd.Invoke();
    }

    public static void VideoLoader(VideoClip clip)
    {
        FindObjectOfType<VideoManager>().PlayVideo(clip);
    }

    public static void ResetVideo()
    {
        if (SceneManager.GetActiveScene().name != "Level 1")
            FindObjectOfType<VideoManager>().RestartVideo();
        else
            FindObjectOfType<Level1Manager>().ReplayVideo();
    }

    public static VideoPlayer GetPlayer() { return FindObjectOfType<VideoManager>().vp; }
}
