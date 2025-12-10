using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class Light2DFade : MonoBehaviour
{
    public float targetIntensity = 1.2f;
    public float duration = 0.35f;
    public AnimationCurve curve;

    Light2D l2d;

    void Awake()
    {
        l2d = GetComponent<Light2D>();
        if (curve == null) curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        l2d.intensity = 0f; 
    }

    public void PlayInstant()
    {
        StopAllCoroutines();
        l2d.intensity = targetIntensity;   // snap on this frame
    }

    public void Play()
    {
        StopAllCoroutines();
        if (duration <= 0f) { l2d.intensity = targetIntensity; return; } // instant if duration is 0
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        // write the first sample BEFORE yielding so it’s visible this frame
        l2d.intensity = Mathf.Lerp(0f, targetIntensity, curve != null && curve.length > 0 ? curve.Evaluate(0f) : 0f);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            l2d.intensity = Mathf.Lerp(0f, targetIntensity, curve != null && curve.length > 0 ? curve.Evaluate(p) : p);
            yield return null;
        }
        l2d.intensity = targetIntensity;
    }

}
