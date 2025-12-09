using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeIn : MonoBehaviour
{
    public float delay = 0.25f;
    public float duration = 1.2f;
    public AnimationCurve curve = null;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (curve == null) curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        var c = sr.color; c.a = 0f; sr.color = c;
    }

    IEnumerator Start()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = curve.Evaluate(Mathf.Clamp01(t / duration));
            var c = sr.color; c.a = a; sr.color = c;
            yield return null;
        }
    }
}
