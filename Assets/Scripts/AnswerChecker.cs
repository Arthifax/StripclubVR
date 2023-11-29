using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class AnswerChecker : MonoBehaviour
{
    public string correctAnswer;
    public UnityEvent answered;
    [SerializeField] private TextMeshProUGUI keyboardInput;

    [SerializeField] private AudioClip _wrongSFX = null;
    [SerializeField] private AudioSource source = null;
    [SerializeField] private AudioClip _correctSFX = null;

    public void CheckAnswer(string input)
    {
        if (input.ToLower() == correctAnswer.ToLower())
        {
            PlaySound(_correctSFX, .7f);
            answered.Invoke();
        }
        else
        {
            PlaySound(_wrongSFX, .9f);
        }
    }

    public void CheckAnswerKeyboard()
    {
        string answer = keyboardInput.text;
        if (answer.ToLower() == correctAnswer.ToLower())
        {
            PlaySound(_correctSFX, .7f);
            answered.Invoke();
        }
        else
        {
            PlaySound(_wrongSFX, .9f);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        source.volume = volume;
        source.PlayOneShot(clip);
    }
}
