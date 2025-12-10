using System.Collections;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    public AudioSource target;
    public float duration = 2f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        float startVol = 0f;
        float endVol = target.volume;

        target.volume = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            target.volume = Mathf.Lerp(startVol, endVol, t / duration);
            yield return null;
        }

        target.volume = endVol;
    }
}

